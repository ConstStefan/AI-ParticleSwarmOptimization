using static ProiectCuAnimatie.ParticleSwarmOptimization;

namespace ProiectCuAnimatie
{
    public class ParticleSwarmOptimization
    {
        public static List<Particle> particles;
        public static Particle globalBest;

        public class Problem
        {
            public Func<double[], double> ObjectiveFunction;
            public Func<int, Tuple<double, double>> Domain;
        }

        public class Parameters
        {
            public int NumberOfParticles;
            public int ProblemDimension;
            public double MaxVelocity;
            public double W;
            public double C1;
            public double C2;
            public int NumberOfIterations;
        }

        public class Particle
        {
            public double[] Position;
            public double Cost;
            public double[] Velocity;
            public Particle PersonalBest;
        }

        public static void Optimize(Problem problem, Parameters parameters)
        {
            UpdateParticles(problem, parameters);
        }

        private static void UpdateParticles(Problem problem, Parameters parameters)
        {
            foreach (var particle in particles)
            {
                var r1 = RandomDouble();
                var r2 = RandomDouble();

                for (int dimension = 0; dimension < parameters.ProblemDimension; dimension++)
                {
                    particle.Velocity[dimension] =
                        parameters.W * particle.Velocity[dimension] +
                        parameters.C1 * r1 * (particle.PersonalBest.Position[dimension] - particle.Position[dimension]) +
                        parameters.C2 * r2 * (globalBest.Position[dimension] - particle.Position[dimension]);

                    particle.Velocity[dimension] = Limit(particle.Velocity[dimension], -parameters.MaxVelocity, parameters.MaxVelocity);
                    particle.Position[dimension] += particle.Velocity[dimension];

                    particle.Position[dimension] = Limit(particle.Position[dimension], problem.Domain(dimension).Item1, problem.Domain(dimension).Item2);
                }

                var newCost = problem.ObjectiveFunction(particle.Position);
                if (newCost < particle.Cost)
                {
                    particle.Cost = newCost;
                    particle.PersonalBest = particle.Cost < particle.PersonalBest.Cost ? particle : particle.PersonalBest;

                    if (particle.Cost < globalBest.Cost)
                    {
                        globalBest = particle;
                    }
                }
            }
        }

        public static List<Particle> Initialize(Problem problem, Parameters parameters)
        {
            var swarm = new List<Particle>();

            for (int i = 0; i < parameters.NumberOfParticles; i++)
            {
                var particle = new Particle
                {
                    Position = new double[parameters.ProblemDimension],
                    Velocity = new double[parameters.ProblemDimension]
                };

                for (int dimension = 0; dimension < parameters.ProblemDimension; dimension++)
                {
                    var domain = problem.Domain(dimension);
                    particle.Position[dimension] = RandomDouble(domain.Item1, domain.Item2);
                    particle.Velocity[dimension] = RandomDouble(-parameters.MaxVelocity, parameters.MaxVelocity);
                }

                particle.Cost = problem.ObjectiveFunction(particle.Position);
                particle.PersonalBest = particle;

                swarm.Add(particle);
            }

            return swarm;
        }

        private static double RandomDouble(double minValue = 0.0, double maxValue = 1.0)
        {
            Random random = new Random();
            return minValue + (maxValue - minValue) * random.NextDouble();
        }

        private static double Limit(double value, double minValue, double maxValue)
        {
            return Math.Min(Math.Max(value, minValue), maxValue);
        }
    }

    public class ParticleVisualization : Form
    {
        private System.Threading.Timer simulationTimer;
        private const int domainWidth = 600;
        private const int domainHeight = 600;
        private int particleSize = 5;
        private int currentCounter = 0;

        Label resultLabel;
        Label resultPositionLabel;

        private Problem problem = new Problem
        {
            ObjectiveFunction = x => x.Sum(),
            Domain = i => Tuple.Create(0.0, 600.0)
        };

        private Parameters parameters = new Parameters
        {
            NumberOfParticles = 50,
            ProblemDimension = 2,
            MaxVelocity = 2.0,
            W = 0.75,
            C1 = 2,
            C2 = 2,
            NumberOfIterations = 200
        };

        public ParticleVisualization()
        {
            particles = Initialize(problem, parameters);

            // Actualizarea vizualizării la fiecare 50 ms
            simulationTimer = new System.Threading.Timer(UpdateVisualization, null, 0, 50);

            this.Text = "Particle Swarm Optimization Visualization";
            this.ClientSize = new Size(domainWidth, domainHeight);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;

            AddLabels();

            this.Paint += ParticleVisualization_Paint;
        } 

        private void AddLabels()
        {
            resultLabel = new Label();
            resultLabel.Location = new Point(20, 520);
            resultLabel.AutoSize = true;

            this.Controls.Add(resultLabel);

            resultPositionLabel = new Label();
            resultPositionLabel.Location = new Point(20, 550);
            resultPositionLabel.AutoSize = true;

            this.Controls.Add(resultPositionLabel);
        }

        private void ParticleVisualization_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            foreach (var particle in particles) {
                var color = Brushes.Gray;
               
                if(particle == globalBest)
                {
                    color = Brushes.Black;
                    particleSize = 8;
                }
                else
                {
                    color = Brushes.Gray;
                    particleSize = 5;
                }
                g.FillEllipse(color, (float)particle.Position[0], (float)particle.Position[1], particleSize, particleSize);
            }

            if (resultLabel != null && resultPositionLabel != null)
            {
                resultLabel.Text = "Minimum Particle Cost: " + globalBest.Cost.ToString(); ;
                resultPositionLabel.Text = $"Position: [{string.Join(", ", globalBest.Position)}]";
            }
        }

        private void UpdateVisualization(object state)
        {
            if (currentCounter < parameters.NumberOfIterations)
            {
                // optim-social, particula cu functia obiectiv minim
                globalBest = particles.OrderBy(p => p.Cost).First();

                //UpdateByVelocity();
                Optimize(problem, parameters);
                currentCounter++;
            }
            else
            {
                simulationTimer.Dispose();
            }           

            try { 
                this.Invoke((MethodInvoker)delegate { this.Invalidate(); }); // Actualizare vizualizare
            }
            catch { }
        }
        
        private void UpdateByVelocity()
        {
            foreach (var particle in particles)
            {
                particle.Position[0] += particle.Velocity[0];
                particle.Position[1] += particle.Velocity[1];

                // Limitarea mișcării particulelor în cadrul domeniului
                if (particle.Position[0] < 0 || particle.Position[0] > domainWidth)
                    particle.Velocity[0] *= -1;
                if (particle.Position[1] < 0 || particle.Position[1] > domainHeight)
                    particle.Velocity[1] *= -1;

                particle.Cost = problem.ObjectiveFunction(particle.Position);
            }
        }

        public static void Main()
        {
            Application.Run(new ParticleVisualization());
        }
    }
}

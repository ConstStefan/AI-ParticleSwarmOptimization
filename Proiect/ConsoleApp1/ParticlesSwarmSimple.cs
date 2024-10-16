using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectIA
{



    public class ParticleSwarmOptimization
    {
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

        public static Particle Optimize(Problem problem, Parameters parameters)
        {
            var swarm = Initialize(problem, parameters);
            // optim-social, particula cu functia obiectiv maxima
            var globalBest = swarm.OrderByDescending(p => p.Cost).First();

            for (int iteration = 0; iteration < parameters.NumberOfIterations; iteration++)
            {
                foreach (var particle in swarm)
                {
                    // numere aleatorii intre 0 si 1 
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

            return globalBest;
        }

        private static List<Particle> Initialize(Problem problem, Parameters parameters)
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

        public static void Main()
        {
            // Define your problem
            var problem = new Problem
            {
                ObjectiveFunction = x => x.Sum(xi => Math.Pow(xi, 2)),
                Domain = i => Tuple.Create(-5.0, 5.0) // Example: [-5, 5] for each dimension
            };

            // Parametri recomandati in curs
            var parameters = new Parameters
            {
                NumberOfParticles = 20,
                ProblemDimension = 3,
                MaxVelocity = 1.0,
                W = 0.75,
                C1 = 2,
                C2 = 2,
                NumberOfIterations = 100
            };

            // Run the optimization algorithm
            var solution = Optimize(problem, parameters);

            // Display the result
            Console.WriteLine("Optimal Solution:");
            Console.WriteLine($"Cost: {solution.Cost}");
            Console.WriteLine($"Position: [{string.Join(", ", solution.Position)}]");
        }
    }

}

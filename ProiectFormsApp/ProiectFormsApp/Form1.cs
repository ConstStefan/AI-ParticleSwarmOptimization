namespace ProiectFormsApp
{
    public partial class Form1 : Form
    {
        public static Form1 Instance;


        public Form1()
        {
            Instance = this;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
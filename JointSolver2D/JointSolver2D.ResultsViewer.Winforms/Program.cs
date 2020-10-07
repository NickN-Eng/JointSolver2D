using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JointSolver2D.ResultsViewer.Winforms
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var example = new Examples.Example1_Simple2Bar();
            Application.Run(new OxyForm_ResultsView(example.DoAnalysis(), example.Name + " Example"));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JointSolver2D.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = new RightArrow_2Bar_Example();


            Console.WriteLine($"Example: {example.Name}");
            
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var analysis = example.DoAnalysis();
            stopWatch.Stop();

            Console.WriteLine($"Completed in {stopWatch.Elapsed.TotalMilliseconds} milliseconds");
            Console.WriteLine("Bar forces as follows:");

            for (int i = 0; i < analysis.Bars.Count; i++)
            {
                var bar = analysis.Bars[i];
                Console.WriteLine($" - Bar {bar.Number}: Force={bar.ForceResult}, Expected={example.ExpectedBarForces[i]}");
            }

            Console.ReadLine();
        }
    }
}

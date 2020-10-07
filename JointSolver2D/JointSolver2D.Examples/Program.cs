using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JointSolver2D;

namespace JointSolver2D.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var example = new Example1_Simple2Bar();

            Console.WriteLine($"Example: {example.Name}");
            
            var analysis = example.DoAnalysis();

            Console.WriteLine();
            Console.WriteLine(analysis.WriteReport());

            Console.ReadLine();
        }
    }
}

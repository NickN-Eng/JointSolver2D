using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JointSolver2D;

namespace JointSolver2D.Examples
{
    public interface IExample
    {
        string Name { get; }

        JointAnalysis DoAnalysis();

        double[] ExpectedBarForces { get; }
    }

    public class RightArrow_2Bar_Example : IExample
    {
        public string Name => "RightArrow 2Bar";

        private double[] _ExpectedBarForces => new double[] { 0.71, -0.71 };

        public double[] ExpectedBarForces => _ExpectedBarForces;

        public JointAnalysis DoAnalysis()
        {
            var lowerSupportNode = new JSNode(new Vector2d(0, 0)) { XRestrained = true, YRestrained = true };
            var upperSupportNode = new JSNode(new Vector2d(0, 10)) { XRestrained = true, YRestrained = true };
            var forceNode = new JSNode(new Vector2d(5, 5));
            var nodes = new JSNode[] { upperSupportNode, forceNode, lowerSupportNode };

            var force = new JSPointLoad(new Vector2d(0, -1), forceNode);
            var forces = new JSPointLoad[] { force };

            var lowerBar = new JSBar(lowerSupportNode, forceNode);
            var upperBar = new JSBar(upperSupportNode, forceNode);
            var bars = new JSBar[] { upperBar, lowerBar };

            var analyser = new JointAnalysis();
            analyser.AddItems(bars, nodes, forces);
            analyser.Solve();
            return analyser;
        }
    }
}

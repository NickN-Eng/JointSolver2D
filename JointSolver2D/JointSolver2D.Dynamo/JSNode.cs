using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using js = JointSolver2D;

namespace JointSolver2D.Dynamo
{

    public class JSNode
    {
        public static js.JSNode FreeNodeByCoordinates(double x, double y) => new js.JSNode(new Vector2d(x, y));
    }
}

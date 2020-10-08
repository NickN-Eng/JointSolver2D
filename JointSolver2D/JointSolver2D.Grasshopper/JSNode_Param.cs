using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using JointSolver2D;
using Grasshopper.Kernel.Types;

namespace JointSolver2D.Grasshopper
{
    public class JSNode_Param : GH_Param<JSNode_Goo>
    {
        public JSNode_Param() : base("JSNode", "JSNode", "Node(s) from the JointSolver library", "JointSolver2D", "Geometry", GH_ParamAccess.item) { }

        public override Guid ComponentGuid => new Guid("{2FEEF1A2-A764-431d-8C78-9BF92C78BAE1}");
    }


    public class JSNode_Goo : GH_Goo<JSNode>
    {
        public override bool IsValid => true;

        public override string TypeName => "JSNode";

        public override string TypeDescription => "A node";
         
        public override IGH_Goo Duplicate()
        {
            return new JSNode_Goo() { Value = new JSNode(Value) };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }



    public class JSBar_Param : GH_Param<JSBar_Goo>
    {
        public JSBar_Param() : base("JSBar", "JSBar", "Bars(s) from the JointSolver library", "JointSolver2D", "Geometry", GH_ParamAccess.item) { }

        public override Guid ComponentGuid => new Guid("{2FEEF1A2-A764-431d-8C78-9BF92C78BAE2}");
    }

    public class JSBar_Goo : GH_Goo<JSBar>
    {
        public override bool IsValid => true;

        public override string TypeName => "JSNode";

        public override string TypeDescription => "A node";

        public override IGH_Goo Duplicate()
        {
            return new JSBar_Goo() { Value = new JSBar(Value) };
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
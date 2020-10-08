using System;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace JointSolver2D.Grasshopper
{
    public class JSNodeFromPoint : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public JSNodeFromPoint() : base(name:"Create JSNode", 
                                        nickname:"JSNode",
                                        description:"Create a JSNode from a point.",
                                        category:"JointSolver2D", 
                                        subCategory:"Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "Pt", "Node position in XY coordinates", GH_ParamAccess.item);
            pManager.AddBooleanParameter("XRestrained", "XFix", "Is this node restrained in the X direction?", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("YRestrained", "YFix", "Is this node restrained in the Y direction?", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new JSNode_Param(), "JSNode", "Node","",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var point = new Point3d(); //Input types. Declare that JointSolver2D.Vector2d<=>Rhino.Geometry.Vector3d is a substituted with Point
            if (!DA.GetData(0, ref point)) return;
            var point2d = new Vector2d(point.X, point.Y); //Convert ot Joint2d type

            bool xRestr = false;
            if (!DA.GetData(1, ref xRestr)) return;

            bool yRestr = false;
            if (!DA.GetData(2, ref yRestr)) return;

            //The actual method!
            var node = new JSNode(point2d, xRestr, yRestr);
            var nodeGoo = new JSNode_Goo() { Value = node };
            
            DA.SetData(0, nodeGoo);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("77bffa30-624d-4f7a-be10-323fde1bdd8e"); }
        }
    }
}
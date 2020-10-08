using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace JointSolver2D.Grasshopper
{
    public class JointSolver2DInfo : GH_AssemblyInfo
  {
    public override string Name
    {
        get
        {
            return "JointSolver2D";
        }
    }
    public override Bitmap Icon
    {
        get
        {
            //Return a 24x24 pixel bitmap to represent this GHA library.
            return null;
        }
    }
    public override string Description
    {
        get
        {
            //Return a short string describing the purpose of this GHA library.
            return "";
        }
    }
    public override Guid Id
    {
        get
        {
            return new Guid("a701dfab-315e-47b3-afdb-51302a03a64c");
        }
    }

    public override string AuthorName
    {
        get
        {
            //Return a string identifying you or your company.
            return "NickN";
        }
    }
    public override string AuthorContact
    {
        get
        {
            //Return a string representing your preferred contact details.
            return "";
        }
    }
}
}

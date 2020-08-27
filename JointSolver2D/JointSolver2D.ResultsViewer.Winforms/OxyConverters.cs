using OxyPlot;

namespace JointSolver2D.ResultsViewer.Winforms
{
    public static class OxyConverters
    {
        public static DataPoint ToDataPoint(this Vector2d vector) => new DataPoint(vector.X, vector.Y);
    }
}

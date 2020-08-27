using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JointSolver2D.ResultsViewer.Winforms
{
    public partial class OxyForm_ResultsView : Form
    {
        PlotModel Model;
        JointAnalysis JointAnalysis;


        public OxyForm_ResultsView(JointAnalysis jointAnalysis, string title)
        {
            InitializeComponent();

            //this.plotView1.Size = this.ClientSize;
            this.SizeChanged += Form1_SizeChanged;

            Model = new PlotModel { Title = title };
            JointAnalysis = jointAnalysis;

            foreach (var bar in JointAnalysis.Bars)
            {
                var series = new LineSeries();
                series.Points.Add(bar.StartPosition.ToDataPoint());
                series.Points.Add(bar.EndPosition.ToDataPoint());
                series.Title = $"Bar {bar.Number}: F={bar.ForceResult.ToString("F2")}";
                Model.Series.Add(series);

                var text = new TextAnnotation();
                text.TextPosition = bar.MidPoint.ToDataPoint();
                text.TextVerticalAlignment = OxyPlot.VerticalAlignment.Middle;
                text.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Center;
                text.Text = bar.ForceResult.ToString("F2");
                text.Background = OxyColor.FromRgb(255, 255, 255);
                Model.Annotations.Add(text);
            }

            foreach (var node in JointAnalysis.Nodes)
            {
                var text = new TextAnnotation();
                text.TextPosition = node.Position.ToDataPoint();
                text.TextVerticalAlignment = OxyPlot.VerticalAlignment.Top;
                text.TextHorizontalAlignment = OxyPlot.HorizontalAlignment.Left;

                var textString = $"Node {node.Number}\n";
                if (node.AppliedForces != Vector2d.Zero) textString += "F=" + node.AppliedForces + "\n";
                if (node.ReactionResult != Vector2d.Zero) textString += "R=" + node.ReactionResult;
                text.Text = textString;

                text.StrokeThickness = 0;
                Model.Annotations.Add(text);
            }


            this.plotView1.Model = Model;

        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.plotView1.Size = this.ClientSize;
            EnsureXYaxesOneToOne();
        }

        /// <summary>
        /// Tries to ensure that the X and Y axes have the same scale in screen space
        /// But this doesnt seem to work once the zoom is used!!!
        /// </summary>
        private void EnsureXYaxesOneToOne()
        {
            var plotRect = Model.PlotArea;
            var widthToHeight = plotRect.Height / plotRect.Width;

            // var widthToHeight
            var horizAxis = Model.Axes[0];
            var horizRange = horizAxis.ActualMaximum - horizAxis.ActualMinimum;

            var vertAxis = Model.Axes[1];
            var vertRange = vertAxis.ActualMaximum - vertAxis.ActualMinimum;

            var newVertMaximum = vertAxis.ActualMinimum + horizRange * widthToHeight;
            vertAxis.Maximum = newVertMaximum;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ZSpriteMaker
{
    internal class LineConnector
    {
        public Line line { get; set; }
        public Canvas canvas { get; set; }

        public LineConnector(Canvas canvas)
        {
            line = new Line();
            line.StrokeThickness = 2;
            line.Stroke = Brushes.White;
            this.canvas = canvas;
            canvas.Children.Add(line);
        }

        public void updateLinePos(Point outLine, Point inLine)
        {

            line.X1 = outLine.X;
            line.Y1 = outLine.Y;
            line.X2 = inLine.X;
            line.Y2 = inLine.Y;
        }
    }
}

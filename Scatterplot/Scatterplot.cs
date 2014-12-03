using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Globalization;

namespace Scatterplot
{
    public class Scatterplot
    {
        List<Point> points;
        double maxX, minX, maxY, minY;
        Bitmap img;

        int tickLen = 7;
        int tickCount = 10;
        int hborder = 30;
        int vborder = 30;

        int circleRadius = 2;

        public Scatterplot(List<Point> data, int width, int height)
        {
            points = data;
            img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            SetBackgroundColor(Color.White);
        }

        private void SetBackgroundColor(Color c)
        {
            using(var g = Graphics.FromImage(img))
            {
                g.FillRectangle(new SolidBrush(c), new Rectangle(0, 0, img.Width, img.Height));
            }
        }

        private void findMaxMins()
        {
            maxX = int.MinValue;
            minX = int.MaxValue;
            maxY = int.MinValue;
            minY = int.MaxValue;

            foreach(var l in points)
            {
                if (l.x > maxX)
                    maxX = l.x;

                if (l.x < minX)
                    minX = l.x;

                if (l.y > maxY)
                    maxY = l.y;

                if (l.y < minY)
                    minY = l.y;
            }
        }

        private double mapInterval(double a, double b, double c, double d, double x)
        {
            double alpha = (c-d) / (a-b);
            double beta = c - alpha * a;

            return alpha * x + beta;
        }

        private void CreateXAxis()
        {
            using(var g = Graphics.FromImage(img))
            {
                g.DrawLine(new Pen(Brushes.Black), vborder, img.Height-hborder, img.Width-vborder,
                    img.Height-hborder);

                int xVal=(int)minX;
                int tickSpace = (int)((maxX-minX)/tickCount);
                if (tickSpace < 1)
                    tickSpace = 1;

                while(xVal <= maxX)
                {
                    int tickPos = (int)mapInterval(minX, maxX, vborder, img.Width - vborder, xVal);
                    g.DrawLine(new Pen(Brushes.Black), tickPos, img.Height - hborder - tickLen / 2, tickPos,
                        img.Height - hborder + tickLen / 2);

                    g.DrawString(xVal.ToString(), new Font("Arial", 8), Brushes.Black, tickPos - 8, img.Height - 20);

                    xVal += tickSpace;
                }

                g.DrawString("X", new Font("Arial", 12), Brushes.Black, img.Width-vborder+5, img.Height-hborder-8);
            }
        }

        private void CreateYAxis()
        {
            using(var g = Graphics.FromImage(img))
            {
                g.DrawLine(new Pen(Brushes.Black), vborder, hborder, vborder,
                    img.Height-hborder);


                int yVal = (int)minY;
                int tickSpace = (int)(maxY - minY) / tickCount;
                if (tickSpace < 1)
                    tickSpace = 1;

                while (yVal <= maxY)
                {
                    int tickPos = (int)mapInterval(minY, maxY, img.Height-hborder, hborder, yVal);
                    g.DrawLine(new Pen(Brushes.Black), vborder-tickLen/2, tickPos, vborder+tickLen/2,
                        tickPos);

                    g.DrawString(yVal.ToString(), new Font("Arial", 8), Brushes.Black, 0, tickPos - 8);

                    yVal += tickSpace;
                }

                g.DrawString("Y", new Font("Arial", 12), Brushes.Black, vborder-7, 0);
            }
        }

        private void CreatePoints()
        {
            using(var g = Graphics.FromImage(img))
            {
                foreach (var l in points)
                {
                    int xPos = (int)mapInterval(minX, maxX, vborder, img.Width-vborder, l.x);
                    int yPos = (int)mapInterval(minY, maxY, img.Height - hborder, hborder, l.y);
                    g.FillEllipse(Brushes.Black, new Rectangle(xPos - circleRadius, yPos - circleRadius,
                        2*circleRadius, 2*circleRadius));
                }
            }
        }

        public Bitmap create()
        {
            findMaxMins();
            CreateXAxis();
            CreateYAxis();
            CreatePoints();

            return img;
        }

        public Bitmap RenderZoomBox(int x, int y, int width, int height, int radius, int MiddlePointX, int MiddlePointY)
        {
            var zoomImg = new Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            int windowXmin = (int)mapInterval(vborder, img.Width - vborder, minX, maxX, x);
            int windowYmax = (int)mapInterval(img.Height - hborder, hborder, minY, maxY, y);
            int windowXmax = (int)mapInterval(vborder, img.Width - vborder, minX, maxX, x+width);
            int windowYmin = (int)mapInterval(img.Height - hborder, hborder, minY, maxY, y+height);

            using (var g = Graphics.FromImage(zoomImg))
            {
                g.Clear(Color.Transparent);
                g.FillEllipse(Brushes.White, new Rectangle(MiddlePointX-radius, MiddlePointY-radius, 2*radius, 2*radius));

                foreach (var l in points)
                {
                    if(l.x >= windowXmin && l.x <= windowXmax && l.y >= windowYmin && l.y <= windowYmax)
                    {
                        int xPos = (int)mapInterval(windowXmin, windowXmax, 0, 2*radius, l.x);
                        int yPos = (int)mapInterval(windowYmin, windowYmax, 2*radius, 0, l.y);

                        int circlePosX = xPos - radius;
                        int circlePosY = yPos - radius;
                        double dist = Math.Sqrt(Math.Pow(circlePosX, 2) + Math.Pow(circlePosY, 2));

                        if(dist <= radius)
                        {
                            g.FillEllipse(Brushes.Black, new Rectangle(MiddlePointX - radius + xPos - circleRadius,
                                MiddlePointY - radius + yPos - circleRadius, 2*circleRadius, 2*circleRadius));
                        }
                    }
                }

                var p = new Pen(Brushes.DarkGray, 2.0f);
                g.DrawEllipse(p, new Rectangle(MiddlePointX-radius, MiddlePointY-radius, 2*radius, 2*radius));
            }

            return zoomImg;
        }

        public static List<Point> ReadCSV(string file)
        {
            var points = new List<Point>();

            foreach(string line in File.ReadLines(file))
            {
                var numbers = line.Split('\t');
                if (numbers[0].CompareTo("x") == 0)
                    continue;

                var x = double.Parse(numbers[0], CultureInfo.InvariantCulture);
                var y = double.Parse(numbers[1], CultureInfo.InvariantCulture);

                points.Add(new Point(x, y));
            }

            return points;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Scatterplot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.lens;
            this.MinimumSize = new System.Drawing.Size(400+groupBox1.Width, 
                100+groupBox1.Height);
        }

        Scatterplot s;
        List<Point> data;
        int mouseX, mouseY;

        private void button1_Click(object sender, EventArgs e)
        {
            var fd = new OpenFileDialog();
            fd.Filter = "csv files|*.csv";
            fd.Title = "Select a csv file";
            fd.Multiselect = false;

            if (fd.ShowDialog() == DialogResult.OK)
            {
                data = Scatterplot.ReadCSV(fd.FileName);
                s = new Scatterplot(data, pictureBox1.Width, pictureBox1.Height);
                pictureBox1.BackgroundImage = s.create();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AdjustFormSize();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label3.Text = trackBar1.Value.ToString() + "x";

            if (pictureBox1.ClientRectangle.Contains(Control.MousePosition))
            {
                RenderMagnifierLens(mouseX, mouseY);
            }
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            label5.Text = trackBar2.Value.ToString() + "px";

            if (pictureBox1.ClientRectangle.Contains(Control.MousePosition))
            {
                RenderMagnifierLens(mouseX, mouseY);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
            RenderMagnifierLens(e.X, e.Y);
        }

        private void RenderMagnifierLens(int MouseX, int MouseY)
        {
            if (s != null)
            {
                int zoomBoxHeight = 2*trackBar2.Value / trackBar1.Value;
                int zoomBoxWidth = 2*trackBar2.Value / trackBar1.Value;
                int halfWidth = zoomBoxWidth / 2;
                int halfHeight = zoomBoxHeight / 2;

                var img = pictureBox1.Image;
                pictureBox1.Image = s.RenderZoomBox(MouseX - halfWidth, MouseY - halfHeight, zoomBoxWidth, zoomBoxHeight,
                    trackBar2.Value, MouseX, MouseY);
                if(img != null)
                    img.Dispose();

                pictureBox1.Refresh();
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            if(pictureBox1.BackgroundImage != null)
                Cursor.Hide();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (pictureBox1.BackgroundImage != null)
                Cursor.Show();

            if (pictureBox1.Image != null)
                pictureBox1.Image = null;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            AdjustFormSize();
        }

        private void AdjustFormSize()
        {
            int borderSize = 50;
            int vBorder = 60;

            pictureBox1.Size = new Size(this.Size.Width - pictureBox1.Location.X - groupBox1.Size.Width - borderSize,
                this.Size.Height - pictureBox1.Location.Y - vBorder);
            groupBox1.Location = new System.Drawing.Point(pictureBox1.Location.X + pictureBox1.Size.Width + borderSize / 2,
                    groupBox1.Location.Y);

            trackBar2.Maximum = pictureBox1.Width > pictureBox1.Height ? pictureBox1.Width/2 : pictureBox1.Height/2;

            //rerender scatterplot
            if (data != null && s != null)
            {
                s = new Scatterplot(data, pictureBox1.Width, pictureBox1.Height);
                pictureBox1.BackgroundImage = s.create();
            }
        }
    }
}

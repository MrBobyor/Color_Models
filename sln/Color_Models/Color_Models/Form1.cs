using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Color_Models
{
    public partial class Form1 : Form
    {
        ColorConverter colorC = new ColorConverter();

        public Form1()
        {
            InitializeComponent();
        }

        public struct HSVcolor
        {
            public ushort h;
            public byte s;
            public byte v;

            public HSVcolor(ushort _h, byte _s, byte _v)
            {
                h = _h;
                s = _s;
                v = _v;
            }
        }

        public class ColorConverter
        {
            public Bitmap rgbImage;
            public HSVcolor[,] hsvImage;

            public HSVcolor RGBtoHSV(Color sours)
            {
                float eps = 0.00001f;
                ushort hue = 0;
                byte saturation = 0;
                byte value = 0;
                float R_ = sours.R / 255f;
                float G_ = sours.G / 255f;
                float B_ = sours.B / 255f;
                float CMax = Math.Max(R_, Math.Max(G_, B_));
                float CMin = Math.Min(R_, Math.Min(G_, B_));
                float delta = CMax - CMin;

                value = (byte)(100 * CMax);

                if (CMax < eps)
                    saturation = 0;
                else
                    saturation = (byte)(100 * delta / CMax);

                if (delta < eps)
                    hue = 0;
                else if (CMax == R_)
                    hue = (ushort)(60f * (((G_ - B_) / delta) % 6f));
                else if (CMax == G_)
                    hue = (ushort)(60f * (((B_ - R_) / delta) % 6f));
                else if (CMax == B_)
                    hue = (ushort)(60f * (((R_ - G_) / delta) % 6f));

                return new HSVcolor(clamp(hue, 0, 359), saturation, value);
            }

            public Color HSVtoRGB(HSVcolor source)
            {
                float R_ = 0, G_ = 0, B_ = 0;
                float C = source.s * source.v / 100f / 100f;
                float X = C * (1 - Math.Abs((source.h / 60f) % 2f - 1));
                float m = source.v / 100f - C;

                if (source.h < 60)
                { R_ = C; G_ = X; B_ = 0f; }
                else if (source.h < 120)
                { R_ = X; G_ = C; B_ = 0f; }
                else if (source.h < 180)
                { R_ = 0f; G_ = C; B_ = X; }
                else if (source.h < 240)
                { R_ = 0f; G_ = X; B_ = C; }
                else if (source.h < 300)
                { R_ = X; G_ = 0f; B_ = C; }
                else if (source.h < 360)
                { R_ = C; G_ = 0f; B_ = X; }

                byte R = (byte)(255f * (R_ + m));
                byte G = (byte)(255f * (G_ + m));
                byte B = (byte)(255f * (B_ + m));

                return Color.FromArgb(clamp(R, 0, 255), clamp(G, 0, 255), clamp(B, 0, 255));
            }

            public void ConvertRGBimagetoHSV()
            {
                int width = rgbImage.Width;
                int height = rgbImage.Height;

                hsvImage = new HSVcolor[width, height];
                for (int i = 0; i < width; ++i)
                    for (int j = 0; j < height; ++j)
                    {
                        hsvImage[i, j] = RGBtoHSV(rgbImage.GetPixel(i, j));
                    }
            }

            public void ConvertHSVimagetoRGB()
            {
                int width = hsvImage.GetLength(0);
                int height = hsvImage.GetLength(1);

                rgbImage = new Bitmap(width, height);
                for (int i = 0; i < width; ++i)
                    for (int j = 0; j < height; ++j)
                    {
                        rgbImage.SetPixel(i, j, HSVtoRGB(hsvImage[i, j]));
                    }
            }



            byte clamp(byte value, byte min, byte max)
            {
                return Math.Min(Math.Max(value, min), max);
            }

            ushort clamp(ushort value, ushort min, ushort max)
            {
                return Math.Min(Math.Max(value, min), max);
            }

        }

        //click
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string imagePath;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "image files|*.jpg; *.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                imagePath = dialog.FileName;
                colorC.rgbImage = new Bitmap(imagePath);
                this.pictureBox1.Image = colorC.rgbImage;
            } 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            colorC.ConvertRGBimagetoHSV();
            int width = colorC.rgbImage.Width;
            int height = colorC.rgbImage.Height;

            for (int i = 0; i < width; ++i)
                for (int j = 0; j < height; ++j)
                {
                    int tmpH = colorC.hsvImage[i, j].h + trackBar1.Value;
                    if (tmpH < 0)
                        colorC.hsvImage[i, j].h = (ushort)(tmpH + 360);
                    else if (tmpH >= 360)
                        colorC.hsvImage[i, j].h = (ushort)(tmpH - 360);
                    else
                        colorC.hsvImage[i, j].h = (ushort)tmpH;
                }
            trackBar1.Value = 0;
            colorC.ConvertHSVimagetoRGB();
            pictureBox1.Image = colorC.rgbImage;
            this.Cursor = Cursors.Default;
        }
    }
}

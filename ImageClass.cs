using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace Object_Research
{
    internal class ImageClass
    {
        public Bitmap originalImage;
        private Bitmap grayScaledlImage = null;
        private Bitmap binaryImage;
        private int[] histogram = new int[256];
        private int threshold;
        public int[] Histogram()
        {
            return this.histogram;
        }
        public Bitmap GrayScale()
        {
            if (this.grayScaledlImage != null) { return this.grayScaledlImage; }

            ColorMatrix grayMatrix = new ColorMatrix(new float[][]
            {
                new float[] {0.3f, 0.3f, 0.3f, 0, 0},
                new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {0, 0, 0, 0, 1}
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(grayMatrix);

            this.grayScaledlImage = new Bitmap(this.originalImage.Width, this.originalImage.Height);

            using (Graphics g = Graphics.FromImage(grayScaledlImage))
            {
                g.DrawImage(originalImage, new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                            0, 0, originalImage.Width, originalImage.Height, GraphicsUnit.Pixel, attributes);
            }

            return this.grayScaledlImage;
        }

        public Bitmap Create_Binary(ProgressBar progressBar)
        {
            Create_Histogram();
            this.threshold = OtsuThreshold(histogram);

            this.binaryImage = new Bitmap(this.grayScaledlImage.Width, this.grayScaledlImage.Height);
            Color color = new Color();

            int totalPixels = this.grayScaledlImage.Width * this.grayScaledlImage.Height;
            int processedPixels = 0;

            for (int i = 0; i < this.grayScaledlImage.Width; i++)
            {
                for (int j = 0; j < this.grayScaledlImage.Height; j++)
                {
                    color = this.grayScaledlImage.GetPixel(i, j);
                    int K = (color.R + color.G + color.B) / 3;
                    this.binaryImage.SetPixel(i, j, K <= threshold ? Color.Black : Color.White);

                    // Обновление прогресса
                    processedPixels++;
                    int progressPercentage = (int)(((double)processedPixels / totalPixels) * 100);
                }
            }

            return this.binaryImage;
        }

        public void Create_Histogram()
        {
            for (int i = 0; i < 256; i++)
            {
                this.histogram[i] = 0;
            }

            for (int i = 0; i < originalImage.Height; i++)
            {
                for (int j = 0; j < originalImage.Width; j++)
                {
                    Color pixelColor = originalImage.GetPixel(j, i);
                    int intensity = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                    this.histogram[intensity]++;
                }
            }
        }

        public int OtsuThreshold(int[] histogram)
        {
            int totalPixels = histogram.Sum();
            double sum = 0;
            for (int i = 0; i < histogram.Length; ++i)
            {
                sum += i * histogram[i];
            }

            double sumB = 0;
            int wB = 0;
            int wF = 0;
            double maxVariance = 0;
            int threshold = 0;

            for (int i = 0; i < histogram.Length; ++i)
            {
                wB += histogram[i];
                if (wB == 0)
                    continue;

                wF = totalPixels - wB;
                if (wF == 0)
                    break;

                sumB += i * histogram[i];

                double mB = sumB / wB;
                double mF = (sum - sumB) / wF;

                double varianceBetween = wB * wF * Math.Pow((mB - mF), 2);

                if (varianceBetween > maxVariance)
                {
                    maxVariance = varianceBetween;
                    threshold = i;
                }
            }
            return threshold;
        }
        public ImageClass(string filepath) { 
            this.originalImage = new Bitmap(filepath);
        }
    }
}

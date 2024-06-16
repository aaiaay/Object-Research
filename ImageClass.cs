using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Object_Research
{
    internal class ImageClass
    {
        public Bitmap originalImage;

        private Bitmap grayScaledlImage = null;
        private Bitmap negativeImage;
        public Bitmap currentImage;
        public int filledCount = 0;
        public int emptyCount = 0;

        public Bitmap binarySobelImage;
        public Bitmap connectedImage;

        public Bitmap binaryImage;
        private Bitmap sobelImage;
        private int[,] clustersMatrix;

        private int[] histogram = new int[256];
        public int threshold;
        public int[] Histogram()
        {
            return this.histogram;
        }

        private int[,] structuringElement = new int[,]
        {
            { 1, 1, 1 },
            { 1, 1, 1 },
            { 1, 1, 1 }
        };

        public Dictionary<int, Cluster> clusters = new Dictionary<int, Cluster>();
        public Dictionary<int, ClusterCoords> clustersCoords = new Dictionary<int, ClusterCoords>();

        public int clustersNumber;
        public int area;
        public double averageClusterSize;
        public int minCluster;
        public int maxCluster;

        public int countLargerThanAverage;
        public int countSmallerThanAverage;

        public double stdDeviation;
        public int medianClusterSize;
        public double coefficientOfVariation;
        public int range;
        public double skewness;
        public double kurtosis;

        public Bitmap Original()
        {
            this.grayScaledlImage = null;
            this.negativeImage = null;
            this.currentImage = originalImage;
            return this.originalImage;
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

            this.grayScaledlImage = new Bitmap(this.currentImage.Width, this.currentImage.Height);

            using (Graphics g = Graphics.FromImage(grayScaledlImage))
            {
                g.DrawImage(this.currentImage, new Rectangle(0, 0, this.currentImage.Width, this.currentImage.Height),
                            0, 0, this.currentImage.Width, this.currentImage.Height, GraphicsUnit.Pixel, attributes);
            }

            this.currentImage = this.grayScaledlImage;
            return this.grayScaledlImage;
        }
        public Bitmap Negative()
        {
            this.negativeImage = new Bitmap(this.currentImage.Width, this.currentImage.Height);

            for (int x = 0; x < this.currentImage.Width; x++)
            {
                for (int y = 0; y < this.currentImage.Height; y++)
                {
                    Color originalColor = this.currentImage.GetPixel(x, y);
                    Color negativeColor = Color.FromArgb(255 - originalColor.R, 255 - originalColor.G, 255 - originalColor.B);
                    this.negativeImage.SetPixel(x, y, negativeColor);
                }
            }

            this.currentImage = this.negativeImage;
            return this.negativeImage;
        }

        public void Create_Histogram()
        {
            Array.Clear(this.histogram, 0, this.histogram.Length);

            int width = this.originalImage.Width;
            int height = this.originalImage.Height;

            BitmapData imageData = this.originalImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = imageData.Stride;
            int bytesPerPixel = Image.GetPixelFormatSize(imageData.PixelFormat) / 8;
            int byteCount = stride * height;
            byte[] pixels = new byte[byteCount];
            Marshal.Copy(imageData.Scan0, pixels, 0, byteCount);
            this.originalImage.UnlockBits(imageData);

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * bytesPerPixel;
                    int intensity = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
                    this.histogram[intensity]++;
                }
            }

            //this.histogram = SmoothHistogram(this.histogram);
        }

        public Bitmap Create_Binary(int T)
        {
            Create_Histogram();
            if (T == 0) this.threshold = OtsuThreshold(histogram);
            else threshold = T;
           

            if (grayScaledlImage == null) GrayScale();

            int width = this.grayScaledlImage.Width;
            int height = this.grayScaledlImage.Height;
            this.binaryImage = new Bitmap(width, height);

            BitmapData grayData = this.grayScaledlImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData binaryData = this.binaryImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = grayData.Stride;
            int bytesPerPixel = Image.GetPixelFormatSize(grayData.PixelFormat) / 8;
            int byteCount = stride * height;
            byte[] grayPixels = new byte[byteCount];
            byte[] binaryPixels = new byte[byteCount];

            Marshal.Copy(grayData.Scan0, grayPixels, 0, byteCount);
            this.grayScaledlImage.UnlockBits(grayData);

            int totalPixels = width * height;
            int processedPixels = 0;

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * bytesPerPixel;
                    int K = (grayPixels[i] + grayPixels[i + 1] + grayPixels[i + 2]) / 3;
                    byte value = (byte)(K <= threshold ? 0 : 255);

                    binaryPixels[i] = value;
                    binaryPixels[i + 1] = value;
                    binaryPixels[i + 2] = value;

                    processedPixels++;
                    if (processedPixels % 1000 == 0)
                    {
                        int progressPercentage = (int)(((double)processedPixels / totalPixels) * 100);
                      
                    }
                }
            }

            Marshal.Copy(binaryPixels, 0, binaryData.Scan0, byteCount);
            this.binaryImage.UnlockBits(binaryData);

         

            this.currentImage = this.binaryImage;
            return this.binaryImage;
        }

        public int[] SmoothHistogram(int[] histogram, int kernelSize = 3)
        {
            int[] smoothed = new int[histogram.Length];
            int halfKernel = kernelSize / 2;

            for (int i = 0; i < histogram.Length; i++)
            {
                int sum = 0;
                int count = 0;

                for (int j = -halfKernel; j <= halfKernel; j++)
                {
                    int index = i + j;
                    if (index >= 0 && index < histogram.Length)
                    {
                        sum += histogram[index];
                        count++;
                    }
                }
                smoothed[i] = sum / count;
            }
            return smoothed;
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

            double[] cumulativeSum = new double[histogram.Length];
            int[] cumulativeWeight = new int[histogram.Length];

            cumulativeSum[0] = 0;
            cumulativeWeight[0] = histogram[0];

            for (int i = 1; i < histogram.Length; ++i)
            {
                cumulativeSum[i] = cumulativeSum[i - 1] + i * histogram[i];
                cumulativeWeight[i] = cumulativeWeight[i - 1] + histogram[i];
            }

            for (int t = 0; t < histogram.Length; ++t)
            {
                wB = cumulativeWeight[t];
                wF = totalPixels - wB;

                if (wB == 0 || wF == 0)
                    continue;

                sumB = cumulativeSum[t];
                double mB = sumB / wB;
                double mF = (sum - sumB) / wF;

                double varianceBetween = wB * wF * Math.Pow((mB - mF), 2);

                if (varianceBetween > maxVariance)
                {
                    maxVariance = varianceBetween;
                    threshold = t;
                }
            }
            return threshold;
        }

        public Bitmap LocalOtsuBinarization(int blockSize)
        {
            int width = this.originalImage.Width;
            int height = this.originalImage.Height;
            this.binaryImage = new Bitmap(width, height);
            for (int y = 0; y < height; y += blockSize)
            {
                for (int x = 0; x < width; x += blockSize)
                {
                    int blockWidth = Math.Min(blockSize, width - x);
                    int blockHeight = Math.Min(blockSize, height - y);

                    Bitmap block = this.originalImage.Clone(new Rectangle(x, y, blockWidth, blockHeight), this.originalImage.PixelFormat);
                    int[] blockHistogram = CreateHistogramForBlock(block);
                    int localThreshold = OtsuThreshold(blockHistogram);
                    Bitmap binaryBlock = ApplyThresholdToBlock(block, localThreshold);

                    using (Graphics g = Graphics.FromImage(this.binaryImage))
                    {
                        g.DrawImage(binaryBlock, x, y);
                    }
                }
            }

            this.currentImage = this.binaryImage;
            return this.binaryImage;
        }

        private int[] CreateHistogramForBlock(Bitmap block)
        {
            int[] histogram = new int[256];
            int width = block.Width;
            int height = block.Height;

            BitmapData imageData = block.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            int stride = imageData.Stride;
            int bytesPerPixel = Image.GetPixelFormatSize(imageData.PixelFormat) / 8;
            int byteCount = stride * height;
            byte[] pixels = new byte[byteCount];
            Marshal.Copy(imageData.Scan0, pixels, 0, byteCount);
            block.UnlockBits(imageData);

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * bytesPerPixel;
                    int intensity = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
                    histogram[intensity]++;
                }
            }

            return histogram;
        }

        private Bitmap ApplyThresholdToBlock(Bitmap block, int threshold)
        {
            int width = block.Width;
            int height = block.Height;
            Bitmap binaryBlock = new Bitmap(width, height);

            BitmapData blockData = block.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            BitmapData binaryData = binaryBlock.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int stride = blockData.Stride;
            int bytesPerPixel = Image.GetPixelFormatSize(blockData.PixelFormat) / 8;
            int byteCount = stride * height;
            byte[] blockPixels = new byte[byteCount];
            byte[] binaryPixels = new byte[byteCount];

            Marshal.Copy(blockData.Scan0, blockPixels, 0, byteCount);
            block.UnlockBits(blockData);

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int i = rowStart + x * bytesPerPixel;
                    int K = (blockPixels[i] + blockPixels[i + 1] + blockPixels[i + 2]) / 3;
                    byte value = (byte)(K <= threshold ? 0 : 255);

                    binaryPixels[i] = value;
                    binaryPixels[i + 1] = value;
                    binaryPixels[i + 2] = value;
                }
            }

            Marshal.Copy(binaryPixels, 0, binaryData.Scan0, byteCount);
            binaryBlock.UnlockBits(binaryData);

            return binaryBlock;
        }

        public Bitmap ApplySobelOperator()
        {
            int[,] sobelX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobelY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            Bitmap gradientImage = new Bitmap(originalImage);

            for (int y = 1; y < originalImage.Height - 1; y++)
            {
                for (int x = 1; x < originalImage.Width - 1; x++)
                {
                    int gx = 0, gy = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color pixel = originalImage.GetPixel(x + j, y + i);
                            int intensity = (pixel.R + pixel.G + pixel.B) / 3;

                            gx += intensity * sobelX[i + 1, j + 1];
                            gy += intensity * sobelY[i + 1, j + 1];
                        }
                    }

                    int gradient = (int)Math.Sqrt(gx * gx + gy * gy);
                    gradient = Math.Max(0, Math.Min(255, gradient)); // Ограничение значений от 0 до 255
                    gradientImage.SetPixel(x, y, Color.FromArgb(gradient, gradient, gradient));
                }
            }
            this.sobelImage = gradientImage;
            return this.sobelImage;
        }

        public Bitmap Erode()
        {
            int width = this.currentImage.Width;
            int height = this.currentImage.Height;
            Bitmap erodedImage = new Bitmap(width, height);

            int seWidth = this.structuringElement.GetLength(0);
            int seHeight = this.structuringElement.GetLength(1);
            int seCenterX = seWidth / 2;
            int seCenterY = seHeight / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool erode = true;
                    for (int j = 0; j < seHeight; j++)
                    {
                        for (int i = 0; i < seWidth; i++)
                        {
                            int pixelX = x + i - seCenterX;
                            int pixelY = y + j - seCenterY;

                            if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                            {
                                Color pixelColor = this.currentImage.GetPixel(pixelX, pixelY);
                                if (this.structuringElement[i, j] == 1 && pixelColor.R != 0)
                                {
                                    erode = false;
                                    break;
                                }
                            }
                        }
                        if (!erode) break;
                    }
                    erodedImage.SetPixel(x, y, erode ? Color.Black : Color.White);
                }
            }

            this.currentImage = erodedImage; // Обновляем текущее изображение
            return this.currentImage;
        }

        public Bitmap Dilate()
        {
            int width = this.currentImage.Width;
            int height = this.currentImage.Height;
            Bitmap dilatedImage = new Bitmap(width, height);

            int seWidth = this.structuringElement.GetLength(0);
            int seHeight = this.structuringElement.GetLength(1);
            int seCenterX = seWidth / 2;
            int seCenterY = seHeight / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool dilate = false;
                    for (int j = 0; j < seHeight; j++)
                    {
                        for (int i = 0; i < seWidth; i++)
                        {
                            int pixelX = x + i - seCenterX;
                            int pixelY = y + j - seCenterY;

                            if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height)
                            {
                                Color pixelColor = this.currentImage.GetPixel(pixelX, pixelY);
                                if (this.structuringElement[i, j] == 1 && pixelColor.R == 0)
                                {
                                    dilate = true;
                                    break;
                                }
                            }
                        }
                        if (dilate) break;
                    }
                    dilatedImage.SetPixel(x, y, dilate ? Color.Black : Color.White);
                }
            }

            this.currentImage = dilatedImage; // Обновляем текущее изображение
            return this.currentImage;
        }

        public void Create_Binary_Sobel()
        {
            this.binarySobelImage = new Bitmap(this.sobelImage.Width, this.sobelImage.Height);
            Color color = new Color();

            for (int i = 0; i < this.sobelImage.Width; i++)
            {
                for (int j = 0; j < this.sobelImage.Height; j++)
                {
                    color = this.sobelImage.GetPixel(i, j);
                    int K = (color.R + color.G + color.B) / 3;
                    this.binarySobelImage.SetPixel(i, j, K <= 70 ? Color.Black : Color.White);
                }
            }
        }

        /* Утолщение границ */
        public void IncreaseEdgeThickness()
        {
            int width = binarySobelImage.Width;
            int height = binarySobelImage.Height;

            Bitmap thickenedEdgesImage = new Bitmap(width, height);

            int thickness = 1;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color pixel = binarySobelImage.GetPixel(i, j);

                    if (pixel.R == 255) 
                    {
                        for (int k = -thickness; k <= thickness; k++)
                        {
                            for (int l = -thickness; l <= thickness; l++)
                            {
                                int x = i + k;
                                int y = j + l;

                                if (x >= 0 && x < width && y >= 0 && y < height)
                                {
                                    thickenedEdgesImage.SetPixel(x, y, Color.White);
                                }
                            }
                        }
                    }
                }
            }

            this.binarySobelImage = thickenedEdgesImage;
        }


     
        /* Соединение изображений */
        public void Connect_Images()
        {
            IncreaseEdgeThickness();
            int width = binaryImage.Width;
            int height = binaryImage.Height;

            this.connectedImage = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color binarySobelPixel = binarySobelImage.GetPixel(i, j);

                    if (binarySobelPixel.R == 255 && binarySobelPixel.G == 255 && binarySobelPixel.B == 255)
                    {
                        // Если пиксель на втором изображении белый, делаем пиксель на первом изображении белым
                        this.connectedImage.SetPixel(i, j, Color.White);
                    }
                    else
                    {
                        // В противном случае оставляем цвет пикселя на первом изображении без изменений
                        this.connectedImage.SetPixel(i, j, binaryImage.GetPixel(i, j));
                    }
                }
            }

            this.currentImage = this.connectedImage;
        }

        /* Алгоритм Хошена-Копельмана */
        public void HK()
        {
            int[,] binaryMatrix = new int[this.currentImage.Width, this.currentImage.Height];

            Color color = new Color();

            for (int j = 0; j < this.currentImage.Height; j++)
            {
                for (int i = 0; i < this.currentImage.Width; i++)
                {
                    color = this.currentImage.GetPixel(i, j);

                    if (color.R == 0 && color.G == 0 && color.B == 0)
                        binaryMatrix[i, j] = 1;
                    else binaryMatrix[i, j] = 0;
                }
            }

            this.clustersMatrix = new int[this.currentImage.Width, this.currentImage.Height];

            this.clustersMatrix = HoshenKopelman(binaryMatrix);
        }
        public int[,] HoshenKopelman(int[,] image)
        {
            int width = image.GetLength(0);
            int height = image.GetLength(1);
            int[,] labels = new int[width, height];

            int label = 1;
            List<int> clusters = new List<int>() { 0 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (image[x, y] == 0) continue;

                    int up = (y > 0) ? labels[x, y - 1] : 0;
                    int left = (x > 0) ? labels[x - 1, y] : 0;

                    if (up == 0 && left == 0)
                    {
                        labels[x, y] = label;
                        clusters.Add(label);
                        label++;
                    }
                    else if (up == 0)
                    {
                        labels[x, y] = left;
                    }
                    else if (left == 0)
                    {
                        labels[x, y] = up;
                    }
                    else
                    {
                        int minLabel = Math.Min(up, left);
                        int maxLabel = Math.Max(up, left);

                        labels[x, y] = minLabel;
                        if (minLabel != maxLabel) clusters[maxLabel] = minLabel;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[x, y] == 0) continue;

                    int correct = labels[x, y];
                    while (clusters[correct] != correct) correct = clusters[correct];

                    labels[x, y] = correct;
                }
            }
            return labels;
        }

        public Dictionary<int, ClusterCoords> Coords()
        {
            int width = this.clustersMatrix.GetLength(0);
            int height = this.clustersMatrix.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int clusterLabel = this.clustersMatrix[x, y];
                    if (clusterLabel != 0)
                    {
                        if (this.clustersCoords.ContainsKey(clusterLabel))
                        {
                            this.clustersCoords[clusterLabel].size++;
                            this.clustersCoords[clusterLabel].sumX += x;
                            this.clustersCoords[clusterLabel].sumY += y;
                        }
                        else
                        {
                            ClusterCoords newCluster = new ClusterCoords(0, 0, 0, 0, 0);
                            this.clustersCoords.Add(clusterLabel, newCluster);
                        }
                    }
                }
            }

            foreach (var cluster in this.clustersCoords)
            {
                if (cluster.Value.size > 0)
                {
                    cluster.Value.avgX = (int) cluster.Value.sumX / cluster.Value.size;
                    cluster.Value.avgY = (int) cluster.Value.sumY / cluster.Value.size;
                }
            }

            return this.clustersCoords;
        }

        public void DoClustersOperations()
        {
            FilterClusters();
            CalculateStatistics();
        }

        public void FilterClusters()
        {
            int width = this.clustersMatrix.GetLength(0);
            int height = this.clustersMatrix.GetLength(1);
            int Counter = 0;
            Dictionary<int, Cluster> filteredClusters = new Dictionary<int, Cluster>();

            // Сначала собираем информацию о кластерах
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int clusterLabel = this.clustersMatrix[x, y];
                    if (clusterLabel != 0)
                    {
                        if (this.clusters.ContainsKey(clusterLabel))
                        {
                            this.clusters[clusterLabel].size++;
                        }
                        else
                        {
                            Cluster newCluster = new Cluster(0, 1);
                            this.clusters.Add(clusterLabel, newCluster);
                        }
                    }
                }
            }

            foreach (var cluster in this.clusters)
            {
                //if (cluster.Value.size > 200 && !IsClusterOnBorder(cluster.Key))
                if (cluster.Value.size > 200)
                {
                    Counter++;
                    cluster.Value.id = Counter;
                    filteredClusters.Add(cluster.Key, cluster.Value);
                }
            }

            this.clustersNumber = Counter;

         
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int clusterLabel = this.clustersMatrix[x, y];
                    if (clusterLabel != 0)
                    {
                        if (filteredClusters.TryGetValue(clusterLabel, out Cluster cluster))
                        {
                            this.clustersMatrix[x, y] = cluster.id;
                        }
                        else
                        {
                            this.clustersMatrix[x, y] = 0;
                        }
                    }
                }
            }
        }

        private bool IsClusterOnBorder(int clusterLabel)
        {
            int width = this.clustersMatrix.GetLength(0);
            int height = this.clustersMatrix.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (this.clustersMatrix[x, y] == clusterLabel)
                    {
                        if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                        {
                            return true; // Кластер находится на границе
                        }
                    }
                }
            }

            return false;
        }

        public void CalculateStatistics()
        {
            this.area = 0;
            this.averageClusterSize = 0;
            this.minCluster = 0;
            this.maxCluster = 0;
            this.stdDeviation = 0;
            this.medianClusterSize = 0;
            this.coefficientOfVariation = 0;
            this.range = 0;
            this.skewness = 0;
            this.kurtosis = 0;
            this.countLargerThanAverage = 0;
            this.countSmallerThanAverage = 0;

            var filteredClusterSizes = clusters.Values.Where(c => c.id != 0).Select(c => (double)c.size).ToList();

            if (filteredClusterSizes.Any())
            {
                this.area = (int)filteredClusterSizes.Sum();
                this.clustersNumber = filteredClusterSizes.Count();

                this.averageClusterSize = this.area / this.clustersNumber;

                double mean = this.averageClusterSize;
                double sumOfSquares = filteredClusterSizes.Select(size => (size - mean) * (size - mean)).Sum();
                this.stdDeviation = Math.Round(Math.Sqrt(sumOfSquares / (filteredClusterSizes.Count - 1)), 3);
                filteredClusterSizes.Sort();
                if (filteredClusterSizes.Count % 2 == 0)
                    this.medianClusterSize = (int)((filteredClusterSizes[filteredClusterSizes.Count / 2 - 1] + filteredClusterSizes[filteredClusterSizes.Count / 2]) / 2);
                else
                    this.medianClusterSize = (int)filteredClusterSizes[filteredClusterSizes.Count / 2];

                this.coefficientOfVariation = Math.Round(stdDeviation / mean, 3);

                this.range = (int)(filteredClusterSizes.Max() - filteredClusterSizes.Min());

                this.minCluster = (int)filteredClusterSizes.Min();
                this.maxCluster = (int)filteredClusterSizes.Max();

                int n = filteredClusterSizes.Count;
                double skewnessSum = filteredClusterSizes.Select(size => Math.Pow((size - mean) / stdDeviation, 3)).Sum();
                this.skewness = Math.Round((n * skewnessSum) / ((n - 1) * (n - 2)), 3);

                double kurtosisSum = filteredClusterSizes.Select(size => Math.Pow((size - mean) / stdDeviation, 4)).Sum();
                this.kurtosis = ((n * (n + 1)) * kurtosisSum) / ((n - 1) * (n - 2) * (n - 3)) - (3 * Math.Pow(n - 1, 2)) / ((n - 2) * (n - 3));

                foreach (var size in filteredClusterSizes)
                {
                    if (size > this.averageClusterSize)
                        this.countLargerThanAverage++;
                    else if (size < this.averageClusterSize)
                        this.countSmallerThanAverage++;
                }
            }
        }

        public ImageClass(string filepath) { 
            this.originalImage = new Bitmap(filepath);
            this.currentImage = this.originalImage;
        }
    }
}

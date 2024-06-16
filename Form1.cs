using FontAwesome.Sharp;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Object_Research
{
    public partial class mainForm : Form
    {
        private Panel border = null;
        ImageClass image;
        public mainForm()
        {
            InitializeComponent();
            clustersCollection.Columns.Add("", 30);
            clustersCollection.Columns.Add("id", 70);
            clustersCollection.Columns.Add("Размер кластера", 400);
        }

        private void closeFormBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void minimizeFormBtn_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void restoreFormBtn_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void loadPage_Click(object sender, EventArgs e)
        {
            headerLabel.Text = "Загрузка изображения";
            Tab.SelectedTab = loadImageTab;
            SelectPage(loadPage, Color.Pink);

        }
        private void processImagePage_Click(object sender, EventArgs e)
        {
            headerLabel.Text = "Обработка изображения";
            Tab.SelectedTab = processImageTab;
            SelectPage(processPage, Color.Pink);
        }

        private void researchPage_Click(object sender, EventArgs e)
        {
            headerLabel.Text = "Исследование изображения";
            Tab.SelectedTab = researchImageTab;
            SelectPage(researchPage, Color.Pink);
        }

        public void SelectPage(IconButton button, Color color)
        {

            loadPage.BackColor = processPage.BackColor = researchPage.BackColor = Color.FromArgb(45, 50, 80); 
            if (border == null) border = new Panel();

            border.BackColor = color;
            border.Dock = DockStyle.Left;
            border.Width = 4;
            button.Parent.Controls.Add(border);

            button.BackColor = Color.FromArgb(50, 60, 95);
        }

        private void loadNewImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png; *.gif; *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы (*.*)|*.*";
                openFileDialog.Title = "Выберите изображение";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string selectedImagePath = openFileDialog.FileName;                    
                        image = new ImageClass(selectedImagePath);
                        originalImageBox.Image = image.originalImage;
                        changedImageBox.Image = image.originalImage;
                    }
                    catch 
                    {
                        MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                }
            }
        }

        private void ShowHistogram(int[] histogram)
        {
            TabPage brightnessHistogramTab = new TabPage("Гистогр.");
            brightnessHistogramTab.Name = "brightnessHistogramTab";
            processTab.TabPages.Add(brightnessHistogramTab);

            Chart brightnessHistogram = new Chart();
            brightnessHistogram.Dock = DockStyle.Fill;

            ChartArea chartArea = new ChartArea();
            brightnessHistogram.ChartAreas.Add(chartArea);

            chartArea.AxisX.Title = "Интенсивность";
            chartArea.AxisY.Title = "Частота";
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisX.Interval = 10;
            chartArea.AxisX.IsMarginVisible = false;

            Series series = new Series("Яркость");
            series.ChartType = SeriesChartType.Column;

            series.Color = Color.FromArgb(45, 50, 80);

            for (int i = 0; i < histogram.Length; i++)
            {
                series.Points.AddXY(i, histogram[i]);
            }

            brightnessHistogram.Series.Add(series);

            brightnessHistogramTab.Controls.Add(brightnessHistogram);

            brightnessHistogram.Update();
        }
 
        private void makeGrayScale()
        {
            changedImageBox.Image = image.GrayScale();
            processTab.SelectedTab = originalImageTab;
        }

        private void makeNegative()
        {
            changedImageBox.Image = image.Negative();
            processTab.SelectedTab = originalImageTab;
        }

        private void backToOriginal()
        {
            changedImageBox.Image = image.Original();
            processTab.SelectedTab = originalImageTab;
        }

        private void chooseFilterButton_Click(object sender, EventArgs e)
        {
            string selectedFilter = filtersCollection.SelectedItem.ToString();

            if (selectedFilter == "Ч/Б")
            {
                makeGrayScale();
            }
            //else if (selectedFilter == "Негатив")
            //{
            //    makeNegative();
            //}
            else if (selectedFilter == "Оригинал")
            {
                backToOriginal();
            }
        }

        private void makeBinary(int threshold)
        {
            TabPage binaryImageTab;
            if (!processTab.TabPages.ContainsKey("binaryImageTab"))
            {
                binaryImageTab = new TabPage("Бинарн.");
                binaryImageTab.Name = "binaryImageTab";
                processTab.TabPages.Add(binaryImageTab);

                PictureBox pictureBox = new PictureBox();
                pictureBox.Dock = DockStyle.Fill;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                binaryImageTab.Controls.Add(pictureBox);


                pictureBox.Image = image.Create_Binary(0);


                processTab.SelectedTab = binaryImageTab;

                int[] histogram = image.Histogram();
                ShowHistogram(histogram);

                binaryThreshold.Text = image.threshold.ToString();
                binaryPanel.Visible = true;
            }
            else
            {
                binaryImageTab = processTab.TabPages["binaryImageTab"];
                PictureBox pictureBox = (PictureBox)binaryImageTab.Controls[0];
                pictureBox.Image = image.Create_Binary(threshold);
            }
        }

        private void defineEdges()
        {
            if (!processTab.TabPages.ContainsKey("edgesImageTab"))
            {
                TabPage edgesImageTab = new TabPage("Границы");
                edgesImageTab.Name = "edgesImageTab";
                processTab.TabPages.Add(edgesImageTab);

                PictureBox pictureBox = new PictureBox();
                pictureBox.Dock = DockStyle.Fill;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                edgesImageTab.Controls.Add(pictureBox);


                image.ApplySobelOperator();
                image.Create_Binary_Sobel();
                image.Connect_Images();
               

                processTab.SelectedTab = edgesImageTab;
                pictureBox.Image = image.connectedImage;
            }
        }

    
        private void erodion ()
        {
            if (processTab.TabPages.ContainsKey("binaryImageTab"))
            {
                TabPage selected = processTab.TabPages["binaryImageTab"];
                PictureBox pictureBox = (PictureBox)selected.Controls[0];
                pictureBox.Image = image.Erode();
                processTab.SelectedTab = selected;
            }
            else
            {
                MessageBox.Show("Сначала необходимо выполнить бинаризацию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dilatation()
        {
            if (processTab.TabPages.ContainsKey("binaryImageTab"))
            {
                TabPage selected = processTab.TabPages["binaryImageTab"];
                PictureBox pictureBox = (PictureBox)selected.Controls[0];
                pictureBox.Image = image.Dilate();
                processTab.SelectedTab = selected;
            }
            else
            {
                MessageBox.Show("Сначала необходимо выполнить бинаризацию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chooseFunctionButton_Click(object sender, EventArgs e)
        {
            string selectedFunction = functionsCollection.SelectedItem.ToString();

            if (selectedFunction == "Бинаризация")
            {
                makeBinary(0);
            }

            if (selectedFunction == "Выделение краев")
            {
                defineEdges();
            }
            if (selectedFunction == "Эрозия")
            {
                erodion();
            }

            if (selectedFunction == "Дилатация")
            {
                dilatation();
            }
        }

        private void iconButton7_Click(object sender, EventArgs e)
        {
            image.HK();
            image.DoClustersOperations();
            Dictionary<int, Cluster> clustersDictionary = new Dictionary<int, Cluster>();
            clustersDictionary = image.clusters;

            int minCluster = image.minCluster;
            int maxCluster = image.maxCluster;

            int numClasses = 5;

            ShowSizeHistogram(minCluster, maxCluster, numClasses, clustersDictionary);

            clustersCollection.Items.Clear();

            foreach (var cluster in clustersDictionary)
            {
                if (cluster.Value.id != 0 && cluster.Value.size > 0)
                {
                    ListViewItem item = new ListViewItem();
                    item.SubItems.Add(cluster.Value.id.ToString());
                    item.SubItems.Add(cluster.Value.size.ToString());
                    clustersCollection.Items.Add(item);
                }
            }

            
            numberOfClusterslb.Text = image.clustersNumber.ToString();
            areaOfClusterslb.Text = image.area.ToString();
            averageSizeClusterslb.Text = image.averageClusterSize.ToString();
            minSizeClusterslb.Text = minCluster.ToString();
            maxSizeClusterslb.Text = maxCluster.ToString();
            standardDeviationlb.Text = image.stdDeviation.ToString();
            medianClusterSizelb.Text = image.medianClusterSize.ToString();
            coefficientOfVariationlb.Text = image.coefficientOfVariation.ToString();
            rangelb.Text = image.range.ToString();
            skewnesslb.Text = image.skewness.ToString();
           

            finishImage.Image = image.binaryImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            draw();
        }

        private void draw ()
        {

            Bitmap labeledImg = image.currentImage;
            using (Graphics graphics = Graphics.FromImage(labeledImg))
            {

                Dictionary<int, ClusterCoords> coordsOfCenters = image.Coords();             
                Font font = new Font("Arial", 50, GraphicsUnit.Pixel);
                Brush brush = new SolidBrush(Color.Red);

                foreach (var coord in coordsOfCenters)
                {
                    string text = coord.Key.ToString();
                    Point textPosition = new Point(coord.Value.avgX-10, coord.Value.avgY-10);
                    graphics.DrawString(text, font, brush, textPosition);
                }
            }

            finishImage.Image = labeledImg;
           
        }
        private void ShowSizeHistogram(int minCluster, int maxCluster, int numClasses, Dictionary<int, Cluster> clusters)
        {
            int step = (maxCluster - minCluster) / numClasses;

            int[] classCounts = new int[numClasses];

            foreach (var cluster in clusters.Values)
            {
                if (cluster.id != 0)
                {
                    int classIndex = (cluster.size - minCluster) / step;
                    classCounts[Math.Min(classIndex, numClasses - 1)]++;
                }
            }

            barChart.Series.Clear();
            Series series = new Series("Распределение по размерам");
            series.ChartType = SeriesChartType.Column;

            for (int i = 0; i < numClasses; i++)
            {
                int classStart = minCluster + i * step;
                int classEnd = classStart + step - 1;
                string className = $"{classStart}-{classEnd}";

                series.Points.AddXY(className, classCounts[i]);
            }

            barChart.Series.Add(series);
            barChart.ChartAreas[0].AxisX.Title = "Размер";
            barChart.ChartAreas[0].AxisY.Title = "Количество";
        }

        private void changeHistogramBtn_Click(object sender, EventArgs e)
        {
            if (numberOfBars.Text.Length > 0)
            {
                Dictionary<int, Cluster> clustersDictionary = new Dictionary<int, Cluster>();
                clustersDictionary = image.clusters;

                int minCluster = image.minCluster;
                int maxCluster = image.maxCluster;

                int.TryParse(numberOfBars.Text, out int numClasses);

                ShowSizeHistogram(minCluster, maxCluster, numClasses, clustersDictionary);
            }
        }

        private void numberOfBars_ValueChanged(object sender, EventArgs e)
        {
            ShowSizeHistogram(image.minCluster, image.maxCluster, (int)numberOfBars.Value, image.clusters);
        }
 

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            makeBinary((int)numericUpDown1.Value);
        }


        private void saveAsButton_Click_1(object sender, EventArgs e)
        {
            PictureBox picture = (PictureBox)processTab.SelectedTab.Controls[0];
            if (picture.Image != null)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png; *.gif; *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Все файлы (*.*)|*.*";
                    saveFileDialog.Title = "Сохранить изображение";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Получаем путь к файлу, в который будет сохранено изображение
                        string filePath = saveFileDialog.FileName;

                        // Сохраняем изображение по указанному пути
                        picture.Image.Save(filePath);

                        MessageBox.Show("Изображение успешно сохранено", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Нет изображения для сохранения", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteClusters()
        {
            for (int i = clustersCollection.Items.Count - 1; i >= 0; i--)
            {
                if (clustersCollection.Items[i].Checked)
                {
                    clustersCollection.Items.RemoveAt(i);
                }
            }
        }

        private void defineAnomalies()
        {

        }

        private void chooseOptionButton_Click(object sender, EventArgs e)
        {
            string selectesOption = optionCollection.SelectedItem.ToString();

            if (selectesOption == "Удалить")
            {
                deleteClusters();
            }

            if (selectesOption == "Пометить как аномалии")
            {
                defineAnomalies();
            }
        }
    }
}

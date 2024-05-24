using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
            SelectPage(processPage, Color.Green);
        }

        private void researchPage_Click(object sender, EventArgs e)
        {
            headerLabel.Text = "Исследование изображения";
            Tab.SelectedTab = researchImageTab;
            SelectPage(researchPage, Color.Blue);
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
                        originalImageBox2.Image = image.originalImage;
                    }
                    catch 
                    {
                        MessageBox.Show("Невозможно открыть выбранный файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                }
            }
        }

        private void makeGrayScaleImageButton_Click(object sender, EventArgs e)
        {  
            if (!processTab.TabPages.ContainsKey("grayScaleImageTab"))
            {
                TabPage grayScaleImageTab = new TabPage("Ч/Б");
                grayScaleImageTab.Name = "grayScaleImageTab";
                processTab.TabPages.Add(grayScaleImageTab);
           
                PictureBox pictureBox = new PictureBox();
                pictureBox.Dock = DockStyle.Fill;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                grayScaleImageTab.Controls.Add(pictureBox);
        
                pictureBox.Image = image.GrayScale();
             
                processTab.SelectedTab = grayScaleImageTab;
            }
        }

        private void ShowHistogram(int[] histogram)
        {
            // Создание новой вкладки
            TabPage brightnessHistogramTab = new TabPage("Гистогр.");
            brightnessHistogramTab.Name = "brightnessHistogramTab";
            processTab.TabPages.Add(brightnessHistogramTab);

            // Создание нового объекта Chart
            Chart brightnessHistogram = new Chart();
            brightnessHistogram.Dock = DockStyle.Fill;

            // Добавление ChartArea
            ChartArea chartArea = new ChartArea();
            brightnessHistogram.ChartAreas.Add(chartArea);

            // Настройка осей
            chartArea.AxisX.Title = "Интенсивность";
            chartArea.AxisY.Title = "Частота";
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.Maximum = 255;
            chartArea.AxisX.Interval = 10; // Интервал между метками по оси X
            chartArea.AxisX.IsMarginVisible = false; // Убираем отступы на краях оси X

            // Создание серии для гистограммы
            Series series = new Series("Яркость");
            series.ChartType = SeriesChartType.Column;

            // Заполнение данных серии
            for (int i = 0; i < histogram.Length; i++)
            {
                series.Points.AddXY(i, histogram[i]);
            }

            // Добавление серии в Chart
            brightnessHistogram.Series.Add(series);

            // Добавление Chart на вкладку
            brightnessHistogramTab.Controls.Add(brightnessHistogram);

            // Обновление графика
            brightnessHistogram.Update();
        }
        private void makeBinaryButton_Click(object sender, EventArgs e)
        {
            if (!processTab.TabPages.ContainsKey("binaryImageTab"))
            {
                TabPage binaryImageTab = new TabPage("Бинарн.");
                binaryImageTab.Name = "binaryImageTab";
                processTab.TabPages.Add(binaryImageTab);

                PictureBox pictureBox = new PictureBox();
                pictureBox.Dock = DockStyle.Fill;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                binaryImageTab.Controls.Add(pictureBox);


                pictureBox.Image = image.Create_Binary(progressBar);


                processTab.SelectedTab = binaryImageTab;

                int[] histogram = image.Histogram();
                ShowHistogram(histogram);
            }
        }
    }
}

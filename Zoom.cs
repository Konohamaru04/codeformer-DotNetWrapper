using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Codeformer
{
    public partial class Zoom : Form
    {
        static string inputImagePath = $@"{Directory.GetCurrentDirectory()}\frames";
        static string outFolderPath = Directory.GetCurrentDirectory() + "\\outCode";
        private bool isPlaying = false;
        private bool isEnhance = false;
        private int frameCount = 0;
        private int startIndex = 0;
        private double fps = 0;

        public Zoom(int frameCount, int startIndex, double fps, bool isEnhance)
        {
            InitializeComponent();

            this.frameCount = frameCount;
            this.startIndex = startIndex;
            this.fps = fps;
            this.isEnhance = isEnhance;

            this.Text = $"Playing at Total: {frameCount} Framecount: {frameCount} FPS: {fps} Enhanced: {isEnhance}";
            //if (pictureBox1.InvokeRequired)
            //{
            //    BeginInvoke(new Action(() =>
            //    {
            //        pictureBox1.ImageLocation = src;
            //    }));
            //}
            //else
            //{
            //    pictureBox1.ImageLocation = src;
            //}
        }

        private void Zoom_Load(object sender, EventArgs e)
        {
            pictureBox1_Click(null, EventArgs.Empty);
        }

        private string GetInputImageWithIndex(int index)
        {
            return $"{inputImagePath}/frame_{index:00000000}.png";
        }

        private string GetOutputImageWithIndex(int index)
        {
            return $"{outFolderPath}/frame_{index:00000000}.png";
        }

        private void LoadOrgImage(string src)
        {
            if (pictureBox1.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox1.ImageLocation = src;
                }));
            }
            else
            {
                pictureBox1.ImageLocation = src;
            }
        }

        private string GetFrameName(int index)
        {
            return $"frame_{index:00000000}.png";
        }

        private async void pictureBox1_Click(object sender, EventArgs e)
        {
            isPlaying = !isPlaying;

            int start = 0;
            start = startIndex;
            if (isPlaying)
            {
                await Task.Run(() =>
                {
                    //using (VideoFileReader reader = new VideoFileReader())
                    //{
                    //    reader.Open(videoFilePath);

                    //BeginInvoke(new Action(() =>
                    //{

                    //}));
                    int i = 0;

                    while (true) {
                        Thread.Sleep(Convert.ToInt32(fps));
                        if (!isPlaying) break;
                        string frameName = GetFrameName(i);
                        string frameFileName = Path.Combine(inputImagePath, frameName);


                        if (!isEnhance)
                        {
                            if (File.Exists(GetInputImageWithIndex(i)))
                                LoadOrgImage(GetInputImageWithIndex(i));
                            else
                                i = 0;
                        }
                        else
                        {
                            if (File.Exists(GetInputImageWithIndex(i)))
                                LoadOrgImage(GetOutputImageWithIndex(i));
                            else
                                i = 0;
                        }

                        if(i < frameCount)
                        {
                            i++;
                        }
                        else
                        {
                            i = 0;
                        }
                        
                    }
                    //for (int i = start; i < frameCount; i++)
                    //{
                        
                    //}
                    //}
                });
            }
        }
    }
}

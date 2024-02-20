using Emgu.CV.Flann;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Codeformer_Dotnet
{
    public partial class Maximize : Form
    {
        string _path;
        double _fps;
        int _index;
        bool isPlaying = false;
        int _frameCount = 0;
        public Maximize(string path, double fps, int index, int frameCount)
        {
            _path = path;
            _fps = fps;
            _index = index;
            _frameCount = frameCount;
            InitializeComponent();
        }

        private void Maximize_Load(object sender, EventArgs e)
        {
            LoadImage(GetInputImageWithIndex(_index));
        }

        private async void pictureBox1_Click(object sender, EventArgs e)
        {
            await PlayVideo();
        }

        private async Task PlayVideo()
        {
            await Task.Run(() => {
                isPlaying = !isPlaying;
                while (true)
                {
                    Thread.Sleep(Convert.ToInt32(_fps));
                    if (!isPlaying)
                    {
                        break;
                    }

                    if (!File.Exists(GetInputImageWithIndex(_index)))
                    {
                        _index = 1;
                    }
                    else
                    {
                        LoadImage(GetInputImageWithIndex(_index));
                    }

                    if (_index < _frameCount)
                    {
                        _index++;
                    }
                    else
                    {
                        _index = 1;
                    }

                }
            });
        }

        private void LoadImage(string src)
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

        private string GetInputImageWithIndex(int index)
        {
            return $"{_path}/frame_{index:00000000}.png";
        }
    }
}

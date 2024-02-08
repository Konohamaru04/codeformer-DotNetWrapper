using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Codeformer
{
    public partial class Zoom : Form
    {
        public Zoom(string src)
        {
            InitializeComponent();

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

        private void Zoom_Load(object sender, EventArgs e)
        {
            
        }
    }
}

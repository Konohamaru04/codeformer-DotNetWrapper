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

namespace Codeformer
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        Form1 form1= new Form1();
        private void Settings_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Form1.background_enhance;
            checkBox2.Checked = Form1.face_upsample;
            trackBar1.Value = Form1.upscale;
            trackBar2.Value = Convert.ToInt32(Form1.codeformer_fidelity * 100);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Form1.background_enhance = checkBox1.Checked;
            form1.textBox1.AppendText($"Background enhance {(Form1.background_enhance ? "Enabled" : "Disabled")}" + Environment.NewLine);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Form1.face_upsample = checkBox2.Checked;
            form1.textBox1.AppendText($"Face upsample {(Form1.face_upsample ? "Enabled" : "Disabled")}" + Environment.NewLine);
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            Form1.upscale = trackBar1.Value;
            form1.textBox1.AppendText($"Upscale {Form1.upscale}" + Environment.NewLine);
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            Form1.codeformer_fidelity = trackBar2.Value / 100;
            form1.textBox1.AppendText($"Codeformer fidelity {Form1.codeformer_fidelity}" + Environment.NewLine);
        }
    }
}

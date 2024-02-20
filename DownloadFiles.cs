using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Codeformer_Dotnet
{
    public partial class DownloadFiles : Form
    {
        public DownloadFiles()
        {
            InitializeComponent();
        }

        private string SRC = @"https://github.com/Konohamaru04/codeformer-DotNetWrapper/releases/download/1.0/CodeFormer.zip";

        private void DownloadFiles_Load(object sender, EventArgs e)
        {

        }
    }
}

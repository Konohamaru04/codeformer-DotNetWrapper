namespace Codeformer_Dotnet
{
    partial class Main
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            pictureBox2 = new PictureBox();
            pictureBox1 = new PictureBox();
            panel2 = new Panel();
            textBox1 = new TextBox();
            panel3 = new Panel();
            button6 = new Button();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            button2 = new Button();
            button1 = new Button();
            videoSelectBtn = new Button();
            trackBar1 = new TrackBar();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(pictureBox1);
            panel1.Location = new Point(2, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1133, 681);
            panel1.TabIndex = 0;
            // 
            // pictureBox2
            // 
            pictureBox2.Dock = DockStyle.Fill;
            pictureBox2.Location = new Point(0, 0);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(1133, 681);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 0;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            pictureBox2.DoubleClick += pictureBox2_DoubleClick;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(1133, 681);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.DoubleClick += pictureBox1_DoubleClick;
            // 
            // panel2
            // 
            panel2.Controls.Add(textBox1);
            panel2.Location = new Point(1141, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(453, 683);
            panel2.TabIndex = 1;
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.Black;
            textBox1.BorderStyle = BorderStyle.None;
            textBox1.ForeColor = Color.Lime;
            textBox1.Location = new Point(3, 10);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(447, 669);
            textBox1.TabIndex = 0;
            // 
            // panel3
            // 
            panel3.Controls.Add(button6);
            panel3.Controls.Add(button5);
            panel3.Controls.Add(button4);
            panel3.Controls.Add(button3);
            panel3.Controls.Add(button2);
            panel3.Controls.Add(button1);
            panel3.Controls.Add(videoSelectBtn);
            panel3.Controls.Add(trackBar1);
            panel3.Location = new Point(4, 687);
            panel3.Name = "panel3";
            panel3.Size = new Size(1591, 121);
            panel3.TabIndex = 2;
            // 
            // button6
            // 
            button6.Location = new Point(630, 66);
            button6.Name = "button6";
            button6.Size = new Size(75, 23);
            button6.TabIndex = 7;
            button6.Text = "Export";
            button6.UseVisualStyleBackColor = true;
            button6.Visible = false;
            button6.Click += button6_Click;
            // 
            // button5
            // 
            button5.Location = new Point(549, 66);
            button5.Name = "button5";
            button5.Size = new Size(75, 23);
            button5.TabIndex = 6;
            button5.Text = "Stop";
            button5.UseVisualStyleBackColor = true;
            button5.Visible = false;
            button5.Click += button5_Click;
            // 
            // button4
            // 
            button4.Location = new Point(462, 66);
            button4.Name = "button4";
            button4.Size = new Size(81, 23);
            button4.TabIndex = 5;
            button4.Text = "Enhance All";
            button4.UseVisualStyleBackColor = true;
            button4.Visible = false;
            button4.Click += button4_Click;
            // 
            // button3
            // 
            button3.Location = new Point(347, 66);
            button3.Name = "button3";
            button3.Size = new Size(109, 23);
            button3.TabIndex = 4;
            button3.Text = "Show Enhanced";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // button2
            // 
            button2.Location = new Point(259, 66);
            button2.Name = "button2";
            button2.Size = new Size(82, 23);
            button2.TabIndex = 3;
            button2.Text = "Codeformer";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button1
            // 
            button1.Location = new Point(169, 66);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 2;
            button1.Text = "Play";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // videoSelectBtn
            // 
            videoSelectBtn.Location = new Point(17, 54);
            videoSelectBtn.Name = "videoSelectBtn";
            videoSelectBtn.Size = new Size(137, 47);
            videoSelectBtn.TabIndex = 1;
            videoSelectBtn.Text = "Select Video";
            videoSelectBtn.UseVisualStyleBackColor = true;
            videoSelectBtn.Click += videoSelectBtn_Click;
            // 
            // trackBar1
            // 
            trackBar1.Location = new Point(3, 3);
            trackBar1.Minimum = 1;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(1587, 45);
            trackBar1.TabIndex = 0;
            trackBar1.Value = 1;
            trackBar1.MouseUp += trackBar1_MouseUp;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1598, 810);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            MaximizeBox = false;
            Name = "Main";
            Text = "Video Enhancer";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private TrackBar trackBar1;
        private Button videoSelectBtn;
        public PictureBox pictureBox2;
        public PictureBox pictureBox1;
        public TextBox textBox1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Button button6;
    }
}

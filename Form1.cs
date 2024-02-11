using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Codeformer
{
    public partial class Form1 : Form
    {
        public static int upscale = 2;
        public static float codeformer_fidelity = 1f;
        public static bool face_upsample = true;
        public static bool background_enhance = true;

        static string videoFilePath = string.Empty;
        public static string FileName = string.Empty;
        public static int frameCount = 0;

        static string inputImagePath = $@"{Directory.GetCurrentDirectory()}\frames";
        static string outFolderPath = Directory.GetCurrentDirectory() + "\\outCode";

        private bool enhanced = false;
        private bool codeFormer = false;
        Process process;
        double fps;
        bool hardStop = false;

        private FileSystemWatcher watcher;
        bool convertInprogress = false;

        private const int MinZoom = 10;
        private const int MaxZoom = 200;
        private const int ZoomStep = 10;

        private int currentZoom = 100;
        private Point mouseDownLocation;
        private bool mouseDragging;
        public Form1()
        {
            InitializeComponent();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // Handle file or directory changes here
            Console.WriteLine($"Change detected: {e.ChangeType} - {e.FullPath}");
            dlog($"Enhanced: {e.ChangeType} - {e.FullPath}");
            //if (trackBar1.InvokeRequired)
            //{
            //    BeginInvoke(new Action(() =>
            //    {
            //        trackBar1.Value++;
            //    }));
            //}
            //else
            //{
            //    trackBar1.Value++;
            //}
        }

        private void CreateDirectory()
        {
            if (!Directory.Exists(outFolderPath))
            {

                Directory.CreateDirectory(outFolderPath);
                dlog("Results directory created successfully.");
            }
            else
            {
                Directory.Delete(outFolderPath, true);
                Directory.CreateDirectory(outFolderPath);
                dlog("Results directory created successfully.");
            }

            if (!Directory.Exists(inputImagePath))
            {

                Directory.CreateDirectory(inputImagePath);
                dlog("Frames directory created successfully.");
            }
            else
            {
                Directory.Delete(inputImagePath, true);
                Directory.CreateDirectory(inputImagePath);
                dlog("Frames directory created successfully.");
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            button8.Visible = false;
            codeFormer = !codeFormer;
            //CreateDirectory();
            if (codeFormer)
            {
                ShowProcessing();
            }

            await Task.Run(() =>
            {
                int frame = 0;

                BeginInvoke(new Action(() =>
                {
                    frame = trackBar1.Value;
                }));

                if (codeFormer)
                {
                    dlog("Codeformer enabled!");
                    ApplyCodeFormer(GetInputImageWithIndex(frame), GetOutputImageWithIndex(frame));
                    LoadImage(frame);
                    BeginInvoke(new Action(() =>
                    {
                        button3.BackColor = Color.Green;
                        trackBar1.Value = frame;
                    }));
                }
                else
                {
                    BeginInvoke(new Action(() =>
                    {
                        button3.BackColor = Color.Red;
                    }));

                    dlog("Codeformer disabled!");
                }
            });

        }

        private void LoadImage(Image image)
        {
            if (pictureBox1.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox1.Image = new Bitmap(image);
                }));
            }
            else
            {
                pictureBox1.Image = new Bitmap(image);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            ShowLoading();
            int value = trackBar1.Value;
            button8.Visible = false;
            //CreateDirectory();
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "MP4 files (*.mp4)|*.mp4|All files (*.*)|*.*";
                openFileDialog.Title = "Select a Video File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    videoFilePath = openFileDialog.FileName;
                    button1.Enabled = false;
                    panel1.Visible = true;
                    FileName = openFileDialog.SafeFileName;
                    //dlog(videoFilePath);
                    button2.BackColor = Color.Green;
                    trackBar1.Enabled = true;
                    trackBar1.Value = value;
                    //button1.Enabled = true;
                    button9.Visible = true;

                    await ExtractAllFrames();


                }
            }
        }

        private async Task ExtractAllFrames()
        {
            int value = trackBar1.Value;
            await Task.Run(() =>
            {
                using (VideoFileReader reader = new VideoFileReader())
                {
                    reader.Open(videoFilePath);
                    
                    fps = (double)reader.FrameRate;


                    for (int i = 0; i < reader.FrameCount; i++)
                    {
                        frameCount = i;
                        Bitmap frame = reader.ReadVideoFrame(i);
                        string frameName = GetFrameName(i);
                        string frameFileName = Path.Combine(inputImagePath, frameName);
                        try
                        {
                            if (!File.Exists(frameFileName))
                                frame.Save(frameFileName, System.Drawing.Imaging.ImageFormat.Png);
                            if (i == 1)
                            {
                                LoadImage(i);
                            }
                        }
                        finally
                        {
                            frame.Dispose();
                        }
                        BeginInvoke(new Action(() =>
                        {
                            trackBar1.Maximum = i;
                        }));
                    }
                }
            });
        }


        private void ApplyCodeFormer(string imgPath, string outImage)
        {
            //await Task.Run(() =>
            //{
            try
            {
                if (!File.Exists(outImage))
                {
                    using (process = new Process())
                    {
                        process.StartInfo.FileName = $"python";
                        process.StartInfo.Arguments = $"cf_clr.py {imgPath} {(background_enhance ? "--background_enhance" : "")} {(face_upsample ? "--face_upsample" : "")} --upscale {upscale} --codeformer_fidelity {codeformer_fidelity}";
                        dlog($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.CreateNoWindow = true;

                        process.Start();

                        // Asynchronously read the output and error streams
                        Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                        Task<string> errorTask = process.StandardError.ReadToEndAsync();

                        // Optionally wait for the process to exit
                        process.WaitForExit();

                        // Get the output and error results
                        string output = outputTask.Result;
                        string error = errorTask.Result;

                        Console.WriteLine("Process exited with code: " + process.ExitCode);
                        dlog("Process exited with code: " + process.ExitCode);
                        Console.WriteLine(output);
                        dlog(output);
                        if (error != null && error != string.Empty && error != "")
                        {
                            Console.WriteLine("Error: " + error);
                            dlog("Error: " + error);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                dlog(ex.Message);
            }
            //});
        }

        private async Task ApplyCodeFormer()
        {
            //await Task.Run(() =>
            //{

            await Task.Delay(10);
            try
            {
                using (process = new Process())
                {
                    process.StartInfo.FileName = $"python";
                    process.StartInfo.Arguments = $"cf_clr.py {inputImagePath} {(background_enhance ? "--background_enhance" : "")} {(face_upsample ? "--face_upsample" : "")} --upscale {upscale} --codeformer_fidelity {codeformer_fidelity}"; ;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;


                    dlog($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
                    //process.OutputDataReceived += (sender, args) => dlog(args.Data);
                    //process.ErrorDataReceived += (sender, args) => dlog(args.Data);

                    process.Start();

                    // Asynchronously read the output and error streams
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask = process.StandardError.ReadToEndAsync();

                    process.WaitForExit();

                    string output = outputTask.Result;
                    string error = errorTask.Result;

                    Console.WriteLine("Process exited with code: " + process.ExitCode);
                    dlog("Process exited with code: " + process.ExitCode);
                    // Get the output and error results


                    Console.WriteLine(output);
                    dlog(output);
                    if (error != null && error != string.Empty && error != "")
                    {
                        Console.WriteLine("Error: " + error);
                        dlog("Error: " + error);
                    }
                }
            }
            catch (Exception ex)
            {
                dlog(ex.Message);
            }
            //});
        }

        private void ConvertTovideo()
        {
            //await Task.Run(() =>
            //{
            try
            {
                using (process = new Process())
                {
                    process.StartInfo.FileName = $"ffmpeg";
                    process.StartInfo.Arguments = $"-framerate {fps} -i {inputImagePath}/frame_%08d.png -c:v h264_nvenc \"{FileName}\"";
                    dlog($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    // Asynchronously read the output and error streams
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask = process.StandardError.ReadToEndAsync();


                    // Optionally wait for the process to exit
                    process.WaitForExit();

                    // Get the output and error results
                    string output = outputTask.Result;
                    string error = errorTask.Result;

                    Console.WriteLine("Process exited with code: " + process.ExitCode);
                    dlog("Process exited with code: " + process.ExitCode);
                    Console.WriteLine(output);
                    dlog(output);
                    if (error != null && error != string.Empty && error != "")
                    {
                        Console.WriteLine("Error: " + error);
                        dlog("Error: " + error);
                    }
                }


            }
            catch (Exception ex)
            {
                dlog(ex.Message);
            }
            //});
        }

        private void dlog(string msg)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(dlog), msg + Environment.NewLine);
                return;
            }
            else
            {
                textBox1.AppendText(msg + Environment.NewLine);
            }

        }

        private string GetFrameName(int index)
        {
            return $"frame_{index:00000000}.png";
        }

        private string GetInputImageWithIndex(int index)
        {
            return $"{inputImagePath}/frame_{index:00000000}.png";
        }

        private string GetOutputImageWithIndex(int index)
        {
            return $"{outFolderPath}/frame_{index:00000000}.png";
        }

        private void LoadImage(int index)
        {
            LoadOrgImage(GetInputImageWithIndex(index));
            LoadEnhImage(GetOutputImageWithIndex(index));
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

        private void LoadEnhImage(string src)
        {
            if (pictureBox2.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox2.ImageLocation = src;
                }));
            }
            else
            {
                pictureBox2.ImageLocation = src;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (codeFormer || isPlaying)
                enhanced = !enhanced;
            else
                enhanced = false;



            if (enhanced)
            {
                button4.BackColor = Color.Green;
                pictureBox1.Visible = false;
                pictureBox2.Visible = true;
                button8.Visible = true;
            }
            else
            {
                button4.BackColor = Color.Red;
                pictureBox1.Visible = true;
                pictureBox2.Visible = false;
                button8.Visible = false;
            }
        }

        private async void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            //int trackbarValue = trackBar1.Value;
            //codeFormer = false;
            //enhanced = false;
            //button3.BackColor = Color.Red;
            //button4.BackColor = Color.Red;
            //pictureBox1.Visible = true;
            //pictureBox2.Visible = false;
            //await Task.Run(() =>
            //{
            //    try
            //    {
            //        dlog($"{trackbarValue}");
            //        if (trackbarValue < frameCount)
            //        {
            //            using (VideoFileReader reader = new VideoFileReader())
            //            {
            //                reader.Open(videoFilePath);
            //                //frameCount = Convert.ToInt32(reader.FrameCount);

            //                Bitmap frame = reader.ReadVideoFrame(trackbarValue);
            //                string frameName = GetFrameName(trackbarValue);
            //                string frameFileName = Path.Combine(inputImagePath, frameName);
            //                frame.Save(frameFileName, System.Drawing.Imaging.ImageFormat.Png);
            //                frame.Dispose();
            //            }

            //            // execute codeformer
            //            //ApplyCodeFormer(GetInputImageWithIndex(trackbarValue));
            //            // Load images
            //            LoadImage(trackbarValue);
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        dlog(ex.Message);
            //    }
            //});
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar1.Enabled = false;
            button4.BackColor = Color.Red;
            button3.BackColor = Color.Red;
            pictureBox1.Visible = true;
            pictureBox2.Visible = false;
            button7.Visible = false;
            button8.Visible = false;
            CreateDirectory();

            // Create a new FileSystemWatcher
            watcher = new FileSystemWatcher(outFolderPath);

            // Set the notification filters
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            // Subscribe to the events
            //watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            //watcher.Deleted += OnChanged;

            watcher.EnableRaisingEvents = true;
            //this.MouseWheel += Form1_MouseWheel;
            //pictureBox1.MouseDown += PictureBox1_MouseDown;
            //pictureBox1.MouseMove += PictureBox1_MouseMove;
            //pictureBox1.MouseUp += PictureBox1_MouseUp;
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDragging = true;
                mouseDownLocation = e.Location;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDragging && pictureBox1.Image != null)
            {
                int deltaX = e.X - mouseDownLocation.X;
                int deltaY = e.Y - mouseDownLocation.Y;
                int newX = Math.Min(Math.Max(pictureBox1.Location.X + deltaX, -(pictureBox1.Image.Width * currentZoom / 100 - pictureBox1.Width)), 0);
                int newY = Math.Min(Math.Max(pictureBox1.Location.Y + deltaY, -(pictureBox1.Image.Height * currentZoom / 100 - pictureBox1.Height)), 0);
                pictureBox1.Location = new Point(newX, newY);
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDragging = false;
            }
        }
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Check if the Ctrl key is pressed
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                // Zoom in or out based on the direction of the mouse wheel
                if (e.Delta > 0)
                    ZoomIn();
                else
                    ZoomOut();
            }
        }

        private void ZoomIn()
        {
            // Increase the zoom level
            currentZoom += ZoomStep;
            if (currentZoom > MaxZoom)
                currentZoom = MaxZoom;

            ApplyZoom();
        }

        private void ZoomOut()
        {
            // Decrease the zoom level
            currentZoom -= ZoomStep;
            if (currentZoom < MinZoom)
                currentZoom = MinZoom;

            ApplyZoom();
        }

        private void ApplyZoom()
        {
            // Calculate the new size of the PictureBox based on the zoom level
            int newWidth = (int)(pictureBox1.Image.Width * currentZoom / 100.0);
            int newWidth1 = (int)(pictureBox2.Image.Width * currentZoom / 100.0);
            int newHeight = (int)(pictureBox1.Image.Height * currentZoom / 100.0);
            int newHeight1 = (int)(pictureBox2.Image.Height * currentZoom / 100.0);

            // Set the new size and update the PictureBox
            pictureBox1.ClientSize = new Size(newWidth, newHeight);
            pictureBox2.ClientSize = new Size(newWidth, newHeight1);
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            convertInprogress = true;
            button8.Visible = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            trackBar1.Enabled = false;
            pictureBox1.Visible = true;
            pictureBox2.Visible = false;
            button2.BackColor = Color.Orange;
            timer1.Enabled = true;
            ShowProcessing();
            await EnhanceVideo();

            //timer1.Enabled = true;
        }


        private async Task EnhanceVideo()
        {
            int start = 0;
            start = trackBar1.Value;
            await Task.Run(async () =>
            {
                try
                {
                    CreateDirectory();
                    using (VideoFileReader reader = new VideoFileReader())
                    {
                        reader.Open(videoFilePath);

                        for (int i = start; i < reader.FrameCount; i++)
                        {
                            string frameName = GetFrameName(i);
                            string frameFileName = Path.Combine(inputImagePath, frameName);
                            if (!File.Exists(frameFileName))
                            {
                                Bitmap frame = reader.ReadVideoFrame(i);
                                LoadImage((Image)frame.Clone());
                                dlog($"Extracting: {frameFileName}");
                                frame.Save(frameFileName, System.Drawing.Imaging.ImageFormat.Png);
                                frame.Dispose();
                            }
                            LoadOrgImage(GetInputImageWithIndex(i));
                            LoadEnhImage(GetInputImageWithIndex(i));         
                        }

                        BeginInvoke(new Action(() =>
                        {
                            pictureBox1.Visible = false;
                            pictureBox2.Visible = true;
                            button7.Visible = true;
                            ShowProcessing();
                        }));

                        await ApplyCodeFormer();

                        // Disable the watcher
                        //watcher.EnableRaisingEvents = false;


                        ConvertTovideo();

                        BeginInvoke(new Action(() =>
                        {
                            timer1.Enabled = true;
                            //timer1.Stop();
                            button2.BackColor = Color.Green;
                            button3.Enabled = true;
                            button4.Enabled = true;
                            button5.Enabled = true;
                            button6.Enabled = true;
                            trackBar1.Enabled = true;
                            button7.Visible = false;
                            button7.Text = "Stop";
                        }));
                        hardStop = false;
                        convertInprogress = false;


                    }
                }


                catch (Exception ex)
                {
                    dlog(ex.Message);
                }
            });
        }

        private async void textBox2_TextChanged(object sender, EventArgs e)
        {
            codeFormer = false;
            enhanced = false;
            button3.BackColor = Color.Red;
            button4.BackColor = Color.Red;
            pictureBox1.Visible = true;
            pictureBox2.Visible = false;
            int value;

            if (Int32.TryParse(trackBar1.Text, out value))
            {
                trackBar1.Value = value;
                await Task.Run(() =>
                {
                    try
                    {
                        dlog($"{value}");
                        if (value < frameCount)
                        {
                            using (VideoFileReader reader = new VideoFileReader())
                            {
                                reader.Open(videoFilePath);
                                //frameCount = Convert.ToInt32(reader.FrameCount);

                                Bitmap frame = reader.ReadVideoFrame(value);
                                LoadImage((Image)frame.Clone());
                                string frameName = GetFrameName(value);
                                string frameFileName = Path.Combine(inputImagePath, frameName);
                                if (!File.Exists(frameFileName))
                                    frame.Save(frameFileName, System.Drawing.Imaging.ImageFormat.Png);
                                frame.Dispose();
                            }

                            // execute codeformer
                            //ApplyCodeFormer(GetInputImageWithIndex(trackbarValue));
                            // Load images
                            LoadImage(value);
                        }

                    }
                    catch (Exception ex)
                    {
                        dlog(ex.Message);
                    }
                });
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            int value;
            if (Int32.TryParse(trackBar1.Text, out value))
            {
                if (value >= 0)
                {
                    value--;
                    trackBar1.Text = value.ToString();
                    trackBar1.Value = value;
                }
                else
                {
                    value = 0;
                    trackBar1.Text = value.ToString();
                    trackBar1.Value = value;
                }

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int value;
            if (Int32.TryParse(trackBar1.Text, out value))
            {
                if (value < frameCount)
                {
                    value++;
                    trackBar1.Text = value.ToString();
                    trackBar1.Value = value;
                }
                else
                {
                    value = frameCount;
                    trackBar1.Text = value.ToString();
                    trackBar1.Value = value;
                }

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            process.Kill();
            hardStop = true;
            button7.Text = "Stopping";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (process != null ? true : false)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }

                }
            }
            catch (Exception ex)
            {
                dlog(ex.Message);
            }

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void ShowLoading()
        {
            if (pictureBox1.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox1.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading1.gif");
                }));
            }
            else
            {
                pictureBox1.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading1.gif");
            }

            if (pictureBox2.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox2.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading1.gif");
                }));
            }
            else
            {
                pictureBox2.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading1.gif");
            }
        }

        private void ShowProcessing()
        {
            if (pictureBox1.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox1.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading2.gif");
                }));
            }
            else
            {
                pictureBox1.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading2.gif");
            }

            if (pictureBox2.InvokeRequired)
            {
                BeginInvoke(new Action(() =>
                {
                    pictureBox2.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading2.gif");
                }));
            }
            else
            {
                pictureBox2.Image = new Bitmap($@"{Directory.GetCurrentDirectory()}\loading2.gif");
            }


        }

        private async void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!convertInprogress)
            {
                codeFormer = false;
                enhanced = false;
                button3.BackColor = Color.Red;
                button4.BackColor = Color.Red;
                pictureBox1.Visible = true;
                pictureBox2.Visible = false;
                button8.Visible = false;
                int value = trackBar1.Value;
                LoadImage(value);
                //await Task.Run(() =>
                //{
                //    try
                //    {
                //        ShowLoading();
                //        dlog($"Loading {value}");
                //        if (value < frameCount)
                //        {
                //            using (VideoFileReader reader = new VideoFileReader())
                //            {
                //                reader.Open(videoFilePath);
                //                //frameCount = Convert.ToInt32(reader.FrameCount);

                //                Bitmap frame = reader.ReadVideoFrame(value);
                //                LoadImage((Image)frame.Clone());
                //                string frameName = GetFrameName(value);
                //                string frameFileName = Path.Combine(inputImagePath, frameName);
                //                if (!File.Exists(frameFileName))
                //                    frame.Save(frameFileName, System.Drawing.Imaging.ImageFormat.Png);
                //                frame.Dispose();
                //            }

                //            // execute codeformer
                //            //ApplyCodeFormer(GetInputImageWithIndex(trackbarValue));
                //            // Load images
                //            LoadImage(value);
                //            dlog($"Loaded {value}");
                //        }

                //    }
                //    catch (Exception ex)
                //    {
                //        dlog(ex.Message);
                //    }
                //});
            }

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Ctrl key is pressed
            if (e.Control)
            {
                // Zoom in when Ctrl + Plus key is pressed
                if (e.KeyCode == Keys.Oemplus || e.KeyCode == Keys.Add)
                    ZoomIn();

                // Zoom out when Ctrl + Minus key is pressed
                if (e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract)
                    ZoomOut();
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Zoom zoom = new Zoom(GetOutputImageWithIndex(trackBar1.Value));
            zoom.Show();
        }

        private void button4_StyleChanged(object sender, EventArgs e)
        {
            if (!button4.Enabled)
            {
                button8.Visible = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string[] imageFiles = Directory.GetFiles(outFolderPath, "*.*", SearchOption.AllDirectories)
                                                    .Where(file => file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpeg"))
                                                    .ToArray();
            if (imageFiles.Length > 0)
            {
                dlog($"{imageFiles[imageFiles.Length - 1]}");
                LoadEnhImage(imageFiles[imageFiles.Length - 1]);
                LoadOrgImage(imageFiles[imageFiles.Length - 1]);

                BeginInvoke(new Action(() =>
                {

                    if (imageFiles.Length >= trackBar1.Minimum && imageFiles.Length <= trackBar1.Maximum)
                    {
                        trackBar1.Value = imageFiles.Length;
                    }

                }));
            }

        }

        bool isPlaying = false;
        private async void button9_Click(object sender, EventArgs e)
        {
            isPlaying = !isPlaying;

            if (isPlaying)
            {
                button9.Text = "Pause";
            }
            else
            {
                button9.Text = "Play";
            }
            int start = 0;
            start = trackBar1.Value;
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

                        for (int i = start; i < frameCount; i++)
                        {
                            Thread.Sleep(Convert.ToInt32(fps));
                            BeginInvoke(new Action(() =>
                            {
                                trackBar1.Value = i;
                            }));
                            if (!isPlaying) break;
                            string frameName = GetFrameName(i);
                            string frameFileName = Path.Combine(inputImagePath, frameName);
                            //if (!File.Exists(frameFileName))
                            //{
                            //    Bitmap frame = reader.ReadVideoFrame(i);
                            //    LoadImage((Image)frame.Clone());
                            //    frame.Save(frameFileName, System.Drawing.Imaging.ImageFormat.Png);
                            //    frame.Dispose();
                            //}
                            dlog($"Playing: {frameFileName}");
                            LoadOrgImage(GetInputImageWithIndex(i));
                            LoadEnhImage(GetInputImageWithIndex(i));
                        }
                    //}
                });
            }
        }


        private void button10_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }
    }
}

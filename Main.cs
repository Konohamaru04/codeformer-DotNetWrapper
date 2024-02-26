using Emgu.CV.CvEnum;
using Emgu.CV;
using System.Diagnostics;
using Emgu.CV.Structure;
using Microsoft.VisualBasic.Logging;
using Python.Runtime;

namespace Codeformer_Dotnet
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            string dll = Environment.GetEnvironmentVariable("PYTHONNET_PYDLL");
            if (dll != null)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }

        }

        public static int upscale = 4;
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
        bool isPlaying = false;
        bool isEnhanceAll = false;

        private DateTime startTime;
        private DateTime endTime;

        private void Main_Load(object sender, EventArgs e)
        {
            PythonEngine.Initialize();
            trackBar1.Enabled = false;
            pictureBox1.Visible = true;
            pictureBox2.Visible = false;

            CreateDirectory();

            //// Create a new FileSystemWatcher
            //watcher = new FileSystemWatcher(outFolderPath);

            //// Set the notification filters
            //watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            //// Subscribe to the events
            ////watcher.Changed += OnChanged;
            //watcher.Created += OnChanged;
            ////watcher.Deleted += OnChanged;

            //watcher.EnableRaisingEvents = true;
        }

        private async void videoSelectBtn_Click(object sender, EventArgs e)
        {
            ShowProcessing();
            int value = trackBar1.Value;
            //button8.Visible = false;
            //CreateDirectory();
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "MP4 files (*.mp4, *.mov)|*.mp4|All files (*.*)|*.*";
                openFileDialog.Title = "Select a Video File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    videoFilePath = openFileDialog.FileName;
                    //button1.Enabled = false;
                    FileName = openFileDialog.SafeFileName;
                    //button2.BackColor = Color.Green;
                    trackBar1.Enabled = true;
                    trackBar1.Value = value;
                    //button9.Visible = true;
                    CreateDirectory();

                    await ExtractAllFrames();

                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            ShowProcessing();
            await RunCodeFormer();
        }

        private async Task RunCodeFormer()
        {
            int index = trackBar1.Value;
            await Task.Run(() =>
            {
                using (Py.GIL())
                {
                    dlog($"started code former for : {GetInputImageWithIndex(index)} || {DateTime.Now.ToString("HH:mm:ss")}");
                    dynamic cfClrModule = Py.Import("cf_clr");
                    cfClrModule.run_codeformer(GetInputImageWithIndex(index), background_enhance, face_upsample, upscale, codeformer_fidelity);
                    LoadEnhImage(GetInputImageWithIndex(index));
                    LoadOrgImage(GetInputImageWithIndex(index));
                    dlog($"Enhanced: {GetInputImageWithIndex(index)} || {DateTime.Now.ToString("HH:mm:ss")}");
                }
            });
        }

        private async Task EnhanceAll()
        {
            await Task.Run(() =>
            {
                using (Py.GIL())
                {
                    isEnhanceAll = true;
                    startTime = DateTime.Now;
                    string[] imageFiles = Directory.GetFiles(inputImagePath, "*.*", SearchOption.AllDirectories)
                                    .Where(file => file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpeg"))
                                    .ToArray();
                    dlog($"started Codeformer for : {inputImagePath} || {DateTime.Now.ToString("HH:mm:ss")}");
                    dynamic cfClrModule = Py.Import("cf_clr");
                    foreach (var item in imageFiles)
                    {
                        cfClrModule.run_codeformer(item, background_enhance, face_upsample, upscale, codeformer_fidelity);
                        BeginInvoke(new Action(() =>
                        {
                            LoadEnhImage(GetOutputImageWithIndex(trackBar1.Value));
                            LoadOrgImage(GetOutputImageWithIndex(trackBar1.Value));
                            dlog($"Enhanced: {GetInputImageWithIndex(trackBar1.Value)} || {DateTime.Now.ToString("HH:mm:ss")}");
                        }));

                        if (hardStop)
                        {
                            hardStop = false;
                            isEnhanceAll = false;
                            break;
                        }
                        BeginInvoke(new Action(() =>
                        {
                            if (trackBar1.Value < trackBar1.Maximum)
                            {
                                trackBar1.Value++;
                            }
                        }));
                    }
                    endTime = DateTime.Now;
                    BeginInvoke(new Action(() =>
                    {
                        videoSelectBtn.Enabled = true;
                        button1.Enabled = true;
                        button2.Enabled = true;
                        button3.Enabled = true;
                        button4.Enabled = true;
                        button5.Visible = false;
                        button6.Visible = true;
                    }));

                    TimeSpan timeSpan = endTime - startTime;
                    int hours = timeSpan.Hours;
                    int minutes = timeSpan.Minutes;
                    int seconds = timeSpan.Seconds;

                    dlog($"Total Time: {hours} hours, {minutes} minutes, {seconds} seconds");
                }
            });
        }

        private async Task ExtractAllFrames()
        {
            await Task.Run(async () =>
            {
                // Path to the input video file
                string inputVideoPath = videoFilePath;

                // Path to the output frames directory
                string outputFramesDirectory = inputImagePath;

                // Initialize VideoCapture object
                using (VideoCapture videoCapture = new VideoCapture(inputVideoPath))
                {
                    // Get video information
                    fps = videoCapture.Get(CapProp.Fps);
                    frameCount = (int)videoCapture.Get(CapProp.FrameCount);
                    double durationInSeconds = frameCount / fps;

                    BeginInvoke(new Action(() =>
                    {
                        trackBar1.Maximum = frameCount;
                    }));

                    dlog($"Total Frames: {frameCount}");
                    dlog($"Frames Per Second (FPS): {fps}");
                    dlog($"Duration: {durationInSeconds} seconds");

                    // Create output directory if not exists
                    if (!Directory.Exists(outputFramesDirectory))
                    {
                        Directory.CreateDirectory(outputFramesDirectory);
                    }

                    // Extract frames
                    //for (int i = 0; i < frameCount; i++)
                    //{
                    //    // Set the frame position
                    //    videoCapture.Set(CapProp.PosFrames, i);

                    //    // Read the frame
                    //    Mat frame = new Mat();
                    //    videoCapture.Read(frame);

                    //    // Save the frame as an image file
                    //    string frameFilePath = $"{outputFramesDirectory}/frame_{i:D8}.png";
                    //    CvInvoke.Imwrite(frameFilePath, frame);

                    //    if (i == 0)
                    //    {
                    //        LoadImage(0);
                    //    }
                    //}

                    await ExtractAllFramesFFMPEG();

                    BeginInvoke(new Action(() =>
                    {
                        button4.Visible = true;
                    }));

                    while (true)
                    {
                        if (File.Exists(GetInputImageWithIndex(1)))
                        {
                            LoadImage(1);
                            break;
                        }
                    }


                }
            });
        }

        private async Task ExtractAllFramesFFMPEG()
        {
            await Task.Run(() =>
            {
                // Execute FFmpeg command to extract frames
                process = new Process();
                process.StartInfo.FileName = "ffmpeg";
                process.StartInfo.Arguments = $"-i \"{videoFilePath}\" -vf fps={fps} \"{inputImagePath}\\frame_%08d.png\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true; // Prevent opening a new console window


                //process.OutputDataReceived += (sender, e) => dlog(e.Data != null ? e.Data : "");
                //process.ErrorDataReceived += (sender, e) => dlog(e.Data != null ? e.Data : "");

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();



                process.WaitForExit();
            });
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

        private string GetDeletedFramesPath(int index)
        {
            return $"{outFolderPath}/Temp_frame_{index:00000000}.png";
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // Handle file or directory changes here
            Console.WriteLine($"Change detected: {e.ChangeType} - {e.FullPath}");
            dlog($"Enhanced: {e.ChangeType} - {e.FullPath} || {DateTime.Now.ToString("HH:mm:ss")}");
            if (!isEnhanceAll)
            {
                BeginInvoke(new Action(() =>
                {
                    LoadOrgImage(GetInputImageWithIndex(trackBar1.Value));
                }));
            }

        }

        public void ShowLoading()
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

        private void dlog(string msg)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(dlog), $"> {msg + Environment.NewLine}");
                return;
            }
            else
            {
                textBox1.AppendText($"> {msg + Environment.NewLine}");
            }

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

        public void ShowProcessing()
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

        private async void button1_Click(object sender, EventArgs e)
        {
            isPlaying = !isPlaying;

            if (isPlaying)
            {
                button1.Text = "Pause";
            }
            else
            {
                button1.Text = "Play";
            }
            int index = 0;
            index = trackBar1.Value;
            if (isPlaying)
            {
                await Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(Convert.ToInt32(fps));
                        if (!isPlaying) break;
                        string frameName = GetFrameName(index);
                        string frameFileName = Path.Combine(inputImagePath, frameName);

                        BeginInvoke(new Action(() =>
                        {
                            trackBar1.Value = index;
                        }));

                        if (!File.Exists(GetInputImageWithIndex(index)) && !File.Exists(GetOutputImageWithIndex(index)))
                        {
                            index = 1;
                        }
                        else if (File.Exists(GetOutputImageWithIndex(index)) && !File.Exists(GetInputImageWithIndex(index)))
                        {
                            LoadOrgImage(GetOutputImageWithIndex(index));
                        }
                        else
                        {
                            LoadOrgImage(GetInputImageWithIndex(index));
                        }

                        if (index < frameCount)
                        {
                            index++;
                        }
                        else
                        {
                            index = 1;
                        }

                    }
                });
            }
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!convertInprogress)
            {
                codeFormer = false;
                enhanced = false;
                //button3.BackColor = Color.Red;
                //button4.BackColor = Color.Red;
                pictureBox1.Visible = true;
                pictureBox2.Visible = false;
                //button8.Visible = false;
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
            //else
            //{
            //    timer1.Enabled = false;
            //}
        }

        private void button3_Click(object sender, EventArgs e)
        {
            enhanced = !enhanced;

            if (enhanced)
            {

                if (!File.Exists(GetOutputImageWithIndex(trackBar1.Value)))
                {
                    enhanced = false;
                    button3.Text = "Show Enhanced";
                    MessageBox.Show("Please use option 'Codeformer' to enhance this frame!", "Frame not found !");
                }
                else
                {
                    button3.Text = "Show Original";
                    LoadOrgImage(GetOutputImageWithIndex(trackBar1.Value));
                    LoadEnhImage(GetOutputImageWithIndex(trackBar1.Value));
                }
            }
            else
            {
                button3.Text = "Show Enhanced";
                LoadOrgImage(GetInputImageWithIndex(trackBar1.Value));
                LoadEnhImage(GetInputImageWithIndex(trackBar1.Value));
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            ShowProcessing();
            trackBar1.Value = 1;
            videoSelectBtn.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Visible = true;
            await EnhanceAll();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            hardStop = true;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                isPlaying = false;
                process.Kill();
            }
            catch (Exception ex)
            {
                dlog(ex.Message);
            }

        }

        private async void button6_Click(object sender, EventArgs e)
        {
            videoSelectBtn.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            await ConvertTovideo();
        }

        private async Task ConvertTovideo()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (process = new Process())
                    {
                        process.StartInfo.FileName = $"ffmpeg";
                        process.StartInfo.Arguments = $"-framerate {fps} -i {outFolderPath}/frame_%08d.png -c:v libx264 -pix_fmt yuv420p \"{FileName}\"";
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

                        BeginInvoke(new Action(() =>
                        {
                            videoSelectBtn.Enabled = true;
                            button1.Enabled = true;
                            button2.Enabled = true;
                            button3.Enabled = true;
                            button4.Enabled = true;
                            button5.Enabled = true;
                            button6.Enabled = true;
                        }));

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
            });
        }

        private void pictureBox2_DoubleClick(object sender, EventArgs e)
        {
            if (!enhanced)
            {
                Maximize maximize = new Maximize(inputImagePath, fps, trackBar1.Value, frameCount);
                maximize.Show();
            }
            else
            {
                Maximize maximize = new Maximize(outFolderPath, fps, trackBar1.Value, frameCount);
                maximize.Show();
            }
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!enhanced)
            {
                Maximize maximize = new Maximize(inputImagePath, fps, trackBar1.Value, frameCount);
                maximize.Show();
            }
            else
            {
                Maximize maximize = new Maximize(outFolderPath, fps, trackBar1.Value, frameCount);
                maximize.Show();
            }
        }
    }
}

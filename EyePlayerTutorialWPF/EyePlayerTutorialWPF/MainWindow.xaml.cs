using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Forms;
using EyeXFramework;
using Tobii.EyeX.Framework;
using System.Windows.Threading;
using EyeXFramework.Wpf;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WMPLib;
using System.Diagnostics;

namespace EyePlayerTutorialWPF
{
    public partial class MainWindow : Window
    {
        DispatcherTimer dt;
        private const UInt32 MOUSEEVENTF_WHEEL = 0x0800;
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, int dwData, uint dwExtraInf);

        public readonly WpfEyeXHost eHost = new WpfEyeXHost();

        ScaleTransform scale = new ScaleTransform();

        double posX;
        double posY;
        double leftEyePosX;
        double leftEyePosY;
        double leftEyePosZ;
        double rightEyePosX;
        double rightEyePosY;
        double rightEyePosZ;
        double EyePosX;
        double EyePosY;
        double EyePosZ;

        Boolean isSelected1 = false;
        Boolean isSelected2 = false;
        Boolean isSelected3 = false;
        Boolean isSelected4 = false;
        Boolean isChoose1 = false;
        Boolean isChoose2 = false;
        Boolean scrollFinishTimeStore = false;
        Boolean clickFinishTimeStore = false;
        Boolean userPresenceTimeStore = false;
        Boolean userNotPresence = false;
        Boolean gazeNotTracked = false;

        Boolean hasGazed = false;


        DateTime selectTime;
        DateTime chooseTime;
        DateTime scrollFinishTime;
        DateTime clickFinishTime;
        DateTime userPresenceTime;

        string state = "appStart";

        double screenHeight = SystemParameters.PrimaryScreenHeight;
        double screenWidth = SystemParameters.PrimaryScreenWidth;

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public MainWindow()
        {
            InitializeComponent();
            eHost.Start();

            

            dt = new DispatcherTimer();
            dt.Tick += DrawAndControl;
            dt.Interval = new TimeSpan(1000 * 1000);
            dt.Start();



            //DrawRectangle();
            startEyeTrack();




            
            scrollViewer.ScrollChanged += OnScrollChanged;

        }

        public void startEyeTrack()
        {
            GazePointDataStream gpda = eHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered);
            gpda.Next += new EventHandler<GazePointEventArgs>(GetPos);
            EyePositionDataStream epds = eHost.CreateEyePositionDataStream();
            epds.Next += new EventHandler<EyePositionEventArgs>(GetEyePos);

            eHost.UserPresenceChanged += new EventHandler<EngineStateValue<UserPresence>>(GetPresence);
            eHost.GazeTrackingChanged += new EventHandler<EngineStateValue<GazeTracking>>(GetGaze);

            //eHost.Start();
        }

        private void GetEyePos(object sender, EyePositionEventArgs e)
        {
            if (e.LeftEyeNormalized.X != 0 || e.LeftEyeNormalized.Y != 0 || e.LeftEyeNormalized.Z != 0 || e.RightEyeNormalized.X != 0 || e.RightEyeNormalized.Y != 0 || e.RightEyeNormalized.Z != 0)
            {
                leftEyePosX = e.LeftEyeNormalized.X;
                leftEyePosY = e.LeftEyeNormalized.Y;
                leftEyePosZ = e.LeftEyeNormalized.Z;
                rightEyePosX = e.RightEyeNormalized.X;
                rightEyePosY = e.RightEyeNormalized.Y;
                rightEyePosZ = e.RightEyeNormalized.Z;
                EyePosX = (leftEyePosX + rightEyePosX) / 2;
                EyePosY = (leftEyePosY + rightEyePosY) / 2;
                EyePosZ = (leftEyePosZ + rightEyePosZ) / 2;
            }
        }

        private void GetPos(object sender, GazePointEventArgs e)
        {
            posX = e.X;
            posY = e.Y;
        }

        public void GetGaze(object sender, EngineStateValue<GazeTracking> e)
        {
            if (eHost.GazeTracking.ToString() == "GazeNotTracked")
            {
                gazeNotTracked = true;

                hasGazed = false;
            }
            if (eHost.GazeTracking.ToString() == "GazeTracked")
            {
                gazeNotTracked = false;

                hasGazed = true;
            }
        }

        public void GetPresence(object sender, EngineStateValue<UserPresence> e)
        {
            if (eHost.UserPresence.ToString() == "NotPresent")
            {
                userNotPresence = true;
            }
            if (eHost.UserPresence.ToString() == "Present")
            {
                userNotPresence = false;
            }
        }

        public void Reset()
        {
            if (gazeNotTracked == true)
            {
                posX = 0;
                posY = screenHeight / 2;
                gazeNotTracked = false;
            }

            if (userNotPresence == true)
            {
                if (userPresenceTimeStore == false)
                {

                    userPresenceTime = DateTime.Now;
                    userPresenceTimeStore = true;
                }

                if ((DateTime.Now - userPresenceTime).TotalSeconds > 5 && userPresenceTimeStore)
                {
                    scrollFinishTimeStore = false;
                    clickFinishTimeStore = false;

                    appStart.Visibility = Visibility.Visible;
                    scrollRegion.Visibility = Visibility.Hidden;
                    scrollUpFixed.Visibility = Visibility.Hidden;
                    scrollFinish.Visibility = Visibility.Hidden;
                    clickVideo.Visibility = Visibility.Hidden;
                    clickBorder.Visibility = Visibility.Hidden;
                    playVideo.Visibility = Visibility.Hidden;
                    clickFinish.Visibility = Visibility.Hidden;
                    tutorialFinish.Visibility = Visibility.Hidden;
                    scrollViewer.UpdateLayout();
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight/2);
                    stepsDisplay.Source = new BitmapImage(new Uri("media/step_00.png", UriKind.Relative));
                    state = "appStart";
                    posX = 0;
                    posY = 0;
                    video1.LoadedBehavior = MediaState.Stop;
                    video2.LoadedBehavior = MediaState.Stop;
                    video3.LoadedBehavior = MediaState.Stop;
                    video4.LoadedBehavior = MediaState.Stop;
                    clickSound.LoadedBehavior = MediaState.Stop;
                    successSound.LoadedBehavior = MediaState.Stop;
                    eyeplayerVideo.Visibility = Visibility.Hidden;
                    eyeplayerVideo.LoadedBehavior = MediaState.Stop;

                    canvasObj.Children.Clear();
                }
            }
            else
            {
                userPresenceTimeStore = false;
            }
        }

        public void DrawAndControl(object sender, EventArgs e)
        {
            canvasObj.Children.Clear();

            //System.Windows.Forms.Cursor.Position = new System.Drawing.Point((int)posX + 10, (int)posY + 10);
            //DrawCross();


            if (state == "appStart")
            {
                AppStart();
                DrawBigEye();
            }
            else if (state == "appChoose")
            {
                AppChoose();
            }
            else
            {
                DrawEye();
            }

            
            if (state == "scroll")
                Scroll();
            if (state == "click")
                Click();
            if (state == "videoPlay")
                VideoPlay();

            InstructionShow();

            Reset();

        }

        public void DrawEye()
        {
            Ellipse leftEllipse = new Ellipse();
            Ellipse rightEllipse = new Ellipse();
            TextBlock eyePosText = new TextBlock();
            eyePosText.FontSize = 20;

            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();


            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            if (EyePosZ < 0.2)
            {
                if (EyePosZ != 0)
                {
                    mySolidColorBrush.Color = Color.FromArgb(100, 255, 255, 255);
                    leftEllipse.Width = 33 * (2 - EyePosZ);
                    leftEllipse.Height = 33 * (2 - EyePosZ);
                    rightEllipse.Width = 33 * (2 - EyePosZ);
                    rightEllipse.Height = 33 * (2 - EyePosZ);
                }
            }
            if (EyePosZ >= 1)
            {
                mySolidColorBrush.Color = Color.FromArgb(100, 255, 255, 255);
                leftEllipse.Width = 10 * (2 - EyePosZ);
                leftEllipse.Height = 10 * (2 - EyePosZ);
                rightEllipse.Width = 10 * (2 - EyePosZ);
                rightEllipse.Height = 10 * (2 - EyePosZ);
            }
            if (EyePosZ < 1
                && EyePosZ >= 0.2)
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 255);
                leftEllipse.Width = 20 * (2 - EyePosZ);
                leftEllipse.Height = 20 * (2 - EyePosZ);
                rightEllipse.Width = 20 * (2 - EyePosZ);
                rightEllipse.Height = 20 * (2 - EyePosZ);
            }

            leftEllipse.Fill = mySolidColorBrush;
            rightEllipse.Fill = mySolidColorBrush;
            //myEllipse.StrokeThickness = 2;
            //myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.

            Canvas.SetTop(leftEllipse, leftEyePosY * screenHeight * 0.19);
            Canvas.SetRight(leftEllipse, leftEyePosX * screenWidth * 0.18);
            Canvas.SetTop(rightEllipse, rightEyePosY * screenHeight * 0.19);
            Canvas.SetRight(rightEllipse, rightEyePosX * screenWidth * 0.18);
            Canvas.SetTop(eyePosText, screenHeight * 0.16);
            Canvas.SetRight(eyePosText, screenWidth * 0.08);

            if ((leftEyePosX != 0 || leftEyePosY != 0) && (rightEyePosX != 0 || rightEyePosY != 0))
            {
                canvasObj.Children.Add(leftEllipse);
                canvasObj.Children.Add(rightEllipse);
                canvasObj.Children.Add(eyePosText);
            }


        }

        public void DrawBigEye()
        {
            Ellipse leftEllipse = new Ellipse();
            Ellipse rightEllipse = new Ellipse();
            TextBlock eyePosText = new TextBlock();
            eyePosText.FontSize = 20;

            // Create a SolidColorBrush with a red color to fill the 
            // Ellipse with.
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();


            // Describes the brush's color using RGB values. 
            // Each value has a range of 0-255.
            if (EyePosZ < 0.2)
            {
                if (EyePosZ != 0)
                {
                    mySolidColorBrush.Color = Color.FromArgb(100, 255, 255, 255);
                    leftEllipse.Width = 33 * (2 - EyePosZ) * 5;
                    leftEllipse.Height = 33 * (2 - EyePosZ) * 5;
                    rightEllipse.Width = 33 * (2 - EyePosZ) * 5;
                    rightEllipse.Height = 33 * (2 - EyePosZ) * 5;
                }
            }
            if (EyePosZ >= 1)
            {
                mySolidColorBrush.Color = Color.FromArgb(100, 255, 255, 255);
                leftEllipse.Width = 10 * (2 - EyePosZ) * 5;
                leftEllipse.Height = 10 * (2 - EyePosZ) * 5;
                rightEllipse.Width = 10 * (2 - EyePosZ) * 5;
                rightEllipse.Height = 10 * (2 - EyePosZ) * 5;
            }
            if (EyePosZ < 1
                && EyePosZ >= 0.2)
            {
                mySolidColorBrush.Color = Color.FromArgb(255, 255, 255, 255);
                leftEllipse.Width = 20 * (2 - EyePosZ) * 5;
                leftEllipse.Height = 20 * (2 - EyePosZ) * 5;
                rightEllipse.Width = 20 * (2 - EyePosZ) * 5;
                rightEllipse.Height = 20 * (2 - EyePosZ) * 5;
            }

            leftEllipse.Fill = mySolidColorBrush;
            rightEllipse.Fill = mySolidColorBrush;
            //myEllipse.StrokeThickness = 2;
            //myEllipse.Stroke = Brushes.Black;

            // Set the width and height of the Ellipse.

            Canvas.SetTop(leftEllipse, leftEyePosY * screenHeight);
            Canvas.SetRight(leftEllipse, leftEyePosX * screenWidth * 0.9);
            Canvas.SetTop(rightEllipse, rightEyePosY * screenHeight);
            Canvas.SetRight(rightEllipse, rightEyePosX * screenWidth * 0.9);
            //Canvas.SetTop(eyePosText, screenHeight * 0.16);
            //Canvas.SetRight(eyePosText, screenWidth * 0.08);

            if ((leftEyePosX != 0 || leftEyePosY != 0) && (rightEyePosX != 0 || rightEyePosY != 0))
            {
                canvasObj.Children.Add(leftEllipse);
                canvasObj.Children.Add(rightEllipse);
                //canvasObj.Children.Add(eyePosText);
            }


        }

        public void AppStart()
        {
            appStart.Visibility = Visibility.Visible;

            if (hasGazed == true)
            {
                appStartLogo.Visibility = Visibility.Hidden;
                appStartText.Visibility = Visibility.Visible;

                //successSound.LoadedBehavior = MediaState.Play;
                //successSound.LoadedBehavior = MediaState.Stop;

                //hasGazed = false;

                userPresenceTimeStore = false;
                
            }
            else
            {
                appStartLogo.Visibility = Visibility.Visible;
                appStartText.Visibility = Visibility.Hidden;

                canvasObj.Children.Clear();
            }
        }

        public void AppChoose()
        {
            appStart.Visibility = Visibility.Hidden;
            appChoose.Visibility = Visibility.Visible;

            if (posX < screenWidth * 0.5 && posX > 0 && posY > 0)
            {
                if (isChoose1 == false)
                {
                    chooseTime = DateTime.Now;
                    isChoose1 = true;
                    isChoose2 = false;
                    eyeplayer_01_h.Visibility = Visibility.Visible;
                    eyeplayer_02_h.Visibility = Visibility.Hidden;
                    chooseCircle1.Visibility = Visibility.Visible;
                    chooseCircle2.Visibility = Visibility.Hidden;
                }


                scale.ScaleX = (2000 - (DateTime.Now - chooseTime).TotalMilliseconds) / 2000;
                scale.ScaleY = (2000 - (DateTime.Now - chooseTime).TotalMilliseconds) / 2000;
                chooseCircle1.RenderTransform = scale;

                if ((DateTime.Now - chooseTime).TotalMilliseconds > 2000)
                {
                    try
                    {
                        var startInfo = new ProcessStartInfo();

                        startInfo.WorkingDirectory = @"C:\Program Files (x86)\SENSE\Eyeplayer";
                        startInfo.FileName = @"Eyeplayer.exe";

                        Process.Start(startInfo);
                        this.Close();
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        System.Windows.MessageBox.Show("請安裝EyePlayer");
                    }

                }
            }
            if (posX > screenWidth * 0.5 && posY > 0)
            {
                if (isChoose2 == false)
                {
                    chooseTime = DateTime.Now;
                    isChoose2 = true;
                    isChoose1 = false;
                    eyeplayer_02_h.Visibility = Visibility.Visible;
                    eyeplayer_01_h.Visibility = Visibility.Hidden;
                    chooseCircle2.Visibility = Visibility.Visible;
                    chooseCircle1.Visibility = Visibility.Hidden;
                }


                scale.ScaleX = (2000 - (DateTime.Now - chooseTime).TotalMilliseconds) / 2000;
                scale.ScaleY = (2000 - (DateTime.Now - chooseTime).TotalMilliseconds) / 2000;
                chooseCircle2.RenderTransform = scale;

                if ((DateTime.Now - chooseTime).TotalMilliseconds > 2000)
                {
                    scrollRegion.Visibility = Visibility.Visible;
                    scrollUpFixed.Visibility = Visibility.Visible;
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight / 2);
                    state = "scroll";
                    appChoose.Visibility = Visibility.Hidden;
                }
            }
        }

        public void Scroll()
        {
            stepsDisplay.Source = new BitmapImage(new Uri("media/step_01.png", UriKind.Relative));

            hasGazed = false;

            SetCursorPos(10, (int)screenHeight - 400);


            

            if (posY < screenHeight / 3)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (int)((screenHeight / 3 - posY) / 3) / 4, 0);
                scrollUp.Source = new BitmapImage(new Uri("media/arrow_top_h.png", UriKind.Relative));
                scrollDown.Source = new BitmapImage(new Uri("media/arrow_bottom.png", UriKind.Relative));
            }
            else if (posY > screenHeight * 2 / 3)
            {
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (int)((screenHeight * 2 / 3 - posY) / 3) / 4, 0);
                scrollDown.Source = new BitmapImage(new Uri("media/arrow_bottom_h.png", UriKind.Relative));
                scrollUp.Source = new BitmapImage(new Uri("media/arrow_top.png", UriKind.Relative));
            }
            else
            {
                scrollDown.Source = new BitmapImage(new Uri("media/arrow_bottom.png", UriKind.Relative));
                scrollUp.Source = new BitmapImage(new Uri("media/arrow_top.png", UriKind.Relative));
            }

            clickVideo.Visibility = Visibility.Hidden;

        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (state == "scroll")
            {
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight || scrollViewer.VerticalOffset == 0)
                {
                    state = "scrollFinish";
                }
            }
        }

        public void Click()
        {
            scrollRegion.Visibility = Visibility.Visible;
            clickBorder.Visibility = Visibility.Visible;
            stepsDisplay.Source = new BitmapImage(new Uri("media/step_02.png", UriKind.Relative));


            if (posX < screenWidth * 0.4 && posY < screenHeight * 0.5 && posX > 0 && posY > 0)
            {
                if (isSelected1 == false)
                {
                    selectTime = DateTime.Now;
                    isSelected1 = true;
                    isSelected2 = false;
                    isSelected3 = false;
                    isSelected4 = false;
                    border1.Visibility = Visibility.Visible;
                    border2.Visibility = Visibility.Hidden;
                    border3.Visibility = Visibility.Hidden;
                    border4.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Visible;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle4.Visibility = Visibility.Hidden;
                }


                scale.ScaleX = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                scale.ScaleY = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                clickCircle1.RenderTransform = scale;

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1500)
                {
                    clickSound.LoadedBehavior = MediaState.Play;
                }

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1700)
                {
                    clickCircle4.Visibility = Visibility.Hidden;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;
                    video1.Visibility = Visibility.Visible;
                    video2.Visibility = Visibility.Hidden;
                    video3.Visibility = Visibility.Hidden;
                    video4.Visibility = Visibility.Hidden;
                    
                    video1.LoadedBehavior = MediaState.Play;
                    border1.Visibility = Visibility.Hidden;
                    isSelected1 = false;



                    state = "videoPlay";

                }
            }
            else if (posX > screenWidth * 0.4 && posY < screenHeight * 0.5 && posX < screenWidth * 0.8)
            {
                if (isSelected2 == false)
                {
                    selectTime = DateTime.Now;
                    isSelected2 = true;
                    isSelected1 = false;
                    isSelected3 = false;
                    isSelected4 = false;
                    border2.Visibility = Visibility.Visible;
                    border1.Visibility = Visibility.Hidden;
                    border3.Visibility = Visibility.Hidden;
                    border4.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;
                    clickCircle2.Visibility = Visibility.Visible;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle4.Visibility = Visibility.Hidden;
                }
                scale.ScaleX = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                scale.ScaleY = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                clickCircle2.RenderTransform = scale;

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1500)
                {
                    clickSound.LoadedBehavior = MediaState.Play;
                }

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1700)
                {
                    clickCircle4.Visibility = Visibility.Hidden;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;
                    video1.Visibility = Visibility.Hidden;
                    video2.Visibility = Visibility.Visible;
                    video3.Visibility = Visibility.Hidden;
                    video4.Visibility = Visibility.Hidden;
                    video2.LoadedBehavior = MediaState.Play;
                    border2.Visibility = Visibility.Hidden;
                    isSelected2 = false;

                    
                    state = "videoPlay";
                }
            }
            else if (posX < screenWidth * 0.4 && posY > screenHeight * 0.5)
            {
                if (isSelected3 == false)
                {

                    selectTime = DateTime.Now;
                    isSelected3 = true;
                    isSelected1 = false;
                    isSelected2 = false;
                    isSelected4 = false;
                    border3.Visibility = Visibility.Visible;
                    border1.Visibility = Visibility.Hidden;
                    border2.Visibility = Visibility.Hidden;
                    border4.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Visible;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;
                    clickCircle4.Visibility = Visibility.Hidden;

                }
                scale.ScaleX = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                scale.ScaleY = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                clickCircle3.RenderTransform = scale;

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1500)
                {
                    clickSound.LoadedBehavior = MediaState.Play;
                }

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1700)
                {
                    clickCircle4.Visibility = Visibility.Hidden;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;
                    video1.Visibility = Visibility.Hidden;
                    video2.Visibility = Visibility.Hidden;
                    video3.Visibility = Visibility.Visible;
                    video4.Visibility = Visibility.Hidden;
                    video3.LoadedBehavior = MediaState.Play;
                    border3.Visibility = Visibility.Hidden;

                    isSelected3 = false;


                    state = "videoPlay";

                }
            }
            else if (posX > screenWidth * 0.4 && posY > screenHeight * 0.5 && posX < screenWidth * 0.8)
            {
                if (isSelected4 == false)
                {

                    selectTime = DateTime.Now;
                    isSelected4 = true;
                    isSelected1 = false;
                    isSelected2 = false;
                    isSelected3 = false;
                    border4.Visibility = Visibility.Visible;
                    border1.Visibility = Visibility.Hidden;
                    border2.Visibility = Visibility.Hidden;
                    border3.Visibility = Visibility.Hidden;
                    clickCircle4.Visibility = Visibility.Visible;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;

                }
                scale.ScaleX = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                scale.ScaleY = (1500 - (DateTime.Now - selectTime).TotalMilliseconds) / 1500;
                clickCircle4.RenderTransform = scale;

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1500)
                {
                    clickSound.LoadedBehavior = MediaState.Play;
                }

                if ((DateTime.Now - selectTime).TotalMilliseconds > 1700)
                {
                    clickCircle4.Visibility = Visibility.Hidden;
                    clickCircle2.Visibility = Visibility.Hidden;
                    clickCircle3.Visibility = Visibility.Hidden;
                    clickCircle1.Visibility = Visibility.Hidden;
                    video1.Visibility = Visibility.Hidden;
                    video2.Visibility = Visibility.Hidden;
                    video3.Visibility = Visibility.Hidden;
                    video4.Visibility = Visibility.Visible;

                    video4.LoadedBehavior = MediaState.Play;
                    border4.Visibility = Visibility.Hidden;
                    isSelected4 = false;

                    state = "videoPlay";

                }
            }
            else
            {
                border1.Visibility = Visibility.Hidden;
                border2.Visibility = Visibility.Hidden;
                border3.Visibility = Visibility.Hidden;
                border4.Visibility = Visibility.Hidden;
                clickCircle4.Visibility = Visibility.Hidden;
                clickCircle2.Visibility = Visibility.Hidden;
                clickCircle3.Visibility = Visibility.Hidden;
                clickCircle1.Visibility = Visibility.Hidden;
                selectTime = new DateTime(2100, 12, 31);
            }

        }

        public void VideoPlay()
        {


            playVideo.Visibility = Visibility.Visible;

            img1.Visibility = Visibility.Hidden;
            img2.Visibility = Visibility.Hidden;
            img3.Visibility = Visibility.Hidden;
            img4.Visibility = Visibility.Hidden;

            if (video1.Position.Seconds == 10 || video2.Position.Seconds == 10 || video3.Position.Seconds == 10 || video4.Position.Seconds == 10)
            {
                state = "clickFinish";
                video1.LoadedBehavior = MediaState.Stop;
                video2.LoadedBehavior = MediaState.Stop;
                video3.LoadedBehavior = MediaState.Stop;
                video4.LoadedBehavior = MediaState.Stop;

            }

        }

        public void InstructionShow()
        {
            if (state == "scrollFinish")
            {

                if (scrollFinishTimeStore == false)
                {
                    scrollFinishTime = DateTime.Now;
                    scrollRegion.Visibility = Visibility.Hidden;
                    scrollFinish.Visibility = Visibility.Visible;
                    scrollFinishText2.Visibility = Visibility.Hidden;
                    scrollFinishText1.Visibility = Visibility.Visible;
                    successSound.LoadedBehavior = MediaState.Play;
                    scrollFinishTimeStore = true;
                }


                if ((DateTime.Now - scrollFinishTime).TotalSeconds > 3)
                {
                    scrollFinishText1.Visibility = Visibility.Hidden;
                    scrollFinishText2.Visibility = Visibility.Visible;
                    successSound.LoadedBehavior = MediaState.Stop;
                }

                if ((DateTime.Now - scrollFinishTime).TotalSeconds > 6)
                {
                    scrollViewer.UpdateLayout();
                    scrollViewer.ScrollToVerticalOffset(0);
                    state = "click";
                    clickVideo.Visibility = Visibility.Visible;
                    img1.Visibility = Visibility.Visible;
                    img2.Visibility = Visibility.Visible;
                    img3.Visibility = Visibility.Visible;
                    img4.Visibility = Visibility.Visible;
                }
            }

            if (state == "clickFinish")
            {
                if (clickFinishTimeStore == false)
                {
                    clickFinishTime = DateTime.Now;
                    clickFinish.Visibility = Visibility.Visible;
                    clickFinishText2.Visibility = Visibility.Hidden;
                    clickFinishText1.Visibility = Visibility.Visible;
                    successSound.LoadedBehavior = MediaState.Play;
                    clickFinishTimeStore = true;
                }

                if ((DateTime.Now - clickFinishTime).TotalSeconds > 3)
                {
                    clickFinishText1.Visibility = Visibility.Hidden;
                    clickFinishText2.Visibility = Visibility.Visible;
                }

                if ((DateTime.Now - clickFinishTime).TotalSeconds > 6)
                {
                    
                    eyeplayerVideo.Visibility = Visibility.Visible;
                    eyeplayerVideo.LoadedBehavior = MediaState.Play;
                    stepsDisplay.Source = new BitmapImage(new Uri("media/step_03.png", UriKind.Relative));

                    state = "tutorialFinish";
                }
            }

            if (state == "tutorialFinish")
            {
                if (eyeplayerVideo.NaturalDuration.HasTimeSpan)
                {

                    if (eyeplayerVideo.Position.TotalSeconds == eyeplayerVideo.NaturalDuration.TimeSpan.TotalSeconds)
                    {
                        eyeplayerVideo.LoadedBehavior = MediaState.Stop;
                        eyeplayerVideo.Visibility = Visibility.Hidden;
                        tutorialFinish.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void keyFunctions(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            if (state == "appStart" && gazeNotTracked == false)
            {
                state = "appChoose";
            }
        }

        private void appStart_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (state == "appStart" && gazeNotTracked == false)
            state = "appChoose";


        }
    }
}

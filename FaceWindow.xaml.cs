﻿// -----------------------------------------------------------------------
// <copyright file="FaceWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------



namespace FaceTrackingBasics
{
    using System;
    using System.Windows;
    using System.Windows.Threading;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Collections.Generic;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;

    /// <summary>
    /// Interaction logic for FaceWindow.xaml
    /// </summary>
    /// 
    public partial class FaceWindow : Window
    {
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private long score = 0;
        private double difficulty = 1.0;
        private int currentGameIndex;
        private List<Game> games;
        private GameEnded gameEndedWindow;
        private MainWindow mainWindow;

        public FaceWindow(List <Game> _games, double difficulty, MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            InitializeComponent();
            currentGameIndex = 0;
            games = _games;

            gameEndedWindow = new GameEnded();

            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);

            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;

            TaskProgressBar.Maximum = (int)games[0].getTargetScore() ;
            setWaitText();

            faceTrackingViewer.setMainWindow(this);
            faceTrackingViewer.setGame(games[0]);

            sensorChooser.Start();
        }

        public void setWaitText()
        {
            GameName.Text = "Proszę czekać...";
        }

        public void setGameText()
        {
            if (score < games[currentGameIndex].getTargetScore())
                GameName.Text = games[currentGameIndex].getInstructions();
        }

        private String getScoreText()
        {
            return "Wynik: " + score.ToString() + "/" + games[currentGameIndex].getTargetScore().ToString();
        }

        private void switchGame()
        {
            score = 0;
            currentGameIndex++;
            if (currentGameIndex < games.Count)
            {
                faceTrackingViewer.setGame(games[currentGameIndex]);
                Counter.Text = getScoreText();
                TaskProgressBar.Maximum = games[currentGameIndex].getTargetScore();
                TaskProgressBar.Value = 0;
                setGameText();
            }
            else
            {
                this.Close();
                //WindowClosed(this, new EventArgs());
            }
                
        }

        public void incrementCounter()
        {
            score++;
            if (score > games[currentGameIndex].getTargetScore())
                return;
            Counter.Text = getScoreText();
            TaskProgressBar.Value = (int)score;
            if (score == games[currentGameIndex].getTargetScore())
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(5d);
                timer.Tick += TimerTick;
                gameEndedWindow.Show();
                timer.Start();
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();
            timer.Tick -= TimerTick;
            gameEndedWindow.Hide();
            switchGame();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            sensorChooser.Stop();
            faceTrackingViewer.Dispose();
            gameEndedWindow.Close();
            mainWindow.displayStatistics();
            mainWindow.Start.IsEnabled = true;
           // this.Close();
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }
                /*
                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat = colorImageFrame.Format;
                    this.colorImageData = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    ColorImage.Source = this.colorImageWritableBitmap;
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData);
                this.colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);*/
            }
        }

        internal void setDifficulty(double _difficulty)
        {
            difficulty = _difficulty;
        }

        public double getDifficulty()
        {
            return difficulty;
        }
    }
}

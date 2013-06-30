// -----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------



namespace FaceTrackingBasics
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private FaceWindow faceWindow = null;
        private double difficulty;

        public MainWindow()
        {
            InitializeComponent();

        }

       

        private void WindowClosed(object sender, EventArgs e)
        {
            this.Close();
        }

        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List <Game> games = new List<Game>();
            int browTarget = Convert.ToInt32(Ex1.Text);
            int lipsTarget = Convert.ToInt32(Ex2.Text);
            int mouthTarget = Convert.ToInt32(Ex3.Text);
            int cycles = Convert.ToInt32(cyclesCount.Text);
            for (int i = 0; i < cycles; i++)
            {
                if (browTarget > 0)
                    games.Add(new BrowGame(browTarget));
                if (lipsTarget > 0)
                    games.Add(new WideLipsGame(lipsTarget));
                if (mouthTarget > 0)
                    games.Add(new OpenMouthGame(mouthTarget));
            }
            faceWindow = new FaceWindow(games, difficulty);
            
            faceWindow.Show();
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            difficulty = e.NewValue;
            if (faceWindow != null)
                faceWindow.setDifficulty(difficulty);
        }
    }
}

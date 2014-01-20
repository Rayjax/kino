using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MjpegProcessor;
using System.Net;
using System.IO;
using System.Drawing;

namespace kino
{
    public partial class MainWindow : Window
    {
        MjpegDecoder _mjpeg;
 
        public MainWindow()
        {
            InitializeComponent();
            _mjpeg = new MjpegDecoder();
            _mjpeg.FrameReady += mjpeg_FrameReady;

        }
 
        private void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            videoStreamImage.Source = e.BitmapImage;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Get URL from input
            String videoStreamURL = textBox_videoStreamURL.Text;

            //Start stream
            _mjpeg.ParseStream(new Uri(videoStreamURL));
        }
    }
}

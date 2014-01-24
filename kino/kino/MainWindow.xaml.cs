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
        Commander commander;
 
        public MainWindow()
        {
            InitializeComponent();

            //Init a decoder and listen for new frames
            _mjpeg = new MjpegDecoder();
            _mjpeg.FrameReady += mjpeg_FrameReady;

        }

        private String getVideoStreamUrl(String IPAddress, int port){
            return "http://" + IPAddress + ":" + port + "/videofeed";
        }
 
        private void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            //Update image on each new frame event
            videoStreamImage.Source = e.BitmapImage;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Get user entered parameters
            String targetIP = tb_targetIP.Text;
            int targetPortVideo = 0;
            int.TryParse(tb_targetPortVideo.Text, out targetPortVideo);

            //Get URL from input IP
            String videoStreamURL = this.getVideoStreamUrl(targetIP, targetPortVideo);

            //Set stream url for the decoder (makes the decoding start)
            _mjpeg.ParseStream(new Uri(videoStreamURL));
        }

        //@todo replace with input from kinect
        private void forward_Click(object sender, RoutedEventArgs e)
        {
            //Get user entered parameters
            int targetPortCommands = 0;
           
            int.TryParse(tb_targetPortCommands.Text, out targetPortCommands);

            //Init a commander which will receive commands and send them to Kino
            this.commander = new Commander(tb_targetIP.Text, targetPortCommands);

            //Send command
            this.commander.sendCommand(Command.forward);
        }
    }
}

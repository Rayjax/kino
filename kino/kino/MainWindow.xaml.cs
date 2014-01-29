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
using System.ComponentModel;


namespace kino
{
    public partial class MainWindow : Window
    {
        MjpegDecoder _mjpeg;
        Commander commander;
        bool commanderReady;
        KeyEventHandler keyboardEventHandler;
        bool listeningToKeyboardEvents;
 
        public MainWindow()
        {
            InitializeComponent();

            //Init a decoder for video and listen for new frames
            _mjpeg = new MjpegDecoder();
            _mjpeg.FrameReady += mjpeg_FrameReady;

            //Init an handler for keyboard
            this.keyboardEventHandler = new KeyEventHandler(OnButtonKeyDown);
            listeningToKeyboardEvents = false;
        }

        private String getVideoStreamUrl(String IPAddress, int port){
            return "http://" + IPAddress + ":" + port + "/videofeed";
        }
 
        private void mjpeg_FrameReady(object sender, FrameReadyEventArgs e)
        {
            //Update image on each new frame event
            videoStreamImage.Source = e.BitmapImage;
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (this.commanderReady)
            {
                if (e.Key == Key.Up)
                {
                    this.commander.sendCommand(Command.forward);
                }
                else if (e.Key == Key.Down)
                {
                    this.commander.sendCommand(Command.backward);
                }
                else if (e.Key == Key.Left)
                {
                    this.commander.sendCommand(Command.left);
                }
                else if (e.Key == Key.Right)
                {
                    this.commander.sendCommand(Command.right);
                }
                else if (e.Key == Key.S)
                {
                    this.commander.sendCommand(Command.stand);
                }
            }
            else
            {
                MessageBox.Show("Connectez vous d'abord");
            }
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            if (this.commanderReady)
            {
                if (!this.commander.sendCommand(Command.stand))
                {
                    MessageBox.Show("Impossible d'envoyer la commande à Kino. \n Veuillez vérifier les paramètres réseau et démarrez l'application sur le smartphone.");
                }
            }
            else
            {
                MessageBox.Show("Connectez vous d'abord");
            }
        }

        private void startGameButton_Click(object sender, RoutedEventArgs e)
        {
            //Get user entered parameters
            String targetIP = tb_targetIP.Text;
            int targetPortVideo = 0;
            int.TryParse(tb_targetPortVideo.Text, out targetPortVideo);
            int targetPortCommands = 0;
            int.TryParse(tb_targetPortCommands.Text, out targetPortCommands);

            //Init a commander which will receive commands and send them to Kino via TCP (transport defined with the boolean)
            this.commander = new Commander(tb_targetIP.Text, targetPortCommands, false);
            this.commanderReady = true;

            //Get video
            MessageBox.Show("L'application va maintenant tenter de récupérer le flux vidéo de la caméra du smartphone. \n Si vous ne voyez pas de vidéo, veuillez vérifier que cet ordinateur est sur le même réseau \n que le smartphone, et que vous avez entré des paramètres corrects.");

            //Get URL from input IP
            String videoStreamURL = this.getVideoStreamUrl(targetIP, targetPortVideo);

            //Set stream url for the decoder (makes the decoding start)
            _mjpeg.ParseStream(new Uri(videoStreamURL));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //If we used a TCP connection and we opened it, close it
            if (this.commanderReady)
            {
                this.commander.closeConnection();
            }
        }

        private void manual_control_Click(object sender, RoutedEventArgs e)
        {
            if (this.commanderReady)
            {
                //Listen to keyboard events
                if (listeningToKeyboardEvents)
                {
                    this.KeyUp -= this.keyboardEventHandler;
                    listeningToKeyboardEvents = false;
                }
                else
                {
                    this.KeyUp += this.keyboardEventHandler;
                    listeningToKeyboardEvents = true;
                }
            }
            else
            {
                MessageBox.Show("Connectez vous d'abord");
            }
        }
    }
}

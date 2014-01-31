using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Microsoft.Kinect;
using System.IO;

namespace kino
{

    public class ControlDataReadyEventArgs : EventArgs
    {
        private double powerLeft;
        private double powerRight;
        public ControlDataReadyEventArgs(double powerLeft, double powerRight)
        {
            this.powerLeft = powerLeft;
            this.powerRight = powerRight;
        }
        public double PowerLeft { get { return powerLeft; } }
        public double PowerRight { get { return powerRight; } }

    }

    public class UserDetectionStatusChangeEventArgs : EventArgs
    {
        private UserDetectionStatus userDetectionStatus;
        public UserDetectionStatusChangeEventArgs(UserDetectionStatus userDetectionStatus)
        {
            this.userDetectionStatus = userDetectionStatus;
        }
        public UserDetectionStatus UserDetectionStatus { get { return userDetectionStatus; } }
    }

    public enum UserDetectionStatus
    {
        Detected,
        NotDetected,
        PartiallyDetected
    }

    public delegate void UserDetectionStatusHasChangedEventHandler(object sender, UserDetectionStatusChangeEventArgs e);
    public delegate void KinectControlerProcessedNewControlDataEventHandler(object sender, ControlDataReadyEventArgs e);

    class KinectControler
    {

        // Events
        public event KinectControlerProcessedNewControlDataEventHandler NewControlDataIsReady;
        public event UserDetectionStatusHasChangedEventHandler UserDetectionStatusChanged;

        // Properties
        public bool IsStarted { get { return (sensor==null) ? false : sensor.IsRunning; } }
        public UserDetectionStatus CurrentUserDetectionStatus { get { return currentUserDetectionStatus; } }

        // Privates
        private KinectSensor sensor;
        private UserDetectionStatus currentUserDetectionStatus = UserDetectionStatus.NotDetected;

        // Constructor
        public KinectControler()
        {
            
        }


        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];

            // Retreiving skeletons from event args
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            // Initializing joints (wrist, hip and shoulder for each side)
            Joint rightWrist = new Joint(), rightShoulder = new Joint(), rightHip = new Joint(), leftWrist = new Joint(), leftShoulder = new Joint(), leftHip = new Joint();

            // Seaching for joints
            int counter = -1; // Minus one is when we don't have any valid skeleton
            foreach (Skeleton s in skeletons)
            {
                // Getting the first tracked skeleton from skeletons list
                if (s.TrackingState == SkeletonTrackingState.Tracked)
                {
                    counter = 0; // Between 0 and 6 excluded is when the skeleton is incomplete
                    // Trying to get the 6 joints we want (wrist, hip and shoulder for each side)
                    foreach (Joint j in s.Joints)
                    {
                        // We only get the data if it is flagged as "tracked"
                        if (j.TrackingState == JointTrackingState.Tracked)
                        {
                            switch (j.JointType)
                            {
                                case JointType.WristRight: rightWrist = j; counter++; break;
                                case JointType.ShoulderRight: rightShoulder = j; counter++; break;
                                case JointType.HipRight: rightHip = j; counter++; break;
                                case JointType.WristLeft: leftWrist = j; counter++; break;
                                case JointType.ShoulderLeft: leftShoulder = j; counter++; break;
                                case JointType.HipLeft: leftHip = j; counter++; break;
                            }
                        }

                    }
                    break;
                }
            }
            
            // If we managed to get the 6 joints we wanted, we continue calculation
            if (counter == 6)
            {
                // Getting angles in radian from joints (inverting left angle because it will be negative
                double rightSideAngleRadian = AngleFromSkeletonPoints(rightHip.Position, rightShoulder.Position, rightWrist.Position);
                double leftSideAngleRadian = AngleFromSkeletonPoints(leftWrist.Position, leftShoulder.Position, leftHip.Position);

                // Converting to degrees (simplier to work with)
                double rightSideAngleDegree = 180.0d * (rightSideAngleRadian) / System.Math.PI;
                double leftSideAngleDegree = 180.0d * (leftSideAngleRadian) / System.Math.PI;

                double invertedRightSideAngleDegree = 180 - rightSideAngleDegree;
                double invertedLeftSideAngleDegree = 180 - leftSideAngleDegree;

                // Triggering NewControlDataIsReady event
                if (NewControlDataIsReady != null)
                {
                    NewControlDataIsReady(this, new ControlDataReadyEventArgs(PowerFromAngle(invertedRightSideAngleDegree), PowerFromAngle(invertedLeftSideAngleDegree)));
                }

                // Triggering UserDetectionStatusChanged if currentUserDetectionStatus changed
                if (currentUserDetectionStatus!=UserDetectionStatus.Detected)
                {
                    currentUserDetectionStatus = UserDetectionStatus.Detected;
                    if (UserDetectionStatusChanged != null)
                    {
                        UserDetectionStatusChanged(this, new UserDetectionStatusChangeEventArgs(UserDetectionStatus.Detected));
                    }
                }
                
            }
            else if (counter > 0) // Skeleton is incomplete
            {
                // Triggering UserDetectionStatusChanged if currentUserDetectionStatus changed
                if (currentUserDetectionStatus != UserDetectionStatus.PartiallyDetected)
                {
                    currentUserDetectionStatus = UserDetectionStatus.PartiallyDetected;
                    if (UserDetectionStatusChanged != null)
                    {
                        UserDetectionStatusChanged(this, new UserDetectionStatusChangeEventArgs(UserDetectionStatus.PartiallyDetected));
                    }
                }
            }
            else // No skeleton at all
            {
                // Triggering UserDetectionStatusChanged if currentUserDetectionStatus changed
                if (currentUserDetectionStatus != UserDetectionStatus.NotDetected)
                {
                    currentUserDetectionStatus = UserDetectionStatus.NotDetected;
                    if (UserDetectionStatusChanged != null)
                    {
                        UserDetectionStatusChanged(this, new UserDetectionStatusChangeEventArgs(UserDetectionStatus.NotDetected));
                    }
                }
            }
        }

        /// <summary>
        /// Calculates angle between 3 points
        /// </summary>
        /// <param name="A">Point A</param>
        /// <param name="B">Point B</param>
        /// <param name="C">Point C</param>
        /// <returns>The angle in radian of the angle (ABC) - equivalent of the angle (AB)^(BC)</returns>
        private double AngleFromSkeletonPoints(SkeletonPoint A, SkeletonPoint B, SkeletonPoint C)
        {

            double v1x = B.X - C.X;
            double v1y = B.Y - C.Y;
            double v2x = A.X - B.X;
            double v2y = A.Y - B.Y;

            double angle = System.Math.Atan2(v1x, v1y) - System.Math.Atan2(v2x, v2y);

            return angle;
        }


        private int ANGLE_IDLE_THRESHOLD = 30; // Naturally, the angle (wrist/shoulder/hip) is about 25 degrees
        private int ANGLE_MAX_POWER_THRESHOLD = 100;
        private int ANGLE_REVERSE_THRESHOLD = 160;

        /// <summary>
        /// Calculates the power from the angle
        /// </summary>
        /// <param name="angleInDegree">The angle between hip, shoulder and wrist in degrees</param>
        /// <returns>A double between -1 and 1 representing the power</returns>
        private double PowerFromAngle(double angleInDegree)
        {
            // If angle is negative, we turn it into positive
            if (angleInDegree < 0)
            {
                angleInDegree *= -1;
            }

            // We use the thresholds defined above to calculate the power

            if (angleInDegree >= 0 && angleInDegree < ANGLE_IDLE_THRESHOLD)
            {
                //Idle, wrist is next to the hip
                return 0.0d;
            }
            else if (angleInDegree >= ANGLE_IDLE_THRESHOLD && angleInDegree < ANGLE_MAX_POWER_THRESHOLD)
            {
                //Analog power, we calculate a value from 0 to 1
                return ((angleInDegree - ANGLE_IDLE_THRESHOLD) / (ANGLE_MAX_POWER_THRESHOLD-ANGLE_IDLE_THRESHOLD));
            }
            else if (angleInDegree >= ANGLE_MAX_POWER_THRESHOLD && angleInDegree < ANGLE_REVERSE_THRESHOLD)
            {
                //Max power, the angle is above max power threshold (but under reverse threshold)
                return 1.0d;
            }
            else
            {
                //Reverse, the angle is above reverse threshold
                return -1.0d;
            }
        }

        public void Start()
        {

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable();

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;
            }
            else
            {
                throw new Exception("No Kinect was found");
            }

            // Starting the sensor!
            try
            {
                this.sensor.Start();
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error happend during Kinect startup", ex);
                throw ex;
            }
        }

        public void Stop()
        {
            if (this.sensor.IsRunning)
            {
                this.sensor.Stop();
                this.sensor = null;
            }
        }
    }
}

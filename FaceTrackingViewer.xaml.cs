// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceTrackingViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FaceTrackingBasics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.FaceTracking;

    using Point = System.Windows.Point;

    /// <summary>
    /// Class that uses the Face Tracking SDK to display a face mask for
    /// tracked skeletons
    /// </summary>
    public partial class FaceTrackingViewer : UserControl, IDisposable
    {
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect",
            typeof(KinectSensor),
            typeof(FaceTrackingViewer),
            new PropertyMetadata(
                null, (o, args) => ((FaceTrackingViewer)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        private const uint MaxMissedFrames = 100;

        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();

        private byte[] colorImage;

        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;

        private short[] depthImage;

        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        private bool disposed;

        private Skeleton[] skeletonData;
        private FaceWindow faceWindow;
        private Game currentGame;

        public FaceTrackingViewer()
        {
            this.InitializeComponent();
        }

        ~FaceTrackingViewer()
        {
            this.Dispose(false);
        }

        public KinectSensor Kinect
        {
            get
            {
                return (KinectSensor)this.GetValue(KinectProperty);
            }

            set
            {
                this.SetValue(KinectProperty, value);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.disposed = true;

                this.ResetFaceTracking();

            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {

                base.OnRender(drawingContext);
                foreach (SkeletonFaceTracker faceInformation in this.trackedSkeletons.Values)
                {
                    if (faceInformation.lastFaceTrackSucceeded && faceInformation.skeletonTrackingState == SkeletonTrackingState.Tracked)
                    {
                        faceWindow.setGameText();
                        if (currentGame.calculateLogic(faceInformation.facePoints, faceWindow.getDifficulty()))
                            faceWindow.incrementCounter();
                        faceInformation.DrawFaceModel(drawingContext);
                    }
                    else
                    {
                        faceWindow.setWaitText();
                    }

                    
                    if (this.disposed)
                        return;
                }
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            try
            {
                colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
                depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
                skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (this.depthImageFormat != depthImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.depthImage = null;
                    this.depthImageFormat = depthImageFrame.Format;
                }

                if (this.colorImageFormat != colorImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.colorImage = null;
                    this.colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (this.depthImage == null)
                {
                    this.depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (this.colorImage == null)
                {
                    this.colorImage = new byte[colorImageFrame.PixelDataLength];
                }

                // Get the skeleton information
                if (this.skeletonData == null || this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                colorImageFrame.CopyPixelDataTo(this.colorImage);
                depthImageFrame.CopyPixelDataTo(this.depthImage);
                skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                // Update the list of trackers and the trackers with the current frame information
                foreach (Skeleton skeleton in this.skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                        || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        // We want keep a record of any skeleton, tracked or untracked.
                        if (!this.trackedSkeletons.ContainsKey(skeleton.TrackingId))
                        {
                            this.trackedSkeletons.Add(skeleton.TrackingId, new SkeletonFaceTracker());
                        }

                        // Give each tracker the upated frame.
                        SkeletonFaceTracker skeletonFaceTracker;
                        if (this.trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                        {
                            skeletonFaceTracker.OnFrameReady(this.Kinect, colorImageFormat, colorImage, depthImageFormat, depthImage, skeleton);
                            skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                        }
                    }
                }

                this.RemoveOldTrackers(skeletonFrame.FrameNumber);

                this.InvalidateVisual();
            }
            finally
            {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }

                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        private void OnSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
                this.ResetFaceTracking();
            }

            if (newSensor != null)
            {
                newSensor.AllFramesReady += this.OnAllFramesReady;
            }
        }

        /// <summary>
        /// Clear out any trackers for skeletons we haven't heard from for a while
        /// </summary>
        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(this.trackedSkeletons.Keys))
            {
                this.RemoveTracker(trackingId);
            }
        }

        private class SkeletonFaceTracker : IDisposable
        {
            public EnumIndexableCollection<FeaturePoint, PointF> facePoints;

            private FaceTracker faceTracker;

            public bool lastFaceTrackSucceeded;

            public SkeletonTrackingState skeletonTrackingState;


            private FeaturePoint[] leftBrow = {
                FeaturePoint.LeftOfLeftEyebrow,
                FeaturePoint.MiddleTopOfLeftEyebrow,
                FeaturePoint.RightOfLeftEyebrow,
                FeaturePoint.MiddleBottomOfLeftEyebrow
                                      };

            private FeaturePoint[] rightBrow = {
                FeaturePoint.LeftOfRightEyebrow,
                FeaturePoint.MiddleBottomOfRightEyebrow,
                FeaturePoint.RightOfRightEyebrow,
                FeaturePoint.MiddleTopOfRightEyebrow
                                       };

            private FeaturePoint[] rightEye = {
                FeaturePoint.AboveThreeFourthRightEyelid,
                FeaturePoint.AboveMidUpperRightEyelid,
                FeaturePoint.AboveOneFourthRightEyelid,
                FeaturePoint.InnerCornerRightEye,
                FeaturePoint.BelowThreeFourthRightEyelid,
                FeaturePoint.OuterCornerOfRightEye
                                      };

            private FeaturePoint[] leftEye = {
                FeaturePoint.AboveThreeFourthLeftEyelid,
                FeaturePoint.AboveMidUpperLeftEyelid,
                FeaturePoint.AboveOneFourthLeftEyelid,
                FeaturePoint.InnerCornerLeftEye,
                FeaturePoint.BelowThreeFourthLeftEyelid,
                FeaturePoint.OuterCornerOfLeftEye
                                     };

            private FeaturePoint[] lips = {
                FeaturePoint.OutsideLeftCornerMouth,
                FeaturePoint.LeftBottomLowerLip,
                FeaturePoint.MiddleBottomLip,
                FeaturePoint.RightBottomLowerLip,
                FeaturePoint.OutsideRightCornerMouth,
                FeaturePoint.RightTopUpperLip,
                FeaturePoint.RightTopDipUpperLip,
                FeaturePoint.MiddleTopDipUpperLip,
                FeaturePoint.LeftTopDipUpperLip,
                FeaturePoint.LeftTopUpperLip
                                          };

            private FeaturePoint[] innerLips = {
                FeaturePoint.RightCornerMouth,
                FeaturePoint.LeftTopLowerLip,
                FeaturePoint.MiddleTopLowerLip,
                FeaturePoint.RightTopLowerLip,
                FeaturePoint.LeftCornerMouth,
                FeaturePoint.RightBottomUpperLip,
                FeaturePoint.MiddleBottomUpperLip,
                FeaturePoint.LeftBottomUpperLip                
                                               };

            private FeaturePoint[] head = {
                FeaturePoint.ContourBottomChin,
                FeaturePoint.ContourLeftChin,
                FeaturePoint.ContourLeftMouth,
                FeaturePoint.ContourLeftCheek,
                FeaturePoint.ContourLeftEye,
                FeaturePoint.ContourLeftTemple,
                FeaturePoint.ContourLeftHair,
                FeaturePoint.ContourLeftTop,
                FeaturePoint.ContourTopSkull,
                FeaturePoint.ContourRightTop,
                FeaturePoint.ContourRightHair,
                FeaturePoint.ContourRightTemple,
                FeaturePoint.ContourRightEye,
                FeaturePoint.ContourRightCheek,
                FeaturePoint.ContourRightMouth,
                FeaturePoint.ContourRightChin
                                          };


            private FeaturePoint[] nose = {
                FeaturePoint.UnderNoseLeftCorner,
                FeaturePoint.UnderNoseLeft,
                FeaturePoint.UnderNoseMiddle,
                FeaturePoint.UnderNoseRight,
                FeaturePoint.UnderNoseRightCorner,
                FeaturePoint.NoseRightBottomCorner,
                FeaturePoint.NoseRightSide,
                FeaturePoint.NoseRightTop,
                FeaturePoint.NoseLeftTop,
                FeaturePoint.NoseLeftSide,
                FeaturePoint.NoseLeftBottomCorner
                                           };

            private FeaturePoint[] noseBottom = {
                FeaturePoint.UnderNoseLeftCorner,
                FeaturePoint.UnderNoseLeft,
                FeaturePoint.UnderNoseMiddle,
                FeaturePoint.UnderNoseRight,
                FeaturePoint.UnderNoseRightCorner,
                FeaturePoint.NoseRightBottom,
                FeaturePoint.NoseTip,
                FeaturePoint.NoseLeftBottom
                                                };

            public int LastTrackedFrame { get; set; }

            public void Dispose()
            {
                if (this.faceTracker != null)
                {
                    this.faceTracker.Dispose();
                    this.faceTracker = null;
                }
            }



            private GeometryGroup connectPoints(List<Point> points)
            {
                GeometryGroup group = new GeometryGroup();
                for (int i = 0; i < points.Count - 1; i++)
                {
                    group.Children.Add(new LineGeometry(points[i], points[i + 1]));
                }
                group.Children.Add(new LineGeometry(points[points.Count - 1], points[0]));
                return group;
            }

            private GeometryGroup createFacePart(FeaturePoint[] featurePoints)
            {
                List<Point> points = new List<Point>();
                foreach (FeaturePoint p in featurePoints)
                    points.Add(GameUtils.convertToPoint(p, facePoints));
                return connectPoints(points);
            }


            private Transform getImageTransform ()
            {
                Point p = GameUtils.convertToPoint(FeaturePoint.NoseTip, facePoints);
                ScaleTransform transform = new ScaleTransform();
                transform.CenterX=p.X;
                transform.CenterY=p.Y;
                transform.ScaleX=2;
                transform.ScaleY=2;
                return transform;
            }

            private void addFaceParts(GeometryGroup faceModelGroup)
            {
                faceModelGroup.Children.Add(createFacePart(leftBrow));
                faceModelGroup.Children.Add(createFacePart(rightBrow));

                faceModelGroup.Children.Add(createFacePart(leftEye));
                faceModelGroup.Children.Add(createFacePart(rightEye));

                faceModelGroup.Children.Add(createFacePart(lips));
                faceModelGroup.Children.Add(createFacePart(innerLips));

                faceModelGroup.Children.Add(createFacePart(head));
                faceModelGroup.Children.Add(createFacePart(nose));
                faceModelGroup.Children.Add(createFacePart(noseBottom));
            }

            public void DrawFaceModel(DrawingContext drawingContext)
            {
                GeometryGroup faceModelGroup = new GeometryGroup();
                addFaceParts(faceModelGroup);
             
                drawingContext.PushTransform(getImageTransform());
                drawingContext.DrawGeometry(Brushes.Black, new Pen(Brushes.Black, 1.0), faceModelGroup);
            }



            /// <summary>
            /// Updates the face tracking information for this skeleton
            /// </summary>
            internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
            {
                this.skeletonTrackingState = skeletonOfInterest.TrackingState;

                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    // nothing to do with an untracked skeleton.
                    return;
                }

                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(kinectSensor);
                    }
                    catch (InvalidOperationException)
                    {
                        // During some shutdown scenarios the FaceTracker
                        // is unable to be instantiated.  Catch that exception
                        // and don't track a face.
                        Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null)
                {
                    FaceTrackFrame frame = this.faceTracker.Track(
                        colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);

                    this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                    if (this.lastFaceTrackSucceeded)
                    {
                        this.facePoints = frame.GetProjected3DShape();
                    }
                }
            }
        }

        internal void setMainWindow(FaceWindow window)
        {
            faceWindow = window;
        }

        internal void setGame(Game game)
        {
            currentGame = game;
        }
    }
}
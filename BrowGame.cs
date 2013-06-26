using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class BrowGame : AbstractGame
    {
        private static string INSTRUCTIONS = "Ruszaj brwiami w górę i w dół";

        protected override double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double averageBrowLength = (GameUtils.getLength(FeaturePoint.LeftOfLeftEyebrow, FeaturePoint.RightOfLeftEyebrow, facePoints) +
                GameUtils.getLength(FeaturePoint.LeftOfRightEyebrow, FeaturePoint.RightOfRightEyebrow, facePoints)) / 2.0;
            double averageBrowToEyeDistance = (GameUtils.getLength(FeaturePoint.MiddleBottomOfLeftEyebrow, FeaturePoint.AboveMidUpperLeftEyelid, facePoints) +
                GameUtils.getLength(FeaturePoint.MiddleBottomOfRightEyebrow, FeaturePoint.AboveMidUpperRightEyelid, facePoints)) / 2.0;
            return averageBrowToEyeDistance / averageBrowLength;
        }

        public BrowGame(int _targetScore) : base (_targetScore, INSTRUCTIONS){}
    }
}

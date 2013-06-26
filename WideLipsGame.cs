using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class WideLipsGame : AbstractGame
    {
        private static string INSTRUCTIONS = "Szeroko otwieraj i zamykaj usta";
        private static double THRESHOLD = 0.2;

        public WideLipsGame(int targetScore) : base(targetScore, THRESHOLD, INSTRUCTIONS){}

        protected override double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double averageBrowLength = (GameUtils.getLength(FeaturePoint.LeftOfLeftEyebrow, FeaturePoint.RightOfLeftEyebrow, facePoints) +
                GameUtils.getLength(FeaturePoint.LeftOfRightEyebrow, FeaturePoint.RightOfRightEyebrow, facePoints)) / 2.0;
            double averageBrowToEyeDistance = (GameUtils.getLength(FeaturePoint.MiddleBottomOfLeftEyebrow, FeaturePoint.AboveMidUpperLeftEyelid, facePoints) +
                GameUtils.getLength(FeaturePoint.MiddleBottomOfRightEyebrow, FeaturePoint.AboveMidUpperRightEyelid, facePoints)) / 2.0;
            return averageBrowToEyeDistance / averageBrowLength;
        }
    }
}

using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class OpenMouthGame : AbstractGame
    {
        private static string INSTRUCTIONS = "Szeroko otwieraj i zamykaj usta";

        public OpenMouthGame(int targetScore) : base(targetScore, INSTRUCTIONS) { }

        protected override double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double normLength = GameUtils.getLength(FeaturePoint.UnderNoseMiddle, FeaturePoint.NoseTop, facePoints);
            double mouthHeight = GameUtils.getLength(FeaturePoint.MiddleTopDipUpperLip, FeaturePoint.MiddleBottomLip, facePoints);
            return mouthHeight / normLength;
        }
    }
}
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class OpenMouthGame : AbstractGame
    {
        private static string LOW_INSTRUCTIONS = "Szeroko otwórz usta";
        private static string HIGH_INSTRUCTIONS = "Zamknij usta";

        public OpenMouthGame(int targetScore) : base(targetScore, LOW_INSTRUCTIONS, HIGH_INSTRUCTIONS) { }

        protected override double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double normLength = GameUtils.getLength(FeaturePoint.UnderNoseMiddle, FeaturePoint.NoseTop, facePoints);
            double mouthHeight = GameUtils.getLength(FeaturePoint.MiddleTopDipUpperLip, FeaturePoint.MiddleBottomLip, facePoints);
            return mouthHeight / normLength;
        }
    }
}
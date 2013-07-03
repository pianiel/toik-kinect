using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class WideLipsGame : AbstractGame
    {
        private static string LOW_INSTRUCTIONS = "Uśmiechnij się szeroko";
        private static string HIGH_INSTRUCTIONS = "Zwęź usta";

        public WideLipsGame(int targetScore) : base(targetScore, LOW_INSTRUCTIONS, HIGH_INSTRUCTIONS){}

        protected override double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double normLength = GameUtils.getLength(FeaturePoint.AboveMidUpperLeftEyelid, FeaturePoint.AboveMidUpperRightEyelid, facePoints);
            double lipsLength = GameUtils.getLength(FeaturePoint.OutsideLeftCornerMouth, FeaturePoint.OutsideRightCornerMouth, facePoints);
            return lipsLength / normLength;
        }

        public override string getName()
        {
            return "Uśmiechanie się";
        }
    }
}

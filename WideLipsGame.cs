﻿using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class WideLipsGame : AbstractGame
    {
        private static string INSTRUCTIONS = "Uśmiechaj się szeroko i rób dzióbek";

        public WideLipsGame(int targetScore) : base(targetScore, INSTRUCTIONS){}

        protected override double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double normLength = GameUtils.getLength(FeaturePoint.AboveMidUpperLeftEyelid, FeaturePoint.AboveMidUpperRightEyelid, facePoints);
            double lipsLength = GameUtils.getLength(FeaturePoint.OutsideLeftCornerMouth, FeaturePoint.OutsideRightCornerMouth, facePoints);
            return lipsLength / normLength;
        }
    }
}

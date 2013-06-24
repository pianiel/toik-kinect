using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class LipsGame : AbstractGame
    {
        public LipsGame(int targetScore) : base(targetScore) { }

        public override bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty)
        {
            //TODO
            return true; ;
        }

    }
}

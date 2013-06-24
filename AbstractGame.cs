using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    abstract class AbstractGame : Game
    {
        private int targetScore;

        public int getTargetScore()
        {
            return targetScore;
        }

        public AbstractGame(int _targetScore)
        {
            targetScore = _targetScore;
        }

        public abstract bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty);

    }

}

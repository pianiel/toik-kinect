using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    public interface Game
    {
        bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty);

        int getTargetScore();
    }
}

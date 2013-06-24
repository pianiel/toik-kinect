using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Point = System.Windows.Point;

namespace FaceTrackingBasics
{
    class GameUtils
    {
        public static Point convertToPoint(FeaturePoint p,  EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            return new Point(facePoints[p].X, facePoints[p].Y);
        }


        public static double getLength(FeaturePoint a, FeaturePoint b, EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            Point p1 = convertToPoint(a, facePoints);
            Point p2 = convertToPoint(b, facePoints);
            return (Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)));
        }

    }
}

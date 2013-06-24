using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    class BrowGame : AbstractGame
    {
        private static readonly double browStateDifferenceThreshold = 0.2;

        private double previousBrowState = 0.0;

        private int targetScore;


        private double getBrowState(EnumIndexableCollection<FeaturePoint, PointF> facePoints)
        {
            double averageBrowLength = (GameUtils.getLength(FeaturePoint.LeftOfLeftEyebrow, FeaturePoint.RightOfLeftEyebrow, facePoints) +
                GameUtils.getLength(FeaturePoint.LeftOfRightEyebrow, FeaturePoint.RightOfRightEyebrow, facePoints)) / 2.0;
            double averageBrowToEyeDistance = (GameUtils.getLength(FeaturePoint.MiddleBottomOfLeftEyebrow, FeaturePoint.AboveMidUpperLeftEyelid, facePoints) +
                GameUtils.getLength(FeaturePoint.MiddleBottomOfRightEyebrow, FeaturePoint.AboveMidUpperRightEyelid, facePoints)) / 2.0;
            return averageBrowToEyeDistance / averageBrowLength;
        }

        public BrowGame(int _targetScore) : base (_targetScore)
        {}

        public override bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty)
        {
            double browState = getBrowState(facePoints);

            if (previousBrowState != 0.0)
            {
                if (Math.Abs(previousBrowState - browState) > browStateDifferenceThreshold * difficulty)
                {
                    previousBrowState = browState;
                    return true;
                }
            }
            else
                previousBrowState = browState;
            return false;
        }
    }
}

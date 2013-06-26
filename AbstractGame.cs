using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    abstract class AbstractGame : Game
    {
        private string instructions;
        private int targetScore;

        public int getTargetScore()
        {
            return targetScore;
        }

        public AbstractGame(int _targetScore, double threshold, string _instructions)
        {
            targetScore = _targetScore;
            stateDifferenceThreshold = threshold;
            instructions = _instructions;
        }

        public string getInstructions()
        {
            return instructions;
        }

        private double stateDifferenceThreshold;
        private double previousState = 0.0;

        protected abstract double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints);

        public bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty)
        {
            double state = getState(facePoints);

            if (previousState != 0.0)
            {
                if (Math.Abs(previousState - state) > stateDifferenceThreshold * difficulty)
                {
                    previousState = state;
                    return true;
                }
            }
            else
                previousState = state;
            return false;
        }

    }

}

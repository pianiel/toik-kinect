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

        public AbstractGame(int _targetScore, string _instructions)
        {
            targetScore = _targetScore;
            instructions = _instructions;
        }

        public string getInstructions()
        {
            return instructions;
        }

        private double previousState = 0.0;
        private double average;
        private double deviation = 0.0;
        private int measures = 0;
        private int initialStatesSkipCount = 4;

        protected abstract double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints);

        public bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty)
        {
            double state = getState(facePoints);
            measures++;
            if (measures == 1)
            {
                average = state;
            }
            else
            {
                double oldAverage = average;
                average = (state + (average * (measures - 1))) / measures;
                deviation = deviation + ((state - oldAverage) * (state - average));
            }

            if (previousState != 0.0)
            {
                if ((previousState < average && state > Math.Sqrt(deviation/measures)*difficulty + average)
                    ||
                        (previousState > average && state < average - Math.Sqrt(deviation/measures)*difficulty/2))
                {
                    previousState = state;
                    initialStatesSkipCount--;
                    if (initialStatesSkipCount > 0)
                        return false;
                    return true;
                }
            }
            else
                previousState = state;
            return false;
        }

    }

}

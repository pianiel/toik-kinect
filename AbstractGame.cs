using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingBasics
{
    abstract class AbstractGame : Game
    {
        private string instructions, lowInstructions, highInstructions;
        private int targetScore;

        public int getTargetScore()
        {
            return targetScore;
        }

        public AbstractGame(int _targetScore, string _lowInstructions, string _highInstructions)
        {
            targetScore = _targetScore;
            instructions = _highInstructions;
            lowInstructions = _lowInstructions;
            highInstructions = _highInstructions;
        }

        public string getInstructions()
        {
            return instructions;
        }


        private double minimum = Double.MaxValue, maximum = Double.MinValue;
        private double previousState = 0.0;
        private double average;
        private double deviation = 0.0;
        private int measures = 0;
        private int initialStatesSkipCount = 5;



        public double getDeviation()
        {
            if (measures != 0)
                return Math.Sqrt(deviation / measures);
            return 0;
        }
        public double getMinimum()
        {
            return minimum;
        }
        public double getMaximum()
        {
            return maximum;     
        }

        public abstract string getName();

        protected abstract double getState(EnumIndexableCollection<FeaturePoint, PointF> facePoints);

        private bool countPoint(double state)
        {
            previousState = state;
            initialStatesSkipCount--;
            if (initialStatesSkipCount > 0)
                return false;
            return true;
        }

        private bool countHighPoint(double state)
        {
            instructions = highInstructions;
            return countPoint(state);
        }

        private bool countLowPoint(double state)
        {
            instructions = lowInstructions;
            return countPoint(state);
        }

        private void updateMinAndMax(double state)
        {
            if (state < minimum)
                minimum = state;
            if (state > maximum)
                maximum = state;
        }

        public bool calculateLogic(EnumIndexableCollection<FeaturePoint, PointF> facePoints, double difficulty)
        {
            double state = getState(facePoints);
            updateMinAndMax(state);
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
                if (previousState < average && state > getDeviation() * difficulty + average)
                    return countHighPoint(state);
                else if (previousState > average && state < average - getDeviation() * difficulty / 2)
                    return countLowPoint(state);
            }
            else
                previousState = state;
            return false;
        }

    }

}

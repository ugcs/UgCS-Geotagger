using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FileParsers
{
    public static class Interpolator
    {
        public static int MaxTimeDifferenceMs = 1000;

        public static event Action<int> OnOneHundredLinesReplaced;

        public static List<IGeoCoordinates> SetPpkCorrectedCoordinates(List<IGeoCoordinates> csvCoordinates, List<IGeoCoordinates> ppkCoordinates, CancellationTokenSource token)
        {
            var correctedTraces = new List<IGeoCoordinates>();
            var min = ppkCoordinates.First().TimeInMs;
            var max = ppkCoordinates.Last().TimeInMs;
            var countOfReplacedLines = 0;
            foreach (var coordinates in csvCoordinates)
            {
                if (token.IsCancellationRequested)
                    break;
                if (coordinates.TimeInMs < min || coordinates.TimeInMs > max)
                    continue;
                var leftValue = ppkCoordinates.Last(c => c.TimeInMs <= coordinates.TimeInMs);
                var leftBorderIndex = ppkCoordinates.IndexOf(leftValue);
                leftBorderIndex -= leftBorderIndex == ppkCoordinates.Count - 1 ? 1 : 0;
                var rightBorderIndex = leftBorderIndex + 1;
                if (ppkCoordinates[rightBorderIndex].TimeInMs - ppkCoordinates[leftBorderIndex].TimeInMs > MaxTimeDifferenceMs)
                    continue;
                coordinates.Latitude = Interpolate(coordinates.TimeInMs, ppkCoordinates[leftBorderIndex].Latitude, ppkCoordinates[rightBorderIndex].Latitude, ppkCoordinates[rightBorderIndex].TimeInMs, ppkCoordinates[leftBorderIndex].TimeInMs);
                coordinates.Longitude = Interpolate(coordinates.TimeInMs, ppkCoordinates[leftBorderIndex].Longitude, ppkCoordinates[rightBorderIndex].Longitude, ppkCoordinates[rightBorderIndex].TimeInMs, ppkCoordinates[leftBorderIndex].TimeInMs);
                coordinates.Altitude = Interpolate(coordinates.TimeInMs, ppkCoordinates[leftBorderIndex].Altitude, ppkCoordinates[rightBorderIndex].Altitude, ppkCoordinates[rightBorderIndex].TimeInMs, ppkCoordinates[leftBorderIndex].TimeInMs);
                correctedTraces.Add(coordinates);
                countOfReplacedLines++;
                if (countOfReplacedLines % 100 == 0)
                    OnOneHundredLinesReplaced?.Invoke(countOfReplacedLines);
            }
            return correctedTraces;
        }

        private static double Interpolate(double argument, double leftBorderValue, double rightBorderValue, double leftBorder, double rightBorder)
        {
            return leftBorderValue + (rightBorderValue - leftBorderValue) / (rightBorder - leftBorder) * (argument - leftBorder);
        }
    }
}
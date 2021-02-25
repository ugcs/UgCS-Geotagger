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

        public static List<IGeoCoordinates> CreatePpkCorrectedCoordinates(List<IGeoCoordinates> ftuCoordinates, List<IGeoCoordinates> psfCoordinates, int timeOffset, CancellationTokenSource token)
        {
            var correctedTraces = new List<IGeoCoordinates>();
            if (timeOffset != 0)
            {
                foreach (var c in psfCoordinates)
                    c.TimeInMs += timeOffset;
            }
            var min = psfCoordinates.First().TimeInMs;
            var max = psfCoordinates.Last().TimeInMs;
            var countOfReplacedLines = 0;
            foreach (var coordinates in ftuCoordinates)
            {
                if (token.IsCancellationRequested)
                    break;
                if (coordinates.TimeInMs < min || coordinates.TimeInMs > max)
                    continue;
                var leftValue = psfCoordinates.Last(c => c.TimeInMs <= coordinates.TimeInMs);
                var leftBorderIndex = psfCoordinates.IndexOf(leftValue);
                leftBorderIndex -= leftBorderIndex == psfCoordinates.Count - 1 ? 1 : 0;
                var rightBorderIndex = leftBorderIndex + 1;
                if (psfCoordinates[rightBorderIndex].TimeInMs - psfCoordinates[leftBorderIndex].TimeInMs > MaxTimeDifferenceMs)
                    continue;
                coordinates.Latitude = Interpolate(coordinates.TimeInMs, psfCoordinates[leftBorderIndex].Latitude, psfCoordinates[rightBorderIndex].Latitude, psfCoordinates[rightBorderIndex].TimeInMs, psfCoordinates[leftBorderIndex].TimeInMs);
                coordinates.Longitude = Interpolate(coordinates.TimeInMs, psfCoordinates[leftBorderIndex].Longitude, psfCoordinates[rightBorderIndex].Longitude, psfCoordinates[rightBorderIndex].TimeInMs, psfCoordinates[leftBorderIndex].TimeInMs);
                coordinates.Altitude = Interpolate(coordinates.TimeInMs, psfCoordinates[leftBorderIndex].Altitude, psfCoordinates[rightBorderIndex].Altitude, psfCoordinates[rightBorderIndex].TimeInMs, psfCoordinates[leftBorderIndex].TimeInMs);
                correctedTraces.Add(coordinates);
                countOfReplacedLines++;
                if (countOfReplacedLines % 100 == 0)
                    OnOneHundredLinesReplaced?.Invoke(countOfReplacedLines);
            }

            if (timeOffset != 0)
            {
                foreach (var c in psfCoordinates)
                    c.TimeInMs -= timeOffset;
            }
            return correctedTraces;
        }

        private static double Interpolate(double argument, double leftBorderValue, double rightBorderValue, double leftBorder, double rightBorder)
        {
            return leftBorderValue + (rightBorderValue - leftBorderValue) / (rightBorder - leftBorder) * (argument - leftBorder);
        }
    }
}
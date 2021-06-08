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

        public static List<IGeoCoordinates> CreateCorrectedCoordinates(List<IGeoCoordinates> ftuCoordinates, List<IGeoCoordinates> psfCoordinates, int timeOffset, CancellationTokenSource token)
        {
            var correctedTraces = new List<IGeoCoordinates>();
            if (timeOffset != 0)
            {
                foreach (var c in psfCoordinates)
                    c.TimeInMs += timeOffset;
            }
            var min = psfCoordinates.Min(c => c.TimeInMs);
            var max = psfCoordinates.Max(c => c.TimeInMs);
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
                if (coordinates.TimeInMs.HasValue && psfCoordinates[leftBorderIndex].Latitude.HasValue && psfCoordinates[rightBorderIndex].Latitude.HasValue
                    && coordinates.TimeInMs.HasValue && psfCoordinates[leftBorderIndex].Longitude.HasValue && psfCoordinates[rightBorderIndex].Longitude.HasValue && psfCoordinates[rightBorderIndex].TimeInMs.HasValue && psfCoordinates[leftBorderIndex].TimeInMs.HasValue)
                {
                    coordinates.Latitude = Interpolate(coordinates.TimeInMs.Value, psfCoordinates[leftBorderIndex].Latitude.Value, psfCoordinates[rightBorderIndex].Latitude.Value, psfCoordinates[leftBorderIndex].TimeInMs.Value, psfCoordinates[rightBorderIndex].TimeInMs.Value);
                    coordinates.Longitude = Interpolate(coordinates.TimeInMs.Value, psfCoordinates[leftBorderIndex].Longitude.Value, psfCoordinates[rightBorderIndex].Longitude.Value, psfCoordinates[leftBorderIndex].TimeInMs.Value, psfCoordinates[rightBorderIndex].TimeInMs.Value);
                }
                else
                    continue;
                    
                if (coordinates.TimeInMs.HasValue && psfCoordinates[leftBorderIndex].Altitude.HasValue && psfCoordinates[rightBorderIndex].Altitude.HasValue && psfCoordinates[leftBorderIndex].TimeInMs.HasValue && psfCoordinates[rightBorderIndex].TimeInMs.HasValue)
                    coordinates.Altitude = Interpolate(coordinates.TimeInMs.Value, psfCoordinates[leftBorderIndex].Altitude.Value, psfCoordinates[rightBorderIndex].Altitude.Value, psfCoordinates[leftBorderIndex].TimeInMs.Value, psfCoordinates[rightBorderIndex].TimeInMs.Value);
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
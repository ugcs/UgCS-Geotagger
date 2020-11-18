using System.Collections.Generic;
using System.Linq;

namespace FileParsers
{
    public static class Interpolator
    {
        public static List<GeoCoordinates> SetPpkCorrectedCoordinates(List<GeoCoordinates> csvCoordinates, List<GeoCoordinates> ppkCoordinates)
        {
            var correctedTraces = new List<GeoCoordinates>();
            foreach (var coordinates in csvCoordinates)
            {
                if (coordinates.TimeInMs < ppkCoordinates.First().TimeInMs || coordinates.TimeInMs > ppkCoordinates.Last().TimeInMs)
                    continue;
                var leftValue = ppkCoordinates.Last(c => c.TimeInMs <= coordinates.TimeInMs);
                var leftBorderIndex = ppkCoordinates.IndexOf(leftValue);
                leftBorderIndex -= leftBorderIndex == ppkCoordinates.Count - 1 ? 1 : 0;
                var rightBorderIndex = leftBorderIndex + 1;
                var correctedLat = Interpolate(coordinates.TimeInMs, ppkCoordinates[leftBorderIndex].Latitude, ppkCoordinates[rightBorderIndex].Latitude, ppkCoordinates[rightBorderIndex].TimeInMs, ppkCoordinates[leftBorderIndex].TimeInMs);
                var correctedLon = Interpolate(coordinates.TimeInMs, ppkCoordinates[leftBorderIndex].Longitude, ppkCoordinates[rightBorderIndex].Longitude, ppkCoordinates[rightBorderIndex].TimeInMs, ppkCoordinates[leftBorderIndex].TimeInMs);
                correctedTraces.Add(new GeoCoordinates(coordinates.DateTime, correctedLat, correctedLon, coordinates.TraceNumber));
            }
            return correctedTraces;
        }

        private static double Interpolate(double argument, double leftBorderValue, double rightBorderValue, double leftBorder, double rightBorder)
        {
            return leftBorderValue + (rightBorderValue - leftBorderValue) / (rightBorder - leftBorder) * (argument - leftBorder);
        }
    }
}
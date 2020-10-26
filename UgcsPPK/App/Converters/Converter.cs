﻿using Avalonia.Data.Converters;
using System;
using System.Globalization;
using UgCSPPK.Models;

namespace UgCSPPK.Converters
{
    public abstract class Converter : IValueConverter
    {
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReverseBoolConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }

    public class CoveringStatusConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = "Not Covered";
            switch ((CoveringStatus)value)
            {
                case CoveringStatus.Covered:
                    result = "Covered";
                    break;

                case CoveringStatus.PartiallyCovered:
                    result = "Partially Covered";
                    break;

                case CoveringStatus.NotCovered:
                    result = "Not Covered";
                    break;
            }
            return result;
        }
    }
}
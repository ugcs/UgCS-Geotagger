using Avalonia.Data.Converters;
using Avalonia.Input;
using System;
using System.Globalization;
using System.IO;
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

    public class FileNameConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileName((string)value);
        }
    }

    public class ProcessFilesButtonTextConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? "Cancel" : "Process Files";
        }
    }

    public class CursorConverter : Converter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? StandardCursorType.Wait : StandardCursorType.Arrow;
        }
    }
}
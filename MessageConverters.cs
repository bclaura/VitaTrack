using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace VitaTrack
{
    public class MessageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isOwn = (bool)value;
            return isOwn ? Color.FromArgb("#DCF8C6") : Color.FromArgb("#FFFFFF");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (bool)value ? LayoutOptions.End : LayoutOptions.Start;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }

    public class ReadIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isRead = (bool)value;
            return isRead ? "read_blue_icon.png" : "read_grey_icon.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}

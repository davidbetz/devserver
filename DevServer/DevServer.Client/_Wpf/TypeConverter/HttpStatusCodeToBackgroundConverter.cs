using System;
using System.Windows.Data;
using System.Windows.Media;
//+
namespace DevServer.Client.TypeConverter
{
    internal class HttpStatusCodeToBackgroundConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new InvalidOperationException("Target type must be of type Brush");
            }
            //+
            Int32 statusCode = (Int32)value;
            LinearGradientBrush brush = new LinearGradientBrush();
            if (statusCode == 200)
            {
                return Brushes.Transparent;
            }
            else if (statusCode >= 500)
            {
                brush.GradientStops.Add(new GradientStop(Brushes.Red.Color, 0));
            }
            else if (statusCode >= 400)
            {
                brush.GradientStops.Add(new GradientStop(Brushes.Yellow.Color, 0));
            }
            else if (statusCode == 404)
            {
                brush.GradientStops.Add(new GradientStop(Brushes.Blue.Color, 0));
            }
            else if (statusCode == 301 || statusCode == 302)
            {
                brush.GradientStops.Add(new GradientStop(Brushes.Orange.Color, 0));
            }
            else
            {
                brush.GradientStops.Add(new GradientStop(Brushes.Aqua.Color, 0));
            }
            return brush;
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
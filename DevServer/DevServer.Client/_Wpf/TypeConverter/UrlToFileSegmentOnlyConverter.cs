using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    public class UrlToFileSegmentOnlyConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
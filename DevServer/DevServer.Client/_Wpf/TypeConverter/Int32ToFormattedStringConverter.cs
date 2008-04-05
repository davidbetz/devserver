using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    public class Int32ToFormattedStringConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            Int32 number = (Int32)value;
            if (number > 0)
            {
                return String.Format("{0:0,0}", number);
            }
            else
            {
                return "0";
            }
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
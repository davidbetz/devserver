using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    internal class VerboseTypeTracingToTextConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            Boolean enabled = (Boolean)value;
            if (enabled)
            {
                return "Disable Secondary Tracing";
            }
            else
            {
                return "Enable Secondary Tracing";
            }
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
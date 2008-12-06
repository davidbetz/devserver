using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    internal class TraceStatusToTextConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return (Boolean)value ? "Disable Tracing" : "Enable Tracing";
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
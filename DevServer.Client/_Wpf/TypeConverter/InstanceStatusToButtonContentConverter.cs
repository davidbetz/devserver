using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    internal class InstanceStatusToButtonContentConverter : IValueConverter
    {
        //- @Convert -//
        public Object Convert(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            DevServer.Instance.InstanceState state = (DevServer.Instance.InstanceState)value;
            switch (state)
            {
                case DevServer.Instance.InstanceState.Started:
                    return "Stop";
                case DevServer.Instance.InstanceState.Stopped:
                    return "Start";
                default:
                    throw new ArgumentOutOfRangeException("Unknown InstanceState value");
            }
        }

        //- @ConvertBack -//
        public Object ConvertBack(Object value, Type targetType, Object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
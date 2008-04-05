﻿using System;
using System.Windows.Data;
//+
namespace DevServer.Client.TypeConverter
{
    public class InstanceStatusToTextConverter : IValueConverter
    {
        //- @Convert -//
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DevServer.Instance.InstanceState state = (DevServer.Instance.InstanceState)value;
            switch (state)
            {
                case DevServer.Instance.InstanceState.Started:
                    return "Started";
                case DevServer.Instance.InstanceState.Stopped:
                    return "Stopped";
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
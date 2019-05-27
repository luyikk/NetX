using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Xamarin.Forms;

namespace ChaTRoomApp.Converter
{
    public class SelectionIdToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imgId)
            {
                Assembly assembly = typeof(SelectionIdToImageConverter).GetTypeInfo().Assembly;

                switch (imgId)
                {
                    case "room":
                        return ImageSource.FromResource("ChaTRoomApp.Icon.Room.png", assembly);
                    case "role":
                        return ImageSource.FromResource("ChaTRoomApp.Icon.User.png", assembly);
                }

              
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

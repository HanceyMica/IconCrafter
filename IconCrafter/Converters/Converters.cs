using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IconCrafter
{
    /// <summary>
    /// 转换器集合类
    /// </summary>
    public static class Converters
    {
        /// <summary>
        /// 布尔值取反转换器
        /// </summary>
        public static readonly IValueConverter InverseBooleanConverter = new InverseBooleanValueConverter();
        
        /// <summary>
        /// 布尔值到可见性转换器
        /// </summary>
        public static readonly IValueConverter BooleanToVisibilityConverter = new BooleanToVisibilityValueConverter();
    }
    
    /// <summary>
    /// 布尔值取反转换器实现
    /// </summary>
    public class InverseBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            
            return false;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            
            return false;
        }
    }
    
    /// <summary>
    /// 布尔值到可见性转换器实现
    /// </summary>
    public class BooleanToVisibilityValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            
            return Visibility.Collapsed;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            
            return false;
        }
    }
}
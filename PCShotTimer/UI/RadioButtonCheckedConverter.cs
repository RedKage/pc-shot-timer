using System;
using System.Globalization;
using System.Windows.Data;

namespace PCShotTimer.UI
{
    /// <summary>
    ///     RadioButton Converter.
    /// </summary>
    public class RadioButtonCheckedConverter : IValueConverter
    {
        /// <summary>
        ///     Convert the RadioButton value to a True/False string.
        /// </summary>
        /// <param name="radioButton">The RadioButton object.</param>
        /// <param name="targetType">TODO ??</param>
        /// <param name="value">The RadioButton value.</param>
        /// <param name="culture">Culture to use.</param>
        /// <returns>Converted value.</returns>
        public object Convert(object radioButton, Type targetType, object value, CultureInfo culture)
        {
            return radioButton.Equals(value);
        }

        /// <summary>
        ///     Convert a True/False string to a RadioButton value.
        /// </summary>
        /// <param name="value">The RadioButton value.</param>
        /// <param name="targetType">TODO ??</param>
        /// <param name="radioButton">The RadioButton object.</param>
        /// <param name="culture">Culture to use.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object radioButton, CultureInfo culture)
        {
            return value.Equals(true)
                ? radioButton
                : Binding.DoNothing;
        }
    }
}
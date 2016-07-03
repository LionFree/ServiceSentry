// -- FILE ------------------------------------------------------------------
// name       : ConverterGridViewColumn.cs
// created    : Jani Giannoudis - 2008.03.27
// language   : c#
// environment: .NET 3.0
// copyright  : (c) 2008-2012 by Itenso GmbH, Switzerland
// --------------------------------------------------------------------------

#region References

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public abstract class ConverterGridViewColumn : GridViewColumn, IValueConverter
    {
        private readonly Type _bindingType;

        protected ConverterGridViewColumn(Type bindingType)
        {
            if (bindingType == null)
            {
                throw new ArgumentNullException("bindingType");
            }

            _bindingType = bindingType;
            DisplayMemberBinding = new Binding { Mode = BindingMode.OneWay, Converter = this };
        }

        public Type BindingType
        {
            get { return _bindingType; }
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!_bindingType.IsInstanceOfType(value))
            {
                throw new InvalidOperationException();
            }
            return Convert(value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!_bindingType.IsInstanceOfType(value))
            {
                throw new InvalidOperationException();
            }
            return ConvertBack(value);
        }

        protected abstract object Convert(object value);

        protected abstract object ConvertBack(object value);
    }
}
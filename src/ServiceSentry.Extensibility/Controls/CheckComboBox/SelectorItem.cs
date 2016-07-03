#region References

using System.Windows;
using System.Windows.Controls;

#endregion

namespace ServiceSentry.Extensibility.Controls
{
    public class SelectorItem : ContentControl
    {
        #region Constructors

        static SelectorItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SelectorItem),
                                                     new FrameworkPropertyMetadata(typeof (SelectorItem)));
        }

        #endregion //Constructors

        #region Properties

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
                                                                                                   typeof (bool),
                                                                                                   typeof (SelectorItem),
                                                                                                   new UIPropertyMetadata
                                                                                                       (false,
                                                                                                        OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool) GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        internal CheckComboBox ParentSelector
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this) as CheckComboBox; }
        }

        private static void OnIsSelectedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var selectorItem = o as SelectorItem;
            if (selectorItem != null)
                selectorItem.OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnIsSelectedChanged(bool oldValue, bool newValue)
        {
            RaiseEvent(newValue
                           ? new RoutedEventArgs(CheckComboBox.SelectedEvent, this)
                           : new RoutedEventArgs(CheckComboBox.UnSelectedEvent, this));
        }

        #endregion //Properties

        #region Events

        public static readonly RoutedEvent SelectedEvent = CheckComboBox.SelectedEvent.AddOwner(typeof (SelectorItem));

        public static readonly RoutedEvent UnselectedEvent =
            CheckComboBox.UnSelectedEvent.AddOwner(typeof (SelectorItem));

        #endregion
    }
}
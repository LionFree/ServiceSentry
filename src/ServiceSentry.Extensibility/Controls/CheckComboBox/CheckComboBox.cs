using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ServiceSentry.Extensibility.Controls
{
    [TemplatePart(Name = PartPopup, Type = typeof (Popup))]
    public class CheckComboBox : ItemsControl, IWeakEventListener
    {
        private const string PartPopup = "PART_Popup";

        #region Fields

        private readonly ValueChangeHelper _displayMemberPathValuesChangeHelper;
        private readonly List<object> _initialValue = new List<object>();
        private readonly ValueChangeHelper _selectedMemberPathValuesHelper;
        private readonly ValueChangeHelper _valueMemberPathValuesHelper;
        private bool _ignoreSelectedItemChanged;
        private int _ignoreSelectedItemsCollectionChanged;
        private int _ignoreSelectedMemberPathValuesChanged;
        private bool _ignoreSelectedValueChanged;
        private Popup _popup;
        private IList _selectedItems;
        private bool _surpressItemSelectionChanged;

        #endregion

        #region Constructors

        static CheckComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (CheckComboBox),
                                                     new FrameworkPropertyMetadata(typeof (CheckComboBox)));
        }

        public CheckComboBox()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
            _displayMemberPathValuesChangeHelper = new ValueChangeHelper(OnDisplayMemberPathValuesChanged);


            SelectedItems = new ObservableCollection<object>();
            AddHandler(SelectedEvent, new RoutedEventHandler((s, args) => OnItemSelectionChangedCore(args, false)));
            AddHandler(UnSelectedEvent, new RoutedEventHandler((s, args) => OnItemSelectionChangedCore(args, true)));
            _selectedMemberPathValuesHelper = new ValueChangeHelper(OnSelectedMemberPathValuesChanged);
            _valueMemberPathValuesHelper = new ValueChangeHelper(OnValueMemberPathValuesChanged);
        }

        #endregion //Constructors

        #region Properties

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                                typeof (ICommand),
                                                                                                typeof (CheckComboBox),
                                                                                                new PropertyMetadata(
                                                                                                    (ICommand) null));

        [TypeConverter(typeof (CommandConverter))]
        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion

        #region Delimiter

        public static readonly DependencyProperty DelimiterProperty = DependencyProperty.Register("Delimiter",
                                                                                                  typeof (string),
                                                                                                  typeof (CheckComboBox),
                                                                                                  new UIPropertyMetadata
                                                                                                      (",",
                                                                                                       OnDelimiterChanged));

        public string Delimiter
        {
            get { return (string) GetValue(DelimiterProperty); }
            set { SetValue(DelimiterProperty, value); }
        }

        private static void OnDelimiterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ((CheckComboBox) o).UpdateSelectedValue();
        }

        #endregion

        #region SelectedItem property

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem",
                                                                                                     typeof (object),
                                                                                                     typeof (
                                                                                                         CheckComboBox),
                                                                                                     new UIPropertyMetadata
                                                                                                         (null,
                                                                                                          OnSelectedItemChanged));

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((CheckComboBox) sender).OnSelectedItemChanged(args.OldValue, args.NewValue);
        }

        protected virtual void OnSelectedItemChanged(object oldValue, object newValue)
        {
            if (!IsInitialized || _ignoreSelectedItemChanged)
                return;

            _ignoreSelectedItemsCollectionChanged++;
            SelectedItems.Clear();
            if (newValue != null)
            {
                SelectedItems.Add(newValue);
            }
            UpdateFromSelectedItems();
            _ignoreSelectedItemsCollectionChanged--;
        }

        #endregion

        #region SelectedItems Property

        public IList SelectedItems
        {
            get { return _selectedItems; }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                var oldCollection = _selectedItems as INotifyCollectionChanged;
                var newCollection = value as INotifyCollectionChanged;

                if (oldCollection != null)
                {
                    CollectionChangedEventManager.RemoveListener(oldCollection, this);
                }

                if (newCollection != null)
                {
                    CollectionChangedEventManager.AddListener(newCollection, this);
                }

                _selectedItems = value;
            }
        }

        #endregion SelectedItems

        #region Text

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof (string),
                                                                                             typeof (CheckComboBox),
                                                                                             new UIPropertyMetadata(null));

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        #endregion

        #region IsDropDownOpen

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof (bool), typeof (CheckComboBox),
            new UIPropertyMetadata(false, OnIsDropDownOpenChanged));

        public bool IsDropDownOpen
        {
            get { return (bool) GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        private static void OnIsDropDownOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = o as CheckComboBox;
            if (comboBox != null)
                comboBox.OnIsDropDownOpenChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnIsDropDownOpenChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                _initialValue.Clear();
                foreach (var o in SelectedItems)
                    _initialValue.Add(o);
            }
            else
            {
                _initialValue.Clear();
            }
        }

        #endregion //IsDropDownOpen

        #region MaxDropDownHeight

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register("MaxDropDownHeight", typeof (double), typeof (CheckComboBox),
                                        new UIPropertyMetadata(SystemParameters.PrimaryScreenHeight/3.0,
                                                               OnMaxDropDownHeightChanged));

        public double MaxDropDownHeight
        {
            get { return (double) GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        private static void OnMaxDropDownHeightChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var comboBox = o as CheckComboBox;
            if (comboBox != null)
                comboBox.OnMaxDropDownHeightChanged((double) e.OldValue, (double) e.NewValue);
        }

        protected virtual void OnMaxDropDownHeightChanged(double oldValue, double newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion

        #region SelectedItemsOverride property

        public static readonly DependencyProperty SelectedItemsOverrideProperty =
            DependencyProperty.Register("SelectedItemsOverride", typeof (IList), typeof (CheckComboBox),
                                        new UIPropertyMetadata(null, SelectedItemsOverrideChanged));

        public IList SelectedItemsOverride
        {
            get { return (IList) GetValue(SelectedItemsOverrideProperty); }
            set { SetValue(SelectedItemsOverrideProperty, value); }
        }

        private static void SelectedItemsOverrideChanged(DependencyObject sender,
                                                         DependencyPropertyChangedEventArgs args)
        {
            ((CheckComboBox) sender).OnSelectedItemsOverrideChanged((IList) args.OldValue, (IList) args.NewValue);
        }

        protected virtual void OnSelectedItemsOverrideChanged(IList oldValue, IList newValue)
        {
            if (!IsInitialized)
                return;

            SelectedItems = newValue ?? new ObservableCollection<object>();
            UpdateFromSelectedItems();
        }

        #endregion

        #region SelectedMemberPath Property

        public static readonly DependencyProperty SelectedMemberPathProperty =
            DependencyProperty.Register("SelectedMemberPath", typeof (string), typeof (CheckComboBox),
                                        new UIPropertyMetadata(null, OnSelectedMemberPathChanged));

        public string SelectedMemberPath
        {
            get { return (string) GetValue(SelectedMemberPathProperty); }
            set { SetValue(SelectedMemberPathProperty, value); }
        }

        private static void OnSelectedMemberPathChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sel = ((CheckComboBox) o);
            sel.OnSelectedMemberPathChanged((string) e.OldValue, (string) e.NewValue);
        }

        protected virtual void OnSelectedMemberPathChanged(string oldValue, string newValue)
        {
            if (!IsInitialized)
                return;

            UpdateSelectedMemberPathValuesBindings();
        }

        #endregion //SelectedMemberPath

        #region SelectedValue

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue",
                                                                                                      typeof (string),
                                                                                                      typeof (
                                                                                                          CheckComboBox),
                                                                                                      new FrameworkPropertyMetadata
                                                                                                          (null,
                                                                                                           FrameworkPropertyMetadataOptions
                                                                                                               .BindsTwoWayByDefault,
                                                                                                           OnSelectedValueChanged));

        public string SelectedValue
        {
            get { return (string) GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        private static void OnSelectedValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var selector = o as CheckComboBox;
            if (selector != null)
                selector.OnSelectedValueChanged((string) e.OldValue, (string) e.NewValue);
        }

        protected virtual void OnSelectedValueChanged(string oldValue, string newValue)
        {
            UpdateText();

            if (!IsInitialized || _ignoreSelectedValueChanged)
                return;

            UpdateFromSelectedValue();
        }

        #endregion //SelectedValue

        #region ValueMemberPath

        public static readonly DependencyProperty ValueMemberPathProperty =
            DependencyProperty.Register("ValueMemberPath", typeof (string), typeof (CheckComboBox),
                                        new UIPropertyMetadata(OnValueMemberPathChanged));

        public string ValueMemberPath
        {
            get { return (string) GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        private static void OnValueMemberPathChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var sel = ((CheckComboBox) o);
            sel.OnValueMemberPathChanged((string) e.OldValue, (string) e.NewValue);
        }

        protected virtual void OnValueMemberPathChanged(string oldValue, string newValue)
        {
            if (!IsInitialized)
                return;

            UpdateValueMemberPathValuesBindings();
        }

        #endregion

        #region ItemsCollection Property

        protected IEnumerable ItemsCollection
        {
            get { return ItemsSource ?? (Items ?? (IEnumerable) new object[0]); }
        }

        #endregion

        #endregion //Properties

        #region Base Class Overrides

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is SelectorItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new SelectorItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            _surpressItemSelectionChanged = true;
            var selectorItem = element as FrameworkElement;

            if (selectorItem == null) return;
            selectorItem.SetValue((DependencyProperty) SelectorItem.IsSelectedProperty, SelectedItems.Contains(item));

            _surpressItemSelectionChanged = false;
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            var oldCollection = oldValue as INotifyCollectionChanged;
            var newCollection = newValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                CollectionChangedEventManager.RemoveListener(oldCollection, this);
            }

            if (newCollection != null)
            {
                CollectionChangedEventManager.AddListener(newCollection, this);
            }

            if (!IsInitialized)
                return;

            RemoveUnavailableSelectedItems();
            UpdateSelectedMemberPathValuesBindings();
            UpdateValueMemberPathValuesBindings();
            UpdateDisplayMemberPathValuesBindings();
        }

        // When a DataTemplate includes a CheckComboBox, some bindings are
        // not working, like SelectedValue.
        // We use a priority system to select the good items after initialization.
        public override void EndInit()
        {
            base.EndInit();

            if (SelectedItemsOverride != null)
            {
                OnSelectedItemsOverrideChanged(null, SelectedItemsOverride);
            }
            else if (SelectedMemberPath != null)
            {
                OnSelectedMemberPathChanged(null, SelectedMemberPath);
            }
            else if (SelectedValue != null)
            {
                OnSelectedValueChanged(null, SelectedValue);
            }
            else if (SelectedItem != null)
            {
                OnSelectedItemChanged(null, SelectedItem);
            }

            if (ValueMemberPath != null)
            {
                OnValueMemberPathChanged(null, ValueMemberPath);
            }
        }

        protected override void OnDisplayMemberPathChanged(string oldDisplayMemberPath, string newDisplayMemberPath)
        {
            base.OnDisplayMemberPathChanged(oldDisplayMemberPath, newDisplayMemberPath);
            UpdateDisplayMemberPathValuesBindings();
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_popup != null)
                _popup.Opened -= Popup_Opened;

            _popup = GetTemplateChild(PartPopup) as Popup;

            if (_popup != null)
                _popup.Opened += Popup_Opened;
        }

        #endregion //Base Class Overrides

        #region Event Handlers

        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent("SelectedEvent",
                                                                                            RoutingStrategy.Bubble,
                                                                                            typeof (RoutedEventHandler),
                                                                                            typeof (CheckComboBox));

        public static readonly RoutedEvent UnSelectedEvent = EventManager.RegisterRoutedEvent("UnSelectedEvent",
                                                                                              RoutingStrategy.Bubble,
                                                                                              typeof (RoutedEventHandler
                                                                                                  ),
                                                                                              typeof (CheckComboBox));

        public static readonly RoutedEvent ItemSelectionChangedEvent =
            EventManager.RegisterRoutedEvent("ItemSelectionChanged", RoutingStrategy.Bubble,
                                             typeof (ItemSelectionChangedEventHandler), typeof (CheckComboBox));

        public event ItemSelectionChangedEventHandler ItemSelectionChanged
        {
            add { AddHandler(ItemSelectionChangedEvent, value); }
            remove { RemoveHandler(ItemSelectionChangedEvent, value); }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown(false);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsDropDownOpen)
            {
                if (KeyboardUtilities.IsKeyModifyingPopupState(e))
                {
                    IsDropDownOpen = true;
                    // Popup_Opened() will Focus on ComboBoxItem.
                    e.Handled = true;
                }
            }
            else
            {
                if (KeyboardUtilities.IsKeyModifyingPopupState(e))
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    SelectedItems.Clear();
                    foreach (var o in _initialValue)
                        SelectedItems.Add(o);
                    CloseDropDown(true);
                    e.Handled = true;
                }
            }
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            var item = ItemContainerGenerator.ContainerFromItem(SelectedItem) as UIElement;
            if ((item == null) && (Items.Count > 0))
                item = ItemContainerGenerator.ContainerFromItem(Items[0]) as UIElement;
            if (item != null)
                item.Focus();
        }

        #endregion //Event Handlers

        #region Methods

        protected object GetItemValue(object item)
        {
            if (!String.IsNullOrEmpty(ValueMemberPath) && (item != null))
            {
                var property = item.GetType().GetProperty(ValueMemberPath);
                if (property != null)
                    return property.GetValue(item, null);
            }

            return item;
        }

        protected object ResolveItemByValue(string value)
        {
            if (!String.IsNullOrEmpty(ValueMemberPath))
            {
                foreach (var item in ItemsCollection)
                {
                    var property = item.GetType().GetProperty(ValueMemberPath);
                    if (property != null)
                    {
                        var propertyValue = property.GetValue(item, null);
                        if (value.Equals(propertyValue.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            return item;
                    }
                }
            }

            return value;
        }

        private bool? GetSelectedMemberPathValue(object item)
        {
            var prop = GetSelectedMemberPathProperty(item);

            return (prop != null)
                       ? (bool) prop.GetValue(item, null)
                       : (bool?) null;
        }

        private void SetSelectedMemberPathValue(object item, bool value)
        {
            var prop = GetSelectedMemberPathProperty(item);

            if (prop != null)
            {
                prop.SetValue(item, value, null);
            }
        }

        private PropertyInfo GetSelectedMemberPathProperty(object item)
        {
            PropertyInfo propertyInfo = null;
            if (!String.IsNullOrEmpty(SelectedMemberPath) && (item != null))
            {
                var property = item.GetType().GetProperty(SelectedMemberPath);
                if (property != null && property.PropertyType == typeof (bool))
                {
                    propertyInfo = property;
                }
            }

            return propertyInfo;
        }

        /// <summary>
        ///     When SelectedItems collection implements INotifyPropertyChanged, this is the callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_ignoreSelectedItemsCollectionChanged > 0)
                return;

            // Keep it simple for now. Just update all
            UpdateFromSelectedItems();
        }

        private void OnItemSelectionChangedCore(RoutedEventArgs args, bool unselected)
        {
            var item = ItemContainerGenerator.ItemFromContainer((DependencyObject) args.OriginalSource);

            // When the item is it's own container, "UnsetValue" will be returned.
            if (item == DependencyProperty.UnsetValue)
            {
                item = args.OriginalSource;
            }

            if (unselected)
            {
                while (SelectedItems.Contains(item))
                    SelectedItems.Remove(item);
            }
            else
            {
                if (!SelectedItems.Contains(item))
                    SelectedItems.Add(item);
            }

            OnItemSelectionChanged(
                new ItemSelectionChangedEventArgs(ItemSelectionChangedEvent, this, item, !unselected));
        }

        /// <summary>
        ///     When the ItemsSource implements INotifyPropertyChanged, this is the change callback.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            RemoveUnavailableSelectedItems();
            UpdateSelectedMemberPathValuesBindings();
            UpdateValueMemberPathValuesBindings();
        }

        /// <summary>
        ///     This is called when any value of any item referenced by SelectedMemberPath
        ///     is modified. This may affect the SelectedItems collection.
        /// </summary>
        private void OnSelectedMemberPathValuesChanged()
        {
            if (_ignoreSelectedMemberPathValuesChanged > 0)
                return;

            UpdateFromSelectedMemberPathValues();
        }

        /// <summary>
        ///     This is called when any value of any item referenced by ValueMemberPath
        ///     is modified. This will affect the SelectedValue property
        /// </summary>
        private void OnValueMemberPathValuesChanged()
        {
            UpdateSelectedValue();
        }

        private void UpdateSelectedMemberPathValuesBindings()
        {
            _selectedMemberPathValuesHelper.UpdateValueSource(ItemsCollection, SelectedMemberPath);
        }

        private void UpdateValueMemberPathValuesBindings()
        {
            _valueMemberPathValuesHelper.UpdateValueSource(ItemsCollection, ValueMemberPath);
        }

        /// <summary>
        ///     This method will be called when the "IsSelected" property of an SelectorItem
        ///     has been modified.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnItemSelectionChanged(ItemSelectionChangedEventArgs args)
        {
            if (_surpressItemSelectionChanged)
                return;

            RaiseEvent(args);

            if (Command != null)
                Command.Execute(args.Item);
        }

        /// <summary>
        ///     Updates the SelectedValue property based on what is present in the SelectedItems property.
        /// </summary>
        internal void UpdateSelectedValue()
        {
            var newValue = String.Join(Delimiter,
                                       SelectedItems.Cast<object>().Select(x => GetItemValue(x).ToString()).ToArray());

            if (String.IsNullOrEmpty(SelectedValue) || !SelectedValue.Equals(newValue))
            {
                _ignoreSelectedValueChanged = true;
                SelectedValue = newValue;
                _ignoreSelectedValueChanged = false;
            }
        }

        /// <summary>
        ///     Updates the SelectedItem property based on what is present in the SelectedItems property.
        /// </summary>
        private void UpdateSelectedItem()
        {
            if (!SelectedItems.Contains(SelectedItem))
            {
                _ignoreSelectedItemChanged = true;
                SelectedItem = (SelectedItems.Count > 0) ? SelectedItems[0] : null;
                _ignoreSelectedItemChanged = false;
            }
        }

        /// <summary>
        ///     Update the SelectedItems collection based on the values
        ///     refered to by the SelectedMemberPath property.
        /// </summary>
        private void UpdateFromSelectedMemberPathValues()
        {
            _ignoreSelectedItemsCollectionChanged++;
            foreach (var item in ItemsCollection)
            {
                var isSelected = GetSelectedMemberPathValue(item);
                if (isSelected != null)
                {
                    if (isSelected.Value)
                    {
                        if (!SelectedItems.Contains(item))
                        {
                            SelectedItems.Add(item);
                        }
                    }
                    else
                    {
                        if (SelectedItems.Contains(item))
                        {
                            SelectedItems.Remove(item);
                        }
                    }
                }
            }
            _ignoreSelectedItemsCollectionChanged--;
            UpdateFromSelectedItems();
        }

        /// <summary>
        ///     Updates the following based on the content of SelectedItems:
        ///     - All SelectorItems "IsSelected" properties
        ///     - Values refered to by SelectedMemberPath
        ///     - SelectedItem property
        ///     - SelectedValue property
        ///     Refered to by the SelectedMemberPath property.
        /// </summary>
        private void UpdateFromSelectedItems()
        {
            foreach (var o in ItemsCollection)
            {
                var isSelected = SelectedItems.Contains(o);

                _ignoreSelectedMemberPathValuesChanged++;
                SetSelectedMemberPathValue(o, isSelected);
                _ignoreSelectedMemberPathValuesChanged--;

                var selectorItem = ItemContainerGenerator.ContainerFromItem(o) as SelectorItem;
                if (selectorItem != null)
                {
                    selectorItem.IsSelected = isSelected;
                }
            }

            UpdateSelectedItem();
            UpdateSelectedValue();
        }

        /// <summary>
        ///     Removes all items from SelectedItems that are no longer in ItemsSource.
        /// </summary>
        private void RemoveUnavailableSelectedItems()
        {
            _ignoreSelectedItemsCollectionChanged++;
            var hash = new HashSet<object>(ItemsCollection.Cast<object>());

            for (var i = 0; i < SelectedItems.Count; i++)
            {
                if (!hash.Contains(SelectedItems[i]))
                {
                    SelectedItems.RemoveAt(i);
                    i--;
                }
            }
            _ignoreSelectedItemsCollectionChanged--;

            UpdateSelectedItem();
            UpdateSelectedValue();
        }

        /// <summary>
        ///     Updates the SelectedItems collection based on the content of
        ///     the SelectedValue property.
        /// </summary>
        private void UpdateFromSelectedValue()
        {
            _ignoreSelectedItemsCollectionChanged++;
            // Just update the SelectedItems collection content 
            // and let the synchronization be made from UpdateFromSelectedItems();
            SelectedItems.Clear();

            if (!String.IsNullOrEmpty(SelectedValue))
            {
                var selectedValues =
                    SelectedValue.Split(new[] {Delimiter}, StringSplitOptions.RemoveEmptyEntries).ToList();
                var comparer = new ValueEqualityComparer();

                foreach (var item in ItemsCollection)
                {
                    var itemValue = GetItemValue(item);

                    var isSelected = (itemValue != null)
                                     && selectedValues.Contains(itemValue.ToString(), comparer);

                    if (isSelected)
                    {
                        SelectedItems.Add(item);
                    }
                }
            }
            _ignoreSelectedItemsCollectionChanged--;

            UpdateFromSelectedItems();
        }


        private void UpdateDisplayMemberPathValuesBindings()
        {
            _displayMemberPathValuesChangeHelper.UpdateValueSource(ItemsCollection, DisplayMemberPath);
        }

        private void OnDisplayMemberPathValuesChanged()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            var newValue = String.Join(Delimiter,
                                       SelectedItems.Cast<object>()
                                                    .Select(x => GetItemDisplayValue(x).ToString())
                                                    .ToArray());

            if (String.IsNullOrEmpty(Text) || !Text.Equals(newValue))
                Text = newValue;
        }

        protected object GetItemDisplayValue(object item)
        {
            if (!String.IsNullOrEmpty(DisplayMemberPath))
            {
                var property = item.GetType().GetProperty(DisplayMemberPath);
                if (property != null)
                    return property.GetValue(item, null);
            }

            return item;
        }

        private void CloseDropDown(bool isFocusOnComboBox)
        {
            if (IsDropDownOpen)
                IsDropDownOpen = false;
            ReleaseMouseCapture();

            if (isFocusOnComboBox)
                Focus();
        }

        #endregion //Methods

        #region IWeakEventListener Members

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof (CollectionChangedEventManager))
            {
                if (ReferenceEquals(_selectedItems, sender))
                {
                    OnSelectedItemsCollectionChanged(sender, (NotifyCollectionChangedEventArgs) e);
                    return true;
                }

                if (ReferenceEquals(ItemsCollection, sender))
                {
                    OnItemsSourceCollectionChanged(sender, (NotifyCollectionChangedEventArgs) e);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region ValueEqualityComparer private class

        private class ValueEqualityComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.InvariantCultureIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return 1;
            }
        }

        #endregion
    }


    public delegate void ItemSelectionChangedEventHandler(object sender, ItemSelectionChangedEventArgs e);

    public class ItemSelectionChangedEventArgs : RoutedEventArgs
    {
        public ItemSelectionChangedEventArgs(RoutedEvent routedEvent, object source, object item, bool isSelected)
            : base(routedEvent, source)
        {
            Item = item;
            IsSelected = isSelected;
        }

        public bool IsSelected { get; private set; }
        public object Item { get; private set; }
    }

    [TemplatePart(Name = PartActionButton, Type = typeof (Button))]
    public class SplitButton : DropDownButton
    {
        private const string PartActionButton = "PART_ActionButton";

        #region Constructors

        static SplitButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (SplitButton),
                                                     new FrameworkPropertyMetadata(typeof (SplitButton)));
        }

        #endregion //Constructors

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Button = GetTemplateChild(PartActionButton) as Button;
        }

        #endregion //Base Class Overrides
    }

    [TemplatePart(Name = PartDropDownButton, Type = typeof (ToggleButton))]
    [TemplatePart(Name = PartContentPresenter, Type = typeof (ContentPresenter))]
    [TemplatePart(Name = PartPopup, Type = typeof (Popup))]
    public class DropDownButton : ContentControl, ICommandSource
    {
        private const string PartDropDownButton = "PART_DropDownButton";
        private const string PartContentPresenter = "PART_ContentPresenter";
        private const string PartPopup = "PART_Popup";

        #region Members

        private ContentPresenter _contentPresenter;
        private Popup _popup;

        #endregion

        #region Constructors

        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (DropDownButton),
                                                     new FrameworkPropertyMetadata(typeof (DropDownButton)));
        }

        public DropDownButton()
        {
            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion //Constructors

        #region Properties

        private ButtonBase _button;

        protected ButtonBase Button
        {
            get { return _button; }
            set
            {
                if (_button != null)
                    _button.Click -= DropDownButton_Click;

                _button = value;

                if (_button != null)
                    _button.Click += DropDownButton_Click;
            }
        }

        #region DropDownContent

        public static readonly DependencyProperty DropDownContentProperty =
            DependencyProperty.Register("DropDownContent", typeof (object), typeof (DropDownButton),
                                        new UIPropertyMetadata(null, OnDropDownContentChanged));

        public object DropDownContent
        {
            get { return GetValue(DropDownContentProperty); }
            set { SetValue(DropDownContentProperty, value); }
        }

        private static void OnDropDownContentChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var dropDownButton = o as DropDownButton;
            if (dropDownButton != null)
                dropDownButton.OnDropDownContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnDropDownContentChanged(object oldValue, object newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //DropDownContent

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof (bool),
                                                                                               typeof (DropDownButton),
                                                                                               new UIPropertyMetadata(
                                                                                                   false,
                                                                                                   OnIsOpenChanged));

        public bool IsOpen
        {
            get { return (bool) GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var dropDownButton = o as DropDownButton;
            if (dropDownButton != null)
                dropDownButton.OnIsOpenChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            RaiseRoutedEvent(newValue ? OpenedEvent : ClosedEvent);
        }

        #endregion //IsOpen

        #endregion //Properties

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Button = GetTemplateChild(PartDropDownButton) as ToggleButton;

            _contentPresenter = GetTemplateChild(PartContentPresenter) as ContentPresenter;

            if (_popup != null)
                _popup.Opened -= Popup_Opened;

            _popup = GetTemplateChild(PartPopup) as Popup;

            if (_popup != null)
                _popup.Opened += Popup_Opened;
        }

        #endregion //Base Class Overrides

        #region Events

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble,
                                                                                         typeof (RoutedEventHandler),
                                                                                         typeof (DropDownButton));

        public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent("Opened",
                                                                                          RoutingStrategy.Bubble,
                                                                                          typeof (RoutedEventHandler),
                                                                                          typeof (DropDownButton));

        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed",
                                                                                          RoutingStrategy.Bubble,
                                                                                          typeof (RoutedEventHandler),
                                                                                          typeof (DropDownButton));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        #endregion //Events

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!IsOpen)
            {
                if (KeyboardUtilities.IsKeyModifyingPopupState(e))
                {
                    IsOpen = true;
                    // ContentPresenter items will get focus in Popup_Opened().
                    e.Handled = true;
                }
            }
            else
            {
                if (KeyboardUtilities.IsKeyModifyingPopupState(e))
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseDropDown(false);
        }

        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            OnClick();
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged();
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            // Set the focus on the content of the ContentPresenter.
            if (_contentPresenter != null)
            {
                _contentPresenter.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }

        #endregion //Event Handlers

        #region Methods

        private void CanExecuteChanged()
        {
            if (Command != null)
            {
                var command = Command as RoutedCommand;

                // If a RoutedCommand.
                IsEnabled = command != null ? 
                    command.CanExecute(CommandParameter, CommandTarget) : Command.CanExecute(CommandParameter);
            }
        }

        /// <summary>
        ///     Closes the drop down.
        /// </summary>
        private void CloseDropDown(bool isFocusOnButton)
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();

            if (isFocusOnButton)
                Button.Focus();
        }

        protected virtual void OnClick()
        {
            RaiseRoutedEvent(ClickEvent);
            RaiseCommand();
        }

        /// <summary>
        ///     Raises routed events.
        /// </summary>
        private void RaiseRoutedEvent(RoutedEvent routedEvent)
        {
            var args = new RoutedEventArgs(routedEvent, this);
            RaiseEvent(args);
        }

        /// <summary>
        ///     Raises the command's Execute event.
        /// </summary>
        private void RaiseCommand()
        {
            if (Command != null)
            {
                var routedCommand = Command as RoutedCommand;

                if (routedCommand == null)
                    (Command).Execute(CommandParameter);
                else
                    routedCommand.Execute(CommandParameter, CommandTarget);
            }
        }

        /// <summary>
        ///     Unhooks a command from the Command property.
        /// </summary>
        /// <param name="oldCommand">The old command.</param>
        private void UnhookCommand(ICommand oldCommand)
        {
            EventHandler handler = CanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }

        /// <summary>
        ///     Hooks up a command to the CanExecuteChnaged event handler.
        /// </summary>
        /// <param name="newCommand">The new command.</param>
        private void HookUpCommand(ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            _canExecuteChangedHandler = handler;
            if (newCommand != null)
                newCommand.CanExecuteChanged += _canExecuteChangedHandler;
        }

        #endregion //Methods

        #region ICommandSource Members

        // Keeps a copy of the CanExecuteChnaged handler so it doesn't get garbage collected.

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof (object), typeof (DropDownButton),
                                        new PropertyMetadata(null));

        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register("CommandTarget",
                                                                                                      typeof (
                                                                                                          IInputElement),
                                                                                                      typeof (
                                                                                                          DropDownButton
                                                                                                          ),
                                                                                                      new PropertyMetadata
                                                                                                          (null));

        private EventHandler _canExecuteChangedHandler;

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                                                                                                typeof (ICommand),
                                                                                                typeof (DropDownButton),
                                                                                                new PropertyMetadata(
                                                                                                    null,
                                                                                                    OnCommandChanged));

        [TypeConverter(typeof (CommandConverter))]
        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dropDownButton = d as DropDownButton;
            if (dropDownButton != null)
                dropDownButton.OnCommandChanged((ICommand) e.OldValue, (ICommand) e.NewValue);
        }

        protected virtual void OnCommandChanged(ICommand oldValue, ICommand newValue)
        {
            // If old command is not null, then we need to remove the handlers.
            if (oldValue != null)
                UnhookCommand(oldValue);

            HookUpCommand(newValue);

            CanExecuteChanged(); //may need to call this when changing the command parameter or target.
        }

        #endregion //Command

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public IInputElement CommandTarget
        {
            get { return (IInputElement) GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #endregion //ICommandSource Members
    }

    public class ButtonChrome : ContentControl
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius",
                                                                                                     typeof (
                                                                                                         CornerRadius),
                                                                                                     typeof (
                                                                                                         ButtonChrome),
                                                                                                     new UIPropertyMetadata
                                                                                                         (default(
                                                                                                              CornerRadius
                                                                                                              ),
                                                                                                          OnCornerRadiusChanged));

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius) GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static void OnCornerRadiusChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnCornerRadiusChanged((CornerRadius) e.OldValue, (CornerRadius) e.NewValue);
        }

        protected virtual void OnCornerRadiusChanged(CornerRadius oldValue, CornerRadius newValue)
        {
            //we always want the InnerBorderRadius to be one less than the CornerRadius
            var newInnerCornerRadius = new CornerRadius(Math.Max(0, newValue.TopLeft - 1),
                                                        Math.Max(0, newValue.TopRight - 1),
                                                        Math.Max(0, newValue.BottomRight - 1),
                                                        Math.Max(0, newValue.BottomLeft - 1));

            InnerCornerRadius = newInnerCornerRadius;
        }

        #endregion //CornerRadius

        #region InnerCornerRadius

        public static readonly DependencyProperty InnerCornerRadiusProperty =
            DependencyProperty.Register("InnerCornerRadius", typeof (CornerRadius), typeof (ButtonChrome),
                                        new UIPropertyMetadata(default(CornerRadius), OnInnerCornerRadiusChanged));

        public CornerRadius InnerCornerRadius
        {
            get { return (CornerRadius) GetValue(InnerCornerRadiusProperty); }
            set { SetValue(InnerCornerRadiusProperty, value); }
        }

        private static void OnInnerCornerRadiusChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnInnerCornerRadiusChanged((CornerRadius) e.OldValue, (CornerRadius) e.NewValue);
        }

        protected virtual void OnInnerCornerRadiusChanged(CornerRadius oldValue, CornerRadius newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //InnerCornerRadius

        #region RenderChecked

        public static readonly DependencyProperty RenderCheckedProperty = DependencyProperty.Register("RenderChecked",
                                                                                                      typeof (bool),
                                                                                                      typeof (
                                                                                                          ButtonChrome),
                                                                                                      new UIPropertyMetadata
                                                                                                          (false,
                                                                                                           OnRenderCheckedChanged));

        public bool RenderChecked
        {
            get { return (bool) GetValue(RenderCheckedProperty); }
            set { SetValue(RenderCheckedProperty, value); }
        }

        private static void OnRenderCheckedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderCheckedChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRenderCheckedChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderChecked

        #region RenderEnabled

        public static readonly DependencyProperty RenderEnabledProperty = DependencyProperty.Register("RenderEnabled",
                                                                                                      typeof (bool),
                                                                                                      typeof (
                                                                                                          ButtonChrome),
                                                                                                      new UIPropertyMetadata
                                                                                                          (true,
                                                                                                           OnRenderEnabledChanged));

        public bool RenderEnabled
        {
            get { return (bool) GetValue(RenderEnabledProperty); }
            set { SetValue(RenderEnabledProperty, value); }
        }

        private static void OnRenderEnabledChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderEnabledChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRenderEnabledChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderEnabled

        #region RenderFocused

        public static readonly DependencyProperty RenderFocusedProperty = DependencyProperty.Register("RenderFocused",
                                                                                                      typeof (bool),
                                                                                                      typeof (
                                                                                                          ButtonChrome),
                                                                                                      new UIPropertyMetadata
                                                                                                          (false,
                                                                                                           OnRenderFocusedChanged));

        public bool RenderFocused
        {
            get { return (bool) GetValue(RenderFocusedProperty); }
            set { SetValue(RenderFocusedProperty, value); }
        }

        private static void OnRenderFocusedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderFocusedChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRenderFocusedChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderFocused

        #region RenderMouseOver

        public static readonly DependencyProperty RenderMouseOverProperty =
            DependencyProperty.Register("RenderMouseOver", typeof (bool), typeof (ButtonChrome),
                                        new UIPropertyMetadata(false, OnRenderMouseOverChanged));

        public bool RenderMouseOver
        {
            get { return (bool) GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, value); }
        }

        private static void OnRenderMouseOverChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderMouseOverChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRenderMouseOverChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderMouseOver

        #region RenderNormal

        public static readonly DependencyProperty RenderNormalProperty = DependencyProperty.Register("RenderNormal",
                                                                                                     typeof (bool),
                                                                                                     typeof (
                                                                                                         ButtonChrome),
                                                                                                     new UIPropertyMetadata
                                                                                                         (true,
                                                                                                          OnRenderNormalChanged));

        public bool RenderNormal
        {
            get { return (bool) GetValue(RenderNormalProperty); }
            set { SetValue(RenderNormalProperty, value); }
        }

        private static void OnRenderNormalChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderNormalChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRenderNormalChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderNormal

        #region RenderPressed

        public static readonly DependencyProperty RenderPressedProperty = DependencyProperty.Register("RenderPressed",
                                                                                                      typeof (bool),
                                                                                                      typeof (
                                                                                                          ButtonChrome),
                                                                                                      new UIPropertyMetadata
                                                                                                          (false,
                                                                                                           OnRenderPressedChanged));

        public bool RenderPressed
        {
            get { return (bool) GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, value); }
        }

        private static void OnRenderPressedChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var buttonChrome = o as ButtonChrome;
            if (buttonChrome != null)
                buttonChrome.OnRenderPressedChanged((bool) e.OldValue, (bool) e.NewValue);
        }

        protected virtual void OnRenderPressedChanged(bool oldValue, bool newValue)
        {
            // TODO: Add your property changed side-effects. Descendants can override as well.
        }

        #endregion //RenderPressed

        #region Contsructors

        static ButtonChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (ButtonChrome),
                                                     new FrameworkPropertyMetadata(typeof (ButtonChrome)));
        }

        #endregion //Contsructors
    }
}
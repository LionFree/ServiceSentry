using System;
using System.Globalization;
using System.Windows.Controls;
using ServiceSentry.Client.UNTESTED.ViewModels;

namespace ServiceSentry.Client.UNTESTED.Views.Controls
{
    /// <summary>
    ///     Interaction logic for WardenOptionsTabHarness.xaml
    /// </summary>
    public abstract partial class WardenOptionsTabHarness
    {
        internal static WardenOptionsTabHarness GetInstance(WardenOptionsViewModel viewModel)
        {
            return new WOTabHarnessImplementation(viewModel);
        }
        public abstract TabItem GetTabItem();

        private sealed class WOTabHarnessImplementation : WardenOptionsTabHarness
        {
            public WOTabHarnessImplementation(WardenOptionsViewModel viewModel)
            {
                InitializeComponent();
                WardenOptionsTab.DataContext = viewModel;
            }


            public override TabItem GetTabItem()
            {
                var tab = WardenOptionsTab;
                RootTabs.Items.Clear();
                return tab;
            }
        }
    }

    public class PortRule : ValidationRule
    {
        public int Min { get; set; }
        public int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value==null) throw new ArgumentNullException("value");

            int port = 0;

            try
            {
                if (((string)value).Length > 0)
                    port = Int32.Parse((String)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false,
                    String.Format(Strings.Validation_BadPortError, Min, Max));
            }

            if ((port < Min) || (port > Max))
            {
                return new ValidationResult(false, String.Format(Strings.Validation_BadPortError, Min, Max));
            }
            
            return new ValidationResult(true, null);
        }
    }

}
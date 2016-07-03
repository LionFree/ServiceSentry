#region References

using System.Windows;

#endregion

namespace ServiceSentry.Client.UNTESTED.Views.Notification
{
    /// <summary>
    ///     Interaction logic for BalloonPopup.xaml
    /// </summary>
    public partial class BalloonPopup
    {
        #region BalloonText dependency property

        /// <summary>
        ///     Description
        /// </summary>
        public static readonly DependencyProperty BalloonTextProperty =
            DependencyProperty.Register("BalloonText",
                                        typeof (string),
                                        typeof (BalloonPopup),
                                        new FrameworkPropertyMetadata(""));

        /// <summary>
        ///     A property wrapper for the <see cref="BalloonTextProperty" />
        ///     dependency property:<br />
        ///     Description
        /// </summary>
        public string BalloonText
        {
            get { return (string) GetValue(BalloonTextProperty); }
            set { SetValue(BalloonTextProperty, value); }
        }

        #endregion

        public BalloonPopup()
        {
            InitializeComponent();
        }
    }
}
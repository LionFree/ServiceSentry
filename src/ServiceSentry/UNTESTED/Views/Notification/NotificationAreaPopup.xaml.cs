namespace ServiceSentry.Client.UNTESTED.Views.Notification
{
    /// <summary>
    ///     Interaction logic for NotificationAreaPopup.xaml
    /// </summary>
    public partial class NotificationAreaPopup
    {
        public NotificationAreaPopup()
        {
            InitializeComponent();
        }


        ///// <summary>
        ///// Attempts to open and browse a given network location.
        ///// </summary>
        //private void OnDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var source = (DependencyObject)e.OriginalSource;
        //    var row = UIHelpers.TryFindParent<Control>(source);

        //    //the user did not click on a row
        //    if (row == null) return;

        //    //var share = EngineController.GetInstance().ActiveShare;
        //    //if (share != null)
        //    //{
        //    //    bool status = share.IsConnected || ConnectionUtil.Connect(share) == true;
        //    //    if (status) ConnectionUtil.BrowseShare(share);
        //    //}

        //    e.Handled = true;
        //}
    }
}
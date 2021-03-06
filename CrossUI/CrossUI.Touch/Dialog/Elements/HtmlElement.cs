using System;
using CrossUI.Touch.Dialog.Utilities;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace CrossUI.Touch.Dialog.Elements
{
    /// <summary>
    ///  Used to display a cell that will launch a web browser when selected.
    /// </summary>
    public class HtmlElement : Element {
        static readonly NSString Hkey = new NSString ("HtmlElement");
        NSUrl _nsUrl;
        UIWebView _web;
		
        public HtmlElement () : this ("", "")
        {            
        }

        public HtmlElement (string caption, string url) : base (caption)
        {
            Url = url;
        }
		
        public HtmlElement (string caption, NSUrl url) : base (caption)
        {
            _nsUrl = url;
        }
		
        protected override NSString CellKey {
            get {
                return Hkey;
            }
        }

        public string Url {
            get {
                return _nsUrl.ToString ();
            }
            set {
                _nsUrl = new NSUrl (value);
            }
        }
		
        protected override UITableViewCell GetCellImpl (UITableView tv)
        {
            var cell = tv.DequeueReusableCell (CellKey);
            if (cell == null){
                cell = new UITableViewCell (UITableViewCellStyle.Default, CellKey);
                cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
            }
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
            return cell;
        }

        static bool NetworkActivity {
            set {
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
            }
        }
		
        // We use this class to dispose the web control when it is not
        // in use, as it could be a bit of a pig, and we do not want to
        // wait for the GC to kick-in.
        class WebViewController : UIViewController {
            readonly HtmlElement _container;
			
            public WebViewController (HtmlElement container) : base ()
            {
                this._container = container;
            }
			
            public override void ViewWillDisappear (bool animated)
            {
                base.ViewWillDisappear (animated);
                NetworkActivity = false;
                if (_container._web == null)
                    return;

                _container._web.StopLoading ();
                _container._web.Dispose ();
                _container._web = null;
            }

            public bool Autorotate { get; set; }
			
            public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
            {
                return Autorotate;
            }
        }
		
        public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
        {
            var vc = new WebViewController (this) {
                                                      Autorotate = dvc.Autorotate
                                                  };

            _web = new UIWebView (UIScreen.MainScreen.Bounds) {
                                                                 BackgroundColor = UIColor.White,
                                                                 ScalesPageToFit = true,
                                                                 AutoresizingMask = UIViewAutoresizing.All
                                                             };
            _web.LoadStarted += delegate {
                                            NetworkActivity = true;
                                            var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
                                            vc.NavigationItem.RightBarButtonItem = new UIBarButtonItem(indicator);
                                            indicator.StartAnimating();
            };
            _web.LoadFinished += delegate {
                                             NetworkActivity = false;
                                             vc.NavigationItem.RightBarButtonItem = null;
            };
            _web.LoadError += (webview, args) => {
                                                    NetworkActivity = false;
                                                    vc.NavigationItem.RightBarButtonItem = null;
                                                    if (_web != null)
                                                        _web.LoadHtmlString (
                                                            String.Format ("<html><center><font size=+5 color='red'>{0}:<br>{1}</font></center></html>",
                                                                           "An error occurred:".GetText (), args.Error.LocalizedDescription), null);
            };
            vc.NavigationItem.Title = Caption;
			
            vc.View.AutosizesSubviews = true;
            vc.View.AddSubview (_web);
			
            dvc.ActivateController (vc);
            _web.LoadRequest (NSUrlRequest.FromUrl (_nsUrl));
        }
    }
}
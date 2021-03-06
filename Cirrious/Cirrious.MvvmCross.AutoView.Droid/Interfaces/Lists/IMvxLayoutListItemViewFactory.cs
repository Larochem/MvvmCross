using Android.Content;
using Android.Views;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using CrossUI.Core.Elements.Lists;

namespace Cirrious.MvvmCross.AutoView.Droid.Interfaces.Lists
{
    public interface IMvxLayoutListItemViewFactory
        : IListItemLayout
    {
        string UniqueName { get; }
        View BuildView(Context context, IMvxBindingActivity bindingActivity, object source);
    }
}
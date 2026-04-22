using CommunityToolkit.Maui.Views;

namespace RikikiApp.Core.Popups
{
    public interface IPopupResults<T>
    {
        Task<T?> ResultTask { get; }
    }
    public interface IPopupAware
    {
        Popup PopupInstance { set; }
    }
}

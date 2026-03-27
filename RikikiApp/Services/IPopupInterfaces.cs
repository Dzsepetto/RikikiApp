using CommunityToolkit.Maui.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RikikiApp.Services
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

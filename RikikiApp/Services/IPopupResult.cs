using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Services
{
    public interface IPopupResult<T>
    {
        Task<T?> ResultTask { get; }
    }
}

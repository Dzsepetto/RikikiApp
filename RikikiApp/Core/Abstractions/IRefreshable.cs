using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Core.Abstractions
{
    public interface IRefreshable
    {
        Task RefreshAsync();
    }
}

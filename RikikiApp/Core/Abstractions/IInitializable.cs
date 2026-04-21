using System;
using System.Collections.Generic;
using System.Text;

namespace RikikiApp.Core.Abstractions
{
    public interface IInitializable
    {
        Task InitAsync();
    }
}

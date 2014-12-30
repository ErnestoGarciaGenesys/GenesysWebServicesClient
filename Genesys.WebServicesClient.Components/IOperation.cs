using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Genesys.WebServicesClient.Components
{
    public interface IOperation : INotifyPropertyChanged
    {
        void Do();
        bool IsCapable { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncWork
{
    public interface IWorkManagerStateRepository
    {
        ManagerState Load();
        void SaveOrUpdate(ref ManagerState state);
    }
}
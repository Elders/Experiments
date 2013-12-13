using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaCore.Hyperion.Adapters.QuickBloxIntegration
{
    public class BrokenEvent : IQuickBloxCustomObject
    {
        public double Created_at { get; set; }

        public Type EventType { get; set; }

        public string Json { get; set; }

        public BrokenEvent()
        {

        }
    }
}

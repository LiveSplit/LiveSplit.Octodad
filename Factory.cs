using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LiveSplit.Octodad
{
    public class Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Octodad Auto Splits"; }
        }

        public IComponent Create(Model.LiveSplitState state)
        {
            return new OctodadComponent();
        }

        public string UpdateName
        {
            get { return ""; }
        }

        public string UpdateURL
        {
            get { return "http://livesplit.org/update/"; }
        }

        public Version Version
        {
            get { return new Version(); }
        }

        public string XMLURL
        {
            get { return "http://livesplit.org/update/Components/noupdates.xml"; }
        }
    }
}

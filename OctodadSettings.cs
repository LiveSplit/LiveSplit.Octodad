using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LiveSplit.Octodad
{
    public partial class OctodadSettings : UserControl
    {
        public String Path { get; set; }
        public bool ShowActualTimeAsWell { get; set; }

        public OctodadSettings()
        {
            InitializeComponent();
            txtPath.DataBindings.Add("Text", this, "Path", false, DataSourceUpdateMode.OnPropertyChanged);
            chkShowActualTime.DataBindings.Add("Checked", this, "ShowActualTimeAsWell", false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void btnPath_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Filter = "Octodad Executable (OctodadDadliestCatch.exe)|OctodadDadliestCatch.exe|All Files (*.*)|*.*",
                FileName = Path
            };
            var result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                Path = ofd.FileName;
                txtPath.Text = Path;
            }
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            var settingsNode = document.CreateElement("Settings");

            var pathNode = document.CreateElement("Path");
            pathNode.InnerText = Path;
            settingsNode.AppendChild(pathNode);

            var actualTimeNode = document.CreateElement("ShowActualTime");
            actualTimeNode.InnerText = ShowActualTimeAsWell ? "True" : "False";
            settingsNode.AppendChild(actualTimeNode);

            return settingsNode;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            ShowActualTimeAsWell = settings["ShowActualTime"].InnerText == "True";
            Path = settings["Path"].InnerText;
        }
    }
}

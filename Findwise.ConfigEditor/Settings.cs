using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Findwise.Configuration;
using Findwise.Connector.Sharepoint;

namespace Findwise.Connector.ConfigEditor
{
    public class Settings : ConfigurationBase
    {
        private static readonly Size MainWindowDefaultSize = new Size(640, 480);
        public Point MainWindowLocation { get; set; } = new Point(Screen.PrimaryScreen.WorkingArea.Size.Width / 2 - MainWindowDefaultSize.Width / 2, Screen.PrimaryScreen.WorkingArea.Size.Height / 2 - MainWindowDefaultSize.Height / 2);
        public Size MainWindowSize { get; set; } = MainWindowDefaultSize;
        public FormWindowState MainWindowState { get; set; } = FormWindowState.Normal;

        public int MainWindowPropertyGridSplitterPosition { get; set; } = MainWindowDefaultSize.Width / 2 - 28;

        public ConsoleRunnerConfiguration ConsoleRunner { get; set; } = new ConsoleRunnerConfiguration();
    }

    public class ConsoleRunnerConfiguration : ConfigurationBase
    {
        [DefaultValue(RunningMode.Incremental)]
        public RunningMode RunningMode { get; set; }

        [DefaultValue("Connector.Console.exe")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string ConsoleApplicationPath { get; set; }

        [DefaultValue(@"config.xml")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string ConfigurationStore { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Findwise.Configuration;
using Findwise.Connector.Sharepoint.CSOM.Configuration;

namespace Findwise.Connector.ConfigEditor
{
    public partial class Form1 : Form, INotifyPropertyChanged
    {
        private static readonly string SettingsPath = Application.ProductName + ".settings";
        private Settings _settings = null;
        private bool _resizing = false;

        public event PropertyChangedEventHandler PropertyChanged;
        public int propertyGrid1_SplitterPosition
        {
            get { return propertyGrid1.SplitterPosition; }
            set { propertyGrid1.SplitterPosition = value; }
        }

        public Form1()
        {
            InitializeComponent();
            propertyGrid1.PropertyChanged += (sender_, e_) => { if (!_resizing) PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(((Control)sender_).Name + "_" + e_.PropertyName)); };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _settings = ConfigurationBase.Deserialize<Settings>(System.IO.File.ReadAllText(SettingsPath));
            }
            catch
            {
                _settings = new Settings()
                {
                };
            }
            _settings.AddPropertyBinding(this, nameof(Location), nameof(_settings.MainWindowLocation));
            _settings.AddPropertyBinding(this, nameof(Size), nameof(_settings.MainWindowSize));
            _settings.AddPropertyBinding(this, nameof(WindowState), nameof(_settings.MainWindowState));
            _settings.AddPropertyBinding(this, nameof(propertyGrid1_SplitterPosition), nameof(_settings.MainWindowPropertyGridSplitterPosition));

            if (!this.IsOnScreen()) this.Location = Screen.PrimaryScreen.Bounds.Location;

            LoadNewConfiguration();
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _settings.ApplyValuesFromControl();
            try { System.IO.File.WriteAllText(SettingsPath, _settings.Serialize()); } catch { }
        }
        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            _resizing = true;
        }
        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            _resizing = false;
        }

        private void LoadNewConfiguration()
        {
            propertyGrid1.SelectedObject = new SharepointConnectorSettings();
        }

        private void LoadConfigurationFromFile(string filename)
        {
            var configFileContents = System.IO.File.ReadAllText(filename);
            propertyGrid1.SelectedObject = ConfigurationBase.Deserialize<SharepointConnectorSettings>(configFileContents);
        }

        private void SaveConfigurationToFile(string filename)
        {
            var configuration = propertyGrid1.SelectedObject as SharepointConnectorSettings; //No null check on purpose.
            System.IO.File.WriteAllText(filename, configuration.Serialize());
        }

        private void NewToolStripButton_Click(object sender, EventArgs e)
        {
            LoadNewConfiguration();
        }

        private void OpenToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    LoadConfigurationFromFile(openFileDialog1.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    SaveConfigurationToFile(saveFileDialog1.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RestoreDefaultValueToolStripButton_Click(object sender, EventArgs e)
        {
            propertyGrid1.ResetSelectedProperty();
        }


        private IEnumerable<string> _errors = Enumerable.Empty<string>();
        private IEnumerable<string> _warnings = Enumerable.Empty<string>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            var configuration = propertyGrid1.SelectedObject as SharepointConnectorSettings;
            configuration?.ValidateData(out _errors, out _warnings);
            toolStripStatusLabel1.Visible = _errors.Any();
            toolStripStatusLabel2.Visible = _warnings.Any();
        }
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Join(Environment.NewLine, _errors), "Configuration errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            MessageBox.Show(string.Join(Environment.NewLine, _warnings), "Configuration warnings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        private void RunConnectorToolStripButton_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                SaveConfigurationToFile(_settings.ConsoleRunner.ConfigurationStore);
                Process.Start(_settings.ConsoleRunner.ConsoleApplicationPath, $"-{_settings.ConsoleRunner.RunningMode.ToString().ToLower()} -config \"{_settings.ConsoleRunner.ConfigurationStore}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to run console application", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ConsoleRunnerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new Form()
            {
                StartPosition = FormStartPosition.CenterParent,
                ShowInTaskbar = false,
                MinimizeBox = false,
                Text = (sender as ToolStripItem)?.Text
            };
            var propertygrid = new PropertyGrid()
            {
                Dock = DockStyle.Fill,
                SelectedObject = _settings.ConsoleRunner,
                PropertySort = PropertySort.NoSort,
                ToolbarVisible = false,
                Parent = form
            };
            form.ShowDialog(this);
        }

        private void Controls_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }
        private void Controls_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            try
            {
                LoadConfigurationFromFile(fileNames.Single());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

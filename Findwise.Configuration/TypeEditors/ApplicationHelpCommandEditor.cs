using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Findwise.Configuration.TypeEditors
{
    public class ApplicationHelpCommandEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var options = context.PropertyDescriptor.Attributes.OfType<OptionsAttribute>().FirstOrDefault();
            if (options != null)
            {
                var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                var msBuildPath = context.Instance.GetType().GetProperty(options.ExecutablePathPropertyName)?.GetValue(context.Instance) as string;
                if (!string.IsNullOrEmpty(msBuildPath))
                {
                    var window = new Form()
                    {
                        Text = $"{msBuildPath} {options.HelpCommand}",
                        StartPosition = FormStartPosition.WindowsDefaultBounds
                    };
                    var textBox = new RichTextBox()
                    {
                        BackColor = options.WindowBackgroundColor.ToColor(),
                        Font = new System.Drawing.Font("Consolas", 10, System.Drawing.FontStyle.Regular),
                        ReadOnly = true,
                        Dock = DockStyle.Fill,
                        Parent = window
                    };
                    window.Show();
                    var info = new ProcessStartInfo(msBuildPath, options.HelpCommand)
                    {
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };
                    var process = Process.Start(info);
                    window.FormClosed += (s_, e_) => { try { if (!process.HasExited) process.Kill(); } catch { } };
                    Action<string, ConsoleColor> dataReceived = (s, c) =>
                    {
                        textBox.Invoke(new MethodInvoker(() =>
                        {
                            if (!string.IsNullOrEmpty(s))
                            {
                                textBox.SelectionStart = textBox.TextLength;
                                textBox.SelectionColor = c.ToColor();
                                textBox.AppendText(s);
                                textBox.AppendText(Environment.NewLine);
                            }
                        }));
                    };
                    process.OutputDataReceived += (s_, e_) =>
                    {
                        dataReceived(e_.Data, options.StandardOutputColor);
                    };
                    process.ErrorDataReceived+= (s_, e_) =>
                    {
                        dataReceived(e_.Data, options.StandardErrorColor);
                    };
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    //process.WaitForExit();
                    //editorService.ShowDialog(window);
                }
            }
            return base.EditValue(context, provider, value);
        }


        public class OptionsAttribute : Attribute
        {
            public string ExecutablePathPropertyName { get; }
            public string HelpCommand { get; set; } = "/?";

            public ConsoleColor WindowBackgroundColor { get; set; } = ConsoleColor.Black;
            public ConsoleColor StandardOutputColor { get; set; } = ConsoleColor.White;
            public ConsoleColor StandardErrorColor { get; set; } = ConsoleColor.Red;

            public OptionsAttribute(string executablePathPropertyName)
            {
                ExecutablePathPropertyName = executablePathPropertyName;
            }
        }
    }
}

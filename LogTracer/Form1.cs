using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogTracer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private FileSystemWatcher watcher;
        private OpenFileDialog fileDialog;
        private string filePathName;
        private long oldLineCount = 0;

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void Form1_Load(object sender, EventArgs e)
        {

            fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                filePathName = fileDialog.FileName;
                string fileDirectory = System.IO.Path.GetDirectoryName(fileDialog.FileName);
                string fileName = System.IO.Path.GetFileName(fileDialog.FileName);

                watcher = new FileSystemWatcher();
                watcher.Path = fileDirectory;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.Filter = fileName;
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;

                readFile();
            }

        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action(() => readFile()));
            }
        }

        private void readFile()
        {
            
            using (FileStream stream = File.Open(filePathName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    long currentLineCount = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (currentLineCount >= oldLineCount)
                        {

                            if (line.Contains(txtColorFilter.Text) && !txtColorFilter.Text.Equals(""))
                            {
                                txtOutput.AppendText(line + "\n", Color.Red);
                            }
                            else
                            {
                                txtOutput.AppendText(line + "\n", Color.Black);
                            }
                            
                        }

                        currentLineCount++;

                        if (true)
                        {
                            txtOutput.SelectionStart = txtOutput.Text.Length;
                            txtOutput.ScrollToCaret();
                        }
                    }

                    if (currentLineCount < oldLineCount)
                    {
                        // File is reduced, lets begin from the start..
                        txtOutput.Clear();
                        oldLineCount = 0;
                        currentLineCount = 0;
                    }
                    else
                    {
                        // Done reading for now, lets store new max count.
                        oldLineCount = currentLineCount;
                        currentLineCount = 0;
                    }
                    
                }
            }
        }

    }

    public static class RichTextBoxExtensions
    {
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}

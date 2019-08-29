using Parser.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Parser
{
    public partial class MainForm : Form
    {
        Utility utility;

        public MainForm()
        {
            InitializeComponent();
            openFileDialog.Multiselect = false;
            lblFileLocation.Text = "";
            lblFileSize.Text = "";
        }

        private void btnParse_Click(object sender, EventArgs e)
        {
            if(utility != null && utility.isInProgress)
            {
                MessageBox.Show("Operation is in progress", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtSelectedFile.Tag == null)
            {
                MessageBox.Show("File is not selected", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            txtLog.Clear();

            utility = new Utility();
            utility.GetCurrentState = UpdateProgressBar;

            WriteLog("> Parsing is started ...\r\n");

            new Thread(() =>
                {
                    try
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        var result = utility.Parse(txtSelectedFile.Tag.ToString(), cbCIS.Checked);
                        sw.Stop();
                        WriteLog($"> Parsing is finished in {sw.ElapsedMilliseconds} milliseconds\r\n");

                        WriteLog("> Creating csv is started ...\r\n");
                        sw.Restart();

                        File.AppendAllLines("result.csv",
                            result.Select(line => $"{line.Key},{line.Value}").ToList());

                        sw.Stop();
                        WriteLog($"> Creating csv is finished in {sw.ElapsedMilliseconds} milliseconds\r\n{Environment.CurrentDirectory}\\result.csv\r\n");

                        MessageBox.Show($"> Process is finished: {Environment.CurrentDirectory}\\result.csv");
                    }
                    catch (OperationCanceledException)
                    {
                        WriteLog("> Operation is cancelled\r\n");
                    }

                    UpdateProgressBar(0);
                }
                ).Start();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtSelectedFile.Text = openFileDialog.SafeFileName;
                txtSelectedFile.Tag = openFileDialog.FileName;

                FileInfo info = new FileInfo(openFileDialog.FileName);
                lblFileLocation.Text = $"LOCATION: {info.DirectoryName}";
                lblFileSize.Text = $"SIZE: {Math.Round((decimal)info.Length / 1024, 2)} KB";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (!(utility !=null && utility.isInProgress))
            {
                MessageBox.Show("No operation is running", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (utility != null)
                utility.isCancelled = true;
        }


        private delegate void LogDelegate(string text);
        private void WriteLog(string text)
        {
            if (txtLog.InvokeRequired)
            {
                var d = new LogDelegate(WriteLog);
                txtLog.Invoke(d, new object[] { text });
            }
            else
            {
                txtLog.AppendText(text);
            }
        }

        private delegate void ProgressBarDelegate(int value);
        public void UpdateProgressBar(int value)
        {
            if (progressBar.InvokeRequired)
            {
                var d = new ProgressBarDelegate(UpdateProgressBar);
                progressBar.Invoke(d, new object[] { value });
            }
            else
            {
                progressBar.Value = value;
            }
        }
    }
}

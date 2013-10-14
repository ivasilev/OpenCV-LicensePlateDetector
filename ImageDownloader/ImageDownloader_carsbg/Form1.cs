using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace ImageDownloader_carsbg
{
    public partial class Form1 : Form
    {
        private Thread tWorkerThread;
 
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowseLocal_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (DialogResult.OK == fb.ShowDialog())
            {
                tbLocalFolder.Text = fb.SelectedPath;
            }
        }

        private void Log(string LogText)
        {
            if (InvokeRequired)
            {
                //InotiveDemoApp.ucForm.Invoke(new EventHandler(delegate { InotiveDemoApp.ucForm.ShowDialog(); }));
                this.Invoke(new Action<string>(Log), new object[] {LogText});
            }
            else
            {
                tbLog.AppendText(DateTime.Now.ToShortTimeString() + ": " + LogText + "\r\n");
            }
        }

        private void DoWork()
        {
            int nProcessedFiles = 0;
            int nCurrentFile = 0;
            String sTempUrl = String.Empty;
            String sCurrentFilename = String.Empty;
            WebClient tWebClient = new WebClient();
            DateTime tStartTime = DateTime.Now;

            Log("Starting download");

            try
            {
                nCurrentFile = Int32.Parse(tbRangeStartURL.Text);

                try
                {
                    while (true)
                    {
                        sTempUrl = tbBaseURL.Text;
                        sTempUrl = sTempUrl.Replace("*", nCurrentFile.ToString("D4"));
                        Log("Downloading " + sTempUrl);
                        String tLocalFileName = sTempUrl.Substring(sTempUrl.LastIndexOf("/"), sTempUrl.Length - sTempUrl.LastIndexOf("/"));
                        tLocalFileName = tbLocalFolder.Text + tLocalFileName;
                        Uri tUri = new Uri(sTempUrl);
                        tWebClient.DownloadFile(tUri, tLocalFileName);
                        nProcessedFiles++;
                        nCurrentFile++;
                        object lockObj = new object();
                        lock (lockObj)
                        {
                            Monitor.Wait(lockObj, 300);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.ToString() + " - End");
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString() + " - Probably StartRange is not numberical?");
            }
            Log("Got " + nProcessedFiles.ToString() + " files in " + (DateTime.Now - tStartTime).Seconds.ToString() + " seconds");
            Log("Download finished");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            tbLog.Text = String.Empty;

            tWorkerThread = new Thread(DoWork);
            tWorkerThread.Start();
        }
    }
}

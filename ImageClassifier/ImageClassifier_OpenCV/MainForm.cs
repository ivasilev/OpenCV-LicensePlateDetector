using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ImageClassifier_OpenCV
{
    public partial class MainForm : Form
    {
        private DataTable dtImgData;
        private const String dtFilename = "ImageClassifier_DataTable.xml";
        private ImageForm fImg = new ImageForm();
        private DataRow drCurrent;
        private int nCurrentRow;


        public MainForm()
        {
            InitializeComponent();
            fImg.KeyPress += new KeyPressEventHandler(fImg_KeyPress);
            this.KeyPress += new KeyPressEventHandler(Main_KeyPress);
        }



        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fb = new FolderBrowserDialog();
            if (DialogResult.OK == fb.ShowDialog())
                tbFolder.Text = fb.SelectedPath;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbFolder.Text + "\\" + dtFilename))
            {
                if (null != dtImgData)
                {
                    dgvMain.DataSource = null;
                    dtImgData.Dispose();
                }

                dtImgData = new DataTable();
                //dtImgData.ReadXmlSchema(tbFolder.Text + "\\" + dtFilename);
                dtImgData.ReadXml(tbFolder.Text + "\\" + dtFilename);
                dgvMain.DataSource = dtImgData;
            }
            else
            {
                if (null != dtImgData)
                    dtImgData.Dispose();

                dtImgData = new DataTable();
                dtImgData.TableName = "ImgData";
                dtImgData.Columns.Add("Filename", typeof(System.String));
                dtImgData.Columns.Add("Processed", typeof(System.Boolean));
                dtImgData.Columns.Add("Positive", typeof(Boolean));
                dtImgData.Columns.Add("StartX", typeof(Int32));
                dtImgData.Columns.Add("StartY", typeof(Int32));
                dtImgData.Columns.Add("EndX", typeof(Int32));
                dtImgData.Columns.Add("EndY", typeof(Int32));
                dtImgData.PrimaryKey = new DataColumn[] { dtImgData.Columns["Filename"] };

                dgvMain.DataSource = dtImgData;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (null != dtImgData)
            {
                dtImgData.WriteXml(tbFolder.Text + "\\" + dtFilename, XmlWriteMode.WriteSchema);
            }
        }

        private void btnRescan_Click(object sender, EventArgs e)
        {
            int nAdded = 0;
            int nRemoved = 0;

            try
            {
                string[] AllFiles = Directory.GetFiles(tbFolder.Text, "*.jpg");

                int i;
                for (i = 0; i < AllFiles.Length; i++)
                {
                    DataRow drFound = dtImgData.Rows.Find(AllFiles[i]);
                    if (null == drFound)
                    {
                        DataRow drAdd = dtImgData.NewRow();
                        drAdd["Filename"] = AllFiles[i];
                        dtImgData.Rows.Add(drAdd);
                        nAdded++;
                    }
                }

                i = 0;
                while (i < dtImgData.Rows.Count)
                {
                    DataRow drToRemove = dtImgData.Rows[i];
                    String strFilename = (String)drToRemove["Filename"];
                    strFilename = strFilename.Substring(strFilename.LastIndexOf("\\") + 1);

                    Boolean bFound = false;
                    for (int j = 0; (j < AllFiles.Length) && (false == bFound); j++)
                    {
                        if (AllFiles[j].Contains(strFilename))
                            bFound = true;
                    }

                    if (false == bFound)
                    {
                        dtImgData.Rows.Remove(drToRemove);
                        nRemoved++;
                    }
                    i++;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception caught:" + ex.ToString());
            }

            MessageBox.Show("Added: " + nAdded.ToString() + ", Removed: " + nRemoved.ToString());
        }

        private void btnContinue_Click(object sender, EventArgs e)
        {
            if (dgvMain.CurrentCell.RowIndex < 3)
            {
                int i = 0;
                int nFirstUnprocessedRow = -1;

                while ((i < dtImgData.Rows.Count) && (-1 == nFirstUnprocessedRow))
                {
                    if (DBNull.Value == dtImgData.Rows[i]["Processed"])
                    {
                        nFirstUnprocessedRow = i;
                    }
                    else if (false == (Boolean)dtImgData.Rows[i]["Processed"])
                    {
                        nFirstUnprocessedRow = i;
                    }
                    i++;
                }

                if (-1 != nFirstUnprocessedRow)
                {
                    nCurrentRow = nFirstUnprocessedRow;
                    ProcessImage();
                }
            }
            else
                nCurrentRow = dgvMain.CurrentCell.RowIndex;
                ProcessImage();
        }

        private void ProcessImage()
        {
            if (nCurrentRow < dtImgData.Rows.Count)
            {
                drCurrent = dtImgData.Rows[nCurrentRow];
                dgvMain.FirstDisplayedScrollingRowIndex = nCurrentRow;
                fImg.ShowPicture(drCurrent);
                fImg.Show();
            }
        }

        void Main_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)'z':
                    nCurrentRow--;
                    ProcessImage();
                    break;

                case (char)'x':
                    nCurrentRow++;
                    ProcessImage();
                    break;

                case (char)'u':
                case (char)'s':
                    drCurrent["Processed"] = false;
                    nCurrentRow++;
                    ProcessImage();
                    break;

                case (char)'p':
                    if (fImg.tRect.Width > 0 && (fImg.tRect.Height > 0))
                    {
                        drCurrent["Processed"] = true;
                        drCurrent["Positive"] = true;
                        drCurrent["StartX"] = fImg.tRect.X;
                        drCurrent["StartY"] = fImg.tRect.Y;
                        drCurrent["EndX"] = fImg.tRect.X + fImg.tRect.Width;
                        drCurrent["EndY"] = fImg.tRect.Y + fImg.tRect.Height;
                        nCurrentRow++;
                        ProcessImage();
                    }
                    break;
                
                case (char)'n':
                    drCurrent["Processed"] = true;
                    drCurrent["Positive"] = false;
                    nCurrentRow++;
                    ProcessImage();
                    break;
            }
        }

        void fImg_KeyPress(object sender, KeyPressEventArgs e)
        {
            Main_KeyPress(sender, e);
        }

        private void CreateWorkingSet()
        {
            DataRow drWork;
            int i =0 , nPos =0 , nNeg =0;

            String strOutDir = tbFolder.Text + "\\out_" 
                + System.DateTime.Now.Day.ToString() + "_"
                + System.DateTime.Now.Month.ToString() + "_"
                + System.DateTime.Now.Year.ToString() + "-"
                + System.DateTime.Now.Hour.ToString() + "_"
                + System.DateTime.Now.Minute.ToString() + "_"
                + System.DateTime.Now.Second.ToString();
            Directory.CreateDirectory(strOutDir);
            Directory.CreateDirectory(strOutDir + "\\pos");
            Directory.CreateDirectory(strOutDir + "\\neg"); 

            while (i < dtImgData.Rows.Count)
            {
                drWork = dtImgData.Rows[i];
                if (DBNull.Value != drWork["Processed"])
                {
                    if (false != (Boolean)drWork["Processed"])
                    {
                        String strFileName = (String)drWork["Filename"];
                        strFileName = strFileName.Substring(strFileName.LastIndexOf("\\") + 1);

                        if (true == (Boolean)drWork["Positive"])
                        {
                            // Positive image
                            File.Copy((String)drWork["Filename"], strOutDir + "\\pos\\" + strFileName);

                            using (StreamWriter sw = File.AppendText(strOutDir + "\\img.dat"))
                            {
                                Point tStart = new Point((int)drWork["StartX"], (int)drWork["StartY"]);
                                Size tSize = new Size((int)drWork["EndX"] - (int)drWork["StartX"], (int)drWork["EndY"] - (int)drWork["StartY"]);
                                String strRectDesc = tStart.X.ToString() + " " + tStart.Y.ToString() + " " + tSize.Width.ToString() + " " + tSize.Height.ToString();
                                sw.WriteLine("pos/" + strFileName + " 1 " + strRectDesc);
                            }

                            nPos++;
                        }
                        else
                        {
                            // negative image
                            File.Copy((String)drWork["Filename"], strOutDir + "\\neg\\" + strFileName);

                            using (StreamWriter sw = File.AppendText(strOutDir + "\\bg.txt"))
                            {
                                sw.WriteLine("neg/" + strFileName);
                            }

                            nNeg++;
                        }
                    }
                }
                
                i++;
            }

            MessageBox.Show("Total: " + nPos.ToString() + " Positive images, " + nNeg.ToString() + " Negative images");
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            CreateWorkingSet();
        }


    }
}

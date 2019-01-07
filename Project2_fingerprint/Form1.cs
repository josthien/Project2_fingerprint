using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using DPUruNet;

namespace Project2_fingerprint
{
    public partial class frmMain : Form
    {
        Image fingerprint;

        #region Constructor
        /// <summary>
        /// constructor
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Verify fingerprints
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnVerify_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.fingerprint == null)
                {
                    MessageBox.Show("Please upload a fingerprint");
                    return;
                }

                if (this.txtDirectory.Text.Equals(string.Empty))
                {
                    MessageBox.Show("Please choose a directory containing fingerprint sample data!");
                    return;
                }

                FingerprintVerifcation1();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Event click of button Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Load fingerprints from file</remarks>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                string fileName = string.Empty;
                this.openFileDialog1.Filter = "Bitmap files (*.bmp) | *.bmp";
                this.openFileDialog1.FileName = string.Empty;

                if (this.openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                if (!Path.GetExtension(openFileDialog1.FileName).Equals(".bmp"))
                {
                    MessageBox.Show("Please choose a bitmap file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                this.fingerprint = Image.FromFile(openFileDialog1.FileName);

                
                this.picFingerprint.Image = this.fingerprint;
                this.picFingerprint.SizeMode = PictureBoxSizeMode.AutoSize;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Event form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            try
            {
                string dataLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\samples";
                
                EventsInitialization();

                this.txtDirectory.Text = dataLocation;
                //this.txtDirectory.Text = string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Choose directory containing sample data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.txtDirectory.Text = this.folderBrowserDialog1.SelectedPath;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Functions
        /// <summary>
        /// Initialize events
        /// </summary>
        private void EventsInitialization()
        {
            this.btnLoad.Click += new EventHandler(btnLoad_Click);
            this.btnVerify.Click += new EventHandler(btnVerify_Click);
            this.btnBrowse.Click += new EventHandler(btnBrowse_Click);
        }

        /// <summary>
        /// Finger print verification
        /// </summary>
        private void FingerprintVerifcation1()
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                string identification = string.Empty;
                string scores = string.Empty;
                string folderPath = this.txtDirectory.Text; //delcare the folder containing fingerprint samples 
                string fileName = "matchScores.txt";

                //Convert the uploaded fingerprint to raw byte[]
                //Create FMD of the uploaded fingerprint from Raw
                DataResult<Fmd> fmd = FeatureExtraction.CreateFmdFromRaw(ConvertToRaw(this.fingerprint), 0, 0, fingerprint.Width, fingerprint.Height,
                    (int)this.fingerprint.HorizontalResolution, Constants.Formats.Fmd.ISO);

                string[] directories = Directory.GetDirectories(folderPath);

                if (directories.Length > 0)
                {
                    foreach (string subDirPath in directories)
                    {
                        scores = string.Empty;
                        //Convert the fingerprint samples to raw byte[]
                        //Create FMD of the fingerprint samples from Raw
                        IEnumerable<Fmd> fingerPrintFMD = CreateFMD(subDirPath);

                        //Compare the uploaded fingerprint with fingerprint samples
                        foreach (Fmd sampleFMD in fingerPrintFMD)
                        {
                            if (sampleFMD == null)
                            {
                                int matchScore = Int32.MaxValue;
                                scores += matchScore.ToString() + "; ";
                                continue;
                            }
                            else
                            {
                                CompareResult rst = Comparison.Compare(fmd.Data, 0, sampleFMD, 0);
                                
                                if (rst.Score == 0)
                                {
                                    identification += Path.GetFileName(subDirPath) + " - ";
                                }

                                scores += rst.Score.ToString() + "; ";
                            }
                        }

                        //Store match scores in a string array
                        scores = scores.Substring(0, scores.Length - 2);
                        string[] matchScoreArr = scores.Split(';');

                        if (File.Exists(subDirPath + "\\" + fileName))
                        {
                            File.Delete(subDirPath + "\\" + fileName);
                        }

                        //Create data table containing file name and match scores
                        DataTable dtScores = new DataTable();
                        dtScores.Columns.Add("FileName");
                        dtScores.Columns.Add("MatchScores");

                        for (int i = 0; i < Directory.GetFiles(subDirPath).Length; i++)
                        {
                            DataRow dr = dtScores.NewRow();
                            dr["FileName"] = Directory.GetFiles(subDirPath)[i];
                            dr["MatchScores"] = matchScoreArr[i];
                            dtScores.Rows.Add(dr);
                        }

                        //write match scores into a file
                        CreateFile(dtScores, subDirPath + "\\" + fileName);
                    }
                }
                else
                {
                    //Convert the fingerprint samples to raw byte[]
                    //Create FMD of the fingerprint samples from Raw
                    IEnumerable<Fmd> fingerPrintFMD = CreateFMD(folderPath);

                    //Compare the uploaded fingerprint with fingerprint samples
                    foreach (Fmd sampleFMD in fingerPrintFMD)
                    {
                        if (sampleFMD == null)
                        {
                            int matchScore = Int32.MaxValue;
                            scores += matchScore.ToString() + "; ";
                            continue;
                        }
                        else
                        {
                            CompareResult rst = Comparison.Compare(fmd.Data, 0, sampleFMD, 0);
                            //scores += rst.Score.ToString() + "; ";
                            if (rst.Score == 0)
                            {
                                identification += Path.GetFileName(folderPath) + " - ";
                            }

                            scores += rst.Score.ToString() + "; ";
                        }
                    }

                    //Store match scores in a string array
                    scores = scores.Substring(0, scores.Length - 2);
                    string[] matchScoreArr = scores.Split(';');

                    if (File.Exists(folderPath + "\\" + fileName))
                    {
                        File.Delete(folderPath + "\\" + fileName);
                    }

                    //Create data table containing file name and match scores
                    DataTable dtScores = new DataTable();
                    dtScores.Columns.Add("FileName");
                    dtScores.Columns.Add("MatchScores");

                    for (int i = 0; i < Directory.GetFiles(folderPath).Length; i++)
                    {
                        DataRow dr = dtScores.NewRow();
                        dr["FileName"] = Directory.GetFiles(folderPath)[i];
                        dr["MatchScores"] = matchScoreArr[i];
                        dtScores.Rows.Add(dr);
                    }

                    //write match scores into a file
                    CreateFile(dtScores, folderPath + "\\" + fileName);
                }

                if (identification.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("No match found!", "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    identification = identification.Substring(0, identification.Length - 2);
                    MessageBox.Show("Identified candidates: " + identification.Trim(), "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                MessageBox.Show("Verification done! A text file with match scores has been created in every folder containing fingerprint samples",
                    "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                throw;
            }

            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Create FMD from samples
        /// </summary>
        /// <param name="fid"></param>
        /// <returns></returns>
        private IEnumerable<Fmd> CreateFMD(string folderPath)
        {
            //Convert the fingerprint samples to raw byte[]
            //Create FMD of the fingerprint samples from Raw
            foreach (string file in Directory.EnumerateFiles(folderPath, "*.bmp"))
            {
                Image img = Image.FromFile(file);

                //DataResult<Fmd> data = Importer.ImportFmd(imageToByteArray(img), Constants.Formats.Fmd.ISO, Constants.Formats.Fmd.ISO);
                //yield return data.Data;

                DataResult<Fmd> fmd = FeatureExtraction.CreateFmdFromRaw(ConvertToRaw(img), 0, 0, img.Width, img.Height,
                    (int)img.HorizontalResolution, Constants.Formats.Fmd.ISO);

                yield return fmd.Data;
            }
        }

        /// <summary>
        /// Convert bmp file to raw binary byte[]
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private byte[] ConvertToRaw(Image img)
        {

            Byte[] data;

            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, img.RawFormat);
                data = new byte[ms.Length];
                data = ms.ToArray();
            }
            return data;
        }

        /// <summary>
        /// Write result to a text file
        /// </summary>
        /// <param name="submittedDataTable"></param>
        /// <param name="submittedFilePath"></param>
        private void CreateFile(DataTable submittedDataTable, string submittedFilePath)
        {
            int i = 0;
            StreamWriter sw = null;

            sw = new StreamWriter(submittedFilePath, false);

            for (i = 0; i < submittedDataTable.Columns.Count - 1; i++)
            {
                sw.Write(submittedDataTable.Columns[i].ColumnName + ";");
            }
            sw.Write(submittedDataTable.Columns[i].ColumnName);
            sw.WriteLine();

            foreach (DataRow row in submittedDataTable.Rows)
            {
                object[] array = row.ItemArray;

                for (i = 0; i < array.Length - 1; i++)
                {
                    sw.Write(array[i].ToString() + ";");
                }
                sw.Write(array[i].ToString());
                sw.WriteLine();

            }

            sw.Close();
        }
        #endregion
    }
}
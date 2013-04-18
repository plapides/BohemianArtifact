using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BohemianArtifact;

namespace ArtifactEditor
{
    public partial class Form1 : Form
    {
        private ArtifactLibrary library;
        private int currentArtifactId;
        private string imageFilename;
        private Bitmap resized, letterboxed;
        private Color pickColor;
        private StreamWriter streamWriter;

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadArtifact()
        {
            LoadArtifact(currentArtifactId);
        }

        private void LoadArtifact(int id)
        {
            Artifact artifact;
            try
            {
                artifact = library.Artifacts[id];
            }
            catch (Exception e)
            {
                artifact = null;
            }

            if (artifact == null)
            {
                tbArtifactId.Text = "Invalid artifact id";
                return;
            }

            tbArtifactId.Text = id.ToString();
            tbCatalogNumber.Text = artifact.CatalogNumber;
            tbUseDate.Text = artifact.UseDate;
            tbUseQualifier.Text = "";
            tbUseDateEnd.Text = "";

            tbManuDateStart.Text = artifact.ManufactureDate.StartYear.ToString();
            tbManuDateEnd.Text = artifact.ManufactureDate.EndYear.ToString();
            tbManuQualifier.Text = artifact.ManufactureDate.DateQualifier.ToString();

            int i;
            for (i = 0; i < artifact.UseDate.Length; i++)
            {
                if (artifact.UseDate[i] < '0' || '9' < artifact.UseDate[i])
                {
                    break;
                }
            }
            tbUseDateStart.Text = artifact.UseDate.Substring(0, i);
            tbUseDateStart.Focus();

            LoadImage(artifact.CatalogNumber);
        }

        private void LoadImage(string catalogNumber)
        {
            imageFilename = catalogNumber.Substring(0, 9) + ".jpg";

            Bitmap image = new Bitmap("original\\" + imageFilename);
            float aRatio = (float)image.Width / (float)image.Height;
            int newSize = 512;
            int newWidth, newHeight;
            if (1 < aRatio)
            {
                newWidth = newSize;
                newHeight = (int)(newSize / aRatio);
                letterboxed = new Bitmap(newSize, newSize);
                resized = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(letterboxed))
                {
                    g.Clear(Color.White);
                    g.DrawImage(image, 0, (newSize - newHeight) / 2, newWidth, newHeight);
                }
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.Clear(Color.White);
                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                }
            }
            else
            {
                newWidth = (int)(newSize * aRatio);
                newHeight = newSize;
                letterboxed = new Bitmap(newSize, newSize);
                resized = new Bitmap(newWidth, newHeight);
                using (Graphics g = Graphics.FromImage(letterboxed))
                {
                    g.Clear(Color.White);
                    g.DrawImage(image, (newSize - newWidth) / 2, 0, newWidth, newHeight);
                }
                using (Graphics g = Graphics.FromImage(resized))
                {
                    g.Clear(Color.White);
                    g.DrawImage(image, 0, 0, newWidth, newHeight);
                }
            }
            pictureBox.Width = newSize;
            pictureBox.Height = newSize;
            pictureBox.Image = letterboxed;
        }

        private void Save()
        {
            letterboxed.Save("letterboxed\\" + imageFilename, System.Drawing.Imaging.ImageFormat.Jpeg);
            resized.Save("resized\\" + imageFilename, System.Drawing.Imaging.ImageFormat.Jpeg);
            if (tbUseDateEnd.Text == "")
            {
                streamWriter.WriteLine(tbCatalogNumber.Text + "," + DateQualifier() + "," + tbUseDateStart.Text + ", ," + GetColorString(pickColor));
            }
            else
            {
                streamWriter.WriteLine(tbCatalogNumber.Text + "," + DateQualifier() + "," + tbUseDateStart.Text + "," + tbUseDateEnd.Text + "," + GetColorString(pickColor));
            }
            streamWriter.Flush();
        }

        private string DateQualifier()
        {
            string qualifier = "";
            if (rbExact.Checked == true)
            {
                qualifier = "exact";
            }
            else if (rbBetween.Checked == true)
            {
                qualifier = "between";
            }
            else if (rbCirca.Checked == true)
            {
                qualifier = "circa";
            }
            else if (rbBefore.Checked == true)
            {
                qualifier = "before";
            }
            else if (rbAfter.Checked == true)
            {
                qualifier = "after";
            }
            return qualifier;
        }

        private string GetColorString(Color color)
        {
            return "r" + color.R.ToString("d3") + "g" + color.G.ToString("d3") + "b" + color.B.ToString("d3");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //outputFile = new FileStream("newTable.txt", FileMode.OpenOrCreate, FileAccess.Write);
            streamWriter = new StreamWriter("newTable.txt", true);

            library = new ArtifactLibrary("large_sample_sorted.xml");
            currentArtifactId = 0;
            LoadArtifact(currentArtifactId);

            return;
        }

        private void btGoToArtifact_Click(object sender, EventArgs e)
        {
            int id;
            try
            {
                id = Convert.ToInt32(tbArtifactId.Text);
            }
            catch (Exception exception)
            {
                tbArtifactId.Text = "Invalid artifact id";
                return;
            }
            if (0 <= id && id < library.Artifacts.Count)
            {
                currentArtifactId = id;
                LoadArtifact();
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            Save();
            btSave.BackColor = Color.White;
            currentArtifactId++;
            LoadArtifact();
        }

        private void btPrevious_Click(object sender, EventArgs e)
        {
            currentArtifactId--;
            if (currentArtifactId < 0)
            {
                currentArtifactId = library.Artifacts.Count - 1;
            }
            LoadArtifact();
        }

        private void btNext_Click(object sender, EventArgs e)
        {
            currentArtifactId++;
            if (library.Artifacts.Count <= currentArtifactId)
            {
                currentArtifactId = 0;
            }
            LoadArtifact();
        }

        private void pictureBox_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouse = (MouseEventArgs)e;
            pickColor = letterboxed.GetPixel(mouse.X, mouse.Y);
            btSave.BackColor = pickColor;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (streamWriter != null)
            {
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        private void tbQualifier_TextChanged(object sender, EventArgs e)
        {
            if (tbUseQualifier.Text == "e")
            {
                rbExact.Checked = true;
            }
            else if (tbUseQualifier.Text == "b")
            {
                rbBetween.Checked = true;
            }
            else if (tbUseQualifier.Text == "c")
            {
                rbCirca.Checked = true;
            }
            else if (tbUseQualifier.Text == "a")
            {
                rbAfter.Checked = true;
            }
            else if (tbUseQualifier.Text == "f")
            {
                rbBefore.Checked = true;
            }
        }

        private void tbQualifier_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                btSave_Click(sender, e);
            }
        }
    }
}

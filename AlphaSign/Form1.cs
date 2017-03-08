using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Florentis;
using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.io;
using PdfUtils;
using PdfiumViewer;

namespace AlphaSign
{
    public partial class AlphaSign : Form
    {
        public string defaultPdf = "Lorem Ipsum.pdf";
        public bool defaultLoaded = false;

        public string originalFile = "";
        public string currentFile = "";

        public string name = "Name";
        public string reason = "Reason";

        public AlphaSign()
        {
            InitializeComponent();
        }

        private void loadDefaultPDF()
        {
            loadPdf(defaultPdf);
            defaultLoaded = true;
        }

        private void loadPdf(string pdfPath)
        {
            loadPdf(pdfPath, true);
        }

        private void loadPdf(string pdfPath, bool changeOriginal)
        {
            
            //pdfRenderer1.Load(PdfiumViewer.PdfDocument.Load(pdfPath));
            pdfRenderer1.Document = PdfiumViewer.PdfDocument.Load(pdfPath);
            pdfRenderer1.Show();
            
            currentFile = pdfPath;

            if (changeOriginal)
            {
                originalFile = pdfPath;
            }
            defaultLoaded = false;
            this.Text = "AlphaSign - " + originalFile;

        }

        private void AlphaSign_Load(object sender, EventArgs e)
        {
            loadDefaultPDF();
        }

        private void signToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sign();
        }

        public void sign()
        {
            SigCtl sigCtl = new SigCtl();
            sigCtl.Licence = Properties.Settings.Default.License;
            DynamicCapture dc = new DynamicCaptureClass();
            DynamicCaptureResult res = dc.Capture(sigCtl, name, reason, null, null);

            if (res == DynamicCaptureResult.DynCaptOK)
            {
                SigObj sigObj = (SigObj)sigCtl.Signature;
                //sigObj.set_ExtraData("AdditionalData", "C# test: Additional data");

                String filename = System.IO.Path.GetTempFileName();
                try
                {
                    sigObj.RenderBitmap(filename, 400, 200, "image/png", 0.5f, 0xff0000, 0xffffff, 10.0f, 10.0f, RBFlags.RenderOutputFilename | RBFlags.RenderColor32BPP | RBFlags.RenderEncodeData | RBFlags.RenderBackgroundTransparent);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                string newFile = InsertSignatureImageToPdf(filename);
                loadPdf(newFile, false);
                //getSignatureFromPdf(signedPdf);

            }
            
            
        }

        public void save()
        {
            save(originalFile);
        }

        public void save(string path)
        {
            FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            FileStream fsIn = new FileStream(currentFile, FileMode.Open, FileAccess.Read);
            fsIn.CopyTo(fsOut);

            fsIn.Close();
            fsOut.Close();
        }

        public void saveAs()
        {
            saveFileDialog1.Filter = "PDF Documents | *.pdf";
            if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                save(saveFileDialog1.FileName);
                loadPdf(saveFileDialog1.FileName, true);
            }
        }

        public string insertSealToPdf(Bitmap bmp)
        {
            string sealFile = System.IO.Path.GetTempFileName();
            bmp.Save(sealFile, ImageFormat.Png);

            eSeal elecSeal = new eSeal();
            elecSeal.URL = sealFile;
            SigCtl sigCtl = new SigCtl();
            eSealCaptureResult res = elecSeal.Capture(sigCtl, eSealCaptureMode.esRequireSignature, name, reason);

            if (res == eSealCaptureResult.esCaptureOK)
            {
                SigObj sigObj = (SigObj)sigCtl.Signature;
                //sigObj.set_ExtraData("AdditionalData", "C# test: Additional data");

                String filename = System.IO.Path.GetTempFileName();
                try
                {
                    sigObj.RenderBitmap(filename, 600, 300, "image/png", 0.5f, 0xff0000, 0xffffff, 10.0f, 10.0f, RBFlags.RenderOutputFilename | RBFlags.RenderColor32BPP | RBFlags.RenderEncodeData| RBFlags.RenderBackgroundTransparent | RBFlags.RenderClipped);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                string newFile = InsertSignatureImageToPdf(filename);
                loadPdf(newFile, false);

                return newFile;
            }

            return "";
            
        }

        private string InsertSignatureImageToPdf(string imageFileName)
        {
            string signedFile = System.IO.Path.GetTempFileName();

            PdfReader reader = new PdfReader(currentFile);
            FileStream fs = new FileStream(signedFile, FileMode.Create, FileAccess.Write);
            PdfStamper stamper = new PdfStamper(reader, fs);
            PdfContentByte cb = stamper.GetOverContent(1);

            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageFileName);
            image.SetAbsolutePosition(40, 110);
            image.ScalePercent(50f, 50f);

            cb.AddImage(image);
            stamper.Close();
            fs.Close();

            currentFile = signedFile;

            return signedFile;
        }

        private void getSignatureFromPdf(string pdf_filename)
        {
            bool imagesFound = PdfImageExtractor.PageContainsImages(pdf_filename, 1);

            if (imagesFound)
            {
                Dictionary<string, System.Drawing.Image> dict = PdfImageExtractor.ExtractImages(pdf_filename, 1);
                foreach (string key in dict.Keys)
                {
                    System.Drawing.Image img = dict[key];
                    img.Save("sign.png");

                    SigObj sig = new SigObj();
                    ReadEncodedBitmapResult result = sig.ReadEncodedBitmap("sign.png");
                    if (result == ReadEncodedBitmapResult.ReadEncodedBitmapOK)
                    {
                        MessageBox.Show(sig.Who + " " + sig.Why + " " + sig.When);
                    }
                }
            }

        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "PDF Documents | *.pdf";
            openFileDialog1.Multiselect = false;
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {   
                    loadPdf(openFileDialog1.FileName);
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("File not found");
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
        }

        private void mnuInfo_Click(object sender, EventArgs e)
        {
            frmSignatureInfo info = new frmSignatureInfo();
            info.loadSignInfo(currentFile);
            info.ShowDialog();
            
        }

        private void sealToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSealInfo sealInfo = new frmSealInfo();
            sealInfo.stampFn = insertSealToPdf;
            sealInfo.ShowDialog();

            
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
        }



    }
}

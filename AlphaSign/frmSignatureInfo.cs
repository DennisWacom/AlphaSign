using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

namespace AlphaSign
{
    public partial class frmSignatureInfo : Form
    {
        public frmSignatureInfo()
        {
            InitializeComponent();
        }

        public void loadSignInfo(string pdf_filename)
        {
            PdfReader reader = new PdfReader(pdf_filename);
            
            Dictionary<string, System.Drawing.Image> dict = PdfImageExtractor.ExtractImages(pdf_filename);
            int sigIndex = 1;

            foreach (string key in dict.Keys)
            {
                System.Drawing.Image img = dict[key];

                MemoryStream ms = new MemoryStream();
                if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Tiff);
                }
                else if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                {
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                }

                SigObj sig = new SigObj();
                ReadEncodedBitmapResult result = sig.ReadEncodedBitmap(ms.ToArray());
                if (result == ReadEncodedBitmapResult.ReadEncodedBitmapOK)
                {
                    //MessageBox.Show(sig.Who + " " + sig.Why + " " + sig.When);
                    treeView1.BeginUpdate();
                    treeView1.Nodes.Add("Signature " + sigIndex);
                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Name: " + sig.Who);
                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Reason: " + sig.Why);
                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Timestamp: " + sig.When);

                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Digitizer: " + sig.get_AdditionalData(CaptData.CaptDigitizer));
                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Digitizer Driver: " + sig.get_AdditionalData(CaptData.CaptDigitizerDriver));
                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Machine OS: " + sig.get_AdditionalData(CaptData.CaptMachineOS));
                    treeView1.Nodes[sigIndex - 1].Nodes.Add("Network Card: " + sig.get_AdditionalData(CaptData.CaptNetworkCard));
                    treeView1.EndUpdate();

                    sigIndex = sigIndex + 1;
                }

                treeView1.ExpandAll();
            }
            
        }

        private void frmSignatureInfo_Load(object sender, EventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlphaSign;

namespace AlphaSign
{
    public delegate string stampSealOnPdf(Bitmap seal);

    public partial class frmSealInfo : Form
    {
        Bitmap seal;
        public stampSealOnPdf stampFn;
    
        public frmSealInfo()
        {
            InitializeComponent();

        }

        public void createSeal()
        {
            seal = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(seal);
            g.Clear(Color.White);
            Pen pen = new Pen(Color.Red, 3);
            g.DrawRectangle(pen, 5, 5, 90, 90);
            
            StringFormat sf = new StringFormat();
            sf.FormatFlags = StringFormatFlags.DirectionVertical;

            Font f = new Font(FontFamily.GenericSerif, 28);

            g.DrawString(txtFirstName.Text,
                    f,
                    new System.Drawing.SolidBrush(Color.Red),
                    0f, 3f, sf);

            g.DrawString(txtLastName.Text, f, new System.Drawing.SolidBrush(Color.Red), 47f, 3f, sf);
            pictureBox1.Image = seal;
            
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            
            stampFn(seal);
            this.Close();
        }

        private void txtFirstName_TextChanged(object sender, EventArgs e)
        {
            createSeal();
        }

        private void txtLastName_TextChanged(object sender, EventArgs e)
        {
            createSeal();
        }

        private void frmSealInfo_Shown(object sender, EventArgs e)
        {
            txtLastName.Text = "签章";
            txtFirstName.Text = "姓名";
            
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}

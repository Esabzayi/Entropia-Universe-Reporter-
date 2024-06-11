using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Entropia
{
    public partial class FrmcustomValue : DevExpress.XtraEditors.XtraForm
    {
        public FrmcustomValue()
        {
            InitializeComponent();
        }

        private void txtCustomValue_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void txtCustomValue_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
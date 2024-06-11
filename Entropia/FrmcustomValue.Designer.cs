namespace Entropia
{
    partial class FrmcustomValue
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.layoutConverter1 = new DevExpress.XtraLayout.Converter.LayoutConverter(this.components);
            this.FrmcustomValuelayoutControl1ConvertedLayout = new DevExpress.XtraLayout.LayoutControl();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.txtCustomValue = new DevExpress.XtraEditors.CalcEdit();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.FrmcustomValuelayoutControl1ConvertedLayout)).BeginInit();
            this.FrmcustomValuelayoutControl1ConvertedLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomValue.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            this.SuspendLayout();
            // 
            // FrmcustomValuelayoutControl1ConvertedLayout
            // 
            this.FrmcustomValuelayoutControl1ConvertedLayout.Controls.Add(this.txtCustomValue);
            this.FrmcustomValuelayoutControl1ConvertedLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FrmcustomValuelayoutControl1ConvertedLayout.Location = new System.Drawing.Point(0, 0);
            this.FrmcustomValuelayoutControl1ConvertedLayout.Name = "FrmcustomValuelayoutControl1ConvertedLayout";
            this.FrmcustomValuelayoutControl1ConvertedLayout.Root = this.layoutControlGroup1;
            this.FrmcustomValuelayoutControl1ConvertedLayout.Size = new System.Drawing.Size(401, 50);
            this.FrmcustomValuelayoutControl1ConvertedLayout.TabIndex = 1;
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1});
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(401, 50);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // txtCustomValue
            // 
            this.txtCustomValue.Location = new System.Drawing.Point(12, 12);
            this.txtCustomValue.Name = "txtCustomValue";
            this.txtCustomValue.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.txtCustomValue.Size = new System.Drawing.Size(377, 26);
            this.txtCustomValue.StyleController = this.FrmcustomValuelayoutControl1ConvertedLayout;
            this.txtCustomValue.TabIndex = 4;
            this.txtCustomValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCustomValue_KeyDown_1);
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.txtCustomValue;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(381, 30);
            this.layoutControlItem1.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem1.TextVisible = false;
            // 
            // FrmcustomValue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(401, 50);
            this.Controls.Add(this.FrmcustomValuelayoutControl1ConvertedLayout);
            this.MaximizeBox = false;
            this.Name = "FrmcustomValue";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Custom Value";
            ((System.ComponentModel.ISupportInitialize)(this.FrmcustomValuelayoutControl1ConvertedLayout)).EndInit();
            this.FrmcustomValuelayoutControl1ConvertedLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomValue.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private DevExpress.XtraLayout.LayoutControl FrmcustomValuelayoutControl1ConvertedLayout;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraLayout.Converter.LayoutConverter layoutConverter1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        public DevExpress.XtraEditors.CalcEdit txtCustomValue;
    }
}
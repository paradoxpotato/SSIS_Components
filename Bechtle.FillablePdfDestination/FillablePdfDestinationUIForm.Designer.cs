namespace Bechtle.FillablePdfDestination
{
    partial class FillablePdfDestinationUIForm
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
            this.dialogFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.dialogTemplate = new System.Windows.Forms.OpenFileDialog();
            this.pnlDgv = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.grBxSettings = new System.Windows.Forms.GroupBox();
            this.lblNameBuilder = new System.Windows.Forms.Label();
            this.btnName = new System.Windows.Forms.Button();
            this.lblFolder = new System.Windows.Forms.Label();
            this.lblTemplate = new System.Windows.Forms.Label();
            this.btnFolder = new System.Windows.Forms.Button();
            this.btnTemplate = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.pnlDgv.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.grBxSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlDgv
            // 
            this.pnlDgv.Controls.Add(this.dataGridView1);
            this.pnlDgv.Location = new System.Drawing.Point(13, 22);
            this.pnlDgv.Name = "pnlDgv";
            this.pnlDgv.Size = new System.Drawing.Size(557, 208);
            this.pnlDgv.TabIndex = 0;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dataGridView1.Size = new System.Drawing.Size(557, 208);
            this.dataGridView1.StandardTab = true;
            this.dataGridView1.TabIndex = 0;
            // 
            // grBxSettings
            // 
            this.grBxSettings.Controls.Add(this.lblNameBuilder);
            this.grBxSettings.Controls.Add(this.btnName);
            this.grBxSettings.Controls.Add(this.lblFolder);
            this.grBxSettings.Controls.Add(this.lblTemplate);
            this.grBxSettings.Controls.Add(this.btnFolder);
            this.grBxSettings.Controls.Add(this.btnTemplate);
            this.grBxSettings.Location = new System.Drawing.Point(13, 237);
            this.grBxSettings.Name = "grBxSettings";
            this.grBxSettings.Size = new System.Drawing.Size(557, 118);
            this.grBxSettings.TabIndex = 1;
            this.grBxSettings.TabStop = false;
            this.grBxSettings.Text = "Settings";
            // 
            // lblNameBuilder
            // 
            this.lblNameBuilder.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblNameBuilder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblNameBuilder.Location = new System.Drawing.Point(98, 85);
            this.lblNameBuilder.Name = "lblNameBuilder";
            this.lblNameBuilder.Size = new System.Drawing.Size(453, 18);
            this.lblNameBuilder.TabIndex = 5;
            this.lblNameBuilder.Text = "lblNameBuilder";
            // 
            // btnName
            // 
            this.btnName.Location = new System.Drawing.Point(7, 80);
            this.btnName.Name = "btnName";
            this.btnName.Size = new System.Drawing.Size(85, 23);
            this.btnName.TabIndex = 4;
            this.btnName.Text = "Name Builder";
            this.btnName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnName.UseVisualStyleBackColor = true;
            this.btnName.Click += new System.EventHandler(this.btnName_Click);
            // 
            // lblFolder
            // 
            this.lblFolder.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblFolder.Location = new System.Drawing.Point(98, 55);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(453, 18);
            this.lblFolder.TabIndex = 3;
            this.lblFolder.Text = "lblFolder";
            // 
            // lblTemplate
            // 
            this.lblTemplate.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblTemplate.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblTemplate.Location = new System.Drawing.Point(98, 25);
            this.lblTemplate.Name = "lblTemplate";
            this.lblTemplate.Size = new System.Drawing.Size(453, 18);
            this.lblTemplate.TabIndex = 2;
            this.lblTemplate.Text = "lblTemplate";
            // 
            // btnFolder
            // 
            this.btnFolder.Location = new System.Drawing.Point(7, 50);
            this.btnFolder.Name = "btnFolder";
            this.btnFolder.Size = new System.Drawing.Size(85, 23);
            this.btnFolder.TabIndex = 1;
            this.btnFolder.Text = "&Folder Path";
            this.btnFolder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnFolder.UseVisualStyleBackColor = false;
            this.btnFolder.Click += new System.EventHandler(this.btnFolder_Click);
            // 
            // btnTemplate
            // 
            this.btnTemplate.Location = new System.Drawing.Point(7, 20);
            this.btnTemplate.Name = "btnTemplate";
            this.btnTemplate.Size = new System.Drawing.Size(85, 23);
            this.btnTemplate.TabIndex = 0;
            this.btnTemplate.Text = "&Template Path";
            this.btnTemplate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTemplate.UseVisualStyleBackColor = false;
            this.btnTemplate.Click += new System.EventHandler(this.btnTemplate_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(201, 367);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(282, 367);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // FillablePdfDestinationUIForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(582, 402);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.grBxSettings);
            this.Controls.Add(this.pnlDgv);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FillablePdfDestinationUIForm";
            this.Text = "FillablePdfDestinationUIForm";
            this.Load += new System.EventHandler(this.FillablePdfDestinationUIForm_Load);
            this.pnlDgv.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.grBxSettings.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog dialogFolder;
        private System.Windows.Forms.OpenFileDialog dialogTemplate;
        private System.Windows.Forms.Panel pnlDgv;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.GroupBox grBxSettings;
        private System.Windows.Forms.Label lblNameBuilder;
        private System.Windows.Forms.Button btnName;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblTemplate;
        private System.Windows.Forms.Button btnFolder;
        private System.Windows.Forms.Button btnTemplate;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;

    }
}
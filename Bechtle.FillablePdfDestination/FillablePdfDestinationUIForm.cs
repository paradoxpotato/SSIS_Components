// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FillablePdfDestinationUIForm.cs" company="">
//   
// </copyright>
// <summary>
//   The fillable pdf destination ui form.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Bechtle.FillablePdfDestination
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Windows.Forms;

    using iTextSharp.text.pdf;

    using Microsoft.SqlServer.Dts.Runtime.Wrapper;

    /// <summary>
    /// The fillable pdf destination ui form.
    /// </summary>
    public partial class FillablePdfDestinationUIForm : Form
    {
        /// <summary>
        /// The component configuration.
        /// </summary>
        private ComponentConfiguration componentConfiguration;

        /// <summary>
        /// The input column infos.
        /// </summary>
        private InputColumnInfo[] inputColumnInfos;

        /// <summary>
        /// The input config json string.
        /// </summary>
        private string inputConfigJsonString;

        /// <summary>
        /// Initializes a new instance of the <see cref="FillablePdfDestinationUIForm"/> class.
        /// </summary>
        /// <param name="inputConfigJsonString">
        /// The input config json string.
        /// </param>
        /// <param name="inputColumnInfos">
        /// The input column infos.
        /// </param>
        public FillablePdfDestinationUIForm(string inputConfigJsonString, InputColumnInfo[] inputColumnInfos)
            : this()
        {
            this.inputConfigJsonString = inputConfigJsonString;
            this.componentConfiguration = ComponentConfiguration.CreateFromJson(this.inputConfigJsonString);
            this.inputColumnInfos = inputColumnInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FillablePdfDestinationUIForm"/> class.
        /// </summary>
        public FillablePdfDestinationUIForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// The build form.
        /// </summary>
        private void BuildForm()
        {
            this.btnOK.Enabled = false;
            this.lblTemplate.Text = this.componentConfiguration.TemplatePath;
            this.lblFolder.Text = this.componentConfiguration.FolderPath;
            this.lblNameBuilder.Text = this.componentConfiguration.FormatString;

            if (!string.IsNullOrEmpty(this.lblTemplate.Text) && !string.IsNullOrEmpty(this.lblFolder.Text)
                && !string.IsNullOrEmpty(this.lblNameBuilder.Text))
            {
                this.btnOK.Enabled = true;
            }
  
            this.dataGridView1.Columns.Clear();
            DataTable fieldData = this.GetDatatable();

            this.dataGridView1.DataSource = fieldData;
            this.dataGridView1.Columns["dc_colLineageID"].Visible = false;
            this.dataGridView1.Columns["dc_ColumnName"].Visible = false;
            this.dataGridView1.Columns["dc_fieldType"].Visible = false;
            this.dataGridView1.Columns["dc_fieldTypeName"].HeaderText = "Field-Type";
            this.dataGridView1.Columns["dc_fieldName"].HeaderText = "Template-Field";

            foreach (DataGridViewColumn dc in this.dataGridView1.Columns)
            {
                dc.ReadOnly = true;
            }

            DataGridViewComboBoxColumn cbxColColumnNames = new DataGridViewComboBoxColumn();
            cbxColColumnNames.ReadOnly = false;
            cbxColColumnNames.Name = "dgvc_MappedColumnName";
            cbxColColumnNames.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cbxColColumnNames.HeaderText = "Column";

            this.dataGridView1.Columns.Add(cbxColColumnNames);

            foreach (DataGridViewRow gridViewRow in this.dataGridView1.Rows)
            {
                DataGridViewComboBoxCell comboBoxCell =
                    (DataGridViewComboBoxCell)gridViewRow.Cells["dgvc_MappedColumnName"];
                comboBoxCell.Items.Add("--");
                comboBoxCell.Value = comboBoxCell.Items[0];

                foreach (InputColumnInfo info in this.inputColumnInfos)
                { 
                    // ToDo Überarbeiten
                    ComponentConfiguration.FieldDataSet dataSet = this.componentConfiguration.FieldDataSets.First(fds => fds.FieldName == gridViewRow.Cells["dc_fieldName"].Value.ToString());
                    if (this.ColumnTypeFitsToFieldType(info.DataType, dataSet.FieldTypeId))
                    {
                        comboBoxCell.Items.Add(info.ColumnName);
                    }

                    if (comboBoxCell.Items.Contains(dataSet.ColumnName))
                    {
                        comboBoxCell.Value = dataSet.ColumnName;
                    }
                }

                // ToDo letzte gewählte Spalte in Combobox anzeigen
            }
        }

        /// <summary>
        /// The column type fits to field type.
        /// </summary>
        /// <param name="dataType">
        /// The data type.
        /// </param>
        /// <param name="fieldTypeId">
        /// The field type id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ColumnTypeFitsToFieldType(DataType dataType, int fieldTypeId)
        {
            // ToDo ColumnTypeFitsToFieldType implementieren
            
            if (dataType == DataType.DT_IMAGE || dataType == DataType.DT_TEXT)
            {
                return false;
            }

            if (fieldTypeId == 2)
            {
                if (dataType != DataType.DT_BOOL && dataType != DataType.DT_BYREF_BOOL)
                {
                    return false;
                }   
            }

            return true;
        }

        /// <summary>
        /// The get datatable.
        /// </summary>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        private DataTable GetDatatable()
        {
            DataTable dt = new DataTable();

            DataColumn dcFieldName = dt.Columns.Add();
            dcFieldName.ColumnName = "dc_fieldName";
            dcFieldName.Caption = "Template Field";
            dcFieldName.DataType = typeof(string);

            DataColumn dcFieldType = dt.Columns.Add();
            dcFieldType.ColumnName = "dc_fieldType";
            dcFieldType.Caption = "Type";
            dcFieldType.DataType = typeof(int);

            DataColumn dcFieldTypeName = dt.Columns.Add();
            dcFieldTypeName.ColumnName = "dc_fieldTypeName";
            dcFieldTypeName.Caption = "Type";
            dcFieldTypeName.DataType = typeof(string);

            DataColumn dcLineageID = dt.Columns.Add();
            dcLineageID.ColumnName = "dc_colLineageID";
            dcLineageID.Caption = "LineageID";
            dcLineageID.DataType = typeof(int);


            DataColumn dcColumnName = dt.Columns.Add();
            dcColumnName.ColumnName = "dc_ColumnName";
            dcColumnName.Caption = "ColumnName";
            dcColumnName.DataType = typeof(string);

            foreach (ComponentConfiguration.FieldDataSet dataSet in this.componentConfiguration.FieldDataSets)
            {
                DataRow dr = dt.NewRow();
                dr[dcFieldName] = dataSet.FieldName;
                dr[dcFieldType] = dataSet.FieldTypeId;
                dr[dcFieldTypeName] = dataSet.FieldTypeName;
                dr[dcColumnName] = dataSet.ColumnName;
                dr[dcLineageID] = 0;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        // ToDo Update-Config überarbeiten

        /// <summary>
        /// The update config.
        /// </summary>
        /// <returns>
        /// The <see cref="ComponentConfiguration"/>.
        /// </returns>
        private ComponentConfiguration UpdateConfigFromUI()
        {
            ComponentConfiguration cfg = new ComponentConfiguration()
                                             {
                                                 FieldDataSets = new List<ComponentConfiguration.FieldDataSet>(), 
                                                 FileNameFormat = new ComponentConfiguration.NameFormat(), 
                                                 FolderPath = this.lblFolder.Text,
                                                 TemplatePath = this.lblTemplate.Text,
                                                 FormatString = this.lblNameBuilder.Text
                                             };

            foreach (DataGridViewRow dgvRow in this.dataGridView1.Rows)
            {
                ComponentConfiguration.FieldDataSet fds = new ComponentConfiguration.FieldDataSet()
                                                              {
                                                                  FieldName =
                                                                      dgvRow.Cells
                                                                      [
                                                                          "dc_fieldName"
                                                                      ].Value
                                                                      .ToString(),
                                                                  FieldTypeId =int.Parse(dgvRow.Cells["dc_fieldType"].Value.ToString()),
                                                                  ColumnName = 
                                                                      dgvRow.Cells["dgvc_MappedColumnName"].Value.ToString(),
                                                              };
                cfg.FieldDataSets.Add(fds);
            }

            return cfg;
        }

        /// <summary>
        /// The get field data from file.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private List<ComponentConfiguration.FieldDataSet> GetFieldDataFromFile()
        {
            PdfReader pr = new PdfReader(this.componentConfiguration.TemplatePath);
            List<ComponentConfiguration.FieldDataSet> fieldDataSetsFromFile =
                new List<ComponentConfiguration.FieldDataSet>();

            foreach (var v in pr.AcroFields.Fields)
            {
                ComponentConfiguration.FieldDataSet dataSet = new ComponentConfiguration.FieldDataSet(
                    v.Key, 
                    pr.AcroFields.GetFieldType(v.Key), 
                    "Egal", 
                    "--", 
                    string.Empty);
                fieldDataSetsFromFile.Add(dataSet);
            }

            pr.Close();

            if (fieldDataSetsFromFile.Count == 0)
            {
                MessageBox.Show("This template doesn't contain any fillable fields");
            }

            return fieldDataSetsFromFile;
        }

        /// <summary>
        /// The btn template_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnTemplate_Click(object sender, EventArgs e)
        {
            this.dialogTemplate.ShowDialog();

            if (this.dialogTemplate.CheckFileExists)
            {
                this.lblTemplate.Text = this.dialogTemplate.FileName;
                this.componentConfiguration = this.UpdateConfigFromUI();
                this.componentConfiguration.FieldDataSets = this.GetFieldDataFromFile();
                this.BuildForm();
            }
        }

        /// <summary>
        /// The btn folder_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnFolder_Click(object sender, EventArgs e)
        {
            this.dialogFolder.ShowDialog();

            if (!string.IsNullOrEmpty(this.dialogFolder.SelectedPath))
            {
                this.lblFolder.Text = this.dialogFolder.SelectedPath;
                this.componentConfiguration = this.UpdateConfigFromUI();
                this.BuildForm();
            }
        }

        /// <summary>
        /// The btn name_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnName_Click(object sender, EventArgs e)
        {
            NameBuilderForm nameBuilderForm = new NameBuilderForm(inputColumnInfos.ToList(), this.componentConfiguration);
            nameBuilderForm.ShowDialog();
            this.lblNameBuilder.Text = this.componentConfiguration.FormatString;
            this.componentConfiguration = this.UpdateConfigFromUI();
            this.BuildForm();
        }

        /// <summary>
        /// The fillable pdf destination ui form_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void FillablePdfDestinationUIForm_Load(object sender, EventArgs e)
        {
            this.BuildForm();
        }


        /// <summary>
        /// The btn o k_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.componentConfiguration = this.UpdateConfigFromUI();
            this.outputConfigJsonString = this.componentConfiguration.ToJsonString();
            this.DialogResult = DialogResult.OK;
            this.Dispose();
        }

        /// <summary>
        /// The btn cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Gets or sets the output config json string.
        /// </summary>
        public string outputConfigJsonString { get; set; }
    }
}
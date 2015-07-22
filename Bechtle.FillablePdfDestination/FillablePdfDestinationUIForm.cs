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
            this.lblTemplate.Text = this.componentConfiguration.TemplatePath;
            this.lblFolder.Text = this.componentConfiguration.FolderPath;
            this.lblNameBuilder.Text = this.componentConfiguration.FormatString;

            this.dataGridView1.Columns.Clear();
            DataTable fieldData = this.GetDatatable();

            this.dataGridView1.DataSource = fieldData;
            var dataGridViewColumn = this.dataGridView1.Columns["col_LineageID"];

            // ToDo Spalten wenn möglich gemeinsam unsichtbar machen
            if (dataGridViewColumn != null)
            {
                dataGridViewColumn.Visible = false;
            }

            var viewColumn = this.dataGridView1.Columns["col_Name"];
            if (viewColumn != null)
            {
                viewColumn.Visible = false;
            }

            foreach (DataGridViewColumn dc in this.dataGridView1.Columns)
            {
                dc.ReadOnly = true;
            }

            DataGridViewComboBoxColumn cbxColColumnNames = new DataGridViewComboBoxColumn();

            cbxColColumnNames.ReadOnly = false;
            cbxColColumnNames.Name = "dgvcol_MappedColumn";
            cbxColColumnNames.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

            this.dataGridView1.Columns.Add(cbxColColumnNames);

            foreach (DataGridViewRow gridViewRow in this.dataGridView1.Rows)
            {
                DataGridViewComboBoxCell comboBoxCell =
                    (DataGridViewComboBoxCell)gridViewRow.Cells["dgvcol_MappedColumn"];
                comboBoxCell.Items.Add("--");
                comboBoxCell.Value = comboBoxCell.Items[0];

                foreach (InputColumnInfo info in this.inputColumnInfos)
                {
                    ComponentConfiguration.FieldDataSet dataSet =
                        this.componentConfiguration.FieldDataSets.First(
                            fds => fds.FieldName == gridViewRow.Cells["field_Name"].Value.ToString());

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
            dcFieldName.ColumnName = "field_Name";
            dcFieldName.Caption = "Template Field";
            dcFieldName.DataType = typeof(string);

            DataColumn dcFieldType = dt.Columns.Add();
            dcFieldType.ColumnName = "field_Type";
            dcFieldType.Caption = "Type";
            dcFieldType.DataType = typeof(int);

            DataColumn dcLineageID = dt.Columns.Add();
            dcLineageID.ColumnName = "col_LineageID";
            dcLineageID.Caption = "LineageID";
            dcLineageID.DataType = typeof(int);


            DataColumn dcColumnName = dt.Columns.Add();
            dcColumnName.ColumnName = "col_Name";
            dcColumnName.Caption = "ColumnName";
            dcColumnName.DataType = typeof(string);

            foreach (ComponentConfiguration.FieldDataSet dataSet in this.componentConfiguration.FieldDataSets)
            {
                DataRow dr = dt.NewRow();
                dr[dcFieldName] = dataSet.FieldName;
                dr[dcFieldType] = dataSet.FieldTypeId;
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
        private ComponentConfiguration UpdateConfig()
        {
            ComponentConfiguration cfg = new ComponentConfiguration()
                                             {
                                                 FieldDataSets = new List<ComponentConfiguration.FieldDataSet>(), 
                                                 FileNameFormat = new ComponentConfiguration.NameFormat(), 
                                                 FolderPath = this.componentConfiguration.FolderPath, 
                                                 TemplatePath = this.componentConfiguration.TemplatePath,
                                                  FormatString = this.componentConfiguration.FormatString
                                             };

            foreach (DataGridViewRow dgvRow in this.dataGridView1.Rows)
            {
                ComponentConfiguration.FieldDataSet fds = new ComponentConfiguration.FieldDataSet()
                                                              {
                                                                  FieldName =
                                                                      dgvRow.Cells
                                                                      [
                                                                          "field_Name"
                                                                      ].Value
                                                                      .ToString(), 
                                                                  FieldTypeName =
                                                                      "Coming Soon", 
                                                                  FieldTypeId =int.Parse(dgvRow.Cells["field_Type"].Value.ToString()),
                                                                  ColumnName = 
                                                                      dgvRow.Cells["dgvcol_MappedColumn"].Value.ToString
                                                                      (),
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
                this.componentConfiguration.TemplatePath = this.dialogTemplate.FileName;
            }

            this.componentConfiguration.FieldDataSets = this.GetFieldDataFromFile();
            this.BuildForm();
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

            if (this.dialogFolder.SelectedPath.Length > 0)
            {
                this.componentConfiguration.FolderPath = this.dialogFolder.SelectedPath;
                this.lblFolder.Text = componentConfiguration.FolderPath;
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
            this.componentConfiguration = this.UpdateConfig();
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
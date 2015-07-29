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
    ///     Editor to configure the Component
    /// </summary>
    public partial class FillablePdfDestinationUIForm : Form
    {
        /// <summary>
        ///     Contains information about the InputColumns, saved in an array of custom structs
        /// </summary>
        private readonly InputColumnInfo[] inputColumnInfos;

        /// <summary>
        ///     Saved in the MetaData this string is is parsed to the Config-Object
        /// </summary>
        private readonly string inputConfigJsonString;

        /// <summary>
        ///     Instance of the Class ComponentConfiguration, contains all settings made in the UI
        /// </summary>
        private ComponentConfiguration componentConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="FillablePdfDestinationUIForm"/> class. 
        /// Constructor for the Editor<see cref="FillablePdfDestinationUIForm"/>
        /// </summary>
        /// <param name="inputConfigJsonString">
        /// The inputString that is parsed to the ConfigObject
        /// </param>
        /// <param name="inputColumnInfos">
        /// Information about the input colum
        /// </param>
        public FillablePdfDestinationUIForm(string inputConfigJsonString, InputColumnInfo[] inputColumnInfos)

            // Uses the base constructor that only Initializes the component
            : this()
        {
            // Initialize local fields from the constructor
            this.inputConfigJsonString = inputConfigJsonString;
            this.componentConfiguration = ComponentConfiguration.CreateFromJson(this.inputConfigJsonString);
            this.inputColumnInfos = inputColumnInfos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FillablePdfDestinationUIForm"/> class. 
        ///     Constructor for the Editor<see cref="FillablePdfDestinationUIForm"/> class.
        /// </summary>
        public FillablePdfDestinationUIForm()
        {
            this.InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the output config json string, which is saved in the ComponentMetaData later on
        /// </summary>
        public string OutputConfigJsonString { get; set; }

        /// <summary>
        ///     Sets the properties of the controls of the form from the global config-object
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

            // Creates Datatable from Config and sets it as the datasource of the DataGridview
            var fieldData = this.GetDatatable();
            this.dataGridView1.DataSource = fieldData;

            // Hides irrelevant columns
            var gridViewColumn = this.dataGridView1.Columns["dc_colLineageID"];
            if (gridViewColumn != null)
            {
                gridViewColumn.Visible = false;
            }

            var dataGridViewColumn = this.dataGridView1.Columns["dc_ColumnName"];
            if (dataGridViewColumn != null)
            {
                dataGridViewColumn.Visible = false;
            }

            var viewColumn = this.dataGridView1.Columns["dc_fieldType"];
            if (viewColumn != null)
            {
                viewColumn.Visible = false;
            }

            // Sets new header-text to the datagridview colums
            var column = this.dataGridView1.Columns["dc_fieldTypeName"];
            if (column != null)
            {
                column.HeaderText = "Field-Type";
            }

            var dataGridViewColumn1 = this.dataGridView1.Columns["dc_fieldName"];

            if (dataGridViewColumn1 != null)
            {
                dataGridViewColumn1.HeaderText = "Template-Field";
            }

            // Makes columns readonly
            foreach (DataGridViewColumn dc in this.dataGridView1.Columns)
            {
                dc.ReadOnly = true;
            }

            // Creates new ComboboxColumn and adds it to the Datagridview
            var cbxColColumnNames = new DataGridViewComboBoxColumn();
            cbxColColumnNames.ReadOnly = false;
            cbxColColumnNames.Name = "dgvc_MappedColumnName";
            cbxColColumnNames.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cbxColColumnNames.HeaderText = "Column";

            this.dataGridView1.Columns.Add(cbxColColumnNames);

            foreach (DataGridViewRow gridViewRow in this.dataGridView1.Rows)
            {
                var comboBoxCell = (DataGridViewComboBoxCell)gridViewRow.Cells["dgvc_MappedColumnName"];

                // Each Combobox contains the Item "--" which represents if no column is selected
                comboBoxCell.Items.Add("--");

                // By default the selected Item is Items[0], or "--", if no changes are made later on
                comboBoxCell.Value = comboBoxCell.Items[0];

                // Loops over all InputColumInfos
                foreach (var info in this.inputColumnInfos)
                {
                    // Gather the corresponding FieldDataSet by selecting the first (and only) Dataset from the collection via Linq
                    var dataSet =
                        this.componentConfiguration.FieldDataSets.First(
                            fds => fds.FieldName == gridViewRow.Cells["dc_fieldName"].Value.ToString());

                    // If the ColumnType of the ColumnInfo is compatible to the Field-Datatype, it's column name is added to the Combobox-Items
                    if (this.ColumnTypeFitsToFieldType(info.DataType, dataSet.FieldTypeId))
                    {
                        comboBoxCell.Items.Add(info.ColumnName);
                    }

                    // If the Combobox-Item in the current cell contain the name of the column mapped to the assigned Template-Field, the corresponding item is selected automatically
                    if (comboBoxCell.Items.Contains(dataSet.ColumnName))
                    {
                        comboBoxCell.Value = dataSet.ColumnName;
                    }
                }

                // ToDo letzte gewählte Spalte in Combobox anzeigen
            }
        }

        /// <summary>
        /// Checks if the ColumnType of an InputColumn is compatible to the assigned TemplateField
        /// </summary>
        /// <param name="dataType">
        /// The data type of the InputColumn
        /// </param>
        /// <param name="fieldTypeId">
        /// The field type id of the Template-Field
        /// </param>
        /// <returns>
        /// True if the types are compatible, needs to be overhauled  <see cref="bool"/>.
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
        ///     Creates the Data-Table which is shown in the Data-Grid-View by reading from the Config-Object.
        /// </summary>
        /// <returns>
        ///     Created Data-Table <see cref="DataTable" />.
        /// </returns>
        private DataTable GetDatatable()
        {
            var dt = new DataTable();

            var dcFieldName = dt.Columns.Add();
            dcFieldName.ColumnName = "dc_fieldName";
            dcFieldName.Caption = "Template Field";
            dcFieldName.DataType = typeof(string);

            var dcFieldType = dt.Columns.Add();
            dcFieldType.ColumnName = "dc_fieldType";
            dcFieldType.Caption = "Type";
            dcFieldType.DataType = typeof(int);

            var dcFieldTypeName = dt.Columns.Add();
            dcFieldTypeName.ColumnName = "dc_fieldTypeName";
            dcFieldTypeName.Caption = "Type";
            dcFieldTypeName.DataType = typeof(string);

            var dcLineageID = dt.Columns.Add();
            dcLineageID.ColumnName = "dc_colLineageID";
            dcLineageID.Caption = "LineageID";
            dcLineageID.DataType = typeof(int);

            var dcColumnName = dt.Columns.Add();
            dcColumnName.ColumnName = "dc_ColumnName";
            dcColumnName.Caption = "ColumnName";
            dcColumnName.DataType = typeof(string);

            // Loops over each FieldDataSet in the ComponentConfiguration-Object writing it's properties into the Datatable
            foreach (var dataSet in this.componentConfiguration.FieldDataSets)
            {
                var dr = dt.NewRow();
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
        ///     Creates a new Config-Object by the current state of the Form-Controls (especially the DataGridview)
        /// </summary>
        /// <returns>
        ///     Newly generated Config-Object <see cref="ComponentConfiguration" />.
        /// </returns>
        private ComponentConfiguration UpdateConfigFromUI()
        {
            // Folder-Path, TemplatePath and FormatString are adopted from the corresponding label and saved to the Config-Object 
            var cfg = new ComponentConfiguration
                          {
                              FieldDataSets = new List<ComponentConfiguration.FieldDataSet>(), 
                              FileNameFormat = new ComponentConfiguration.NameFormat(), 
                              FolderPath = this.lblFolder.Text, 
                              TemplatePath = this.lblTemplate.Text, 
                              FormatString = this.lblNameBuilder.Text
                          };

            // Loops over the rows of the datagridview, creating and adding new FieldDataSets to the ConfigObject
            foreach (DataGridViewRow dgvRow in this.dataGridView1.Rows)
            {
                var fds = new ComponentConfiguration.FieldDataSet
                              {
                                  FieldName =
                                      dgvRow.Cells["dc_fieldName"].Value.ToString(), 
                                  FieldTypeId =
                                      int.Parse(
                                          dgvRow.Cells["dc_fieldType"].Value
                                      .ToString()), 
                                  ColumnName =
                                      dgvRow.Cells["dgvc_MappedColumnName"].Value
                                      .ToString()
                              };
                cfg.FieldDataSets.Add(fds);
            }

            return cfg;
        }

        /// <summary>
        ///     Reads a PdfTemplate and  generate a new list of FieldDatasets representing the fillable fields in the Template
        /// </summary>
        /// <returns>
        ///     List of FieldDatasets created from the File<see cref="List" />.
        /// </returns>
        private List<ComponentConfiguration.FieldDataSet> GetFieldDataFromFile()
        {
            // Creates an Instance of the PdfReaderClass which reads from the template selected by the user
            var pr = new PdfReader(this.componentConfiguration.TemplatePath);
            var fieldDataSetsFromFile = new List<ComponentConfiguration.FieldDataSet>();

            // Loops over all fillable fields in the pdf gathering their Data and saving it to FieldDatasets
            foreach (var v in pr.AcroFields.Fields)
            {
                var dataSet = new ComponentConfiguration.FieldDataSet(
                    v.Key, 
                    pr.AcroFields.GetFieldType(v.Key), 
                    "Egal", 
                    "--", 
                    string.Empty);
                fieldDataSetsFromFile.Add(dataSet);
            }

            pr.Close();

            // If there was no fillable field found, a warning is displayed
            if (fieldDataSetsFromFile.Count == 0)
            {
                MessageBox.Show("This template doesn't contain any fillable fields");
            }

            return fieldDataSetsFromFile;
        }

        /// <summary>
        /// Called, when the button "Template" is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnTemplate_Click(object sender, EventArgs e)
        {
            // Show OpenFileDialog dialogTemplate
            this.dialogTemplate.ShowDialog();

            // Checks if the selected template exist
            if (this.dialogTemplate.CheckFileExists)
            {
                // Sets text in the corresponding label to selected filePath;
                this.lblTemplate.Text = this.dialogTemplate.FileName;

                // Updates configuration
                this.componentConfiguration = this.UpdateConfigFromUI();

                // sets configurations fieldDataset from the selectedTemplate
                this.componentConfiguration.FieldDataSets = this.GetFieldDataFromFile();

                // Rebuilds the form
                this.BuildForm();
            }
        }

        /// <summary>
        /// Called when the folder-button is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnFolder_Click(object sender, EventArgs e)
        {
            // Shows FolderBrowser dialogFolder
            this.dialogFolder.ShowDialog();

            // If the folder is a valid folderPath
            if (!string.IsNullOrEmpty(this.dialogFolder.SelectedPath))
            {
                // Sets text in the corresponding label to the selected path
                this.lblFolder.Text = this.dialogFolder.SelectedPath;

                // Updates configuration
                this.componentConfiguration = this.UpdateConfigFromUI();

                // Rebuilds the form
                this.BuildForm();
            }
        }

        /// <summary>
        /// Called when the Name-Format button is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnName_Click(object sender, EventArgs e)
        {
            // Instanciates and displays new NameBuilderForm
            var nameBuilderForm = new NameBuilderForm(this.inputColumnInfos.ToList(), this.componentConfiguration);
            nameBuilderForm.ShowDialog(this);

            // Sets text in the corresponding label to the FormatString returned by the Form
            this.lblNameBuilder.Text = this.componentConfiguration.FormatString;

            // Updates configuration
            this.componentConfiguration = this.UpdateConfigFromUI();

            // Rebuilds the form
            this.BuildForm();
        }

        /// <summary>
        /// Called when the form is loaded
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void FillablePdfDestinationUIForm_Load(object sender, EventArgs e)
        {
            // Builds the form for the first time
            this.BuildForm();
        }

        /// <summary>
        /// Called when OK is clicked
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            // Updates configuration (to be sure)
            this.componentConfiguration = this.UpdateConfigFromUI();

            // Serializes the current configuration to a JsonString
            this.OutputConfigJsonString = this.componentConfiguration.ToJsonString();

            // Sets DialogResult to "OK";
            this.DialogResult = DialogResult.OK;

            // Disposes the form
            this.Dispose();
        }

        /// <summary>
        /// Called when the Cancel-Button is clicked, closes the form and discards all settings made
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
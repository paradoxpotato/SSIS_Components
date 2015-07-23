// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FillablePdfDestination.cs" company="">
//   
// </copyright>
// <summary>
//   Main Class, Implementing Methods of the Pipeline-Component Interface
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Bechtle.FillablePdfDestination
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using iTextSharp.text.pdf;

    using Microsoft.SqlServer.Dts.Pipeline;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

    /// <summary>
    ///     Main Class, Implementing Methods of the Pipeline-Component Interface
    /// </summary>
    [DtsPipelineComponent(ComponentType = ComponentType.DestinationAdapter, 
        DisplayName = "Bechtle FillablePdfDestination", IconResource = "Bechtle.FillablePdfDestination.pdfforge.ico", 
        Description = "Creates PDFs writing input Data to fields defined by a fillable pdf-Template and another Test", 
        UITypeName =
            "Bechtle.FillablePdfDestination.FillablePdfDestinationUI, Bechtle.FillablePdfDestination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9f75f02631159792"
        )]
    public class FillablePdfDestination : PipelineComponent
    {
        /// <summary>
        ///     The column name to buffer id.
        /// </summary>
        private Dictionary<string, int> _columnNameToBufferId;

        /// <summary>
        ///     The config.
        /// </summary>
        private ComponentConfiguration _config;

        /// <summary>
        ///     The row count.
        /// </summary>
        private int _rowCount;

        /// <summary>
        ///     The sw debug.
        /// </summary>
        public StreamWriter SwDebug;

        /// <summary>
        ///     The swResult.
        /// </summary>
        public StreamWriter SwResult;

        /// <summary>
        ///     The provide component properties.
        /// </summary>
        public override void ProvideComponentProperties()
        {
            this.RemoveAllInputsOutputsAndCustomProperties();

            var input = this.ComponentMetaData.InputCollection.New();
            input.Name = "Input";

            var settings = this.ComponentMetaData.CustomPropertyCollection.New();
            settings.Name = "Settings";
            settings.TypeConverter = "NOTBROWSABLE";
        }

        /// <summary>
        ///     The pre execute.
        /// </summary>
        public override void PreExecute()
        {
            var input = this.ComponentMetaData.InputCollection["Input"];

            string settingsString = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value.ToString();
            this._config = ComponentConfiguration.CreateFromJson(settingsString);

            var debugFilePath = this._config.FolderPath + @"\debugfile.txt";
            var resultPath = this._config.FolderPath + @"\resultfile.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));

            Stream testfileStream = new FileStream(resultPath, FileMode.Create);
            this.SwResult = new StreamWriter(testfileStream);

            Stream debugFileStream = new FileStream(debugFilePath, FileMode.Create);
            this.SwDebug = new StreamWriter(debugFileStream);

            this._columnNameToBufferId = new Dictionary<string, int>();

            this._columnNameToBufferId.Add("--", -1);

            var testString = string.Empty;

            this._rowCount = 0;

            foreach (IDTSInputColumn100 inputColumn100 in input.InputColumnCollection)
            {
                var columnName = inputColumn100.Name;
                var bufferId = this.BufferManager.FindColumnByLineageID(input.Buffer, inputColumn100.LineageID);
                this._columnNameToBufferId.Add(columnName, bufferId);
            }

            base.PreExecute();
        }

        /// <summary>
        /// The on input path attached.
        /// </summary>
        /// <param name="inputId">
        /// The input id.
        /// </param>
        public override void OnInputPathAttached(int inputId)
        {
            {
                base.OnInputPathAttached(inputId);

                for (var i = 0; i < this.ComponentMetaData.InputCollection.Count; i++)
                {
                    this.ComponentMetaData.InputCollection[i].InputColumnCollection.RemoveAll();
                    var input = this.ComponentMetaData.InputCollection[i].GetVirtualInput();
                    foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
                    {
                        input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
                    }
                }
            }
        }

        /// <summary>
        ///     The validate.
        /// </summary>
        /// <returns>
        ///     The <see cref="DTSValidationStatus" />.
        /// </returns>
        public override DTSValidationStatus Validate()
        {
            ComponentConfiguration cfg;
            object value = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value;
            var pbCancel = true;

            if (value == null)
            {
                this.ComponentMetaData.FireError(
                    0, 
                    this.ComponentMetaData.Name, 
                    "The component is not configured!", 
                    string.Empty, 
                    0, 
                    out pbCancel);
                return DTSValidationStatus.VS_NEEDSNEWMETADATA;
            }

            cfg = ComponentConfiguration.CreateFromJson(value.ToString());
            var columnNames = new List<string>();
            columnNames.AddRange(cfg.FieldDataSets.Select(x => x.ColumnName).ToList());

            var inputColumns = this.ComponentMetaData.InputCollection[0].InputColumnCollection;

            var componentInputColumnNames = new List<string>();

            foreach (IDTSInputColumn100 col in inputColumns)
            {
                componentInputColumnNames.Add(col.Name);
            }

            foreach (var fds in cfg.FieldDataSets)
            {
                if (!componentInputColumnNames.Contains(fds.ColumnName) && fds.ColumnName != "--")
                {
                    this.ComponentMetaData.FireError(
                        0, 
                        this.ComponentMetaData.Name, 
                        "Column " + fds.ColumnName + " not found!", 
                        string.Empty, 
                        0, 
                        out pbCancel);
                    return DTSValidationStatus.VS_NEEDSNEWMETADATA;
                }
            }

            if (this.ComponentMetaData.InputCollection.Count != 1)
            {
                this.ComponentMetaData.FireError(
                    0, 
                    this.ComponentMetaData.Name, 
                    "Incorrect number of inputs.", 
                    string.Empty, 
                    0, 
                    out pbCancel);
                return DTSValidationStatus.VS_ISCORRUPT;
            }

            return base.Validate();
        }

        /// <summary>
        /// The process input.
        /// </summary>
        /// <param name="inputId">
        /// The input id.
        /// </param>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        public override void ProcessInput(int inputId, PipelineBuffer buffer)
        {
            while (buffer.NextRow())
            {
                // Read values from Buffer
                var valuesById = new string[buffer.ColumnCount];
                for (var i = 0; i < valuesById.Length; i++)
                {
                    var o = buffer[i];
                    valuesById[i] = string.Empty;
                    if (o != null)
                    {
                        valuesById[i] = o.ToString();
                    }
                }

                // BuildFileName
                var s = this._config.FormatString;
                var nameElements = s.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);
                var fileName = string.Empty;

                foreach (var nameElement in nameElements)
                {
                    // # Kennzeichnet Spalte!
                    if (nameElement.Contains("#"))
                    {
                        var columnName = Regex.Replace(nameElement, "#", string.Empty);
                        var bufferId = this._columnNameToBufferId[columnName];
                        fileName = fileName.Insert(fileName.Length, valuesById[bufferId]);
                    }
                    else
                    {
                        fileName = fileName.Insert(fileName.Length, nameElement);
                    }
                }

                fileName = fileName.Insert(fileName.Length, ".pdf");
                fileName = fileName.Insert(0, this._config.FolderPath + @"\");

                var fileNameIsValid = false;
                FileInfo fileInfo = null;
                try
                {
                    fileInfo = new FileInfo(fileName);
                }
                catch (ArgumentException)
                {
                }
                catch (PathTooLongException)
                {
                }
                catch (NotSupportedException)
                {
                }

                if (fileInfo == null)
                {
                    this.SwDebug.WriteLine("Filepath " + fileName + " is not valid!");
                }
                else
                {
                    this.SwDebug.WriteLine(fileName);
                    fileNameIsValid = true;
                }

                if (!fileNameIsValid)
                {
                    continue;
                }
                try
                {
                    var stamper = new PdfStamper(
                        new PdfReader(this._config.TemplatePath), 
                        new FileStream(fileName, FileMode.Create));

                    foreach (var fieldDataSet in this._config.FieldDataSets)
                    {
                        var bufferId = this._columnNameToBufferId[fieldDataSet.ColumnName];
                        var value = string.Empty;
                        if (bufferId != -1)
                        {
                            value = valuesById[bufferId];

                            if (fieldDataSet.FieldTypeId.Equals(AcroFields.FIELD_TYPE_CHECKBOX)
                                | fieldDataSet.FieldTypeId.Equals(AcroFields.FIELD_TYPE_PUSHBUTTON))
                            {
                                value = value.Equals("True") ? "Yes" : "No";
                            }
                        }
                        stamper.AcroFields.SetField(fieldDataSet.FieldName, value);
                    }
                    stamper.Close();
                }
                catch (Exception exception)
                {
                    this.SwDebug.WriteLine("Error while creating file " + fileName);
                    this.SwDebug.WriteLine(exception.Message);
                }
            }
        }

        /// <summary>
        /// The post execute.
        /// </summary>
        public override void PostExecute()
        {
            this.SwDebug.Close();
            this.SwResult.Close();
            base.PostExecute();
        }
    }
}
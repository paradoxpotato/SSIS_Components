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
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using iTextSharp.text.pdf;

    using Microsoft.SqlServer.Dts.Pipeline;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;

    /// <summary>
    /// Main Class, Implementing Methods of the Pipeline-Component Interface
    /// </summary>
    [DtsPipelineComponent(ComponentType = ComponentType.DestinationAdapter,
        DisplayName = "Bechtle FillablePdfDestination", IconResource = "Bechtle.FillablePdfDestination.pdfforge.ico",
        Description = "Creates PDFs writing input Data to fields defined by a fillable pdf-Template and another Test",
        UITypeName = "Bechtle.FillablePdfDestination.FillablePdfDestinationUI, Bechtle.FillablePdfDestination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9f75f02631159792"
        )]
    public class FillablePdfDestination : PipelineComponent
    {
        /// <summary>
        /// The column name to buffer id.
        /// </summary>
        private Dictionary<string, int> columnNameToBufferID;

        /// <summary>
        /// The config.
        /// </summary>
        private ComponentConfiguration config;

        /// <summary>
        /// The row count.
        /// </summary>
        private int rowCount;

        /// <summary>
        /// The sw debug.
        /// </summary>
        public StreamWriter swDebug;

        /// <summary>
        /// The swResult.
        /// </summary>
        public StreamWriter swResult;

        /// <summary>
        /// The provide component properties.
        /// </summary>
        public override void ProvideComponentProperties()
        {
            this.RemoveAllInputsOutputsAndCustomProperties();

            IDTSInput100 input = this.ComponentMetaData.InputCollection.New();
            input.Name = "Input";

            IDTSCustomProperty100 settings = this.ComponentMetaData.CustomPropertyCollection.New();
            settings.Name = "Settings";
            settings.TypeConverter = "NOTBROWSABLE";
        }

        /// <summary>
        /// The pre execute.
        /// </summary>
        public override void PreExecute()
        {
            IDTSInput100 input = this.ComponentMetaData.InputCollection["Input"];

            string settingsString = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value.ToString();
            this.config = ComponentConfiguration.CreateFromJson(settingsString);


            string debugFilePath = this.config.FolderPath + @"\debugfile.txt";
            string resultPath = this.config.FolderPath + @"\resultfile.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));

            Stream testfileStream = new FileStream(resultPath, FileMode.Create);
            this.swResult = new StreamWriter(testfileStream);


            Stream debugFileStream = new FileStream(debugFilePath, FileMode.Create);
            this.swDebug = new StreamWriter(debugFileStream);

           

            this.columnNameToBufferID = new Dictionary<string, int>();

            this.columnNameToBufferID.Add("--", -1);

            string testString = string.Empty;

            this.rowCount = 0;

            foreach (IDTSInputColumn100 inputColumn100 in input.InputColumnCollection)
            {
                string columnName = inputColumn100.Name;
                int bufferId = this.BufferManager.FindColumnByLineageID(input.Buffer, inputColumn100.LineageID);
                this.columnNameToBufferID.Add(columnName, bufferId);
            }

            base.PreExecute();
        }

        /// <summary>
        /// The on input path attached.
        /// </summary>
        /// <param name="inputID">
        /// The input id.
        /// </param>
        public override void OnInputPathAttached(int inputID)
        {
            {
                base.OnInputPathAttached(inputID);

                for (int i = 0; i < this.ComponentMetaData.InputCollection.Count; i++)
                {
                    this.ComponentMetaData.InputCollection[i].InputColumnCollection.RemoveAll();
                    IDTSVirtualInput100 input = this.ComponentMetaData.InputCollection[i].GetVirtualInput();
                    foreach (IDTSVirtualInputColumn100 vcol in input.VirtualInputColumnCollection)
                    {
                        input.SetUsageType(vcol.LineageID, DTSUsageType.UT_READONLY);
                    }
                }
            }
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <returns>
        /// The <see cref="DTSValidationStatus"/>.
        /// </returns>
        public override DTSValidationStatus Validate()
        {
            ComponentConfiguration cfg;
            object value = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value;
            bool pbCancel = true;

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
            List<string> columnNames = new List<string>();
            columnNames.AddRange(cfg.FieldDataSets.Select(x => x.ColumnName).ToList());

            IDTSInputColumnCollection100 inputColumns = ComponentMetaData.InputCollection[0].InputColumnCollection;

            List<string> componentInputColumnNames = new List<string>();

            foreach (IDTSInputColumn100 col in inputColumns)
            {
                componentInputColumnNames.Add(col.Name);
            }

            foreach (ComponentConfiguration.FieldDataSet fds in cfg.FieldDataSets)
            {
                if (!componentInputColumnNames.Contains(fds.ColumnName) && fds.ColumnName != "--")
                {
                    this.ComponentMetaData.FireError(
                         0,
                  this.ComponentMetaData.Name, "Column " + fds.ColumnName + " not found!",
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
        /// <param name="inputID">
        /// The input id.
        /// </param>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            while (buffer.NextRow())
            {
                try
                {
                    string s = this.config.FormatString;
                    string[] name_Elements = s.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);
                    string dynamicFileName = string.Empty;

                    for (int i = 0; i < name_Elements.Length; i++)
                    {
                        // Namens-Bestandteil ist ein Spaltenname
                        if (name_Elements[i].Contains("#"))
                        {
                            string columnName = Regex.Replace(name_Elements[i], "#", string.Empty);

                            int bufferID = this.columnNameToBufferID[columnName];

                            var value = string.Empty;
                            if (bufferID != -1)
                            {
                                value = buffer[bufferID] != null ? buffer[bufferID].ToString() : string.Empty;
                            }

                            dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, value);
                        }
                        else
                        {
                            dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, name_Elements[i]);
                        }
                    }

                    dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, ".pdf");
                    this.swResult.WriteLine(dynamicFileName);

                    PdfStamper stamper = new PdfStamper(
                        new PdfReader(this.config.TemplatePath),
                        new FileStream(this.config.FolderPath + @"\" + dynamicFileName, FileMode.Create));

                    foreach (ComponentConfiguration.FieldDataSet fi in this.config.FieldDataSets)
                    {
                        int bufferID = this.columnNameToBufferID[fi.ColumnName];

                        // ToDo Typ-Konvertierung!!!!!
                        var value = string.Empty;
                        if (bufferID != -1)
                        {
                            value = buffer[bufferID] != null ? buffer[bufferID].ToString() : string.Empty;

                            if (fi.FieldTypeId == 2)
                            {
                                // Nur als Test gedacht!
                                value = value.Equals("True") ? "Yes" : "No";
                            }
                        }

                        stamper.AcroFields.SetField(fi.FieldName, value);
                        this.swResult.Write(
                            "|" + fi.FieldName + " : " + fi.FieldTypeId + " : " + bufferID + ":" + value + "|");
                    }

                    stamper.Close();
                    this.rowCount++;

                    this.swResult.WriteLine();
                    this.swResult.Write(dynamicFileName);
                    this.swResult.WriteLine();
                }
                catch (Exception exception)
                {
                    for (int i = 0; i < buffer.ColumnCount; i++)
                    {
                        try
                        {
                            swDebug.Write(buffer[i].ToString() + "|");
                        }
                        catch
                        {
                            swDebug.Write("FEHLER!");
                        }
                    }
                    this.swDebug.WriteLine(exception.Message);
                    swDebug.WriteLine();
                }
            }
        }

        public override void PostExecute()
        {
            this.swDebug.Close();
            this.swResult.Close();
            base.PostExecute();
        }
    }
}


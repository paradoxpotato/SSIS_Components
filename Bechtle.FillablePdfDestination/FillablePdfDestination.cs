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
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using iTextSharp.text.pdf;

    using Microsoft.SqlServer.Dts.Pipeline;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime.Wrapper;

    /// <summary>
    /// Main Class, Implementing Methods of the Pipeline-Component Interface
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
        /// The column name to buffer id.
        /// </summary>
        private Dictionary<string, int> columnNameToBufferID;

        private delegate Int16 GetBufferValue(int bufferID);

        /// <summary>
        /// The config.
        /// </summary>
        private ComponentConfiguration config;

        /// <summary>
        /// The row count.
        /// </summary>
        private int rowCount;

        /// <summary>
        /// The sw.
        /// </summary>
        public StreamWriter sw;

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
        }

        /// <summary>
        /// The pre execute.
        /// </summary>
        public override void PreExecute()
        {
            IDTSInput100 input = this.ComponentMetaData.InputCollection["Input"];

            string settingsString = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value.ToString();

            this.config = ComponentConfiguration.CreateFromJson(settingsString);

            string examplePath = this.config.FolderPath + @"\resultfile.txt";

            Stream testfileStream = new FileStream(examplePath, FileMode.Create);
            this.sw = new StreamWriter(testfileStream);

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
                            value = buffer[bufferID] != null ? buffer.GetString(bufferID) : string.Empty;
                        }

                        dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, value);
                    }
                    else
                    {
                        dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, name_Elements[i]);
                    }
                }

                dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, ".pdf");
                sw.WriteLine(dynamicFileName);

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
                        value = buffer[bufferID].ToString() != null ? buffer[bufferID].ToString() : string.Empty;

                        if (fi.FieldTypeId == 2)
                        {
                            //Nur als Test gedacht!
                            value = value.Equals("True") ? "Yes" : "No";
                        }
                    }

                    stamper.AcroFields.SetField(fi.FieldName, value);
                    this.sw.Write("|" + fi.FieldName + " : " + fi.FieldTypeId + " : " + bufferID + ":" + value + "|");
                }

                stamper.Close();
                this.rowCount++;

                this.sw.WriteLine();
                this.sw.Write(dynamicFileName);
                this.sw.WriteLine();
            }
        }

        /// <summary>
        /// The buffer column value to string.
        /// </summary>
        /// <param name="buffer">
        /// The buffer.
        /// </param>
        /// <param name="columnId">
        /// The column id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>

        /// <summary>
        /// The post execute.
        /// </summary>
        public override void PostExecute()
        {
            this.sw.Close();
            base.PostExecute();
        }
    }
}
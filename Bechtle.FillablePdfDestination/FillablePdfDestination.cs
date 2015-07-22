using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bechtle.FillablePdfDestination
{
    using System.Drawing;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using iTextSharp.text.pdf;

    using Microsoft.SqlServer.Dts.Pipeline;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime;

    /// <summary>
    /// Main Class, Implementing Methods of the Pipeline-Component Interface
    /// </summary>
    [DtsPipelineComponent(ComponentType = ComponentType.DestinationAdapter,
       DisplayName = "Bechtle FillablePdfDestination",
       IconResource = "Bechtle.FillablePdfDestination.pdfforge.ico",
       Description = "Creates PDFs writing input Data to fields defined by a fillable pdf-Template and another Test",
       UITypeName = "Bechtle.FillablePdfDestination.FillablePdfDestinationUI, Bechtle.FillablePdfDestination, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9f75f02631159792")]

    public class FillablePdfDestination : PipelineComponent
    {
        private Dictionary<string, int> columnNameToBufferID;
        private ComponentConfiguration config;

        public StreamWriter sw;

        private int rowCount;

        public override void ProvideComponentProperties()
        {
            this.RemoveAllInputsOutputsAndCustomProperties();

            IDTSInput100 input = this.ComponentMetaData.InputCollection.New();
            input.Name = "Input";

            IDTSCustomProperty100 settings = this.ComponentMetaData.CustomPropertyCollection.New();
            settings.Name = "Settings";
        }

        public override void PreExecute()
        {
            IDTSInput100 input = this.ComponentMetaData.InputCollection["Input"];

            string settingsString = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value.ToString();

            this.config = ComponentConfiguration.CreateFromJson(settingsString);

            string examplePath = this.config.FolderPath + @"\resultfile.txt";

            Stream testfileStream = new FileStream(examplePath, FileMode.Create);
            sw = new StreamWriter(testfileStream);

            this.columnNameToBufferID = new Dictionary<string, int>();

            this.columnNameToBufferID.Add("--", -1);

            string testString = String.Empty;

            rowCount = 0;

            foreach (IDTSInputColumn100 inputColumn100 in input.InputColumnCollection)
            {
                string columnName = inputColumn100.Name;
                int bufferId = this.BufferManager.FindColumnByLineageID(input.Buffer, inputColumn100.LineageID);
                this.columnNameToBufferID.Add(columnName, bufferId);
            }

            base.PreExecute();
        }

        public override void ProcessInput(int inputID, PipelineBuffer buffer)
        {
            while (buffer.NextRow())
            {
                string s = config.FormatString;

                string[] name_Elements = s.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);

                string dynamicFileName = string.Empty;

                for(int i = 0; i< name_Elements.Length ; i++)
                {     
                    //Namens-Bestandteil ist ein Spaltenname
                    if (name_Elements[i].Contains("#"))
                    {
                        string columnName = Regex.Replace(name_Elements[i], "#", string.Empty);

                        int bufferID = this.columnNameToBufferID[columnName];

                        var value = string.Empty;
                        if (bufferID != -1)
                        {
                            value = buffer.GetString(bufferID) != null ? buffer.GetString(bufferID) : "";
                        }

                        dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, value);
                    }
                    else
                    {
                        dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, name_Elements[i]);
                    }
                }
                dynamicFileName = dynamicFileName.Insert(dynamicFileName.Length, ".pdf");

                //ToDo: Name-Settings
                PdfStamper stamper = new PdfStamper(new PdfReader(config.TemplatePath), new FileStream(config.FolderPath + @"\" + dynamicFileName, FileMode.Create));
                foreach (ComponentConfiguration.FieldDataSet fi in this.config.FieldDataSets)
                {
                    int bufferID = columnNameToBufferID[fi.ColumnName];
                    // ToDo Typ-Konvertierung!!!!!

                    var value = string.Empty;
                    if (bufferID != -1)
                    {
                        value = buffer.GetString(bufferID) != null ? buffer.GetString(bufferID) : "";
                    }

                    stamper.AcroFields.SetField(fi.FieldName, value);
                    this.sw.Write("|" + fi.FieldName + " : " + fi.FieldTypeId + " : " + bufferID + ":" + value + "|");
                }
                stamper.Close();
                rowCount++;

                this.sw.WriteLine();
                this.sw.Write(dynamicFileName);
                this.sw.WriteLine();
            }
        }

        public override void PostExecute()
        {
            this.sw.Close();
            base.PostExecute();
        }
    }
}

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FillablePdfDestination.cs" company="Bechtle-AG">
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
    using System.Diagnostics.CodeAnalysis;
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
        ///     Contains IDs of the Input-Buffer with the Corresponding names
        /// </summary>
        private Dictionary<string, int> bufferIdByColumnName;

        /// <summary>
        ///     Contains all the user-defined settings of the component (see Component-Configuration)
        /// </summary>
        private ComponentConfiguration config;

        /// <summary>
        ///     (not Used) Number of the processed rows
        /// </summary>
        private int rowCount;

        /// <summary>
        ///     Writer to Debug-Txt
        /// </summary>
        private StreamWriter streamWriterDebug;

        /// <summary>
        ///     (not used) Writer to result-Txt
        /// </summary>
        private StreamWriter streamWriterResult;

        /// <summary>
        ///     Overrides Provide Component-Properties method of the Pipeline-Component-Class and
        ///     Provides Input, Output and User-Defined properties used by the Pipeline-Component
        ///     Called once when the component is dragged into the dataflow
        /// </summary>
        public override void ProvideComponentProperties()
        {
            // All Default-Properties are removed
            this.RemoveAllInputsOutputsAndCustomProperties();

            // Defines new Input and new Component-Properties in the Meta-Data of the Component

            // Input
            var input = this.ComponentMetaData.InputCollection.New();
            input.Name = "Input";

            // This property contains all settings set from the UI, safed in a single JSON-String
            var settings = this.ComponentMetaData.CustomPropertyCollection.New();
            settings.Name = "Settings";

            // As the user should not create an invalid Json-String, this property is made invisible
            settings.TypeConverter = "NOTBROWSABLE";
        }

        /// <summary>
        ///     Called once for each component in the dataflow before executing it. Does everything that is necessary to process
        ///     the input-data.
        /// </summary>
        public override void PreExecute()
        {
            // ToDO Map Buffers

            // References to the Component-Input named "Input" (see Component-Properties)
            var input = this.ComponentMetaData.InputCollection["Input"];

            // Gets Json-String from the Settings-Property and deserializes the configuration for this component
            string settingsString = this.ComponentMetaData.CustomPropertyCollection["Settings"].Value.ToString();
            this.config = ComponentConfiguration.CreateFromJson(settingsString);

            // Two FileWriters are instanciated to write debug-Information in the Output-Folder defined by the configurations folderPath
            var debugFilePath = this.config.FolderPath + @"\debugfile.txt";
            var resultPath = this.config.FolderPath + @"\resultfile.txt";

            // If the requested Direcoty doesn't exist, it is created
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));

            Stream testfileStream = new FileStream(resultPath, FileMode.Create);
            this.streamWriterResult = new StreamWriter(testfileStream);

            Stream debugFileStream = new FileStream(debugFilePath, FileMode.Create);
            this.streamWriterDebug = new StreamWriter(debugFileStream);

            
            // Mapps the name of each input-column to it's assigned bufferID for easier access during processing the input-data
            this.bufferIdByColumnName = new Dictionary<string, int>();
            this.bufferIdByColumnName = new Dictionary<string, int>();
            this.rowCount = 0;

            foreach (IDTSInputColumn100 inputColumn100 in input.InputColumnCollection)
            {
                var columnName = inputColumn100.Name;
                var bufferId = this.BufferManager.FindColumnByLineageID(input.Buffer, inputColumn100.LineageID);
                this.bufferIdByColumnName.Add(columnName, bufferId);
            }

            // "--" represents that the template does not receive any data from the input, so the buffer id is set to -1, a non existent buffer-ID
            this.bufferIdByColumnName.Add("--", -1);

            // The default PreExecute-Method of Pipeline-Component is called to round up the preparation
            base.PreExecute();
        }

        /// <summary>
        /// Design-time method: Is called when a upstream dataflow-component is connected
        /// </summary>
        /// <param name="inputId">
        /// ID of the connected Input
        /// </param>
        public override void OnInputPathAttached(int inputId)
        {
            {
                // Calls default method from SSIS
                base.OnInputPathAttached(inputId);

                // Automatically adds all output-columns of the upstream-component to the component's InputColumn-Collection
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
        ///     runtime/design-Time method: Validates the component after each change to find errors which would prevent the
        ///     dataflow from being executed
        /// </summary>
        /// <returns>
        ///     Validation-Status processed by the SSDT for Visual Studio <see cref="DTSValidationStatus" />.
        /// </returns>
        public override DTSValidationStatus Validate()
        {
            // Checks if the component has been configured before executing by testing if the Property "Settings" has been set
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

            // Checks if all columns to read are present in the components InputColumnCollection
            cfg = ComponentConfiguration.CreateFromJson(value.ToString());

            var inputColumns = this.ComponentMetaData.InputCollection[0].InputColumnCollection;

            var componentInputColumnNames = (from IDTSInputColumn100 col in inputColumns select col.Name).ToList();

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

            // Checks if exacly one input is attached to the component
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

            // base-Validation by SSIS-Datatools
            return base.Validate();
        }

        /// <summary>
        /// Processes all input directed to the component (Write to pdf)
        /// </summary>
        /// <param name="inputId">
        /// Id of the Input (in fact not used as only one input is connected)
        /// </param>
        /// <param name="buffer">
        /// The Runtime-Buffer providing the data contained in the dataflow
        /// </param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", 
            Justification = "Reviewed. Suppression is OK here.")]
        public override void ProcessInput(int inputId, PipelineBuffer buffer)
        {
            // While there are still rows in the Buffer...
            while (buffer.NextRow())
            {
                // Tires to convert the value of each column in the current buffer to a string and writes it into an array to avoid to access the buffer twice, decreasing the performance
                var valuesById = new string[buffer.ColumnCount];
                for (var i = 0; i < valuesById.Length; i++)
                {
                    var o = buffer[i];

                    // Default value in the array is an empty string
                    valuesById[i] = string.Empty;

                    // If the column-value is not null, it is converted to a string and writen to the array
                    if (o != null)
                    {
                        valuesById[i] = o.ToString();
                    }
                }

                // Creates the name of the output-file from the newly-created string-array containing the buffer data and the component-configuration's formatString

                // The ComponentConfiguration's Format string is split into it's name-elements
                var s = this.config.FormatString;
                var nameElements = s.Split(new[] { "%" }, StringSplitOptions.RemoveEmptyEntries);
                var fileName = string.Empty;
                foreach (var nameElement in nameElements)
                {
                    // '#' in a nameElement implies, that this nameElement is a columnName
                    if (nameElement.Contains("#"))
                    {
                        var columnName = Regex.Replace(nameElement, "#", string.Empty);

                        var bufferId = this.bufferIdByColumnName[columnName];

                        // The value of the buffer-column cached in the string-array is accessed by the BufferID and appended to the fileName;
                        fileName = fileName.Insert(fileName.Length, valuesById[bufferId]);
                    }
                    else
                    {
                        // If there's no '#' the value of the nameElement is appended to the fileName:
                        fileName = fileName.Insert(fileName.Length, nameElement);
                    }
                }

                // Completes the files path by adding the ".pdf"-extension and the user-set folderPath;
                fileName = fileName.Insert(fileName.Length, ".pdf");
                fileName = fileName.Insert(0, this.config.FolderPath + @"\");

                // Checks if the generated fileName is valid and unique
                var fileNameIsValid = false;
                FileInfo fileInfo = null;
                try
                {
                    if (File.Exists(fileName))
                    {
                        this.streamWriterDebug.WriteLine("File " + fileName + " already exists!");
                        throw new NotSupportedException();
                    }

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
                    this.streamWriterDebug.WriteLine("Filepath " + fileName + " is not valid!");
                }
                else
                {
                    this.streamWriterDebug.WriteLine(fileName);
                    fileNameIsValid = true;
                }

                if (!fileNameIsValid)
                {
                    // If the fileName is not valid, no other steps are made;
                    continue;
                }

                try
                {
                    //Instanciates a new PdfStamper to fill the fields in the template
                    var stamper = new PdfStamper(
                        new PdfReader(this.config.TemplatePath), 
                        new FileStream(fileName, FileMode.Create));

                    // Loops over each fieldDataSet in the config's components fieldDataset-collection
                    foreach (var fieldDataSet in this.config.FieldDataSets)
                    {
                        // Gets the bufferID of  the column mapped to the fillable field in the template
                        var bufferId = this.bufferIdByColumnName[fieldDataSet.ColumnName];
                        var value = string.Empty;
                        if (bufferId != -1)
                        {
                            // The value of the buffer-column cached in the string-array is accessed by the BufferID and safed to temporary variable 
                            value = valuesById[bufferId];

                            // iTextsharp Field-Types "Checkbox" and "Pushbutton" need the strings "Yes" or "No" to check or uncheck template-fields. Because of this "True" and "False" have to be converted
                            if (fieldDataSet.FieldTypeId.Equals(AcroFields.FIELD_TYPE_CHECKBOX)
                                | fieldDataSet.FieldTypeId.Equals(AcroFields.FIELD_TYPE_PUSHBUTTON))
                            {
                                value = value.Equals("True") ? "Yes" : "No";
                            }
                        }

                        // Writes value to the field in the template assigned to the fieldDatasets fieldName;
                        stamper.AcroFields.SetField(fieldDataSet.FieldName, value);
                    }

                    // Closes the Pdf.Stamper
                    stamper.Close();
                }
                catch (Exception exception)
                {
                    // If any error happens (for example while filling the form), the  occuring exception is caught and the fileName and the corresponding errorMEssage is written to the debug file
                    this.streamWriterDebug.WriteLine("Error while creating file " + fileName);
                    this.streamWriterDebug.WriteLine(exception.Message);
                }
            }
        }

        /// <summary>
        ///     Called once after the dataflow has finished successfully
        /// </summary>
        public override void PostExecute()
        {
            // All fileStreams are closed and the default PostExecute-Method is called;
            this.streamWriterDebug.Close();
            this.streamWriterResult.Close();
            base.PostExecute();
        }
    }
}
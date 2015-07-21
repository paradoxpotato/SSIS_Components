﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ComponentConfiguration.cs" company="">
//   
// </copyright>
// <summary>
//   The component configuration.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Bechtle.FillablePdfDestination
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using Newtonsoft.Json;

    /// <summary>
    /// The component configuration.
    /// </summary>
    public class ComponentConfiguration
    {
        /// <summary>
        /// The field data sets.
        /// </summary>
        private List<FieldDataSet> fieldDataSets = new List<FieldDataSet>();

        /// <summary>
        /// The file name format.
        /// </summary>
        private NameFormat fileNameFormat;

        /// <summary>
        /// The folder path.
        /// </summary>
        private string folderPath;

        /// <summary>
        /// The template path.
        /// </summary>
        private string templatePath;

        /// <summary>
        /// The to json string.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentConfiguration"/> struct.
        /// </summary>
        /// <param name="jsonString">
        /// The json string.
        /// </param>
        /// <returns>
        /// The <see cref="ComponentConfiguration"/>.
        /// </returns>
        public static ComponentConfiguration CreateFromJson(string jsonString)
        {

            if (!string.IsNullOrEmpty(jsonString) && !string.IsNullOrWhiteSpace(jsonString))
            {
                try
                {
                    return JsonConvert.DeserializeObject<ComponentConfiguration>(jsonString);
                }
                catch (JsonException jsonException)
                {
                    MessageBox.Show(
                        "Fehler bei Json-String" + jsonString + " : " + Environment.NewLine + jsonException.Message);
                    return new ComponentConfiguration();
                }
            }

            return new ComponentConfiguration();
        }

        /// <summary>
        /// The field data set.
        /// </summary>
        public struct FieldDataSet
        {
            /// <summary>
            /// The column name.
            /// </summary>
            private string columnName;

            /// <summary>
            /// The field name.
            /// </summary>
            private string fieldName;

            /// <summary>
            /// The field type id.
            /// </summary>
            private int fieldTypeID;

            /// <summary>
            /// The field type name.
            /// </summary>
            private string fieldTypeName;

            /// <summary>
            /// The mapped column identification string.
            /// </summary>
            private string mappedColumnIdentificationString;

            /// <summary>
            /// Initializes a new instance of the <see cref="FieldDataSet"/> struct.
            /// </summary>
            /// <param name="fieldName">
            /// The field name.
            /// </param>
            /// <param name="fieldTypeId">
            /// The field type id.
            /// </param>
            /// <param name="fieldTypeName">
            /// The field type name.
            /// </param>
            /// <param name="mappedColumnIdentificationString">
            /// The mapped column identification string.
            /// </param>
            /// <param name="columnName">
            /// The column Name.
            /// </param>
            public FieldDataSet(
                string fieldName,
                int fieldTypeId,
                string fieldTypeName,
                string mappedColumnIdentificationString,
                string columnName)
            {
                this.fieldName = fieldName;
                this.fieldTypeID = fieldTypeId;
                this.fieldTypeName = fieldTypeName;
                this.mappedColumnIdentificationString = mappedColumnIdentificationString;
                this.columnName = columnName;
            }


            /// <summary>
            /// Gets or sets the field name.
            /// </summary>
            public string FieldName
            {
                get
                {
                    return this.fieldName;
                }

                set
                {
                    this.fieldName = value;
                }
            }

            /// <summary>
            /// Gets or sets the field type id.
            /// </summary>
            public int FieldTypeId
            {
                get
                {
                    return this.fieldTypeID;
                }

                set
                {
                    this.fieldTypeID = value;
                }
            }

            /// <summary>
            /// Gets or sets the field type name.
            /// </summary>
            public string FieldTypeName
            {
                get
                {
                    return this.fieldTypeName;
                }

                set
                {
                    this.fieldTypeName = value;
                }
            }

            /// <summary>
            /// Gets or sets the mapped column identification string.
            /// </summary>
            public string MappedColumnIdentificationString
            {
                get
                {
                    return this.mappedColumnIdentificationString;
                }

                set
                {
                    this.mappedColumnIdentificationString = value;
                }
            }

            /// <summary>
            /// Gets or sets the column name.
            /// </summary>
            public string ColumnName
            {
                get
                {
                    return this.columnName;
                }

                set
                {
                    this.columnName = value;
                }
            }

            public override string ToString()
            {
                return 
                    this.FieldName + " : " + 
                    this.FieldTypeId + " : "  + 
                    this.FieldTypeName + " : " +
                    this.ColumnName + " : " + 
                    this.MappedColumnIdentificationString;
            }
        }

        /// <summary>
        /// The name format.
        /// </summary>
        public struct NameFormat
        {
        }

        /// <summary>
        /// Gets or sets the file name format.
        /// </summary>
        public NameFormat FileNameFormat
        {
            get
            {
                return this.fileNameFormat;
            }

            set
            {
                this.fileNameFormat = value;
            }
        }

        /// <summary>
        /// Gets or sets the folder path.
        /// </summary>
        public string FolderPath
        {
            get
            {
                return this.folderPath;
            }

            set
            {
                this.folderPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the template path.
        /// </summary>
        public string TemplatePath
        {
            get
            {
                return this.templatePath;
            }

            set
            {
                this.templatePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the field data sets.
        /// </summary>
        public List<FieldDataSet> FieldDataSets
        {
            get
            {
                return this.fieldDataSets;
            }

            set
            {
                this.fieldDataSets = value;
            }
        }
    }
}
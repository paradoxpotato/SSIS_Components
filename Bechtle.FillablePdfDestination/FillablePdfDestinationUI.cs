// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FillablePdfDestinationUI.cs" company="">
//   
// </copyright>
// <summary>
//   The fillable pdf destination ui.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bechtle.FillablePdfDestination
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    using Microsoft.SqlServer.Dts.Pipeline.Design;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime;

    /// <summary>
    /// The fillable pdf destination ui.
    /// </summary>
    internal class FillablePdfDestinationUI : IDtsComponentUI
    {
        /// <summary>
        /// The meta data.
        /// </summary>
        private IDTSComponentMetaData100 metaData;

        /// <summary>
        /// The initialize.
        /// </summary>
        /// <param name="dtsComponentMetadata">
        /// The dts component metadata.
        /// </param>
        /// <param name="serviceProvider">
        /// The service provider.
        /// </param>
        public void Initialize(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            this.metaData = dtsComponentMetadata;
        }

        /// <summary>
        /// The new.
        /// </summary>
        /// <param name="parentWindow">
        /// The parent window.
        /// </param>
        public void New(IWin32Window parentWindow)
        {
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="parentWindow">
        /// The parent window.
        /// </param>
        /// <param name="variables">
        /// The variables.
        /// </param>
        /// <param name="connections">
        /// The connections.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            string jsonString = string.Empty;
            var value = this.metaData.CustomPropertyCollection["Settings"].Value;

            if (value != null)
            {
                jsonString = value.ToString();
            }

            List<InputColumnInfo> inputColumnInfos = new List<InputColumnInfo>();
            foreach (IDTSInputColumn100  col in this.metaData.InputCollection[0].InputColumnCollection)
            {
                inputColumnInfos.Add(
                    new InputColumnInfo() { ColumnName = col.Name, DataType = col.DataType, LineageId = col.LineageID });
            }

            FillablePdfDestinationUIForm editor = new FillablePdfDestinationUIForm(
                jsonString, 
                inputColumnInfos.ToArray());

            editor.ShowDialog(parentWindow);

            if (editor.DialogResult == DialogResult.OK || editor.DialogResult == DialogResult.Yes)
            {
                this.metaData.CustomPropertyCollection["Settings"].Value = editor.OutputConfigJsonString;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when the component is removed from the Dataflow (not implemented)
        /// </summary>
        /// <param name="parentWindow">
        /// Not implemented
        /// </param>
        public void Delete(IWin32Window parentWindow)
        {
        }

        /// <summary>
        /// The help.
        /// </summary>
        /// <param name="parentWindow">
        /// The parent window.
        /// </param>
        public void Help(IWin32Window parentWindow)
        {
        }
    }
}
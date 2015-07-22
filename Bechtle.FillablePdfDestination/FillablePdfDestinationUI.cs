using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bechtle.FillablePdfDestination
{
    using System.Windows.Forms;

    using Microsoft.SqlServer.Dts.Pipeline.Design;
    using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
    using Microsoft.SqlServer.Dts.Runtime;

    class FillablePdfDestinationUI : IDtsComponentUI
    {
        private IDTSComponentMetaData100 metaData;

        public void Initialize(IDTSComponentMetaData100 dtsComponentMetadata, IServiceProvider serviceProvider)
        {
            this.metaData = dtsComponentMetadata;
        }

        public void New(IWin32Window parentWindow)
        {
        }

        public bool Edit(IWin32Window parentWindow, Variables variables, Connections connections)
        {
            string jsonString = String.Empty;
            var value = this.metaData.CustomPropertyCollection["Settings"].Value;
            
            if (value != null)
            {
                jsonString = value.ToString();
            }


            List<InputColumnInfo> inputColumnInfos = new List<InputColumnInfo>();
            foreach (IDTSInputColumn100  col in metaData.InputCollection[0].InputColumnCollection)
            {
                inputColumnInfos.Add(new InputColumnInfo()
                                         {
                                             ColumnName = col.Name,
                                             DataType = col.DataType,
                                             LineageId = col.LineageID
                                         });
            }

            FillablePdfDestinationUIForm editor = new FillablePdfDestinationUIForm(jsonString, inputColumnInfos.ToArray());
            
            editor.ShowDialog(parentWindow);

            if (editor.DialogResult == DialogResult.OK || editor.DialogResult == DialogResult.Yes)
            {
                this.metaData.CustomPropertyCollection["Settings"].Value = editor.outputConfigJsonString; 
                return true;
            }

            return false;
        }

        public void Delete(IWin32Window parentWindow)
        {
        }

        public void Help(IWin32Window parentWindow)
        {
        }
    }
}

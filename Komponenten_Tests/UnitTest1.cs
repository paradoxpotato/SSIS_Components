using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Komponenten_Tests
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    using Bechtle.FillablePdfDestination;

    using iTextSharp.text.pdf;

    using Microsoft.SqlServer.Dts.Runtime.Wrapper;

    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<InputColumnInfo> inputColumnInfos = new List<InputColumnInfo>();

            inputColumnInfos.Add(new InputColumnInfo("Boolspalte", DataType.DT_BOOL, 13, "ID: Boolspalte"));
            inputColumnInfos.Add(new InputColumnInfo("Stringspalte", DataType.DT_STR, 13, "ID: Stringspalte"));
            inputColumnInfos.Add(new InputColumnInfo("Datumspalte", DataType.DT_DATE, 13, "ID: Datumspalte"));
            inputColumnInfos.Add(new InputColumnInfo("Intspalte", DataType.DT_I4, 13, "ID: Intspalte"));
            inputColumnInfos.Add(new InputColumnInfo("Floatspalte", DataType.DT_DECIMAL, 13, "ID: Floatspalte"));


            ComponentConfiguration config = new ComponentConfiguration();
            config.FieldDataSets = new List<ComponentConfiguration.FieldDataSet>();

            config.FieldDataSets.Add(new ComponentConfiguration.FieldDataSet("Feld 1", 3, "Combobox", "ID: Spalte 1", "Boolspalte"));
            config.FieldDataSets.Add(new ComponentConfiguration.FieldDataSet("Feld 2", 4, "Combobox", "ID: Spalte 2", "Stringspalte"));
            config.FieldDataSets.Add(new ComponentConfiguration.FieldDataSet("Feld 3", 4, "Combobox", "ID: Spalte 3", "Datumspalte"));
            config.FieldDataSets.Add(new ComponentConfiguration.FieldDataSet("Feld 4", 2, "Combobox", "ID: Spalte 4", "Intspalte"));
            config.FieldDataSets.Add(new ComponentConfiguration.FieldDataSet("Feld 5", 1, "Combobox", "ID: Spalte 5", "Floatspalte"));

            string configString = config.ToJsonString();

            MessageBox.Show(configString);

            FillablePdfDestinationUIForm editor = new FillablePdfDestinationUIForm(configString, inputColumnInfos.ToArray());
            editor.ShowDialog();
            MessageBox.Show(editor.DialogResult.ToString());
            MessageBox.Show(editor.OutputConfigJsonString);

            while (editor.DialogResult == DialogResult.OK | editor.DialogResult == DialogResult.Yes)
            {
                editor = new FillablePdfDestinationUIForm(editor.OutputConfigJsonString, inputColumnInfos.ToArray());
                editor.ShowDialog();
            }

          /*  NameBuilderForm form = new NameBuilderForm(inputColumnInfos, config);
            form.ShowDialog();*/
        }
    }
}

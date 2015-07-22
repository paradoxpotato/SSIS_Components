// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameBuilderForm.cs" company="">
//   
// </copyright>
// <summary>
//   The name builder form.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Bechtle.FillablePdfDestination
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    /// <summary>
    /// The name builder form.
    /// </summary>
    public partial class NameBuilderForm : Form
    {
        private ComponentConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameBuilderForm"/> class.
        /// </summary>
        /// <param name="inputColums">
        /// The input colums.
        /// </param>
        /// <param name="cfg">
        /// The cfg.
        /// </param>
        public NameBuilderForm(List<InputColumnInfo> inputColums, ComponentConfiguration cfg)
        {
            this.InitializeComponent();
            this.config = cfg;
            this.contextMenuStrip1.Items.Clear();

            foreach (InputColumnInfo info in inputColums)
            {
                this.contextMenuStrip1.Items.Add(info.ColumnName);
            }

            var componentConfiguration = this.config;
            if (componentConfiguration.FormatString != null)
            {
                this.textBox1.Text = componentConfiguration.FormatString;
            }
     
        }

        /// <summary>
        /// The btn add_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnAdd_Click(object sender, EventArgs e)
        {
            this.contextMenuStrip1.Show(this, new Point(0, 0));
        }

        /// <summary>
        /// The context menu strip 1_ item clicked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            this.textBox1.Text = this.textBox1.Text.Insert(this.textBox1.Text.Length, "%#" + e.ClickedItem.Text + "%");
        }

        /// <summary>
        /// The btn o k_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.config.FormatString = this.textBox1.Text;
            this.Dispose();
        }

        /// <summary>
        /// The btn cancel_ click.
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

        /// <summary>
        /// The text box 1_ text changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.label2.Text = Regex.Replace(this.textBox1.Text, "%", string.Empty) + ".pdf";
            this.FormatPreview = this.label2.Text;
        }

        /// <summary>
        /// Gets the format string.
        /// </summary>
        public string FormatString { get; private set; }

        /// <summary>
        /// Gets or sets the format preview.
        /// </summary>
        public string FormatPreview { get; set; }

        private void lblPreview_Click(object sender, EventArgs e)
        {

        }

        private void NameBuilderForm_Load(object sender, EventArgs e)
        {

        }
    }
}
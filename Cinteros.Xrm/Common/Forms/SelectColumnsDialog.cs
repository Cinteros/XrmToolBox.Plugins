﻿namespace Cinteros.Xrm.Common.Forms
{
    using System.Linq;
    using System.Windows.Forms;
    using System.Xml;

    public partial class SelectColumnsDialog : Form
    {
        #region Public Constructors

        public SelectColumnsDialog(XmlDocument fetchXml, XmlDocument layoutXml)
            : this()
        {
            clbColumns.ItemCheck -= clbColumns_ItemCheck;

            this.LayoutXml = layoutXml;

            var allowed = fetchXml.SelectNodes("//attribute").Cast<XmlNode>().Select(x => x.Attributes["name"].Value);

            var selected = this.LayoutXml.SelectNodes("//cell");

            foreach (var column in allowed)
            {
                var item = selected.Cast<XmlNode>().Where(x => x.Attributes["name"].Value == column).FirstOrDefault();

                var status = (item != null) ? true : false;

                clbColumns.Items.Add(column, status);
            }

            clbColumns.ItemCheck += clbColumns_ItemCheck;
        }

        #endregion Public Constructors

        #region Private Constructors

        private SelectColumnsDialog()
        {
            InitializeComponent();
        }

        #endregion Private Constructors

        #region Public Properties

        public XmlDocument LayoutXml
        {
            get;
            private set;
        }

        #endregion Public Properties

        #region Private Methods

        private void clbColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var list = (CheckedListBox)sender;

            XmlNode cell;

            if (e.NewValue == CheckState.Unchecked)
            {
                // Removing column from layout
                var pattern = string.Format("//cell[@name='{0}']", list.Items[e.Index]);
                cell = this.LayoutXml.SelectNodes(pattern).Cast<XmlNode>().FirstOrDefault();

                cell.ParentNode.RemoveChild(cell);
            }
            else
            {
                // Adding column to layout
                XmlAttribute attribute;
                cell = this.LayoutXml.CreateNode(XmlNodeType.Element, "cell", string.Empty);
                
                attribute = this.LayoutXml.CreateAttribute("name");
                attribute.Value = (string)list.Items[e.Index];

                cell.Attributes.Append(attribute);

                attribute = this.LayoutXml.CreateAttribute("width");
                attribute.Value = 100.ToString();

                cell.Attributes.Append(attribute);

                var row = this.LayoutXml.SelectNodes("//row").Cast<XmlNode>().FirstOrDefault();
                row.AppendChild(cell);
            }
        }

        #endregion Private Methods
    }
}
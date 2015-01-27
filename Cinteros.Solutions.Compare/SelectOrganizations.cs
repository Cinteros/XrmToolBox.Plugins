﻿namespace Cinteros.Solutions.Compare
{
    using Cinteros.Solutions.Compare.Utils;
    using McTools.Xrm.Connection;
    using System;
    using System.Linq;
    using System.Windows.Forms;

    public partial class SelectOrganizations : UserControl
    {
        #region Public Constructors

        public SelectOrganizations()
        {
            InitializeComponent();

            this.ParentChanged += this.SelectEnvironments_ParentChanged;
        }

        #endregion Public Constructors

        #region Private Methods

        private void cbToggleOrganizations_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;

            this.lvOrganizations.ItemChecked -= lvOrganizations_ItemChecked;
            foreach (var item in this.lvOrganizations.Items.Cast<ListViewItem>().ToArray())
            {
                item.Checked = cb.Checked;
            }
            this.lvOrganizations.ItemChecked += lvOrganizations_ItemChecked;

            this.UpdateCompareSolutionsButton();
        }

        private void cbToggleSolutions_CheckedChanged(object sender, EventArgs e)
        {
            var cb = (CheckBox)sender;

            this.lvSolutions.ItemChecked -= lvSolutions_ItemChecked;
            foreach (var item in this.lvSolutions.Items.Cast<ListViewItem>().ToArray())
            {
                item.Checked = cb.Checked;
            }
            this.lvSolutions.ItemChecked += lvSolutions_ItemChecked;

            this.UpdateCompareSolutionsButton();
        }

        private void lvOrganizations_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.cbToggleOrganizations.CheckedChanged -= this.cbToggleOrganizations_CheckedChanged;
            if (!e.Item.Checked)
            {
                this.cbToggleOrganizations.Checked = false;
            }
            else
            {
                var list = (ListView)sender;
                if (list.CheckedItems.Count == list.Items.Count)
                {
                    this.cbToggleOrganizations.Checked = true;
                }
            }
            this.cbToggleOrganizations.CheckedChanged += this.cbToggleOrganizations_CheckedChanged;

            this.UpdateCompareSolutionsButton();
        }

        private void lvOrganizations_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var list = (ListView)sender;

            foreach (var item in list.Items.Cast<ListViewItem>().Where(x => x.Selected == true))
            {
                list.ItemChecked -= this.lvOrganizations_ItemChecked;
                item.Checked = !item.Checked;
                list.ItemChecked += this.lvOrganizations_ItemChecked;
            }

            this.UpdateCompareSolutionsButton();
        }

        private void lvSolutions_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.cbToggleSolutions.CheckedChanged -= this.cbToggleSolutions_CheckedChanged;
            if (!e.Item.Checked)
            {
                this.cbToggleSolutions.Checked = false;
            }
            else
            {
                var list = (ListView)sender;
                if (list.CheckedItems.Count == list.Items.Count)
                {
                    this.cbToggleSolutions.Checked = true;
                }
            }
            this.cbToggleSolutions.CheckedChanged += this.cbToggleSolutions_CheckedChanged;

            this.UpdateCompareSolutionsButton();
        }

        private void lvSolutions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var list = (ListView)sender;

            foreach (var item in list.Items.Cast<ListViewItem>().Where(x => x.Selected == true))
            {
                list.ItemChecked -= this.lvSolutions_ItemChecked;
                item.Checked = !item.Checked;
                list.ItemChecked += this.lvSolutions_ItemChecked;
            }

            this.UpdateCompareSolutionsButton();
        }

        private void SelectEnvironments_ParentChanged(object sender, EventArgs e)
        {
            var parent = (MainScreen)this.Parent;

            if (parent != null)
            {
                if (parent.ConnectionDetail != null)
                {
                    string[] row;
                    ListViewItem item;

                    parent.WorkAsync(string.Format("Getting solutions information from '{0}'...", parent.ConnectionDetail.OrganizationFriendlyName),
                        (a) => // Work To Do Asynchronously
                        {
                            a.Result = parent.Service.RetrieveMultiple(Helpers.CreateSolutionsQuery()).Entities.Select(x => new Solution(x)).ToArray<Solution>();
                        },
                        (a) =>  // Cleanup when work has completed
                        {
                            lvSolutions.Items.Clear();
                            foreach (var solution in (Solution[])a.Result)
                            {
                                row = new string[] {
                                    solution.FriendlyName,
                                    solution.Version.ToString(),
                                };

                                item = new ListViewItem(row);
                                item.Tag = solution;

                                lvSolutions.Items.Add(item);
                            }
                        }
                    );

                    row = new string[] {
                        parent.ConnectionDetail.OrganizationFriendlyName,
                        parent.ConnectionDetail.ServerName,
                    };

                    lvReference.Items.Clear();
                    lvReference.Items.Add(new ListViewItem(row));

                    lvOrganizations.Items.Clear();

                    foreach (var connection in new ConnectionManager().ConnectionsList.Connections.Where(x => x.ConnectionId != parent.ConnectionDetail.ConnectionId))
                    {
                        row = new string[] {
                            connection.OrganizationFriendlyName,
                            connection.ServerName,
                        };

                        item = new ListViewItem(row);
                        item.Tag = connection;

                        lvOrganizations.Items.Add(item);
                    }
                }

                this.lvOrganizations_ItemSelectionChanged(this.lvOrganizations, null);
                this.lvSolutions_ItemSelectionChanged(this.lvSolutions, null);
            }
        }

        private void UpdateCompareSolutionsButton()
        {
            var button = Helpers.GetCompareSolutionButton(this);

            if (button != null)
            {
                if (this.lvSolutions.CheckedItems.Count > 0 && this.lvOrganizations.CheckedItems.Count > 0)
                {
                    button.Enabled = true;
                }
                else
                {
                    button.Enabled = false;
                }
            }
        }

        #endregion Private Methods
    }
}
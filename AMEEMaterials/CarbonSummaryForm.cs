using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AMEE;

namespace AMEEMaterials
{
    public partial class CarbonSummaryForm : Form
    {
        public CarbonSummaryForm()
        {
            InitializeComponent();
        }

        public void UpdateData(ProfileItem item)
        {
            List<List<string>> results = new List<List<string>>();
            foreach (ProfileItem.Amount x in item.Amounts)
            {
                List<string> cols = new List<string>();
                cols.Add(x.Type);
                cols.Add(x.Value);
                cols.Add(x.Unit);
                results.Add(cols);
            }
            UpdateData(results);
        }

        public void UpdateData(List<List<string>> table) 
        {
            listView.Items.Clear();
            foreach (List<string> row in table) 
            {
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Text = row[0];
                listViewItem.SubItems.Add(row[1]);
                listViewItem.SubItems.Add(row[2]);
                listView.Items.Add(listViewItem);
            }
        }


    }
}

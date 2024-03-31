using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Guard {
    public partial class Form2 : Form {

        public List<string> SelectedDuplicates { get; private set; }
        private List<string> deletedFiles = new List<string>();
        
        public Form2(List<string> duplicates, List<string> selectedPhotos)
        {
            InitializeComponent();
            listBox1.Items.AddRange(duplicates.ToArray());
            listBox1.SelectionMode = SelectionMode.MultiExtended;
        }

        private List<string> LeaveOneFromEachGroup(List<string> duplicates) {
            var toKeep = new List<string>();
            var grouped = duplicates.GroupBy(Path.GetFileName).ToList();

            foreach (var group in grouped) {
                toKeep.Add(group.First());
            }
            return toKeep;
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count == 0)
            {
                MessageBox.Show("First, select the items you want to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SelectedDuplicates = listBox1.SelectedItems.Cast<string>().ToList();
                deletedFiles.AddRange(SelectedDuplicates);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void deleteDuplicatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SelectedDuplicates = LeaveOneFromEachGroup(listBox1.Items.Cast<string>().ToList());
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
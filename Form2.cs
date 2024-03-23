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

        public Form2(List<string> duplicates) {
            InitializeComponent();
            listBox1.Items.AddRange(duplicates.ToArray());
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            CenterControls();
        }

        private void CenterControls() {
            listBox1.Location = new System.Drawing.Point((this.ClientSize.Width - listBox1.Width) / 2, (this.ClientSize.Height - listBox1.Height) / 2);
            int buttonWidth = listBox1.Width / 2;
            button1.Width = buttonWidth;
            button2.Width = buttonWidth;
            button1.Location = new System.Drawing.Point(listBox1.Left, listBox1.Top - button1.Height - 10);
            button2.Location = new System.Drawing.Point(listBox1.Left + buttonWidth, listBox1.Top - button2.Height - 10);
        }

        private void button2_Click(object sender, EventArgs e) {
            SelectedDuplicates = LeaveOneFromEachGroup(listBox1.Items.Cast<string>().ToList());
            DialogResult = DialogResult.Yes;
            Close();
        }

        private List<string> LeaveOneFromEachGroup(List<string> duplicates) {
            var toKeep = new List<string>();
            var grouped = duplicates.GroupBy(Path.GetFileName).ToList();

            foreach (var group in grouped) {
                toKeep.Add(group.First());
            }
            return toKeep;
        }

        private void button1_Click(object sender, EventArgs e) {
            if (listBox1.SelectedItems.Count == 0) {
                MessageBox.Show("First, select the items you want to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                SelectedDuplicates = listBox1.SelectedItems.Cast<string>().ToList();
                deletedFiles.AddRange(SelectedDuplicates);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
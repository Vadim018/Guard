using System;
using System.IO;
using System.Windows.Forms;

namespace Guard {
    public partial class Form3 : Form {
        
        public string NewFileName { get; private set; }

        public Form3(string filePath) {
            InitializeComponent();
            textBox1.Text = Path.GetFileNameWithoutExtension(filePath);
        }

        private void button1_Click(object sender, EventArgs e) {
            NewFileName = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(NewFileName)) {
                MessageBox.Show("Please, enter a file name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
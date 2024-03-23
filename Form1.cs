using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Guard
{
    public partial class Form1 : Form
    {
        private List<string> selectedPhotos = new List<string>();

        public Form1()
        {
            InitializeComponent();
            listBox1.DrawMode = DrawMode.OwnerDrawFixed;
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            listBox1.DoubleClick += new EventHandler(listBox1_DoubleClick);
            listBox1.MouseDown += new MouseEventHandler(listBox1_MouseDown);
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private void listBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int index = listBox1.IndexFromPoint(e.Location);

                if (index != ListBox.NoMatches)
                {
                    listBox1.SelectedIndex = index;
                }
                else
                {
                    listBox1.ClearSelected();
                }
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                string selectedFile = selectedPhotos[listBox1.SelectedIndex];

                if (IsImageFile(selectedFile))
                {
                    Form3 form3 = new Form3(selectedFile);
                    form3.Show();
                }
                else
                {
                    MessageBox.Show("Selected file isn't an image!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "All files|*.*|Photo Files (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Video Files (*.mp4;*.mkv;*.avi;*.mov)|*.mp4;*.mkv;*.avi;*.mov";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var selectedFiles = openFileDialog.FileNames;

                foreach (var file in selectedFiles)
                {
                    if (!IsImageFile(file) && !IsVideoFile(file))
                    {
                        MessageBox.Show($"The file '{Path.GetFileName(file)}' isn't a photo or video and can't be added!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                if (selectedFiles.Length < 2)
                {
                    MessageBox.Show("Please, select at least two photos or videos!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int photoCount = selectedFiles.Count(file => IsImageFile(file));
                int videoCount = selectedFiles.Length - photoCount;

                if (photoCount > 0 && videoCount > 0)
                {
                    MessageBox.Show("Select files of only one type: either all photos or all videos!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                selectedPhotos.AddRange(selectedFiles.Where(file => !selectedPhotos.Contains(file)));
                UpdatePhotoList();
            }
        }

        private bool IsImageFile(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            return extension == ".jpg" || extension == ".jpeg" || extension == ".png" || extension == ".gif" || extension == ".bmp";
        }

        private bool IsVideoFile(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            return extension == ".mp4" || extension == ".mkv" || extension == ".avi" || extension == ".mov";
        }

        private void UpdatePhotoList()
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();

            foreach (var photo in selectedPhotos)
            {
                listBox1.Items.Add(new ListBoxItem
                {
                    Text = photo,
                    IsDuplicate = IsDuplicate(photo)
                });
            }
            listBox1.EndUpdate();
        }

        private bool IsDuplicate(string photo)
        {
            return selectedPhotos.Count(p => GetFileSignature(p) == GetFileSignature(photo)) > 1;
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            ListBoxItem item = (ListBoxItem)listBox1.Items[e.Index];
            e.DrawBackground();
            Graphics g = e.Graphics;
            Brush textBrush;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                textBrush = Brushes.White;
            }
            else
            {
                textBrush = item.IsDuplicate ? Brushes.Red : Brushes.Black;
            }

            string itemText = $"{e.Index + 1}. {item.Text}";
            SizeF textSize = g.MeasureString(itemText, e.Font);
            float textX = e.Bounds.Left + 10;
            float textY = e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2;
            g.DrawString(itemText, e.Font, textBrush, textX, textY);
            e.DrawFocusRectangle();
        }

        private List<List<string>> FindDuplicateGroups(List<string> photos)
        {
            var duplicateGroups = new List<List<string>>();
            var uniqueHashes = new HashSet<string>();
            var duplicateHashes = new Dictionary<string, string>();

            foreach (var photo in photos)
            {
                string photoHash = GetFileSignature(photo);

                if (!uniqueHashes.Add(photoHash))
                {
                    if (!duplicateHashes.ContainsKey(photoHash))
                    {
                        duplicateHashes[photoHash] = photo;
                    }
                }
            }

            foreach (var hash in duplicateHashes.Keys)
            {
                var group = photos.Where(photo => GetFileSignature(photo) == hash).ToList();

                if (group.Count > 1)
                {
                    duplicateGroups.Add(group);
                }
            }
            return duplicateGroups;
        }

        private void RemoveSelectedDuplicates(List<string> selectedDuplicates)
        {
            foreach (var file in selectedDuplicates)
            {
                selectedPhotos.Remove(file);
                File.Delete(file);
            }
        }

        private void RemoveAllDuplicates(List<List<string>> duplicateGroups)
        {
            foreach (var group in duplicateGroups)
            {
                for (int i = 1; i < group.Count; i++)
                {
                    selectedPhotos.Remove(group[i]);
                    File.Delete(group[i]);
                }
            }
        }

        private string GetFileSignature(string filePath)
        {
            try
            {
                using (var stream = File.OpenRead(filePath))
                {
                    long fileSize = new FileInfo(filePath).Length;
                    byte[] fileContent = new byte[Math.Min(fileSize, 8192)];
                    stream.Read(fileContent, 0, fileContent.Length);

                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        byte[] hashBytes = md5.ComputeHash(fileContent);
                        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                }
            } 
            catch (Exception ex) 
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }

        private (DialogResult, List<string>) ShowFormDialog(List<string> duplicates)
        {
            var form2 = new Form2(duplicates);
            var dialogResult = form2.ShowDialog();
            return (dialogResult, form2.SelectedDuplicates);
        }

        private void RenameFile(string filePath, Form form1)
        {
            var renameForm = new RenameForm(filePath);
            form1.Hide();

            while (true)
            {
                if (renameForm.ShowDialog() == DialogResult.OK)
                {
                    string newFileName = renameForm.NewFileName;

                    if (newFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                    {
                        MessageBox.Show("The file name contains illegal characters. Please choose another name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    string directory = Path.GetDirectoryName(filePath);
                    string newFilePath = Path.Combine(directory, newFileName + Path.GetExtension(filePath));

                    if (File.Exists(newFilePath))
                    {
                        MessageBox.Show("A file with that name already exists. Please choose another name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }
                    try
                    {
                        File.Move(filePath, newFilePath);

                        if (selectedPhotos.Contains(filePath))
                        {
                            selectedPhotos[selectedPhotos.IndexOf(filePath)] = newFilePath;
                        }
                        UpdatePhotoList();
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"An error occurred while renaming the file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    break;
                }
            }
            form1.Show();
        }

        private void вихідToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void перевіритиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!selectedPhotos.Any())
            {
                MessageBox.Show("First, add photo or video to listbox!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var duplicateGroups = FindDuplicateGroups(selectedPhotos);

            if (duplicateGroups.Any())
            {
                var duplicates = duplicateGroups.SelectMany(g => g).ToList();
                var (deleteResult, selectedDuplicates) = ShowFormDialog(duplicates);

                if (deleteResult == DialogResult.OK && selectedDuplicates != null && selectedDuplicates.Any())
                {
                    RemoveSelectedDuplicates(selectedDuplicates);
                }
                else if (deleteResult == DialogResult.Yes)
                {
                    RemoveAllDuplicates(duplicateGroups);
                }
                UpdatePhotoList();
            }
            else
            {
                StylishMessageBox.Show("No duplicates found among these files!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void перейменуватиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Please, select a file to rename!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (listBox1.SelectedIndices.Count > 1)
            {
                MessageBox.Show("Please, select only one file to rename at a time!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            string selectedFile = selectedPhotos[listBox1.SelectedIndex];
            RenameFile(selectedFile, this);
        }

        private void очиститиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            selectedPhotos.Clear();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedPhotos.Count == 0)
            {
                MessageBox.Show("Please add files before saving!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to save the files!";

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string destinationFolder = folderDialog.SelectedPath;
                    SaveFiles(destinationFolder);
                }
            }
        }

        private void SaveFiles(string destinationFolder)
        {
            try
            {
                foreach (var file in selectedPhotos)
                {
                    string fileName = Path.GetFileName(file);
                    string destinationPath = Path.Combine(destinationFolder, fileName);

                    if (File.Exists(destinationPath))
                    {
                        var result = MessageBox.Show($"The file {fileName} already exists. Do you want to replace it?", "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.No)
                        {
                            continue;
                        }
                    }
                    File.Copy(file, destinationPath, true);
                }
                MessageBox.Show("Files have been successfully saved!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedPhotos.Count > 0)
            {
                var result = MessageBox.Show("Are you sure you want to delete all files?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    foreach (var photoPath in selectedPhotos.ToList())
                    {
                        try
                        {
                            File.Delete(photoPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to delete {photoPath}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    listBox1.Items.Clear();
                    selectedPhotos.Clear();
                }
            }
            else
            {
                MessageBox.Show("There are no files to delete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }
    }
}
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Guard {

    public partial class Form3 : Form {

        private PictureBox pictureBox;
        private Image originalImage;

        public Form3(string imagePath) {
            InitializeComponent();
            this.originalImage = Image.FromFile(imagePath);
            int maxWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int maxHeight = Screen.PrimaryScreen.WorkingArea.Height;
            Size resizedSize = GetResizedSize(originalImage.Size, maxWidth, maxHeight);
            this.ClientSize = resizedSize;
            this.pictureBox = new PictureBox();
            this.pictureBox.Dock = DockStyle.Fill;
            this.pictureBox.Image = ResizeImage(originalImage, resizedSize.Width, resizedSize.Height);
            this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            this.Controls.Add(this.pictureBox);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private Size GetResizedSize(Size originalSize, int maxWidth, int maxHeight) {
            int screenPadding = 30;
            maxWidth -= screenPadding * 7;
            maxHeight -= screenPadding * 7;
            double ratioX = (double)maxWidth / originalSize.Width;
            double ratioY = (double)maxHeight / originalSize.Height;
            double ratio = Math.Min(ratioX, ratioY);
            int newWidth = (int)(originalSize.Width * ratio);
            int newHeight = (int)(originalSize.Height * ratio);
            return new Size(newWidth, newHeight);
        }

        private Image ResizeImage(Image image, int width, int height) {
            Bitmap result = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(result)) {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return result;
        }
    }
}
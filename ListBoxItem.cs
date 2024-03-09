using System.Drawing;

namespace Guard {
    internal class ListBoxItem {
        public string Text { get; set; }
        public Image Thumbnail { get; set; }
        public bool IsDuplicate { get; set; }
    }
}
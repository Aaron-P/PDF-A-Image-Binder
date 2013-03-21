using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// Refactor all the temporary code
// Better name elements

// NOTES ///////////////////////////////////////////////////////////////////////
//convert all lossless to png
//pass through jpeg
//crush png?
//strip exif?
//save last path files loaded from, check if exists, if not open default (desktop?)
//when image selected, check it exists before loading preview
//make sure image exists while processing
//jpeg passthrough
//pdf settings, meta data, disable editing, disable image selection?
//disable bind button if no images
//prompt on close if new images since last save
//embed background image as resource (if we use a background image)
//add sorting to list
//size listview to deal with scrollbars? allways on?
//add ability to remove selected item from list (button, del key)?
//allow multiselect? bind by selection? checkbox? (display based on last selected?)
//fix button enable when drag drop files
//disable jpeg settings if -> lossless is selected
////////////////////////////////////////////////////////////////////////////////

namespace PDF_A_Image_Binder
{
    /*
        public class TransparentTrackBar : TrackBar
        {
            protected override void OnCreateControl()
            {
                SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                if (Parent != null)
                    BackColor = Parent.BackColor;

                base.OnCreateControl();
            }
        }
 
        public class PercentNumericUpDown : NumericUpDown
        {
            protected override void UpdateEditText()
            {
                // Append the units to the end of the numeric value
                this.Text = this.Value + "%";
            }
        }
     */

    public partial class MainForm : Form
    {
        private System.Drawing.Imaging.ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {

            System.Drawing.Imaging.ImageCodecInfo[] codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageDecoders();

            foreach (System.Drawing.Imaging.ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /*
        bmp  = image/bmp
        gif  = image/gif
        jpg  = image/jpeg (image/pjpeg)
        jpeg = image/jpeg (image/pjpeg)
        png  = image/png (image/x-png)
        tiff = image/tiff
        */

        public void test()
        {
            System.IO.FileStream stream = System.IO.File.Create("test.jpg");
            // Initialize the bytes array with the stream length and then fill it with data
//            byte[] bytesInStream = new byte[stream.Length];
//            stream.Read(bytesInStream, 0, (int)bytesInStream.Length);    
//            // Use write method to write to the specified file
//            fileStream.Write(bytesInStream, 0, (int) bytesInStream.Length);

            System.Drawing.Imaging.ImageCodecInfo jgpEncoder = GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg);

            // Create an Encoder object based on the GUID 
            // for the Quality parameter category.
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            // Create an EncoderParameters object. 
            // An EncoderParameters object has an array of EncoderParameter 
            // objects. In this case, there is only one 
            // EncoderParameter object in the array.
            System.Drawing.Imaging.EncoderParameters myEncoderParameters = new System.Drawing.Imaging.EncoderParameters(1);
            System.Drawing.Imaging.EncoderParameter myEncoderParameter = new System.Drawing.Imaging.EncoderParameter(myEncoder, 95L);
            myEncoderParameters.Param[0] = myEncoderParameter;

//            System.IO.Stream stream1 = new System.IO.MemoryStream();
            System.Drawing.Image image1 = System.Drawing.Image.FromFile(@"test.png");
            image1.Save(stream, jgpEncoder, myEncoderParameters);
            
        }




//            colorDialog1.Color = System.Drawing.Color.White;
//            colorDialog1.ShowDialog();


        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetMenuItemCount(IntPtr hMenu);
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        public const Int32 WM_SYSCOMMAND = 0x112;
        public const Int32 MF_SEPARATOR = 0x800;
        public const Int32 MF_BYPOSITION = 0x400;
        public const Int32 MF_STRING = 0x0;
        public const Int32 IDM_CUSTOMITEM1 = 1000;

        public MainForm()
        {
            InitializeComponent();

            IntPtr sysMenuHandle = GetSystemMenu(this.Handle, false);
            Int32 menuCount = GetMenuItemCount(sysMenuHandle);
            InsertMenu(sysMenuHandle, menuCount - 1, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(sysMenuHandle, menuCount - 1, MF_BYPOSITION, IDM_CUSTOMITEM1, "Reset Window");
        }

        protected override void WndProc(ref Message m)
        {
            if(m.Msg == WM_SYSCOMMAND)
            {
                switch(m.WParam.ToInt32())
                {
                    case IDM_CUSTOMITEM1:
                        //change these to default values, don't know how to get this at compile time
                        this.WindowState = FormWindowState.Normal;
                        this.Width = 888;
                        this.Height = 554;
                        this.CenterToScreen();
                        return;
                    default:
                        break;
                } 
            }
            base.WndProc(ref m);
        }







        //http://tutorialgenius.blogspot.com/2010/12/net-determining-file-mime-types.html
        [System.Runtime.InteropServices.DllImport("urlmon.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        static extern int FindMimeFromData(IntPtr pBC,
                                           [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pwzUrl,
                                           [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPArray, ArraySubType = System.Runtime.InteropServices.UnmanagedType.I1, SizeParamIndex = 3)] 
                                           byte[] pBuffer, int cbSize,
                                           [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string pwzMimeProposed, int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

        /// <summary>
        /// Ensures that file exists and retrieves the mime type
        /// </summary>
        /// <param name="file">Full path to the file</param>
        /// <returns>Returns the Mime Type</returns>
        public string GetMimeFromFile(string filePath)
        {
            IntPtr mimeOut;
            if (!System.IO.File.Exists(filePath))
                throw new System.IO.FileNotFoundException(filePath + " not found");

            int bufferLength = (int)new System.IO.FileInfo(filePath).Length;
            if (bufferLength > 4096)
                bufferLength = 4096;
            System.IO.FileStream fstream = System.IO.File.OpenRead(filePath);

            byte[] buffer = new byte[bufferLength];
            fstream.Read(buffer, 0, bufferLength);
            fstream.Close();
            int result = FindMimeFromData(IntPtr.Zero, filePath, buffer, bufferLength, null, 0, out mimeOut, 0);

            if (result != 0)
                throw System.Runtime.InteropServices.Marshal.GetExceptionForHR(result);

            string mimeType = System.Runtime.InteropServices.Marshal.PtrToStringUni(mimeOut);
            System.Runtime.InteropServices.Marshal.FreeCoTaskMem(mimeOut);

            return mimeType;
        }








        private void jpegQualitySlider_Scroll(object sender, EventArgs e)
        {
            jpegQualityLabel.Text = "JPEG Quality ("+jpegQualitySlider.Value.ToString() + ")";
            jpegQualityLabel.Location = new Point((jpegQualityLabel.Parent.Width / 2) - (jpegQualityLabel.Width / 2), jpegQualityLabel.Location.Y);//keep label centered, is there a better way?
        }

        private void imagePreviewBox_Resize(object sender, EventArgs e)
        {
            if (imagePreviewBox.Image != null)
            {
                if (imagePreviewBox.Image.Width > imagePreviewBox.Width || imagePreviewBox.Image.Height > imagePreviewBox.Height)
                    imagePreviewBox.SizeMode = PictureBoxSizeMode.Zoom;
                else
                    imagePreviewBox.SizeMode = PictureBoxSizeMode.CenterImage;
            }
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            if (magnificationZoomRadioButton.Checked)
                zoomFactorNumericUpDown.Enabled = true;
            else
                zoomFactorNumericUpDown.Enabled = false;
        }

        private void fillColorSelectButton_Click(object sender, EventArgs e)
        {
            DialogResult result = fillColorDialog.ShowDialog();
            // See if user pressed ok.
            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                fillColorPreviewBox.BackColor = fillColorDialog.Color;
            }
        }

        private void fillColorPreviewBox_Click(object sender, EventArgs e)
        {
            //same as fillColorSelectButton_Click

        }

        private void selectedImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            imagePreviewBox.Image = null;
            if (selectedImageList.SelectedItems.Count > 0)
            {
                //STORE THE FULL PATH SOMEPLACE AND USE THAT INSTEAD OF PARTS
                //should we store the full path someplace? hidden?
                //                string path = SelectedImageList.SelectedItems[0].SubItems[columnHeader3.Index].Text + "\\" + SelectedImageList.SelectedItems[0].SubItems[columnHeader1.Index].Text;
                //                Image bgImage = Image.FromFile(path);
                Image bgImage = Image.FromFile(selectedImageList.SelectedItems[0].Tag.ToString());

                if (bgImage.Width > imagePreviewBox.Width || bgImage.Height > imagePreviewBox.Height)
                    imagePreviewBox.SizeMode = PictureBoxSizeMode.Zoom;
                else
                    imagePreviewBox.SizeMode = PictureBoxSizeMode.CenterImage;



                bgImage.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, 0);
                imagePreviewBox.Image = bgImage;

                //                ImagePreviewBox.

                //ImagePreviewBox.SizeMode
            }
        }

        private void selectImagesButton_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.imageSelectDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                if (imageSelectDialog.FileNames.Count() > 0)//this check should be done after file types are checked
                {
                    bindButton.Enabled = true;
                    clearImagesButton.Enabled = true;

                    //STORE THE FULL PATH SOMEPLACE AND USE THAT INSTEAD OF PARTS

                    // Read the files 
                    foreach (String file in imageSelectDialog.FileNames)
                    {
                        try
                        {
                            string path = System.IO.Path.GetDirectoryName(file);
                            string name = System.IO.Path.GetFileName(file);
                            string ext = System.IO.Path.GetExtension(file);

                            //do this in a better place
                            if (selectedImageList.Items.Count == 0 && pdfTitleTextBox.Text.Trim().Length == 0)
                                pdfTitleTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(file).Trim();

                            ListViewItem item = new ListViewItem(name);
                            item.Checked = true;
//                            item.SubItems.Add(ext);//sniff proper type
                            item.SubItems.Add(GetMimeFromFile(file));
                            item.SubItems.Add(path);
                            item.Tag = file;
                            selectedImageList.Items.Add(item);
                        }
                        //                        catch (SecurityException ex)
                        //                        {
                        //                            // The user lacks appropriate permissions to read files, discover paths, etc.
                        //                            MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                        //                                "Error message: " + ex.Message + "\n\n" +
                        //                                "Details (send to Support):\n\n" + ex.StackTrace
                        //                            );
                        //                        }
                        catch (Exception ex)
                        {
                            // Could not load the image - probably related to Windows file system permissions.
                            MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void selectedImageList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            selectedImageList.DoDragDrop(selectedImageList.SelectedItems, DragDropEffects.Move);
        }

        private void selectedImageList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))//do we need the typeof?
                e.Effect = DragDropEffects.Move;
        }

        private void selectedImageList_DragDrop(object sender, DragEventArgs e)
        {//off by one when dragging up
            //change to not show default cursor
            //allow drag drop of external files

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                //there should only be one add file method for all events using it
//                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (String file in (string[])e.Data.GetData(DataFormats.FileDrop))
                {
                    try
                    {
                        string path = System.IO.Path.GetDirectoryName(file);
                        string name = System.IO.Path.GetFileName(file);
                        string ext = System.IO.Path.GetExtension(file);

                        //do this in a better place
                        if (selectedImageList.Items.Count == 0 && pdfTitleTextBox.Text.Trim().Length == 0)
                            pdfTitleTextBox.Text = System.IO.Path.GetFileNameWithoutExtension(file).Trim();

                        ListViewItem item = new ListViewItem(name);
                        item.Checked = true;
                        item.SubItems.Add(GetMimeFromFile(file));
//                        item.SubItems.Add(ext);//sniff proper type
                        item.SubItems.Add(path);
                        item.Tag = file;
                        selectedImageList.Items.Add(item);
                    }
                    //                        catch (SecurityException ex)
                    //                        {
                    //                            // The user lacks appropriate permissions to read files, discover paths, etc.
                    //                            MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                    //                                "Error message: " + ex.Message + "\n\n" +
                    //                                "Details (send to Support):\n\n" + ex.StackTrace
                    //                            );
                    //                        }
                    catch (Exception ex)
                    {
                        // Could not load the image - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                        + ". You may not have permission to read the file, or " +
                        "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
            
            
            }
            else if (e.Data.GetDataPresent(typeof(ListView.SelectedListViewItemCollection)))//do we need the typeof?
            {
                if (selectedImageList.SelectedItems.Count == 0)
                    return;

                Point p = selectedImageList.PointToClient(new Point(e.X, e.Y));
                ListViewItem dragToItem = selectedImageList.GetItemAt(p.X, p.Y);

                if (dragToItem == null)
                    return;

                int dragIndex = dragToItem.Index;

                ListViewItem dataItem = (e.Data.GetData(typeof(ListView.SelectedListViewItemCollection)) as ListView.SelectedListViewItemCollection)[0];
                int itemIndex = dragIndex;
                if (itemIndex == dataItem.Index)
                    return;

                itemIndex = itemIndex + 1;

                ListViewItem insertItem = (ListViewItem)dataItem.Clone();
                selectedImageList.Items.Insert(itemIndex, insertItem);
                selectedImageList.Items.Remove(dataItem);
            }
        }

        private void selectedImageList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            //sorting code goes here
        }

        private void pdfSaveDialog_FileOk(object sender, CancelEventArgs e)
        {
            //do saving here
            //show progress bar?
        }

        private void selectedImageList_KeyUp(object sender, KeyEventArgs e)//keypress instead?
        {
            if (e.KeyCode == Keys.Delete)
            {
                //delete selected
            }
        }

        private void clearImagesButton_Click(object sender, EventArgs e)
        {
            selectedImageList.Items.Clear();
            bindButton.Enabled = false;
            clearImagesButton.Enabled = false;
            imagePreviewBox.Image = null;
            pdfTitleTextBox.Text = string.Empty;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void bindButton_Click(object sender, EventArgs e)
        {
            this.pdfSaveDialog.ShowDialog();
            //this.test();
        }







/* //drag file into listview
        private void selectedImageList_DragDrop(object sender, DragEventArgs e)
        {
            selectedImageList.Items.Add(e.Data.ToString());
        }

        private void selectedImageList_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
*/
    }
}

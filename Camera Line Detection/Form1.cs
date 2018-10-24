using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Camera_Line_Detection {
	public partial class Form1 : Form {
		public Form1() {
			InitializeComponent();
			// enumerate video devices
			FilterInfoCollection videoDevices = new FilterInfoCollection(
					FilterCategory.VideoInputDevice);
			// create video source
			VideoCaptureDevice videoSource = new VideoCaptureDevice(
					videoDevices[0].MonikerString);
			// set NewFrame event handler
			videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
			// start the video source
			videoSource.Start();
			// ...
			// signal to stop
			//videoSource.SignalToStop();
			// ...

		}

		private void video_NewFrame(object sender,
		NewFrameEventArgs eventArgs) {
			// get new frame
			Bitmap sourceImg = eventArgs.Frame;
			// process the frame


			Image clonedImg = MakeGrayscale3(sourceImg);
			pictureBox1.InitialImage = null;
			pictureBox1.Image = clonedImg;
			//pictureBox1.Image = (Image)bitmap;
		}

		public void DetectLine(Bitmap bitmap) {
			float MaxValue = 0;
			float MinValue = int.MaxValue;
			for (int y = 0; y < bitmap.Size.Height; y++) {
				for (int x = 0; x < bitmap.Size.Width; x++) {
					if (bitmap.GetPixel(x, y).GetBrightness() > MaxValue) {
						MaxValue = bitmap.GetPixel(x, y).GetBrightness();
					}
					if (bitmap.GetPixel(x, y).GetBrightness() < MinValue) {
						MinValue = bitmap.GetPixel(x, y).GetBrightness();
					}
				}
			}
		}

		public static Bitmap MakeGrayscale3(Bitmap original) {
			//create a blank bitmap the same size as original
			Bitmap newBitmap = new Bitmap(original.Width, original.Height);

			//get a graphics object from the new image
			Graphics g = Graphics.FromImage(newBitmap);

			//create the grayscale ColorMatrix
			ColorMatrix colorMatrix = new ColorMatrix(
			   new float[][]
			   {
		 new float[] {.3f, .3f, .3f, 0, 0},
		 new float[] {.59f, .59f, .59f, 0, 0},
		 new float[] {.11f, .11f, .11f, 0, 0},
		 new float[] {0, 0, 0, 1, 0},
		 new float[] {0, 0, 0, 0, 1}
			   });

			//create some image attributes
			ImageAttributes attributes = new ImageAttributes();

			//set the color matrix attribute
			attributes.SetColorMatrix(colorMatrix);

			//draw the original image on the new image
			//using the grayscale color matrix
			g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
			   0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

			//dispose the Graphics object
			g.Dispose();
			return newBitmap;
		}
	}
}

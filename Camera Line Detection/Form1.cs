using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Drawing.Imaging;
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


			Image scaledImg = resizeImage(sourceImg, new Size(400, 400));
			scaledImg = MakeGrayscale3((Bitmap)scaledImg);
			pictureBox1.InitialImage = null;
			pictureBox1.Image = scaledImg;
			//pictureBox1.Image = (Image)bitmap;
		}

		public void DetectLine(Bitmap bitmap) {
			//Get scale of colors
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

			//Determen which pixels are black and white
			//			Threshold = .24 // use experimentation to determine this value
			//if pixel(x, y) < (MaxActualValue - MinActualValue) * Threshold + MinActualValue then
			//  pixel(x, y) = black
			//else
			//  pixel(x, y) = white
			float threshold = .24f;
			float thing = (MaxValue - MinValue) * threshold + MinValue;
			for (int y = 0; y < bitmap.Size.Height; y++) {
				for (int x = 0; x < bitmap.Size.Width; x++) {
					if (bitmap.GetPixel(x, y).GetBrightness() > thing) {

					}
				}
			}
		}

		public static Image resizeImage(Image sourceImage, Size newSize) {
			Image newImage = new Bitmap(newSize.Width, newSize.Height);
			using (Graphics GFX = Graphics.FromImage((Bitmap)newImage)) {
				GFX.DrawImage(sourceImage, new Rectangle(Point.Empty, newSize));
			}
			return newImage;
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

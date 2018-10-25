using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
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


			Image scaledImg = resizeImage(sourceImg, new Size(150, 150));
			scaledImg = MakeGrayscale3((Bitmap)scaledImg);
			scaledImg = DetectLine((Bitmap)scaledImg);
			pictureBox1.InitialImage = null;
			pictureBox1.Image = scaledImg;
			//pictureBox1.Image = (Image)bitmap;
		}

		public Image DetectLine(Bitmap bitmap) {
			//Get scale of colors
			Bitmap outputImg = new Bitmap(bitmap.Size.Width, bitmap.Size.Height);
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
			List<cordinate> CenterLines = new List<cordinate>();
			for (int y = 0; y < bitmap.Size.Height; y++) {
				int BlackCount = 0;
				int BlackSum = 0;
				for (int x = 0; x < bitmap.Size.Width; x++) {
					if (bitmap.GetPixel(x, y).GetBrightness() > thing) {
						outputImg.SetPixel(x, y, Color.White);
					} else {
						outputImg.SetPixel(x, y, Color.Black);
						BlackCount++;
						BlackSum += x;
					}
				}
				if (BlackCount != 0) {
					int LineCenterX = BlackSum / BlackCount;
					CenterLines.Add(new cordinate() { x = LineCenterX, y = y });
				}
			}

			//			for y = MinY to MaxY
			//  BlackCount = 0
			//  BlackSum = 0
			//  for x = MinX to MaxX

			//	if pixel(x, y) = Black then
			//	   increment BlackCount by 1

			//	  increment BlackSum by x

			//	end if
			//  next x
			//  LineCenterX = BlackSum / BlackCount
			//  AddNewCenterCoordinate(LineCenterX, y)
			//next y
			for (int y = 0; y < bitmap.Size.Height; y++) {

				for (int x = 0; x < bitmap.Size.Width; x++) {
					if (outputImg.GetPixel(x, y) == Color.Black) {
						
					}
				}
				
			}

			//			for i = 1 to n // n is the number of center points collected
			//  s1 = s1 + x(i) * y(i)
			//  s2 = s2 + x(i)
			//  s3 = s3 + y(i)
			//  s4 = s4 + x(i) * x(i)
			//next i
			//SlopeNumerator = n * s1 - s2 * s3
			//SlopeDenominator = n * s4 - s2 * s2
			//if SlopeDenominator is not near 0 then Slope = SlopeNumerator / SlopeDenominator
			int s1 = 0, s2 = 0, s3 = 0, s4 = 0;
			int n = CenterLines.Count;
			for (int i = 0; i < n; i++) {
				s1 = s1 + (CenterLines[i].x * CenterLines[i].y);
				s2 = s2 + CenterLines[i].x;
				s3 = s3 + CenterLines[i].y;
				s4 = s4 + (CenterLines[i].x * CenterLines[i].x);
			}
			float SlopeNumerator = (n * s1) - (s2 * s3);
			float SlopeDenominator = (n * s4) - (s2 * s2);
			float Slope = 0f;
			if (Math.Abs(SlopeDenominator) > 0.05) {
				Slope = SlopeNumerator / SlopeDenominator;
			}

			//			for i = 1 to n // n is the number of points collected – watch for 0 in the division!
			//  SumX = SumX + x(i)
			//  SumY = SumY + y(i)
			//next i
			//Intercept = SumY / n - Slope * SumX / n
			int SumX = 0;
			int SumY = 0;
			for (int i = 0; i < n; i++) {
				SumX += CenterLines[i].x;
				SumY += CenterLines[i].y;
			}
			float Intercept = (float)SumY / n - Slope * (float)SumX / n;
			//			if Slope is close to 0
			//  x = AverageCenterValueX
			//else
			//  x = (y - Intercept) / Slope
			//end if
			bool CloseToZero = Math.Abs(Slope) < 0.05;
			float AverageX = (float)s2 / n;
			for (int y = 0; y < outputImg.Height; y++) {
				int x = 0;
				if (CloseToZero) {
					x = (int)AverageX;
				} else {
					x = (int)((y - Intercept) / Slope);
				}
				if (x > 0 && x < outputImg.Width) {
					outputImg.SetPixel(x, y, Color.Red);
				}
			}
			return outputImg;

		}

		struct cordinate {
			public int x;
			public int y;
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

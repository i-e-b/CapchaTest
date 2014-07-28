using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace CaptchaTest {
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class Captcha : IHttpHandler {
		protected static int WIDTH = 300;
		protected static int HEIGHT = 120;

		public void ProcessRequest (HttpContext context) {
			List<Bitmap> frames = new List<Bitmap>();

			Random rnd = new Random();
			for (int i = 0; i < 50; i++) {
				Color A = Color.White;
				Color B = Color.Black;
				frames.Add(DrawImage("159786", A, B, i));
			}

			OutputAnimatedGif(frames.ToArray(), context);
		}

		public void OutputAnimatedGif (Bitmap[] frames, HttpContext context) {
			MemoryStream memoryStream;
			BinaryWriter binaryWriter;
			Byte[] buf1;
			Byte[] buf2;
			Byte[] buf3;
			//Variable declaration

			context.Response.ContentType = "image/gif";
			memoryStream = new MemoryStream();
			buf2 = new Byte[19];
			buf3 = new Byte[8];
			buf2[0] = 33;  //extension introducer
			buf2[1] = 255; //application extension
			buf2[2] = 11;  //size of block
			buf2[3] = 78;  //N
			buf2[4] = 69;  //E
			buf2[5] = 84;  //T
			buf2[6] = 83;  //S
			buf2[7] = 67;  //C
			buf2[8] = 65;  //A
			buf2[9] = 80;  //P
			buf2[10] = 69; //E
			buf2[11] = 50; //2
			buf2[12] = 46; //.
			buf2[13] = 48; //0
			buf2[14] = 3;  //Size of block
			buf2[15] = 1;  //
			buf2[16] = 0;  //
			buf2[17] = 0;  //
			buf2[18] = 0;  //Block terminator
			buf3[0] = 33;  //Extension introducer
			buf3[1] = 249; //Graphic control extension
			buf3[2] = 4;   //Size of block
			buf3[3] = 9;   //Flags: reserved, disposal method, user input, transparent color
			buf3[4] = 1;  //Delay time low byte
			buf3[5] = 0;   //Delay time high byte
			buf3[6] = 255; //Transparent color index
			buf3[7] = 0;   //Block terminator
			binaryWriter = new BinaryWriter(context.Response.OutputStream);
			bool first = true;
			foreach (Bitmap image in frames) {
				image.Save(memoryStream, ImageFormat.Gif);
				buf1 = memoryStream.ToArray();

				if (first) {
					first = false;
					//only write these the first time....
					binaryWriter.Write(buf1, 0, 781); //Header & global color table
					binaryWriter.Write(buf2, 0, 19); //Application extension
				}

				binaryWriter.Write(buf3, 0, 8); //Graphic extension
				binaryWriter.Write(buf1, 789, buf1.Length - 790); //Image data

				memoryStream.SetLength(0);
			}
			binaryWriter.Write(";"); //Image terminator
			binaryWriter.Close();
			context.Response.Flush();
			context.Response.End();
		}

		public Bitmap DrawImage (string Code, Color A, Color B, int idx) {

			Bitmap img = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb);
			Graphics g = Graphics.FromImage(img);
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
			Rectangle r = new Rectangle(0, 0, WIDTH, HEIGHT);


			HatchBrush background = new HatchBrush(HatchStyle.Percent50, A, B);
			HatchBrush foreground = new HatchBrush(HatchStyle.Percent50, B, A);

			Font f = new Font("Arial Black", 48);

			g.FillRectangle(background, r);
			g.DrawString(Code, f, foreground, 10, 10);
			g.DrawString(Code, f, background,
					8 + ((idx % 3)*2) ,
					8 + (((idx+1) % 3)*2) 
				);

			g.Flush();
			g.Dispose();

			return img;
		}

		public bool IsReusable {
			get {
				return false;
			}
		}
	}
}

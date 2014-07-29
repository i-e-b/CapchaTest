namespace CaptchaTest {
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Web;
    using System.Web.Services;

    [WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class Captcha : IHttpHandler {
        protected static int HEIGHT = 120;
        protected static int WIDTH = 300;
        public bool IsReusable {
            get {
                return false;
            }
        }

        public void ProcessRequest (HttpContext context) {
			var frames = new List<Bitmap>();

			for (var i = 0; i < 50; i++) {
				var A = Color.White;
				var B = Color.Black;
				frames.Add(DrawImage("159786", A, B, i));
			}

			OutputAnimatedGif(frames.ToArray(), context);
		}

		public void OutputAnimatedGif (Bitmap[] frames, HttpContext context) {
			context.Response.ContentType = "image/gif";
			var memoryStream = new MemoryStream();
            var applicationExtension = new byte[]{
                 33,  //extension introducer
                 255, //application extension
                 11,  //size of block
                 78,  //N
                 69,  //E
                 84,  //T
                 83,  //S
                 67,  //C
                 65,  //A
                 80,  //P
                 69, //E
                 50, //2
                 46, //.
                 48, //0
                 3,  //Size of block
                 1,  //
                 0,  //
                 0,  //
                 0   //Block terminator
            };
            var graphicControlExtension = new byte[]{
                 33,   //Extension introducer
                 249,  //Graphic control extension
                 4,    //Size of block
                 9,    //Flags: reserved, disposal method, user input, transparent color
                 1,    //Delay time low byte
                 0,    //Delay time high byte
                 255,  //Transparent color index
                 0     //Block terminator
            };
			var binaryWriter = new BinaryWriter(context.Response.OutputStream);
			var first = true;
			foreach (var image in frames) {
				image.Save(memoryStream, ImageFormat.Gif);
				var gifFrame = memoryStream.ToArray();

				if (first) {
					first = false;
					//only write these the first time....
					binaryWriter.Write(gifFrame, 0, 781); //Header & global color table
					binaryWriter.Write(applicationExtension, 0, 19); //Application extension
				}

				binaryWriter.Write(graphicControlExtension, 0, 8); //Graphic extension
				binaryWriter.Write(gifFrame, 789, gifFrame.Length - 790); //Image data (with duplicated headers chopped out)

				memoryStream.SetLength(0);
			}
			binaryWriter.Write(";"); //Image terminator
			binaryWriter.Close();
			context.Response.Flush();
			context.Response.End();
		}

		public Bitmap DrawImage (string Code, Color A, Color B, int idx) {

			var img = new Bitmap(WIDTH, HEIGHT, PixelFormat.Format24bppRgb);
			var g = Graphics.FromImage(img);
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.SmoothingMode = SmoothingMode.None;
			var r = new Rectangle(0, 0, WIDTH, HEIGHT);


			var background = new HatchBrush(HatchStyle.Percent50, A, B);
			var foreground = new HatchBrush(HatchStyle.Percent50, B, A);

			var f = new Font("Arial Black", 48);

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
	}
}

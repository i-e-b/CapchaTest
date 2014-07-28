using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using System.Drawing;
using System.Collections.Generic;

namespace GroupCaptcha.Captcha {
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class CaptchaImage : IHttpHandler {

		private struct Pair {
			public int group;
			public int index;
		}

		public void ProcessRequest (HttpContext context) {
			context.Response.ContentType = "image/gif";

			int seed = int.Parse(context.Request.QueryString.ToString());
			Random rnd = new Random(seed);

			Bitmap bmp = new Bitmap(256, 256);

			int[] numbers = new int[9];
			for (int i = 0; i < numbers.Length; i++) numbers[i] = rnd.Next(1,9);

			int c_gr = 15;
			Pair[] groups = new Pair[c_gr];
			for (int i = 0; i < groups.Length; i++) {
				groups[i].group = i % 4;
				groups[i].index = rnd.Next(0, 9);
			}

			using (Graphics g = Graphics.FromImage(bmp)) {

				Font f = new Font("Arial Black", 24, GraphicsUnit.Point);
					int hh = f.Height / 2;
				g.Clear(Color.White);

				// draw connections
				foreach (Pair pr in groups) {
					Pen pn = new Pen(cGroup(pr.group), 2.0f);
					g.DrawLine(pn, pGroup(pr.group), pNum(pr.index, pr.group));
				}

				// draw numbers
				for (int i = 0; i < numbers.Length; i++) {
					Point p = pNum(i);
					g.DrawString(numbers[i].ToString(), f, Brushes.Black, p.X, p.Y-hh);
				}

				// draw groups
				for (int i = 0; i < 4; i++) {
					Brush gb = new SolidBrush(cGroup(i));
					Point p = pGroup(i);
					g.FillEllipse(Brushes.White, new Rectangle(p.X - hh, p.Y - hh, f.Height, f.Height));
					g.DrawString(nGroup(i).ToUpper(), f, gb, p.X-hh, p.Y - hh);
				}

				g.Flush();
			}

			bmp.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Gif);
			context.Response.Flush();
		}

		private string nGroup (int group) {
			switch (group) {
				case 0: return "a";
				case 1: return "b";
				case 2: return "c";
				case 3: return "d";
				default: return "?";
			}
		}

		private Color cGroup (int group) {
			switch (group) {
				case 0: return Color.FromArgb(128, 0, 0);
				case 1: return Color.FromArgb(0, 0, 150);
				case 2: return Color.FromArgb(0, 128, 0);
				case 3: return Color.FromArgb(72, 0, 72);
				default: return Color.FromArgb(0, 0, 0);
			}
		}

		private Point pGroup (int group) {
			switch (group) {
				case 0: return new Point(64, 64);
				case 1: return new Point(192, 64);
				case 2: return new Point(64, 192);
				case 3: return new Point(192, 192);
				default: return new Point(0, 0);
			}
		}

		private Point pNum (int index) {
			return new Point((index * 28), 128);
		}

		private Point pNum (int index, int group) {
			int off = (group < 2)?(-14):(14);
			return new Point((index * 28) + 14, 128+off);
		}

		public bool IsReusable {
			get {
				return false;
			}
		}
	}
}

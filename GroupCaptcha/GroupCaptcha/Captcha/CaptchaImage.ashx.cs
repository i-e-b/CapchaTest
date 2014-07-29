namespace GroupCaptcha.Captcha {
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.Web;
    using System.Web.Services;

    [WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class CaptchaImage : IHttpHandler {
        public bool IsReusable {
            get {
                return false;
            }
        }

        private struct Pair {
			public int group;
			public int index;
		}

		public void ProcessRequest (HttpContext context) {
			context.Response.ContentType = "image/gif";

			var seed = int.Parse(context.Request.QueryString.ToString());
			var rnd = new Random(seed);

			var bmp = new Bitmap(256, 256);

			var numbers = new int[9];
			for (var i = 0; i < numbers.Length; i++) numbers[i] = rnd.Next(1,9);

			const int c_gr = 15;
			var groups = new Pair[c_gr];
			for (var i = 0; i < groups.Length; i++) {
				groups[i].group = i % 4;
				groups[i].index = rnd.Next(0, 9);
			}

			using (var g = Graphics.FromImage(bmp)) {

				var f = new Font("Arial Black", 24, GraphicsUnit.Point);
					var hh = f.Height / 2;
				g.Clear(Color.White);

				// draw connections
				foreach (var pr in groups) {
					var pn = new Pen(cGroup(pr.group), 2.0f);
					g.DrawLine(pn, pGroup(pr.group), pNum(pr.index, pr.group));
				}

				// draw numbers
				for (var i = 0; i < numbers.Length; i++) {
					var p = pNum(i);
					g.DrawString(numbers[i].ToString(CultureInfo.InvariantCulture), f, Brushes.Black, p.X, p.Y-hh);
				}

				// draw groups
				for (var i = 0; i < 4; i++) {
					Brush gb = new SolidBrush(cGroup(i));
					var p = pGroup(i);
					g.FillEllipse(Brushes.White, new Rectangle(p.X - hh, p.Y - hh, f.Height, f.Height));
					g.DrawString(nGroup(i).ToUpper(), f, gb, p.X-hh, p.Y - hh);
				}

				g.Flush();
			}

			bmp.Save(context.Response.OutputStream, ImageFormat.Gif);
			context.Response.Flush();
		}

		private static string nGroup (int group) {
			switch (group) {
				case 0: return "a";
				case 1: return "b";
				case 2: return "c";
				case 3: return "d";
				default: return "?";
			}
		}

		private static Color cGroup (int group) {
			switch (group) {
				case 0: return Color.FromArgb(128, 0, 0);
				case 1: return Color.FromArgb(0, 0, 150);
				case 2: return Color.FromArgb(0, 128, 0);
				case 3: return Color.FromArgb(72, 0, 72);
				default: return Color.FromArgb(0, 0, 0);
			}
		}

		private static Point pGroup (int group) {
			switch (group) {
				case 0: return new Point(64, 64);
				case 1: return new Point(192, 64);
				case 2: return new Point(64, 192);
				case 3: return new Point(192, 192);
				default: return new Point(0, 0);
			}
		}

		private static Point pNum (int index) {
			return new Point((index * 28), 128);
		}

		private static Point pNum (int index, int group) {
			var off = (group < 2)?(-14):(14);
			return new Point((index * 28) + 14, 128+off);
		}
	}
}

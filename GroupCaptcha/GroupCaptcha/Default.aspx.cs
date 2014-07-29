namespace GroupCaptcha {
    using System;
    using System.Web.UI;

    public partial class _Default : Page {

		public int rnd { get { var random = new Random(); return random.Next(); } }

		protected void Page_Load (object sender, EventArgs e) {

		}
	}
}

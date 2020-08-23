using System;
using System.Windows.Forms;
using System.Net;

namespace Ron_BAN
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}

		// ------------------------------------------------

		public static WebProxy Get_WebProxy()
		{
			WebProxy web_proxy = new WebProxy("http://***");
			web_proxy.Credentials = new NetworkCredential("***", "***");
			return web_proxy;
		}

		// ------------------------------------------------

		public static string Get_ProxyHost()
		{
			return "***";
		}

		public static int Get_ProxyPort()
		{
			return 0;
		}

		public static string Get_ProxyAuthStr()
		{
			return "***" + ":" + "***";
		}
	}
}

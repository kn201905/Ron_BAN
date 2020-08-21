using System;
using System.Windows.Forms;
using System.Net;

namespace Ron_BAN
{
	static class Program
	{
		static TextBox ms_tbox_status = null;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm main_form = new MainForm();
			ms_tbox_status = main_form.m_tbox_status;
			Application.Run(main_form);
		}

		public static void WriteStatus(string str)
		{
			ms_tbox_status.AppendText(str);
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

using System;
using System.Windows.Forms;
using System.Net;

namespace Ron_BAN
{
	static class Program
	{
		static System.Windows.Forms.TextBox ms_tbox_status = null;

		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			MainForm main_form = new MainForm();
			ms_tbox_status = main_form.m_tbox_status;
			Application.Run(main_form);
		}

		public static void WriteStBox(string str)
		{
			ms_tbox_status.AppendText(str);
		}
}

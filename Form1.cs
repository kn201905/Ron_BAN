using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

using AngleSharp.Html.Parser;

namespace Ron_BAN
{
	public partial class MainForm : Form
	{
		WebClient m_wc = null;
		string m_str_Cookie = null;

		public MainForm()
		{
			InitializeComponent();

			ServicePointManager.Expect100Continue = false;  // HTTP 接続の接続管理
			m_wc = new WebClient();
			m_wc.Encoding = Encoding.UTF8;
			m_wc.Proxy = Program.Get_WebProxy();
		}

		~MainForm()
		{
			m_wc.Dispose();
		}

		private void OnBtnClk_connect(object sender, EventArgs e)
		{
			m_tbox_status.Text += "--- 接続開始\r\n";

			byte[] buf_DL = m_wc.DownloadData("http://drrrkari.com/");

			// クッキーの取得
			m_str_Cookie = GetCookieStr(m_wc.ResponseHeaders);
			m_tbox_status.Text += $"--- Cookie 情報：\r\n{m_str_Cookie}\r\n";

			// token の取得
			var dom_document = new HtmlParser().ParseDocument(Encoding.UTF8.GetString(buf_DL));

			AngleSharp.Dom.IElement dom_token = null;
			foreach (var dom_input in dom_document.GetElementById("loginForm").GetElementsByTagName("input"))
			{
				if (dom_input.GetAttribute("name") == "token")
				{
					dom_token = dom_input;
					break;
				}
			}

			if (dom_token == null)
			{ throw new Exception("!!! token の取得に失敗しました。"); }

			string str_token = dom_token.GetAttribute("value");
			m_tbox_status.Text += $"--- token 情報：\r\n{str_token}\r\n";

			// リクエストヘッダの設定
			SetReqHeader(m_wc.Headers, m_str_Cookie);

			m_tbox_status.Text += "--- リクエストヘッダ\r\n";
			WebHeaderCollection wc_headers = m_wc.Headers;
			for (int i = 0; i < wc_headers.Count; i++)
			{
				m_tbox_status.Text += wc_headers.GetKey(i) + " = " + wc_headers.Get(i) + "\r\n";
			}

			// Form の設定
			string str_form = $"language=ja-JP&icon=setton&name=master&login=login&token={str_token}";

			// ログイン処理
			string str_reply = m_wc.UploadString("http://drrrkari.com/", str_form);
			m_tbox_status.Text += $"+++ リクエスト結果\r\n{str_reply}\r\n";
		}

		private void m_btn_test_Click(object sender, EventArgs e)
		{
/*
			using (var sr = new StreamReader(@"Z:drrr.html", Encoding.GetEncoding("Shift_JIS")))
			{
				var dom_document = new HtmlParser().ParseDocument(sr.ReadToEnd());
			}
*/
		}

		private void m_btn_test_2_Click(object sender, EventArgs e)
		{
			try
			{
				var drrr_socket = new DrrrSocket();
				drrr_socket.GetHome_html();
			}
			catch (Exception ex)
			{
				m_tbox_status.Text += ex;
			}
		}

		private static string GetCookieStr(WebHeaderCollection res_headers)
		{
			string cookie_cf = null;
			string cookie_drrr = null;

			{
				string[] ary_str = res_headers["Set-Cookie"].Split(';');

				foreach (string str in ary_str)
				{
					if (str.Contains("__cf")) { cookie_cf = str.Split('=')[1]; }
					else if (str.Contains("dura")) { cookie_drrr = str.Split('=')[2]; ; }
				}

				if (cookie_cf == null)
				{ throw new Exception("!!! クッキー：__cfduid が見つかりませんでした。"); }

				if (cookie_drrr == null)
				{ throw new Exception("!!! クッキー：durarara が見つかりませんでした。"); }
			}

			return "Cookie: __cfduid=" + cookie_cf + "; durarara-like-chat1=" + cookie_drrr;
		}

		private static void SetReqHeader(WebHeaderCollection req_headers, string str_Cookie)
		{
			req_headers.Add("Host: drrrkari.com");
			req_headers.Add("Content-Type: application/x-www-form-urlencoded");
			req_headers.Add("Origin: http://drrrkari.com");
//			req_headers.Add("Connection: keep-alive");
			req_headers.Add("Referer: http://drrrkari.com/");
			req_headers.Add(str_Cookie);
			req_headers.Add("Upgrade-Insecure-Requests: 1");
		}
	}

	public class DrrrSocket
	{
		const int SIZE_BUF_RECV_WND = 8192;				// 8 kbytes（ウィンドウサイズ）
		const int SIZE_MEM_STREAM_RECV = 80 * 1024;  // 80 kbytes
		int m_size_buf_send = 4096;			 // 4 kbytes


		Socket m_sckt = null;
		Byte[] m_buf_recv_wnd = new Byte[SIZE_BUF_RECV_WND];
		MemoryStream m_mem_stream_recv = new MemoryStream(SIZE_MEM_STREAM_RECV);
		Byte[] m_buf_send = null;

		public DrrrSocket()
		{
			m_buf_send = new byte[m_size_buf_send];

			Program.WriteStBox("--- drrrkari.com アドレス情報\r\n");
			IPHostEntry host_entry = Dns.GetHostEntry("drrrkari.com");

			foreach (IPAddress ip_addr in host_entry.AddressList)
			{
				IPEndPoint ip_ep = new IPEndPoint(ip_addr, 80);
				Socket temp_sckt = new Socket(ip_ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				temp_sckt.Connect(ip_ep);
				if (temp_sckt.Connected)
				{
					Program.WriteStBox(ip_addr.ToString() + " -> ○ 接続成功\r\n");
					m_sckt = temp_sckt;
					break;
				}

				Program.WriteStBox(ip_addr.ToString() + " -> ☓ 接続失敗\r\n");
			}
		}

		~DrrrSocket()
		{
			if (m_sckt != null) { m_sckt.Close(); }
		}

		public void GetHome_html()
		{
			if (m_sckt == null)
			{ throw new Exception("!!! 接続が確率できませんでした。（m_sckt == null）"); }

			string str_req = "GET http://drrrkari.com/ HTTP/1.1\r\n" + "Host: drrrkari.com\r\n"
									+ "Connection: keep-alive\r\n\r\n";

			int bytes_send = Encoding.UTF8.GetByteCount(str_req);
			if (bytes_send > m_size_buf_send) { m_buf_send = new byte[m_size_buf_send + 256]; }
			int bytes_wrtn = Encoding.UTF8.GetBytes(str_req, 0, str_req.Length, m_buf_send, 0);

			if (bytes_send != bytes_wrtn)
			{ throw new Exception("!!! bytes_send != bytes_wrtn となりました。");  }

			Program.WriteStBox($"bytes_send: {bytes_send}, bytes_wrtn: {bytes_wrtn}\r\n");

			m_mem_stream_recv.Position = 0;

			m_sckt.Send(m_buf_send, bytes_send, 0);

			int bytes = m_sckt.Receive(m_buf_recv_wnd, SIZE_BUF_RECV_WND, 0);
			Program.WriteStBox($"Receive: {bytes}");
			m_mem_stream_recv.Write(m_buf_recv_wnd, 0, bytes);

			m_mem_stream_recv.Position = 0;
			var sr = new StreamReader(m_mem_stream_recv, Encoding.UTF8);
			Program.WriteStBox(sr.ReadToEnd());

			/*
						int bytes;
						do
						{
							bytes = m_sckt.Receive(m_buf_recv_wnd, SIZE_BUF_RECEIVE_WND, 0);
							m_sb_recv.Append(Encoding.UTF8.GetString(m_buf_recv_wnd, 0, bytes));
						}
						while (bytes > 0);

						Program.WriteStBox(m_sb_recv.ToString());
			*/
		}
	}
}

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
			if (m_wc != null) { m_wc.Dispose(); }
			Drrr_Proxy.Dispose();
		}

		private void OnBtnClk_connect(object sender, EventArgs e)
		{
			// ------------------------------------------------------------
			// index.html の取得
			Program.WriteStBox("--- GET 開始\r\n");

			string str_GET = m_wc.DownloadString("http://drrrkari.com/");
			// Program.WriteStBox(str_GET);

			// クッキーの取得
			m_str_Cookie = GetCookieStr(m_wc.ResponseHeaders);
			Program.WriteStBox($"--- Cookie 情報：\r\n{m_str_Cookie}\r\n");

			// token の取得
			var dom_document = new HtmlParser().ParseDocument(str_GET);

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
			Program.WriteStBox($"--- token 情報：\r\n{str_token}\r\n");

			// ------------------------------------------------------------
			// ログイン処理
			SetReqHeader(m_wc.Headers, m_str_Cookie);
			Program.WriteStBox($"--- リクエストヘッダ\r\n");
			Show_HttpHeader(m_wc.Headers);
			
			m_tbox_status.Text += "--- ログイン前に２秒間待機します。\r\n";
			Task_Delay(2000);
			m_tbox_status.Text += "--- ログイン実行\r\n";

			// Form の設定
			string str_form = $"language=ja-JP&icon=setton&name=guardian&login=login&token={str_token}";

			// ログイン処理
			string str_reply;
			str_reply = m_wc.UploadString("http://drrrkari.com/", str_form);
			// Program.WriteStBox($"+++ リクエスト結果\r\n{str_reply}\r\n");

			// ------------------------------------------------------------
			// 部屋の作成処理
			SetReqHeader(m_wc.Headers, m_str_Cookie);
			Program.WriteStBox($"--- リクエストヘッダ\r\n");
			Show_HttpHeader(m_wc.Headers);

			m_tbox_status.Text += "--- 部屋の作成処理前に２秒間待機します。\r\n";
			Task_Delay(2000);
			m_tbox_status.Text += "--- 部屋の作成実行\r\n";

			// 作成URLへ移行
			str_reply = m_wc.UploadString("http://drrrkari.com/create_room/", "");
			Program.WriteStBox($"+++ リクエスト結果\r\n{str_reply}\r\n");
		}

		static void Task_Delay(int msec)
		{
			var task = Task.Run(async () => { await Task.Delay(msec); });
			task.Wait();
		}

		private void m_btn_test_Click(object sender, EventArgs e)
		{
			/*
			using (var sr = new StreamReader(@"Z:drrr.html", Encoding.GetEncoding("Shift_JIS")))
			{
				var dom_document = new HtmlParser().ParseDocument(sr.ReadToEnd());
			}
			*/
			try
			{
				var drrr_socket = new DrrrClient();
				string index_html = drrr_socket.Get_index_html();

//				m_tbox_status.Text += index_html;
			}
			catch (Exception ex)
			{
				Program.WriteStBox(ex.ToString());
			}
		}

		private void m_btn_test_2_Click(object sender, EventArgs e)
		{
			try
			{
				Drrr_Proxy.Init();
				Drrr_Proxy.Get_index_html(); ;
			}
			catch (Exception ex)
			{
				Program.WriteStBox(ex.ToString());
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
			req_headers.Clear();
			req_headers.Add("Host: drrrkari.com");
			req_headers.Add("Content-Type: application/x-www-form-urlencoded");
			req_headers.Add("Origin: http://drrrkari.com");
//			req_headers.Add("Connection: keep-alive");
			req_headers.Add("Referer: http://drrrkari.com/");
			req_headers.Add(str_Cookie);
//			req_headers.Add("Upgrade-Insecure-Requests: 1");
		}

		private static void Show_HttpHeader(WebHeaderCollection http_header)
		{
			for (int i = 0; i < http_header.Count; i++)
			{
				Program.WriteStBox($"{http_header.GetKey(i)} = {http_header.Get(i)}\r\n");
			}
		}
	}

	static class Drrr_Proxy
	{
		const int SIZE_BUF_RECV_WND = 8192;          // 8 kbytes（ウィンドウサイズ）
		const int SIZE_MEM_STREAM_RECV = 80 * 1024;  // 80 kbytes
		static int ms_size_buf_send = 4096;          // 4 kbytes


		static TcpClient ms_Proxy_TcpClient = null;
		static NetworkStream ms_ns_proxy = null;

		static Byte[] ms_buf_recv_wnd = new Byte[SIZE_BUF_RECV_WND];
		static MemoryStream ms_mem_stream_recv = new MemoryStream(SIZE_MEM_STREAM_RECV);
		static Byte[] ms_buf_send = null;

		static public void Init()
		{
			Program.WriteStBox("--- Drrr_Proxy.Init()\r\n");

			ms_buf_send = new byte[ms_size_buf_send];

			ms_Proxy_TcpClient = new TcpClient(Program.Get_ProxyHost(), Program.Get_ProxyPort());
		}

		static public void Dispose()
		{
			if (ms_ns_proxy != null) { ms_ns_proxy.Dispose(); }
			if (ms_Proxy_TcpClient != null) { ms_Proxy_TcpClient.Close(); }
		}

		static public string Get_index_html()
		{
			if (ms_Proxy_TcpClient.Connected == false)
			{ throw new Exception("!!! m_TcpClient.Connected == false on DrrrClient.GetHome_html()"); }

			ms_ns_proxy = ms_Proxy_TcpClient.GetStream();

			string str_req = "GET http://drrrkari.com HTTP/1.1\r\n"
				+ "Host: drrrkari.com\r\n"
				+ "Connection: keep-alive\r\n"
				+ "Proxy-Connection: keep-alive\r\n"
				+ "Proxy-Authenticate: Basic realm=\"proxy\"\r\n"
				+ "Proxy-Authorization: basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Program.Get_ProxyAuthStr()))
				+ "\r\n\r\n";

			int bytes_send = Encoding.UTF8.GetByteCount(str_req);
			if (bytes_send > ms_size_buf_send)
			{
				ms_size_buf_send = bytes_send + 256;
				ms_buf_send = new byte[ms_size_buf_send];
			}
			int bytes_wrtn = Encoding.UTF8.GetBytes(str_req, 0, str_req.Length, ms_buf_send, 0);

			if (bytes_send != bytes_wrtn)
			{ throw new Exception("!!! bytes_send != bytes_wrtn となりました。"); }

			Program.WriteStBox("--- proxy の接続要求を始めます。\r\n");
			ms_ns_proxy.Write(ms_buf_send, 0, bytes_send);

			ms_mem_stream_recv.Position = 0;
			do
			{
				int bytes_read = ms_ns_proxy.Read(ms_buf_recv_wnd, 0, SIZE_BUF_RECV_WND);
				Program.WriteStBox($"+++ Receive: {bytes_read} bytes\r\n");
				ms_mem_stream_recv.Write(ms_buf_recv_wnd, 0, bytes_read);
			} while (ms_Proxy_TcpClient.Available > 0);
			//			} while (ms_ns_proxy.DataAvailable);

			int bytes_ = ms_ns_proxy.Read(ms_buf_recv_wnd, 0, SIZE_BUF_RECV_WND);
			Program.WriteStBox($"+++ last: {bytes_} bytes\r\n");

			Program.WriteStBox($"+++ Connected: {ms_Proxy_TcpClient.Connected.ToString()}\r\n");

			ms_mem_stream_recv.Position = 0;
			var sr = new StreamReader(ms_mem_stream_recv, Encoding.UTF8);
			Program.WriteStBox(sr.ReadToEnd());

			return "--- OK\r\n";
		}
	}
}


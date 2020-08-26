using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using AngleSharp.Html.Parser;

namespace Ron_BAN
{
	static partial class Drrr_Host2
	{
		// 現在のところ、アプリ起動につき、接続は１回のみとして設計している
		static bool ms_bEstablish_once = false;

		static HttpClientHandler ms_http_handler = null;
		static HttpClient ms_http_client = null;

		static string ms_str_Cookie = null;

		public static void Dispose()
		{
			if (ms_http_handler != null) { ms_http_handler.Dispose(); }
			if (ms_http_client != null) { ms_http_client.Dispose(); }
		}

		// ------------------------------------------------------------------------------------
		// Establish_cnct() で、例外が発生した場合はアプリを起動し直す
		// 部屋の生成に成功した場合は、null が返される。
		// 生成に失敗した場合は、失敗事由がが返される。
		public static async Task<string> Establish_cnct(string user_name, string str_icon, string room_name)
		{
			if (ms_bEstablish_once) { return "+++ 既に部屋が作成済みでした。何もしません。"; }
			ms_bEstablish_once = true;

			ServicePointManager.Expect100Continue = false;  // HTTP 接続の接続管理

			ms_http_handler = new HttpClientHandler();
			ms_http_handler.Proxy = Program.Get_WebProxy();
			ms_http_handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
//			MainForm.WriteStatus(Convert.ToString(ms_http_handler.AutomaticDecompression));

			ms_http_client = new HttpClient(ms_http_handler);
			ms_http_client.DefaultRequestHeaders.Add("Host", "drrrkari.com");
			ms_http_client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
			ms_http_client.DefaultRequestHeaders.Add("Accept-Language", "ja,en-US");
			ms_http_client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
			ms_http_client.DefaultRequestHeaders.Add("Connection", "keep-alive");

			ms_http_client.Timeout = TimeSpan.FromSeconds(10);  // タイムアウトは 10秒 に設定

			// ------------------------------------------------------------
			// index.html の取得
			MainForm.WriteStatus("--- 接続処理を開始します。\r\n");
			HttpRequestMessage http_req_1st = new HttpRequestMessage(HttpMethod.Get, "http://drrrkari.com/");
			http_req_1st.Headers.Add("Accept", "text/html");
			HttpResponseMessage http_res;
			http_res = await ms_http_client.SendAsync(http_req_1st);
//			http_res = await ms_http_client.SendAsync(http_req_1st, HttpCompletionOption.ResponseContentRead);

			// クッキーの取得
			ms_str_Cookie = GetCookieStr(http_res.Headers);
			MainForm.WriteStatus($"--- Cookie 情報：\r\n\t{ms_str_Cookie}\r\n");

			// token の取得（token はログインのときのみに利用される模様）
			string str_GET = await http_res.Content.ReadAsStringAsync();
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
			MainForm.WriteStatus($"--- token 情報：\r\n\t{str_token}\r\n");

			// ------------------------------------------------------------
			// ログイン処理
			MainForm.WriteStatus("--- ２秒待機後に、ログインを実行します。\r\n");
			await Task.Delay(2000);

			ms_http_client.DefaultRequestHeaders.Add("Origin", "http://drrrkari.com/");
			ms_http_client.DefaultRequestHeaders.Add("Cookie", ms_str_Cookie);

			HttpRequestMessage http_req_login = new HttpRequestMessage(HttpMethod.Post, "http://drrrkari.com/");
			http_req_login.Headers.Add("Accept", "text/html");
			http_req_login.Headers.Add("Referer", "http://drrrkari.com/");

			http_req_login.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(
					$"language=ja-JP&icon={str_icon}&name={WebUtility.UrlEncode(user_name)}&login=login&token={str_token}"));
			http_req_login.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			http_res = await ms_http_client.SendAsync(http_req_login);

			string str_reply;
			str_reply = await http_res.Content.ReadAsStringAsync();

			if (str_reply.Contains("雑談部屋") == false)
			{ throw new Exception("!!! ログインに失敗しました。");  }

			MainForm.WriteStatus("--- ログイン成功\r\n");

			// ------------------------------------------------------------
			// 部屋の作成ページへ移行
			MainForm.WriteStatus("--- ２秒待機後に、部屋作成ページへ移行します。\r\n");
			await Task.Delay(2000);

			HttpRequestMessage http_req_CreateRoomPage
					= new HttpRequestMessage(HttpMethod.Post, "http://drrrkari.com/create_room/");
			http_req_CreateRoomPage.Headers.Add("Accept", "text/html");
			http_req_CreateRoomPage.Headers.Add("Referer", "http://drrrkari.com/lounge/");

			http_req_CreateRoomPage.Content = new ByteArrayContent(new byte[0]);
			http_req_CreateRoomPage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			http_res = await ms_http_client.SendAsync(http_req_CreateRoomPage);

			str_reply = await http_res.Content.ReadAsStringAsync();
			if (str_reply.Contains("必ず選択") == false)
			{ throw new Exception("!!! 部屋作成ページへの移行に失敗しました。"); }

			MainForm.WriteStatus("--- 部屋作成ページへの移行成功\r\n");

			// ------------------------------------------------------------
			// 部屋を生成
			MainForm.WriteStatus("--- ２秒待機後に、部屋作成を実行します。\r\n");
			await Task.Delay(2000);

			HttpRequestMessage http_req_CreateRm = new HttpRequestMessage(HttpMethod.Post, "http://drrrkari.com/create_room/");
			http_req_CreateRm.Headers.Add("Accept", "text/html");
			http_req_CreateRm.Headers.Add("Referer", "http://drrrkari.com/create_room/");

			http_req_CreateRm.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(
					$"name={WebUtility.UrlEncode(room_name)}"
						+ "&type=zatsu&limit=6&knock=0&password=&image=1&language=ja-JP&submit=submit"));
			http_req_CreateRm.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			http_res = await ms_http_client.SendAsync(http_req_CreateRm);

			str_reply = await http_res.Content.ReadAsStringAsync();
			if (str_reply.Contains("入室しました") == false)
			{ throw new Exception("!!! 部屋の作成に失敗しました。"); }

			MainForm.WriteStatus("--- 部屋の作成に成功しました。\r\n");

			return null;
		}

		///////////////////////////////////////////////////////////////////////////////////////

		static bool msb_Discnct_Started = false;  // 最重要フラグ

		// 操作が正しく終了した場合は、null が返される
		public static async Task<string> Disconnect()
		{
			if (ms_bEstablish_once == false) { return "+++ まだ未接続です。何もしません。"; }

			msb_Discnct_Started = true;  // 切断処理開始のフラグ

			// 現在実行中の http シーケンスがあれば、その終了を待つ
			if (Lo_HttpTask.ms_lo_task_cur != null) { await Lo_HttpTask.ms_lo_task_cur; }
			if (Mid_HttpTask.ms_mid_task_cur != null) { await Mid_HttpTask.ms_mid_task_cur; }

			// ------------------------------------------------------------
			// 部屋から退室する
			MainForm.WriteStatus("--- 退室を実行します。\r\n");

			HttpRequestMessage http_req_ExitRm = new HttpRequestMessage(HttpMethod.Post, "http://drrrkari.com/room/?ajax=1");
			http_req_ExitRm.Headers.Add("Accept", "*/*");
			http_req_ExitRm.Headers.Add("X-Requested-With", "XMLHttpRequest");
			http_req_ExitRm.Headers.Add("Origin", "http://drrrkari.com/");
			http_req_ExitRm.Headers.Add("Referer", "http://drrrkari.com/room/");
			http_req_ExitRm.Headers.Add("Cookie", ms_str_Cookie);

			http_req_ExitRm.Content = new ByteArrayContent(Encoding.UTF8.GetBytes("logout=logout"));
			var content_type = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
			content_type.CharSet = "UTF-8";
			http_req_ExitRm.Content.Headers.ContentType = content_type;
			
			HttpResponseMessage http_res;
			http_res = await ms_http_client.SendAsync(http_req_ExitRm);

			string str_reply;
			str_reply = await http_res.Content.ReadAsStringAsync();

			if (str_reply.Contains("雑談部屋") == false)
			{ throw new Exception("!!! 退室に失敗しました。"); }
			MainForm.WriteStatus("--- 退室成功\r\n");

			// ------------------------------------------------------------
			// ログアウトする
			MainForm.WriteStatus("--- ２秒待機後に、ログアウトを実行します。\r\n");
			await Task.Delay(2000);

			HttpRequestMessage http_req_Logout = new HttpRequestMessage(HttpMethod.Post, "http://drrrkari.com/logout/");
			http_req_Logout.Headers.Add("Accept", "text/html");
			http_req_Logout.Headers.Add("X-Requested-With", "XMLHttpRequest");
			http_req_Logout.Headers.Add("Origin", "http://drrrkari.com/");
			http_req_Logout.Headers.Add("Referer", "http://drrrkari.com/lounge/");
			http_req_Logout.Headers.Add("Cookie", ms_str_Cookie);

			http_req_Logout.Content = new ByteArrayContent(new byte[0]);
			http_req_Logout.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			http_res = await ms_http_client.SendAsync(http_req_Logout);

			str_reply = await http_res.Content.ReadAsStringAsync();
			if (str_reply.Contains("再現した") == false)
			{ throw new Exception("!!! ログアウトに失敗しました。"); }

			MainForm.WriteStatus("--- ログアウト成功\r\n");

			return null;
		}
		
		// ------------------------------------------------------------------------------------

		public static async Task<HttpTask> GetJSON()
		{
			HttpTask getJSON_task = GetJSON_Task_Factory.Create();
			if (getJSON_task.m_str_cancel != null) { return getJSON_task; }

			await HttpScheduler.Set(getJSON_task);
			return getJSON_task;  // getJSON_task にエラーも含めた実行結果が代入されている
		}

		static class GetJSON_Task_Factory
		{
			const int MAX_getJSON_task = 1;

			public static HttpTask Create()
			{
				if (msb_Discnct_Started)
				{ return new Err_HttpTask("+++ 切断処理が開始されました。GetJSON() はキャンセルされました。\r\n"); }

				if (GetJSON_Task.ms_num_getJSON_task >= MAX_getJSON_task)
				{ return new Err_HttpTask("+++ GetJSON() が２重に実行されました。GetJSON() はキャンセルされました。\r\n"); }

				return new GetJSON_Task();
			}
		}

		// ------------------------------------------------------------------------------------
		
		public static async Task<HttpTask> PostMsg(string msg_to_post)
		{
			HttpTask postMsg_task = PostMsg_Task_Factory.Create(msg_to_post);
			if (postMsg_task.m_str_cancel != null) { return postMsg_task; }

			await HttpScheduler.Set(postMsg_task);
			return postMsg_task;  // getJSON_task にエラーも含めた実行結果が代入されている
		}

		static class PostMsg_Task_Factory
		{
			const uint MAX_postMsg_task = 3;

			public static HttpTask Create(string msg_to_post)
			{
				if (msb_Discnct_Started)
				{ return new Err_HttpTask("+++ 切断処理が開始されました。PostMsg() はキャンセルされました。\r\n"); }

				if (PostMsg_Task.ms_num_post_msg_task >= MAX_postMsg_task)
				{ return new Err_HttpTask("+++ PostMsg() が連続して実行されました。PostMsg() はキャンセルされました。\r\n"); }

				return new PostMsg_Task(msg_to_post);
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////

		static string GetCookieStr(HttpResponseHeaders res_headers)
		{
			string cookie_cf = null;
			string cookie_drrr = null;

			foreach(string str in res_headers.GetValues("Set-Cookie"))
			{
				if (str.Contains("__cf")) { cookie_cf = str.Split(';')[0]; }
				else if (str.Contains("dura")) { cookie_drrr = str.Split(';')[0]; ; }
			}

			if (cookie_cf == null)
			{ throw new Exception("!!! クッキー：__cfduid が見つかりませんでした。"); }

			if (cookie_drrr == null)
			{ throw new Exception("!!! クッキー：durarara が見つかりませんでした。"); }

			return $"{cookie_cf}; {cookie_drrr}";
		}
	}
}

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using AngleSharp.Html.Parser;

namespace Ron_BAN
{
	static class Drrr_Host
	{
		// 現在のところ、アプリ起動につき、接続は１回のみとして設計している
		static bool ms_bEstablish_once = false;

		static WebClient ms_wc = null;
		static string ms_str_Cookie = null;

		public static void Dispose()
		{
			if (ms_wc == null)
			{
				ms_wc.Dispose();
			}
		}

		// ------------------------------------------------------------------------------------
		// Establish_cnct() で、例外が発生した場合はアプリを起動し直す
		public static async Task<bool> Establish_cnct(string user_name, string room_name)
		{
			if (ms_bEstablish_once) { return false; }
			ms_bEstablish_once = true;

			ServicePointManager.Expect100Continue = false;  // HTTP 接続の接続管理
			ms_wc = new WebClient();
			ms_wc.Encoding = Encoding.UTF8;
			ms_wc.Proxy = Program.Get_WebProxy();

			// ------------------------------------------------------------
			// index.html の取得
			Program.WriteStBox("--- 接続処理を開始します。");
			string str_GET = ms_wc.DownloadString("http://drrrkari.com/");

			// クッキーの取得
			ms_str_Cookie = GetCookieStr(ms_wc.ResponseHeaders);
			Program.WriteStBox($"--- Cookie 情報：\r\n{ms_str_Cookie}\r\n");

			// token の取得（token はログインのときのみに利用される模様）
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
			Program.WriteStBox("--- ２秒待機後に、ログインを実行します。\r\n");
			await Task.Delay(2000);

			SetReqHeader_normal(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			string str_reply;
			str_reply = ms_wc.UploadString("http://drrrkari.com/"
					, $"language=ja-JP&icon=setton&name={user_name}&login=login&token={str_token}");

			if (str_reply.Contains("雑談部屋") == false)
			{ throw new Exception("!!! ログインに失敗しました。");  }

			Program.WriteStBox("--- ログイン成功\r\n");

			// ------------------------------------------------------------
			// 部屋の作成ページへ移行
			Program.WriteStBox("--- ２秒待機後に、部屋作成ページへ移行します。\r\n");
			await Task.Delay(2000);

			SetReqHeader_normal(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			str_reply = ms_wc.UploadString("http://drrrkari.com/create_room/", "");
			if (str_reply.Contains("必ず選択") == false)
			{ throw new Exception("!!! 部屋作成ページへの移行に失敗しました。"); }

			Program.WriteStBox("--- 部屋作成ページへの移行成功\r\n");

			// ------------------------------------------------------------
			// 部屋を生成
			Program.WriteStBox("--- ２秒待機後に、部屋作成を実行します。\r\n");
			await Task.Delay(2000);

			SetReqHeader_normal(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			str_reply = ms_wc.UploadString("http://drrrkari.com/create_room/"
					, $"name={WebUtility.UrlEncode(room_name)}"
						+ "&type=zatsu&limit=5&knock=0&password=&image=1&language=ja-JP&submit=submit");

			if (str_reply.Contains("入室しました") == false)
			{ throw new Exception("!!! 部屋の作成に失敗しました。"); }

			Program.WriteStBox("--- 部屋の作成に成功しました。\r\n");

			return true;
		}

		// ------------------------------------------------------------------------------------

		public static async Task<bool> Disconnect()
		{
			if (ms_bEstablish_once == false) { return false; }

			// ------------------------------------------------------------
			// 部屋から退室する
			Program.WriteStBox("--- 退室を実行します。\r\n");

			SetReqHeader_Ajax(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			string str_reply = ms_wc.UploadString("http://drrrkari.com/room/?ajax=1", "logout=logout");

			if (str_reply.Contains("雑談部屋") == false)
			{ throw new Exception("!!! 退室に失敗しました。"); }

			Program.WriteStBox("--- 退室成功\r\n");

			// ------------------------------------------------------------
			// ログアウトする
			Program.WriteStBox("--- ２秒待機後に、ログアウトを実行します。\r\n");
			await Task.Delay(2000);

			SetReqHeader_normal(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			str_reply = ms_wc.UploadString("http://drrrkari.com/logout/", "");

			if (str_reply.Contains("再現した") == false)
			{ throw new Exception("!!! ログアウトに失敗しました。"); }

			Program.WriteStBox("--- ログアウト成功\r\n");

			return true;
		}

		// ------------------------------------------------------------------------------------

		public static void PostMsg(string msg)
		{
			Program.WriteStBox("--- 発言を実行します。\r\n");

			SetReqHeader_Ajax(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			// ?ajax=1 でアクセスした場合、サーバーからの返答には JSON等の有意義な情報が含まれない
			ms_wc.UploadString("http://drrrkari.com/room/?ajax=1"
					, $"message={WebUtility.UrlEncode(msg)}&valid=1");
		}

		// ------------------------------------------------------------------------------------

		public static string GetJSON()
		{
			Program.WriteStBox("--- JSON を取得します。\r\n");

			SetReqHeader_Ajax(ms_wc.Headers, ref ms_str_Cookie);
//			Show_HttpHeader(ms_wc.Headers);

			string str_reply = ms_wc.UploadString("http://drrrkari.com/ajax.php", "");
			return str_reply;
		}

		// ------------------------------------------------------------------------------------

		static string GetCookieStr(WebHeaderCollection res_headers)
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

			return $"Cookie: __cfduid={cookie_cf}; durarara-like-chat1={cookie_drrr}";
		}

		static void SetReqHeader_normal(WebHeaderCollection req_headers, ref string str_Cookie)
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

		static void SetReqHeader_Ajax(WebHeaderCollection req_headers, ref string str_Cookie)
		{
			req_headers.Clear();
			req_headers.Add("Host: drrrkari.com");
			req_headers.Add("Content-Type: application/x-www-form-urlencoded; charset=UTF-8");
			req_headers.Add("X-Requested-With: XMLHttpRequest");
			req_headers.Add("Origin: http://drrrkari.com");
//			req_headers.Add("Connection: keep-alive");
			req_headers.Add("Referer: http://drrrkari.com/room/");
			req_headers.Add(str_Cookie);
		}

		static void Show_HttpHeader(WebHeaderCollection http_header)
		{
			Program.WriteStBox("+++ リクエストヘッダ出力\r\n");
			for (int i = 0; i < http_header.Count; i++)
			{
				Program.WriteStBox($"{http_header.GetKey(i)} = {http_header.Get(i)}\r\n");
			}
		}
	}
}
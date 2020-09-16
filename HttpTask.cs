using System;
using System.Text;

using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Ron_BAN
{
	static partial class Drrr_Host2
	{
		public abstract class HttpTask
		{
			public string m_str_cancel = null;  // これが null でない場合、タスクがキャンセルされたことを表す
			protected HttpRequestMessage m_http_req = null;
			public HttpResponseMessage m_http_res = null;

			public abstract Task Queueing();
			public abstract void SetLatestTask(Task task);

//			public abstract int Count_AsKind();  // 現在実行中のタスク ＋ キューされてるタスク
			public abstract void DecCount_AsKind();

			public Task DoWork()
			{
				// 処理中の HttpTask、または、SendAsync() の Task が返される
				Task task = this.Queueing();
				this.SetLatestTask(task);
				return task;
			}
		}

		// ------------------------------------------------------------------------------------
		public abstract class Lo_HttpTask : HttpTask
		{
			static uint ms_num_lo_task = 0;  // 現在実行中のタスク ＋ キューされてるタスク
			static Task ms_lo_task_latest = null;
			public static Task<HttpResponseMessage> ms_lo_task_cur = null;  // MidTask を割り込ませるためだけに利用

			public override void SetLatestTask(Task task) { ms_lo_task_latest = task; }
			public override async Task Queueing()
			{
				try
				{
					ms_num_lo_task++;
					if (ms_num_lo_task > 1)
					{
						await ms_lo_task_latest;
					}
					while (Mid_HttpTask.ms_num_mid_task > 0)
					{
						await Mid_HttpTask.ms_mid_task_latest;
					}
					if (msb_Discnct_Started)
					{
						m_str_cancel = "+++ 切断処理が開始されたため、タスク実行は中断されました。\r\n";
						return;
					}

					try
					{
						ms_lo_task_cur = ms_http_client.SendAsync(m_http_req);
						m_http_res = await ms_lo_task_cur;
					}
					finally
					{
						ms_lo_task_cur = null;
					}
				}
				catch (Exception ex)
				{
					m_str_cancel = ex.ToString() + "\r\n";
				}
				finally
				{
					ms_num_lo_task--;
					this.DecCount_AsKind();
				}
			}
		}

		// ------------------------------------------------------------------------------------
		public abstract class Mid_HttpTask : HttpTask
		{
			public static uint ms_num_mid_task = 0;  // 現在実行中のタスク ＋ キューされてるタスク
			public static Task ms_mid_task_latest = null;
			public static Task<HttpResponseMessage> ms_mid_task_cur = null;  // HiTask を割り込ませるためだけに利用（現在未使用）

			public override void SetLatestTask(Task task) { ms_mid_task_latest = task; }
			public override async Task Queueing()
			{
				try
				{
					ms_num_mid_task++;
					if (ms_num_mid_task > 1)
					{
						await ms_mid_task_latest;
					}
					if (Lo_HttpTask.ms_lo_task_cur != null)
					{
						await Lo_HttpTask.ms_lo_task_cur;
					}
					if (msb_Discnct_Started)
					{
						m_str_cancel = "+++ 切断処理が開始されたため、タスク実行は中断されました。";
						return;
					}

					try
					{
						ms_mid_task_cur = ms_http_client.SendAsync(m_http_req);
						m_http_res = await ms_mid_task_cur;
					}
					finally
					{
						ms_mid_task_cur = null;
					}
				}
				catch (Exception ex)
				{
					m_str_cancel = ex.ToString() + "\r\n";
				}
				finally
				{
					ms_num_mid_task--;
					this.DecCount_AsKind();
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////

		// CS1998: この非同期メソッドには 'await' 演算子がないため、同期的に実行されます。
		#pragma warning disable CS1998
		public class Err_HttpTask : HttpTask
		{
			public Err_HttpTask(string err_msg)
			{
				m_str_cancel = err_msg;
			}

			public override void SetLatestTask(Task task) {}
			public override async Task Queueing() {}
	
			public override void DecCount_AsKind() {}
		}
		#pragma warning restore CS1998
		
		// ------------------------------------------------------------------------------------

		public static class GetJSON_Task_Factory
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

		class GetJSON_Task : Lo_HttpTask
		{
			static Uri ms_uri_getJSON = new Uri("http://drrrkari.com/ajax.php");
			static Uri ms_uri_referer_getJSON = new Uri("http://drrrkari.com/room/");
			static ByteArrayContent ms_content_getJSON = new ByteArrayContent(new byte[0]);

			public GetJSON_Task()
			{
				m_http_req = new HttpRequestMessage(HttpMethod.Post, ms_uri_getJSON);
				m_http_req.Headers.Add("Accept", "application/json");
				m_http_req.Headers.Add("X-Requested-With", "XMLHttpRequest");
				m_http_req.Headers.Referrer = ms_uri_referer_getJSON;

				m_http_req.Content = ms_content_getJSON;
				ms_num_getJSON_task++;
			}

			// -----------------------------------------------
			public static int ms_num_getJSON_task = 0;
			public override void DecCount_AsKind()
			{
				if (ms_num_getJSON_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_getJSON_task <= 0」"); }

				ms_num_getJSON_task--;
			}
		}

		// ------------------------------------------------------------------------------------

		public static class PostMsg_Task_Factory
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

		class PostMsg_Task : Lo_HttpTask
		{
			static Uri ms_uri_postMsg = new Uri("http://drrrkari.com/room/?ajax=1");
			static Uri ms_uri_referer_postMsg = new Uri("http://drrrkari.com/room/");
			static MediaTypeHeaderValue ms_content_type_postMsg
				= new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

			public PostMsg_Task(string msg_to_post)
			{
				m_http_req = new HttpRequestMessage(HttpMethod.Post, ms_uri_postMsg);
				// m_http_req.Headers.Add("Accept", "*,*");  // 例外が発生する、、、
				m_http_req.Headers.Add("X-Requested-With", "XMLHttpRequest");
				m_http_req.Headers.Referrer = ms_uri_referer_postMsg;

				m_http_req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(
																	$"message={WebUtility.UrlEncode(msg_to_post)}&valid=1"));
				m_http_req.Content.Headers.ContentType = ms_content_type_postMsg;

				ms_num_post_msg_task++;
			}

			// -----------------------------------------------
			public static int ms_num_post_msg_task = 0;
			public override void DecCount_AsKind()
			{
				if (ms_num_post_msg_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_post_msg_task <= 0」"); }

				ms_num_post_msg_task--;
			}
		}

		// ------------------------------------------------------------------------------------

		public static class BanUsr_Task_Factory
		{
			const uint MAX_banUsr_task = 3;

			public static HttpTask Create(string uid_to_ban)
			{
				if (msb_Discnct_Started)
				{ return new Err_HttpTask("+++ 切断処理が開始されました。BanUsr() はキャンセルされました。\r\n"); }

				if (BanByUid_Task.ms_num_ban_task >= MAX_banUsr_task)
				{ return new Err_HttpTask("+++ BanUsr() が連続して実行されました。BanUsr() はキャンセルされました。\r\n"); }

				return new BanByUid_Task(uid_to_ban);
			}
		}

		class BanByUid_Task : Mid_HttpTask
		{
			static Uri ms_uri_ban = new Uri("http://drrrkari.com/room/?ajax=1");
			static Uri ms_uri_referer_ban = new Uri("http://drrrkari.com/room/");
			static MediaTypeHeaderValue ms_content_type_ban
				= new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

			public BanByUid_Task(string uid_to_ban)
			{
				m_http_req = new HttpRequestMessage(HttpMethod.Post, ms_uri_ban);
				m_http_req.Headers.Add("X-Requested-With", "XMLHttpRequest");
				m_http_req.Headers.Referrer = ms_uri_referer_ban;

				m_http_req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(
																	$"ban_user={WebUtility.UrlEncode(uid_to_ban)}&block=1"));
				m_http_req.Content.Headers.ContentType = ms_content_type_ban;

				ms_num_ban_task++;
			}

			// -----------------------------------------------
			public static int ms_num_ban_task = 0;
			public override void DecCount_AsKind()
			{
				if (ms_num_ban_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_ban_task <= 0」"); }

				ms_num_ban_task--;
			}
		}

		// ------------------------------------------------------------------------------------

		public static class Chg_RmLimit_Task_Factory
		{
			const uint MAX_num_task = 1;

			public static HttpTask Create(int num_room_limit)
			{
				if (msb_Discnct_Started)
				{ return new Err_HttpTask("+++ 切断処理が開始されました。Chg_RmLimit() はキャンセルされました。\r\n"); }

				if (Chg_RmLimit.ms_num_task >= MAX_num_task)
				{ return new Err_HttpTask("+++ Chg_RmLimit() が連続して実行されました。Chg_RmLimit() はキャンセルされました。\r\n"); }

				return new Chg_RmLimit(num_room_limit);
			}
		}

		class Chg_RmLimit : Mid_HttpTask
		{
			static Uri ms_uri = new Uri("http://drrrkari.com/room/?ajax=1");
			static Uri ms_uri_referer = new Uri("http://drrrkari.com/room/");
			static MediaTypeHeaderValue ms_content_type
				= new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };

			public Chg_RmLimit(int num_room_limit)
			{
				m_http_req = new HttpRequestMessage(HttpMethod.Post, ms_uri);
				m_http_req.Headers.Add("X-Requested-With", "XMLHttpRequest");
				m_http_req.Headers.Referrer = ms_uri_referer;

				m_http_req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes($"room_limit={num_room_limit}"));
				m_http_req.Content.Headers.ContentType = ms_content_type;

				ms_num_task++;
			}

			// -----------------------------------------------
			public static int ms_num_task = 0;
			public override void DecCount_AsKind()
			{
				if (ms_num_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_task <= 0」"); }

				ms_num_task--;
			}
		}
	}
}

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
		static class HttpScheduler
		{
			public static Task Set(HttpTask http_task)
			{
				Task task = http_task.Queueing();
				http_task.SetLatestTask(task);
				return task;
			}
		}

		///////////////////////////////////////////////////////////////////////////////////////

		public abstract class HttpTask
		{
			public string m_str_cancel = null;  // これが null でない場合、タスクがキャンセルされたことを表す
			protected HttpRequestMessage m_http_req = null;
			public HttpResponseMessage m_http_res = null;

			public abstract Task Queueing();
			public abstract void SetLatestTask(Task task);

//			public abstract uint CountAsKind();  // 現在実行中のタスク ＋ キューされてるタスク
			public abstract void DecCount_AsKind();
		}

		// ------------------------------------------------------------------------------------
		public abstract class Lo_HttpTask : HttpTask
		{
			static uint ms_num_lo_task = 0;  // 現在実行中のタスク ＋ キューされてるタスク
			static Task ms_lo_task_latest = null;
			public static Task<HttpResponseMessage> ms_lo_task_cur = null;

			public override void SetLatestTask(Task task) { ms_lo_task_latest = task; }
			public override async Task Queueing()
			{
				ms_num_lo_task++;
				if (ms_num_lo_task > 1)
				{
					try
					{
						await ms_lo_task_latest;
					}
					catch(Exception ex)
					{
						m_str_cancel = ex.ToString() + "\r\n";
						return;
					}
				}
				while (Mid_HttpTask.ms_num_mid_task > 0)
				{
					try
					{
						await Mid_HttpTask.ms_mid_task_latest;
					}
					catch(Exception ex)
					{
						m_str_cancel = ex.ToString() + "\r\n";
						return;
					}
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
				catch (Exception ex)
				{
					m_str_cancel = ex.ToString() + "\r\n";
				}

				ms_lo_task_cur = null;
				this.DecCount_AsKind();

				ms_num_lo_task--;
			}
		}

		// ------------------------------------------------------------------------------------
		public abstract class Mid_HttpTask : HttpTask
		{
			public static uint ms_num_mid_task = 0;  // 現在実行中のタスク ＋ キューされてるタスク
			public static Task ms_mid_task_latest = null;
			public static Task<HttpResponseMessage> ms_mid_task_cur = null;

			public override void SetLatestTask(Task task) { ms_mid_task_latest = task; }
			public override async Task Queueing()
			{
				ms_num_mid_task++;
				if (ms_num_mid_task > 1)
				{
					try
					{
						await ms_mid_task_latest;
					}
					catch(Exception ex)
					{
						m_str_cancel = ex.ToString() + "\r\n";
						return;
					}
				}
				if (Lo_HttpTask.ms_lo_task_cur != null)
				{
					try
					{
						await Lo_HttpTask.ms_lo_task_cur;
					}
					catch(Exception ex)
					{
						m_str_cancel = ex.ToString() + "\r\n";
						return;
					}
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
				catch (Exception ex)
				{
					m_str_cancel = ex.ToString() + "\r\n";
				}

				ms_mid_task_cur = null;
				this.DecCount_AsKind();

				ms_num_mid_task--;
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
		class GetJSON_Task : Lo_HttpTask
		{
			static Uri ms_uri_getJSON = new Uri("http://drrrkari.com/ajax.php");
			static Uri ms_uri_referer_getJSON = new Uri("http://drrrkari.com/room/");
			static ByteArrayContent ms_content_getJSON = new ByteArrayContent(new byte[0]);
			public static int ms_num_getJSON_task = 0;

			public GetJSON_Task()
			{
				m_http_req = new HttpRequestMessage(HttpMethod.Post, ms_uri_getJSON);
				m_http_req.Headers.Add("Accept", "application/json");
				m_http_req.Headers.Add("X-Requested-With", "XMLHttpRequest");
				m_http_req.Headers.Referrer = ms_uri_referer_getJSON;

				m_http_req.Content = ms_content_getJSON;
				ms_num_getJSON_task++;
			}

			public override void DecCount_AsKind()
			{
				if (ms_num_getJSON_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_getJSON_task <= 0」"); }

				ms_num_getJSON_task--;
			}
		}

		// ------------------------------------------------------------------------------------
		class PostMsg_Task : Lo_HttpTask
		{
			static Uri ms_uri_postMsg = new Uri("http://drrrkari.com/room/?ajax=1");
			static Uri ms_uri_referer_postMsg = new Uri("http://drrrkari.com/room/");
			static MediaTypeHeaderValue ms_content_type_postMsg
				= new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
			public static int ms_num_post_msg_task = 0;

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

			public override void DecCount_AsKind()
			{
				if (ms_num_post_msg_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_post_msg_task <= 0」"); }

				ms_num_post_msg_task--;
			}
		}

		// ------------------------------------------------------------------------------------
		class BanByUid_Task : Mid_HttpTask
		{
			static Uri ms_uri_ban = new Uri("http://drrrkari.com/room/?ajax=1");
			static Uri ms_uri_referer_ban = new Uri("http://drrrkari.com/room/");
			static MediaTypeHeaderValue ms_content_type_ban
				= new MediaTypeHeaderValue("application/x-www-form-urlencoded") { CharSet = "UTF-8" };
			public static int ms_num_ban_task = 0;

			public BanByUid_Task(string uid_to_ban)
			{
				m_http_req = new HttpRequestMessage(HttpMethod.Post, ms_uri_ban);
				// m_http_req.Headers.Add("Accept", "*,*");  // 例外が発生する、、、
				m_http_req.Headers.Add("X-Requested-With", "XMLHttpRequest");
				m_http_req.Headers.Referrer = ms_uri_referer_ban;

				m_http_req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(
																	$"ban_user={WebUtility.UrlEncode(uid_to_ban)}&block=1"));
				m_http_req.Content.Headers.ContentType = ms_content_type_ban;

				ms_num_ban_task++;
			}

			public override void DecCount_AsKind()
			{
				if (ms_num_ban_task <= 0)
				{ throw new Exception("!!! 未知の不具合：「ms_num_ban_task <= 0」"); }

				ms_num_ban_task--;
			}
		}
	}
}

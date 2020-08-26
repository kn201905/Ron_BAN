namespace Ron_BAN
{
		// テスト１
		static uint ms_lo_task_id = 1;
		async void m_btn_test_1_Click(object sender, EventArgs e)
		{

			tst_lo_task lo_task = new tst_lo_task() { m_task_id = ms_lo_task_id };
			ms_lo_task_id++;

			try
			{
				await tst_scheduler.Set(lo_task);;
				ms_RBox_usrMsg.AppendText($"result: {lo_task.m_result}\r\n");
			}
			catch (Exception ex)
			{
				ms_RBox_usrMsg.AppendText(ex.ToString() + "\r\n");
			}
		}

		// ------------------------------------------------------------------------------------
		// テスト２
		static uint ms_mid_task_id = 101;
		async void m_btn_test_2_Click(object sender, EventArgs e)
		{
			tst_mid_task mid_task = new tst_mid_task() { m_task_id = ms_mid_task_id };
			ms_mid_task_id++;

			try
			{
				await tst_scheduler.Set(mid_task);;
				ms_RBox_usrMsg.AppendText($"result: {mid_task.m_result}\r\n");
			}
			catch (Exception ex)
			{
				ms_RBox_usrMsg.AppendText(ex.ToString() + "\r\n");
			}
		}

		// ------------------------------------------------------------------------------------

		static class tst_scheduler
		{
			public static Task Set(tst_http_task http_task)
			{
				Task task = http_task.Queueing();
				http_task.SetLatestTask(task);
				return task;
			}
		}

		public abstract class tst_http_task
		{
			public string m_str_cancel = null;  // これが null でない場合、タスクがキャンセルされたことを表す
			public uint m_task_id;
			public uint m_result;

			public abstract Task Queueing();
			public abstract void SetLatestTask(Task task);
		}

		public class tst_mid_task : tst_http_task
		{
			public static uint ms_num_mid_task = 0;
			public static Task ms_mid_task_latest = null;

			public override void SetLatestTask(Task task) { ms_mid_task_latest = task; }
			public static void ResetQueue()
			{
				ms_num_mid_task = 0;
				ms_mid_task_latest = null;
			}

			public override async Task Queueing()
			{
				ms_num_mid_task++;
				if (ms_num_mid_task > 1)
				{
					await ms_mid_task_latest;
				}
				if (tst_lo_task.ms_lo_task_cur != null)
				{
					await tst_lo_task.ms_lo_task_cur;
				}

				if (m_task_id == 103)
				{
					ms_RBox_usrMsg.AppendText("+++ throw テスト\r\n");

					ResetQueue();
					tst_lo_task.ResetQueue();

					throw new Exception("!!! throw テスト： m_task_id == 103)");
				}

				ms_RBox_usrMsg.SelectionColor = System.Drawing.Color.FromArgb(255, 100, 50);
				ms_RBox_usrMsg.AppendText($"--- Mid_Ex_Task　task_id: {m_task_id} <-- START\r\n");
				ms_RBox_usrMsg.SelectionColor = System.Drawing.Color.Black;

				await Task.Delay(2000);
				m_result = m_task_id + 1000;

				ms_RBox_usrMsg.SelectionColor = System.Drawing.Color.FromArgb(255, 100, 50);
				ms_RBox_usrMsg.AppendText($"--- Mid_Ex_Task　task_id: {m_task_id} <-- END\r\n");
				ms_RBox_usrMsg.SelectionColor = System.Drawing.Color.Black;

				ms_num_mid_task--;
			}
		}

		public class tst_lo_task : tst_http_task
		{
			static uint ms_num_lo_task = 0;
			static Task ms_lo_task_latest = null;
			public static Task ms_lo_task_cur = null;

			public override void SetLatestTask(Task task) { ms_lo_task_latest = task; }
			public static void ResetQueue()
			{
				ms_num_lo_task = 0;
				ms_lo_task_latest = null;
				// ms_lo_task_cur = null;
			}

			public override async Task Queueing()
			{
				ms_num_lo_task++;
				if (ms_num_lo_task > 1)
				{
					await ms_lo_task_latest;
				}
				while (tst_mid_task.ms_num_mid_task > 0)
				{
					await tst_mid_task.ms_mid_task_latest;
				}

				ms_RBox_usrMsg.AppendText($"--- Lo_Ex_Task　task_id: {m_task_id} <-- START\r\n");

				ms_lo_task_cur= Task.Delay(2000);
				await ms_lo_task_cur;
				ms_lo_task_cur = null;
				m_result = m_task_id + 1000;

				ms_RBox_usrMsg.AppendText($"--- Lo_Ex_Task　task_id: {m_task_id} <-- END\r\n");
				ms_num_lo_task--;
			}
		}
}

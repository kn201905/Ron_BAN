namespace Ron_BAN
{
		// ------------------------------------------------------------------------------------
		// テスト１
		static uint ms_lo_task_id = 1;
		async void m_btn_test_1_Click(object sender, EventArgs e)
		{
			LoTask lo_task = new LoTask() { m_task_id = ms_lo_task_id };
			ms_lo_task_id++;
			await HttpScheduler.Set(lo_task);

			ms_RBox_usrMsg.AppendText($"result: {lo_task.m_result}\r\n");
		}

		// ------------------------------------------------------------------------------------
		// テスト２
		static uint ms_mid_task_id = 101;
		async void m_btn_test_2_Click(object sender, EventArgs e)
		{
			MidTask mid_task = new MidTask() { m_task_id = ms_mid_task_id };
			ms_mid_task_id++;

			await HttpScheduler.Set(mid_task);

			ms_RBox_usrMsg.AppendText($"result: {mid_task.m_result}\r\n");
		}

		// ------------------------------------------------------------------------------------

		static class HttpScheduler
		{
			public static Task Set(HttpTask http_task)
			{
				Task task = http_task.Queueing();
				http_task.SetLatestTask(task);
				return task;
			}
		}

		public abstract class HttpTask
		{
			public string m_str_cancel = null;  // これが null でない場合、タスクがキャンセルされたことを表す
			public uint m_task_id;
			public uint m_result;

			public abstract Task Queueing();
			public abstract void SetLatestTask(Task task);
		}

		public class MidTask : HttpTask
		{
			public static uint ms_num_mid_task = 0;
			public static Task ms_mid_task_latest = null;

			public override void SetLatestTask(Task task) { ms_mid_task_latest = task; }
			public override async Task Queueing()
			{
				ms_num_mid_task++;
				if (ms_num_mid_task > 1)
				{
					await ms_mid_task_latest;
				}
				if (LoTask.ms_lo_task_cur != null)
				{
					await LoTask.ms_lo_task_cur;
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

		public class LoTask : HttpTask
		{
			static uint ms_num_lo_task = 0;
			static Task ms_lo_task_latest = null;
			public static Task ms_lo_task_cur = null;

			public override void SetLatestTask(Task task) { ms_lo_task_latest = task; }
			public override async Task Queueing()
			{
				ms_num_lo_task++;
				if (ms_num_lo_task > 1)
				{
					await ms_lo_task_latest;
				}
				while (MidTask.ms_num_mid_task > 0)
				{
					await MidTask.ms_mid_task_latest;
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

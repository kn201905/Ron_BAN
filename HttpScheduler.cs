namespace Ron_BAN
{
		// ------------------------------------------------------------------------------------
		// テスト１
		static uint ms_lo_task_id = 1;
		async void m_btn_test_1_Click(object sender, EventArgs e)
		{
			LoTask lo_task = new LoTask() { m_task_id = ms_lo_task_id };
			ms_lo_task_id++;
			await TaskScheduler.Set(lo_task);

			ms_RBox_usrMsg.AppendText($"result: {lo_task.m_result}\r\n");
		}

		// ------------------------------------------------------------------------------------
		// テスト２
		static uint ms_mid_task_id = 101;
		async void m_btn_test_2_Click(object sender, EventArgs e)
		{
			http_task mid_task = new http_task{ m_task_id = ms_mid_task_id };
			ms_mid_task_id++;

			http_task result = await Task_SC.Set_Task(mid_task);

			ms_RBox_usrMsg.AppendText($"result: {result.m_result}\r\n");
		}

		// ------------------------------------------------------------------------------------

		static class HttpScheduler
		{
			public static async Task Set(HttpTask http_task)
			{
				Task task = http_task.Queueing();
				http_task.SetLatestTask(task);
				await task;
			}
		}

		public abstract class HttpTask
		{
			public uint m_task_id;
			public uint m_result;

			public abstract Task Queueing();
			public abstract void SetLatestTask(Task task);
		}

		public class LoTask : HttpTask
		{
			static uint ms_num_lo_task = 0;
			static Task ms_lo_task_latest = null;

			public override void SetLatestTask(Task task) { ms_lo_task_latest = task; }
			public override async Task Queueing()
			{
				ms_num_lo_task++;
				if (ms_num_lo_task > 1)
				{
					await ms_lo_task_latest;
				}

				ms_RBox_usrMsg.AppendText($"--- Lo_Ex_Task　task_id: {m_task_id} <-- START\r\n");

				await Task.Delay(2000);
				m_result = m_task_id + 1000;

				ms_RBox_usrMsg.AppendText($"--- Lo_Ex_Task　task_id: {m_task_id} <-- END\r\n");
				ms_num_lo_task--;
			}
		}
}

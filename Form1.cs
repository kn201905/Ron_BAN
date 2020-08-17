using System;
using System.Windows.Forms;


namespace Ron_BAN
{
	public partial class MainForm : Form
	{
		bool m_bConnected_once = false;

		public MainForm()
		{
			InitializeComponent();
		}

		~MainForm()
		{
			Drrr_Host.Dispose();
			Drrr_Proxy.Dispose();
		}

		async void OnBtnClk_connect(object sender, EventArgs e)
		{
			try
			{
				if (m_bConnected_once == false)
				{
					m_bConnected_once = true;
					m_btn_connect.Text = "切断";

					// 接続処理開始
					bool b_result = await Drrr_Host.Establish_cnct("ベア", "今日は暑い");
					if (b_result == false)
					{ throw new Exception("!!! 接続処理に失敗しました。"); }
				}
				else
				{
					m_btn_connect.Text = "切断済み";
					m_btn_connect.Enabled = false;

					// 切断処理開始
					bool b_result = await Drrr_Host.Disconnect();
					if (b_result == false)
					{ throw new Exception("!!! 切断処理に失敗しました。"); }
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());

				m_bConnected_once = true;
				m_btn_connect.Text = "エラー発生";
				m_btn_connect.Enabled = false;
			}
		}

		void m_btn_leave_rm_Click(object sender, EventArgs e)
		{
		}

		void m_btn_logout_Click(object sender, EventArgs e)
		{
		}

		private void m_btn_issue_Click(object sender, EventArgs e)
		{
			Drrr_Host.PostMsg("発言実行テスト。test message");
		}

		private void m_btn_GetJSON_Click(object sender, EventArgs e)
		{
			string str_JSON = Drrr_Host.GetJSON();
			Program.WriteStBox(str_JSON);
/*
			Program.WriteStBox("--- xhr を実行します。\r\n");

			// Ajax用ヘッダ
			SetReqHeader_Ajax(m_wc.Headers, m_str_Cookie);
			Program.WriteStBox($"--- リクエストヘッダ\r\n");
			Show_HttpHeader(m_wc.Headers);

			string str_reply = m_wc.UploadString("http://drrrkari.com/ajax.php", "");
			Program.WriteStBox(str_reply)
*/
		}

		void m_btn_test_1_Click(object sender, EventArgs e)
		{
		}

		void m_btn_test_2_Click(object sender, EventArgs e)
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
	}
}


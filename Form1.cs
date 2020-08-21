using System;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

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

		void m_btn_proxy_Click(object sender, EventArgs e)
		{
			try
			{
				Drrr_Proxy.Init();
				Drrr_Proxy.Get_index_html(); ;
			}
			catch (Exception ex)
			{
				Program.WriteStatus(ex.ToString());
			}
		}

		private void m_btn_issue_Click(object sender, EventArgs e)
		{
			Drrr_Host.PostMsg("発言実行テスト。test message");
		}

		private void m_btn_GetJSON_Click(object sender, EventArgs e)
		{
			string str_JSON = Encoding.UTF8.GetString(Drrr_Host.GetJSON());
			Program.WriteStatus(str_JSON);

//			File.WriteAllText(@"Y:\test_code\err.json", str_JSON);
		}

		// ------------------------------------------------------------------------------------
		// テスト１
		void m_btn_test_1_Click(object sender, EventArgs e)
		{
		}

		// ------------------------------------------------------------------------------------
		// テスト２
		void m_btn_test_2_Click(object sender, EventArgs e)
		{
		}

		// ------------------------------------------------------------------------------------
		// テスト３
		void m_btn_test_3_Click(object sender, EventArgs e)
		{
			try
			{
				Read_JsonFile(@"Y:\test_code\_sample1-1_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-1_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-1_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-2_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-2_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-2_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-3_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-3_knk.json");
				Read_JsonFile(@"Y:\test_code\_sample1-3_knk.json");
			}
			catch (Exception ex)
			{
				Program.WriteStatus(ex.ToString());
			}
		}

		static void Read_JsonFile(string filepath)
		{
			using (FileStream fs = File.OpenRead(filepath))
			{
				int bytes_file = (int)fs.Length;
				byte[] buf_utf8 = new byte[bytes_file];
				fs.Read(buf_utf8, 0, bytes_file);

				StringBuilder sb = DB_cur.Set_RoomJSON(buf_utf8);
				if (sb.Length > 0)
				{
					Program.WriteStatus(sb.ToString());
				}
			}
		}
	}
}


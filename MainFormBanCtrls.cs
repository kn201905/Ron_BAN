using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace Ron_BAN
{
	public partial class MainForm
	{
		const int NUM_Btn_BAN = 15;
		const int TOP_Btn_BAN = 40;
		const int HEIGHT_Line = 33;
		
		const int WIDTH_Btn_BAN = 100;
		const int HEIGHT_Btn_BAN = 22;

		const int LEFT_TBox_BAN = 115;
		const int WIDTH_TBox_BAN = 730;

		Button[] ma_Btn_BAN = new Button[NUM_Btn_BAN];
		TextBox[] ma_TBox_BAN_info = new TextBox[NUM_Btn_BAN];

		// 未設定は 0、入室者は 正の数、退室者は 負の数
		int[] ma_id_this_session = new int[NUM_Btn_BAN];
		string[] ma_uid_on_room = new string[NUM_Btn_BAN];
		string[] ma_encip_exit_usr = new string[NUM_Btn_BAN];

		class BindIdx
		{
			public BindIdx(MainForm form, int idx) { m_form = form; m_idx = idx; }
			MainForm m_form;
			int m_idx;

			public void Call_Handler(object sender, EventArgs e)
			{
				m_form.OnClk_BanBtn(m_idx);
			}
		}
		
		// ------------------------------------------------------------------------------------
		void Create_BanCtrls()
		{
			Font font_meiyo_Ke_P = new Font("MeiryoKe_PGothic", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(128)));

			{
				int top_btn = TOP_Btn_BAN;
				Size size_btn = new Size(WIDTH_Btn_BAN, HEIGHT_Btn_BAN);

				for (int idx = 0; idx < NUM_Btn_BAN; idx++)
				{
					Button btn = new Button();

					btn.Font = font_meiyo_Ke_P;
					btn.Location = new Point(0, top_btn);
					top_btn += HEIGHT_Line;
					btn.Size = size_btn;
					btn.Text = "　---";
					btn.Enabled = false;
					btn.UseVisualStyleBackColor = true;
					btn.TextAlign = ContentAlignment.MiddleLeft;
					btn.Click += (new BindIdx(this, idx)).Call_Handler;

					splitContainer1.Panel1.Controls.Add(btn);

					ma_Btn_BAN[idx] = btn;
				}
			}

			{
				int top_tbox = TOP_Btn_BAN;
				Size size_tbox = new Size(WIDTH_TBox_BAN, HEIGHT_Btn_BAN);

				for (int idx = 0; idx < NUM_Btn_BAN; idx++)
				{
					TextBox tbox = new TextBox();

					tbox.Font = font_meiyo_Ke_P;
					tbox.Location = new Point(LEFT_TBox_BAN, top_tbox);
					top_tbox += HEIGHT_Line;
					tbox.Size = size_tbox;
					tbox.Visible = false;

					splitContainer1.Panel1.Controls.Add(tbox);

					ma_TBox_BAN_info[idx] = tbox;
				}
			}

			for (int idx = 0; idx < NUM_Btn_BAN; idx++)
			{ ma_id_this_session[idx] = 0; }
		}

		// ------------------------------------------------------------------------------------

		void Update_BanCtrl()
		{
			List<UInfo> ary_uinfo_on_room = UInfo_onRoom.msa_uinfo;
			int num_on_room = Math.Min(NUM_Btn_BAN, ary_uinfo_on_room.Count);
			
			for (int idx = 0; idx < num_on_room; idx++)
			{
				UInfo uinfo = ary_uinfo_on_room[idx];
				if (ma_id_this_session[idx] != uinfo.m_id_this_session)
				{
					ma_Btn_BAN[idx].Text = uinfo.m_uname;
					ma_TBox_BAN_info[idx].Text
							= $"[{string.Join(", ", uinfo.m_unames_this_session)}] / {uinfo.m_uid.m_str_uid}";
					ma_id_this_session[idx] = uinfo.m_id_this_session;

					ma_uid_on_room[idx] = uinfo.m_uid.m_str_uid;
					ma_encip_exit_usr[idx] = null;

					ma_Btn_BAN[idx].Enabled = true;
					ma_TBox_BAN_info[idx].Visible = true;
				}
			}

			int idx_tmnt;
			{
				List<string> ary_eusr_encip = ExitEip_onTalks.msa_encip_on_talks;
				List<int> ary_eusr_id_this_session = ExitEip_onTalks.msa_id_this_session;

				int idx_eusr = ary_eusr_encip.Count;
				idx_tmnt = Math.Min(NUM_Btn_BAN, num_on_room + idx_eusr);
				idx_eusr--;  // eusr は、後方から処理をしていく

				for (int idx = num_on_room; idx < idx_tmnt; idx++)
				{
					if (ma_id_this_session[idx] != ary_eusr_id_this_session[idx_eusr])
					{
						var eusr_unames = ExitEip_onTalks.msa_unames_on_talks[idx_eusr];
						ma_Btn_BAN[idx].Text = string.Join(", ", eusr_unames);
						ma_TBox_BAN_info[idx].Text
								= $"[{string.Join(", ", eusr_unames)}] / {ary_eusr_encip[idx_eusr]}";
						ma_id_this_session[idx] = ary_eusr_id_this_session[idx_eusr];

						ma_uid_on_room[idx] = null;
						ma_encip_exit_usr[idx] = ary_eusr_encip[idx_eusr];

						if ((ExitEip_onTalks.msa_flags[idx] & ExitEip_onTalks.ExitUsr_Stt.EN_Banned) == 0)
						{ ma_Btn_BAN[idx].Enabled = true; }
						else
						{ ma_Btn_BAN[idx].Enabled = false; }

						ma_TBox_BAN_info[idx].Visible = true;
					}
					idx_eusr--;
				}
			}

			for (int idx = idx_tmnt; idx < NUM_Btn_BAN; idx++)
			{
				if (ma_id_this_session[idx] == 0) { break; }

				ma_Btn_BAN[idx].Text = "　---";
				ma_id_this_session[idx] = 0;

				ma_Btn_BAN[idx].Enabled = false;
				ma_TBox_BAN_info[idx].Visible = false;
			}
		}

		// ------------------------------------------------------------------------------------
		System.Text.StringBuilder m_sb_ban_ret_msg = new System.Text.StringBuilder(50);

		async void OnClk_BanBtn(int idx_btn)
		{
			// まず、on_room なのか 退室者 なのかの判断
			if (ma_uid_on_room[idx_btn] != null)
			{
				try
				{
					ma_Btn_BAN[idx_btn].Enabled = false;
					Drrr_Host2.HttpTask ban_task = await Drrr_Host2.Ban_byUid(ma_uid_on_room[idx_btn]);

					if (ban_task.m_str_cancel != null)
					{
						MainForm.WriteStatus($"!!! 「Ban_byUid」がキャンセルされました。\r\n{ban_task.m_str_cancel}");
						ma_Btn_BAN[idx_btn].Enabled = true;
						return;
					}

					// 受信メッセージを表示する
					byte[] bytes_utf8 = await ban_task.m_http_res.Content.ReadAsByteArrayAsync();
					m_sb_ban_ret_msg.Clear();
					m_sb_ban_ret_msg.Append(System.Text.Encoding.UTF8.GetString(bytes_utf8));
					m_sb_ban_ret_msg.Append($"-> [{ma_Btn_BAN[idx_btn].Text}]\r\n");
					MainForm.WriteStatus(m_sb_ban_ret_msg.ToString());
				}
				catch (Exception ex)
				{
					MainForm.WriteStatus(ex.ToString() + "\r\n");
					ma_Btn_BAN[idx_btn].Enabled = true;
					return;
				}
			}
			else
			{
				// 現時点では、退室者の対応を行っていない
			}
		}
	}
}

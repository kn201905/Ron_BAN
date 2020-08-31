using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace Ron_BAN
{
	public partial class MainForm
	{
		//------------------------------------------------
		// GUI 部品サイズ等の定数
		const int NUM_Btn_BAN = 15;
		const int HEIGHT_Line = 33;
		const int TOP_Btn_BAN = 40;
		const int HEIGHT_Btn_BAN = 22;

		const int LEFT_Btn_delayBAN = 0;
		const int WIDTH_Btn_delayBAN = 55;
		
		const int LEFT_Btn_BAN = 63;
		const int WIDTH_Btn_BAN = 100;

		const int LEFT_TBox_BAN = 109 + LEFT_Btn_BAN;
		const int WIDTH_TBox_BAN = 735 - LEFT_Btn_BAN;
		//------------------------------------------------
		
		class BanCtrl_GUI
		{
			public Button m_Btn_delay_ban = new Button();
			public Button m_Btn_ban = new Button();
			public TextBox m_TBox_info = new TextBox();

			// 未設定は 0、入室者は 正の数、退室者は 負の数
			public int m_id_this_session = 0;
			public BanCtrl m_ban_ctrl = null;
		}

		static BanCtrl_GUI[] msa_BanCtrl_GUIs = new BanCtrl_GUI[NUM_Btn_BAN];

		// ------------------------------------------------------------------------------------

		static class BanCtrls_Ctrlr
		{
			public static BanCtrl ms_BanCtrl_top = null;
			static BanCtrl ms_BanCtrl_bottom = null;

			public static void Init()
			{
				ms_BanCtrl_top = new BanCtrl();

				BanCtrl prev = ms_BanCtrl_top;
				BanCtrl cur = null;
				for (int i = 0; i < NUM_Btn_BAN - 1; i++)
				{
					cur = new BanCtrl();
					prev.m_next = cur;
					cur.m_prev = prev;

					prev = cur;
				}
				ms_BanCtrl_bottom = cur;
			}

			// ban_ctrl に変更を行うときにコールされる関数
			// 戻り値： 実際に登録された BanCtrl
			public static BanCtrl Update_RoomUsr(BanCtrl pos_ctrl, int id_this_session, int idx_GUI, UInfo uinfo)
			{
				BanCtrl ctrl = pos_ctrl;
				if (ctrl == ms_BanCtrl_bottom)
				{
					ctrl.Set_NewUInfo(id_this_session, idx_GUI, uinfo);
				}
				else
				{
					while (true)
					{
						int ctrl_id_this_session = ctrl.m_id_this_session;
						if (ctrl_id_this_session == id_this_session)
						{
							// ユーザ情報はそのままでよい
							ctrl.m_GUI = msa_BanCtrl_GUIs[idx_GUI];
							break;
						}

						if (ctrl_id_this_session == 0)
						{
							ctrl.Set_NewUInfo(id_this_session, idx_GUI, uinfo);
							break;
						}
						if (ctrl_id_this_session < 0)
						{
							ctrl = InsertFromLast(ctrl);
							ctrl.Set_NewUInfo(id_this_session, idx_GUI, uinfo);
							break;
						}
						if (ctrl_id_this_session > id_this_session)
						{ throw new Exception(
								"!!! エラー検出： ctrl_id__this_session > id_this_session in BanCtrls_Ctrlr.Update_onRoomUsr()"); }

						ctrl = MoveToLast(ctrl);
					}
				}

				// GUI の表示を update する
				ctrl.Draw_AsRoomUsr();
				return ctrl;
			}

			// -----------------------------------------------------------
			// ban_ctrl に変更を行うときにコールされる関数
			// 戻り値： 実際に登録された BanCtrl
			public static BanCtrl Update_ExitUsr(BanCtrl pos_ctrl, int id_this_session, int idx_GUI, int idx_eusr)
			{
				BanCtrl ctrl = pos_ctrl;
				if (ctrl == ms_BanCtrl_bottom)
				{
					ctrl.Set_NewExitUsr(id_this_session, idx_GUI, idx_eusr);
				}
				else
				{
					while (true)
					{
						int ctrl_id_this_session = ctrl.m_id_this_session;
						if (ctrl_id_this_session == id_this_session)
						{
							// ユーザ情報はそのままでよい
							ctrl.m_GUI = msa_BanCtrl_GUIs[idx_GUI];
							break;
						}

						if (ctrl_id_this_session == 0)
						{
							ctrl.Set_NewExitUsr(id_this_session, idx_GUI, idx_eusr);
							break;
						}
						if (ctrl_id_this_session < 0)
						{
							if (ctrl_id_this_session < id_this_session)
							{ throw new Exception(
									"!!! エラー検出： ctrl_id_this_session < id_this_session in BanCtrls_Ctrlr.Update_ExitUsr()"); }

							ctrl = InsertFromLast(ctrl);
							ctrl.Set_NewExitUsr(id_this_session, idx_GUI, idx_eusr);
							break;
						}
						ctrl = MoveToLast(ctrl);
					}
				}

				// GUI の表示を update する
				ctrl.Draw_AsExitUsr();
				return ctrl;
			}
			
			// -----------------------------------------------------------
			//【注意】 tgt != ms_BanCtrl_bottom であること
			// tgt の m_id_this_session はクリアされる
			static BanCtrl MoveToLast(BanCtrl tgt)
			{
				tgt.m_id_this_session = 0;

				// tgt をリストから外す
				BanCtrl ret_ctrl = tgt.m_next;
				ret_ctrl.m_prev = tgt.m_prev;
				if (tgt.m_prev == null)
				{
					ms_BanCtrl_top = ret_ctrl;
				}
				else
				{
					tgt.m_prev.m_next = ret_ctrl;
				}
				// tgt を最後尾に回す
				ms_BanCtrl_bottom.m_next = tgt;
				tgt.m_prev = ms_BanCtrl_bottom;
				tgt.m_next = null;
				ms_BanCtrl_bottom = tgt;

				return ret_ctrl;
			}

			// -----------------------------------------------------------
			//【注意】 insertBefore != ms_BanCtrl_bottom であること
			static BanCtrl InsertFromLast(BanCtrl insertBefore)
			{
				// 末尾をリストから外す
				BanCtrl ret_ctrl = ms_BanCtrl_bottom;
				ms_BanCtrl_bottom = ms_BanCtrl_bottom.m_prev;
				ms_BanCtrl_bottom.m_next = null;

				// insertBefore の前に入れる
				ret_ctrl.m_next = insertBefore;
				ret_ctrl.m_prev = insertBefore.m_prev;
				if (insertBefore.m_prev == null)
				{
					ms_BanCtrl_top = ret_ctrl;
				}
				else
				{
					insertBefore.m_prev.m_next = ret_ctrl;
				}
				insertBefore.m_prev = ret_ctrl;

				return ret_ctrl;
			}
		}

		class BanCtrl
		{
			public BanCtrl m_prev = null;
			public BanCtrl m_next = null;

			public int m_id_this_session = 0;
			public BanCtrl_GUI m_GUI = null;

			// 以下はどちらか一方が有効となる
			public UInfo m_uinfo = null;
			public string m_exit_unames = null;

			public string m_encip = null;
			
			public bool mb_Rgst_ban = false;
			public bool mb_Exec_Ban = false;
			public int m_count_down_delay_ban = -1;

			// -----------------------------------------------
			public void Set_NewUInfo(int id_this_session, int idx_GUI, UInfo uinfo)
			{
				m_id_this_session = id_this_session;
				m_GUI = msa_BanCtrl_GUIs[idx_GUI];

				m_uinfo = uinfo;
				m_exit_unames = null;
				m_encip = uinfo.m_encip.m_str_encip;

				mb_Rgst_ban = uinfo.mb_to_ban_onAttend;
				mb_Exec_Ban = false;
				m_count_down_delay_ban = -1;
			}

			// -----------------------------------------------
			public void Set_NewExitUsr(int id_this_session, int idx_GUI, int idx_eusr)
			{
				m_id_this_session = id_this_session;
				m_GUI = msa_BanCtrl_GUIs[idx_GUI];

				m_uinfo = null;
				m_exit_unames = string.Join(", ", ExitEip_onTalks.msa_unames_on_talks[idx_eusr]);
				m_encip = ExitEip_onTalks.msa_encip_on_talks[idx_eusr];

				if ((ExitEip_onTalks.msa_flags[idx_eusr] & ExitEip_onTalks.ExitUsr_Stt.EN_Regist_BAN) == 0)
				{ mb_Rgst_ban = false; }
				else
				{ mb_Rgst_ban = true; }

				mb_Exec_Ban = false;
				m_count_down_delay_ban = -1;
			}

			// -----------------------------------------------
			public void Draw_AsRoomUsr()
			{
				Button btn_ban = m_GUI.m_Btn_ban;
				btn_ban.Text = m_uinfo.m_uname;
				btn_ban.Enabled = true;

				TextBox tbox_info = m_GUI.m_TBox_info;
				tbox_info.Text = $"[{string.Join(", ", m_uinfo.m_unames_this_session)}] / {m_uinfo.m_uid.m_str_uid}";
				tbox_info.Visible = true;

				if (m_count_down_delay_ban >= 0)
				{
					m_GUI.m_Btn_delay_ban.Enabled = false;
					tbox_info.BackColor = Color.Orange;
				}
				else
				{
					m_GUI.m_Btn_delay_ban.Enabled = true;
					if (mb_Rgst_ban)
					{ tbox_info.BackColor = Color.HotPink; }
					else
					{ tbox_info.BackColor = Color.White; }
				}
			}

			// -----------------------------------------------
			public void Draw_AsExitUsr()
			{
				m_GUI.m_Btn_delay_ban.Enabled = false;

				Button btn_ban = m_GUI.m_Btn_ban;
				btn_ban.Text = m_exit_unames;

				TextBox tbox_info = m_GUI.m_TBox_info;
				tbox_info.Text =  $"[{m_exit_unames}] / {m_encip}";
				tbox_info.Visible = true;

				if (mb_Rgst_ban)
				{
					btn_ban.Enabled = false;
					tbox_info.BackColor = Color.LightPink;
				}
				else
				{
					btn_ban.Enabled = true;
					tbox_info.BackColor = Color.Gainsboro;
				}
			}

			// -----------------------------------------------
			System.Text.StringBuilder m_sb_ban_ret_msg = new System.Text.StringBuilder(50);

			// TODO： DB_static への登録


			public async void OnClk_Btn_BAN(object sender, EventArgs e)
			{
				if (m_id_this_session == 0)
				{ throw new Exception("!!! エラー検出： m_id_this_session == 0 in BanCtrl.OnClk_Btn_BAN()"); }

				mb_Rgst_ban = true;
				mb_Exec_Ban = true;

				m_GUI.m_Btn_ban.Enabled = false;
				m_GUI.m_Btn_delay_ban.Enabled = false;

				// まず、on_room なのか 退室者 なのかの判断
				if (m_id_this_session > 0)
				{
					m_GUI.m_TBox_info.BackColor = Color.HotPink;

					Drrr_Host2.HttpTask ban_usr_task = Drrr_Host2.BanUsr_Task_Factory.Create(m_uinfo.m_uid.m_str_uid);
					if (ban_usr_task.m_str_cancel == null)
					{
						try
						{
							await ban_usr_task.DoWork();
						}
						catch (Exception ex)
						{
							ban_usr_task.m_str_cancel = ex.ToString() + "\r\n";
						}
					}
					// まとめてエラー処理を行う
					if (ban_usr_task.m_str_cancel != null)
					{
						MainForm.WriteStatus($"!!! 「BanUsr」がキャンセルされました。\r\n{ban_usr_task.m_str_cancel}");
						m_GUI.m_Btn_ban.Enabled = true;
						m_GUI.m_Btn_delay_ban.Enabled = true;
						return;
					}

					// 受信メッセージを表示する
					byte[] bytes_utf8 = await ban_usr_task.m_http_res.Content.ReadAsByteArrayAsync();
					m_sb_ban_ret_msg.Clear();
					m_sb_ban_ret_msg.Append(System.Text.Encoding.UTF8.GetString(bytes_utf8));
					m_sb_ban_ret_msg.Append($"-> [{m_uinfo.m_uname}]\r\n");

					// 強制退室 or 部屋にいませんでした の２択になるため、その判定が必要
					string ret_str = m_sb_ban_ret_msg.ToString();
					if (ret_str.Contains("強制退室") == false)
					{
						// 強制退室失敗
						m_GUI.m_Btn_ban.Enabled = true;
						m_GUI.m_Btn_delay_ban.Enabled = true;
					}

					MainForm.WriteStatus(ret_str);
				}
				else
				{
					m_GUI.m_TBox_info.BackColor = Color.LightPink;

					// 退室者の eip to ban 登録
					ExitEip_onTalks.Regist_to_BAN(m_encip);
				}
			}
		}

		// ------------------------------------------------------------------------------------
		void Create_BanCtrls_GUI()
		{
			int top_btn = TOP_Btn_BAN;
			Size size_delay_btn = new Size(WIDTH_Btn_delayBAN, HEIGHT_Btn_BAN);
			Size size_tbox = new Size(WIDTH_TBox_BAN, HEIGHT_Btn_BAN);
			Size size_btn = new Size(WIDTH_Btn_BAN, HEIGHT_Btn_BAN);

			for (int idx = 0; idx < NUM_Btn_BAN; top_btn += HEIGHT_Line, idx++)
			{
				msa_BanCtrl_GUIs[idx] = new BanCtrl_GUI();

				// ---------------------------
				Button delay_btn = new Button();

				delay_btn.Font = MainForm.ms_meiryo_Ke_P_8pt;
				delay_btn.Location = new Point(LEFT_Btn_delayBAN, top_btn);
				delay_btn.Size = size_delay_btn;
				delay_btn.Text = "遅延ban";
				delay_btn.Enabled = false;
				delay_btn.UseVisualStyleBackColor = true;

				splitContainer1.Panel1.Controls.Add(delay_btn);
				msa_BanCtrl_GUIs[idx].m_Btn_delay_ban = delay_btn;

				// ---------------------------
				Button btn = new Button();

				btn.Font = MainForm.ms_meiryo_Ke_P_9pt;
				btn.Location = new Point(LEFT_Btn_BAN, top_btn);
				btn.Size = size_btn;
				btn.Text = "　---";
				btn.Enabled = false;
				btn.UseVisualStyleBackColor = true;
				btn.TextAlign = ContentAlignment.MiddleLeft;

				splitContainer1.Panel1.Controls.Add(btn);
				msa_BanCtrl_GUIs[idx].m_Btn_ban = btn;

				// ---------------------------

				TextBox tbox = new TextBox();

				tbox.Font = MainForm.ms_meiryo_Ke_P_9pt;
				tbox.Location = new Point(LEFT_TBox_BAN, top_btn);
				tbox.Size = size_tbox;
				tbox.Visible = false;

				splitContainer1.Panel1.Controls.Add(tbox);
				msa_BanCtrl_GUIs[idx].m_TBox_info = tbox;
			}
		}

		// ------------------------------------------------------------------------------------
		int[] ma_btn_idx_to_ban = new int[NUM_Btn_BAN];
		int m_tmnt_btn_idx_to_ban = 0;

		void Update_BanCtrls()
		{
			int c_num_on_room = Math.Min(NUM_Btn_BAN, UInfo_onRoom.msa_uinfo.Count);
			BanCtrl ban_ctrl = BanCtrls_Ctrlr.ms_BanCtrl_top;
			
			for (int idx = 0; idx < c_num_on_room; idx++)
			{
				UInfo uinfo = UInfo_onRoom.msa_uinfo[idx];
				BanCtrl_GUI ban_ctrl_GUI = msa_BanCtrl_GUIs[idx];

				// uinfo、ban_ctrl、ban_ctrl_GUI の３つを lead_id_this_session を軸として結びつけていく
				int lead_id_this_session = uinfo.m_id_this_session;

				if (ban_ctrl_GUI.m_id_this_session != lead_id_this_session)
				{
					ban_ctrl = BanCtrls_Ctrlr.Update_RoomUsr(ban_ctrl, lead_id_this_session, idx, uinfo);

					if (ban_ctrl.mb_Rgst_ban && ban_ctrl.mb_Exec_Ban == false)
					{
						ma_btn_idx_to_ban[m_tmnt_btn_idx_to_ban] = idx;
						m_tmnt_btn_idx_to_ban++;
					}
				
					ban_ctrl_GUI.m_id_this_session = lead_id_this_session;
					ban_ctrl_GUI.m_ban_ctrl = ban_ctrl;
					ban_ctrl_GUI.m_Btn_ban.Click += ban_ctrl.OnClk_Btn_BAN;
				}
				ban_ctrl = ban_ctrl.m_next;
			}

			int idx_tmnt;
			{
				int idx_eusr = ExitEip_onTalks.msa_encip_on_talks.Count;
				idx_tmnt = Math.Min(NUM_Btn_BAN, c_num_on_room + idx_eusr);
				idx_eusr--;  // eusr は、後方から処理をしていく（時間的順序を考慮）

				for (int idx_GUI = c_num_on_room; idx_GUI < idx_tmnt; idx_eusr--, idx_GUI++)
				{
					BanCtrl_GUI ban_ctrl_GUI = msa_BanCtrl_GUIs[idx_GUI];

					// exit_user、ban_ctrl、ban_ctrl_GUI の３つを lead_id_this_session を軸として結びつけていく
					int lead_id_this_session = ExitEip_onTalks.msa_id_this_session[idx_eusr];
					
					if (ban_ctrl_GUI.m_id_this_session != lead_id_this_session)
					{
						ban_ctrl = BanCtrls_Ctrlr.Update_ExitUsr(ban_ctrl, lead_id_this_session, idx_GUI, idx_eusr);

						ban_ctrl_GUI.m_id_this_session = lead_id_this_session;
						ban_ctrl_GUI.m_ban_ctrl = ban_ctrl;
						ban_ctrl_GUI.m_Btn_ban.Click += ban_ctrl.OnClk_Btn_BAN;
					}
					ban_ctrl = ban_ctrl.m_next;
				}
			}

			for (int idx_GUI = idx_tmnt; idx_GUI < NUM_Btn_BAN; idx_GUI++)
			{
				BanCtrl_GUI ban_ctrl_GUI = msa_BanCtrl_GUIs[idx_GUI];
				if (ban_ctrl_GUI.m_id_this_session == 0) { break; }

				ban_ctrl_GUI.m_Btn_delay_ban.Enabled = false;

				ban_ctrl_GUI.m_Btn_ban.Text = "　---";
				ban_ctrl_GUI.m_Btn_ban.Enabled = false;

				ban_ctrl_GUI.m_TBox_info.Visible = false;
				ban_ctrl_GUI.m_id_this_session = 0;
			}

			// BanCtrl の表示更新を終えたタイミングで、即時 BAN 対象者がいたら BAN を実行する
			for (int i = 0; i < m_tmnt_btn_idx_to_ban; i++)
			{
				msa_BanCtrl_GUIs[ma_btn_idx_to_ban[i]].m_ban_ctrl.OnClk_Btn_BAN(null, null);
			}
		}

		// ------------------------------------------------------------------------------------

		void OnTimer_delayBAN(Object src, System.Timers.ElapsedEventArgs ev_time)
		{
		}
	}
}

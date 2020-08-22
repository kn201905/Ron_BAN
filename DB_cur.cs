using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

namespace Ron_BAN
{
	struct Uencip
	{
		public Uencip(string str_encip) { m_str_encip = str_encip; }
		public string m_str_encip;
		public bool IsEqualTo(ref Uencip cmp) { return (m_str_encip == cmp.m_str_encip); }
		public bool IsEqualTo(string cmp) { return (m_str_encip == cmp); }
	}

	struct Uid
	{
		public Uid(string str_uid) { m_str_uid = str_uid; }
		public string m_str_uid;
		public bool IsEqualTo(ref Uid cmp) { return (m_str_uid == cmp.m_str_uid); }
	}

	///////////////////////////////////////////////////////////////////////////////////////

	ref struct JSON_Reader
	{
		Utf8JsonReader m_reader;

		public JSON_Reader(byte[] buf_utf8)
		{
			var options = new JsonReaderOptions
			{
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip
			};
			m_reader = new Utf8JsonReader(buf_utf8, options);
		}

		// -----------------------------------------
		public JsonTokenType GetNextType()
		{
			if (m_reader.Read() == false)
			{ throw new Exception("!!! JSON のパースに失敗しました。"); }

			return m_reader.TokenType;
		}

		// -----------------------------------------
		public string GetString()
		{
			return m_reader.GetString();
		}
	
		// -----------------------------------------
		public ulong GetUInt64()
		{
			return m_reader.GetUInt64();
		}

		// -----------------------------------------
		public void Search_users_key()
		{
			while (true)
			{
				if (m_reader.Read() == false)
				{ throw new Exception("!!! JSON のパースに失敗しました。"); }

				if (m_reader.TokenType != JsonTokenType.PropertyName) { continue; }
				if (m_reader.GetString() == "users") { return; }
			}
		}

		// -----------------------------------------
		public void Search_talks_key()
		{
			while (true)
			{
				if (m_reader.Read() == false)
				{ throw new Exception("!!! JSON のパースに失敗しました。"); }

				if (m_reader.TokenType != JsonTokenType.PropertyName) { continue; }
				if (m_reader.GetString() == "talks")
				{
					if (this.GetNextType() != JsonTokenType.StartArray)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					return;
				}
			}
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////
	// 現時点で部屋にいるユーザを管理するクラス（uid を軸として記録する）

	class UInfo
	{
		public UInfo(ref Uid uid, string uname, ref Uencip encip)
		{
			m_uid = uid;
			m_uname = uname;
			m_encip = encip;
			mb_multi = false;
		}

		public Uid m_uid;
		public string m_uname;
		public Uencip m_encip;
		public bool mb_multi;
	}

	// ------------------------------------------------------------------------------------

	static class UInfo_onRoom
	{
		static List<UInfo> msa_uinfo =  new List<UInfo>();
		static List<bool> msab_attend = new List<bool>();  // 情報更新にのみ利用

		// ---------------------------------------------------
		public static void Clear_AttendFlag()
		{
			for (int idx = msab_attend.Count; --idx >= 0; ) { msab_attend[idx] = false; }
		}

		// ---------------------------------------------------
		public static void Attend(ref Uid uid, string uname, ref Uencip encip)
		{
			int idx_uid = -1;
			for (int idx = msa_uinfo.Count; --idx >= 0; )
			{
				if (msa_uinfo[idx].m_uid.IsEqualTo(ref uid) == true) { idx_uid = idx;  break; }
			}

			if (idx_uid < 0)
			{
				msa_uinfo.Add(new UInfo(ref uid, uname, ref encip));
				msab_attend.Add(true);
			}
			else
			{
				// 念のための確認
				if (msa_uinfo[idx_uid].m_uname != uname)
				{ throw new Exception("!!! 未知の不具合を検出　「msa_uinfo[idx_uid].m_uname != uname」"); }

				if (msa_uinfo[idx_uid].m_encip.IsEqualTo(ref encip) == false)
				{ throw new Exception("!!! 未知の不具合を検出　「msa_uinfo[idx_uid].m_encip.IsEqualTo(ref encip) == false」"); }

				msab_attend[idx_uid] = true;
			}
		}

		// ---------------------------------------------------
		public static void Remove_Absent()
		{
			// idx による削除は後方から行う必要がある
			for (int idx = msab_attend.Count; --idx >= 0; )
			{
				if (msab_attend[idx] == false)
				{
					msa_uinfo.RemoveAt(idx);
					msab_attend.RemoveAt(idx);
				}
			}

			// 多窓チェック
			int idx_tmnt = msa_uinfo.Count;
			for (int idx = idx_tmnt; --idx >= 0; ) { msa_uinfo[idx].mb_multi = false; }

			if (idx_tmnt <= 1) { return; }

			for (int src = 0; src < idx_tmnt - 1; src++)
			{
				if (msa_uinfo[src].mb_multi == true) { continue; }

				ref Uencip src_encip = ref msa_uinfo[src].m_encip;
				for (int cmp = src + 1; cmp < idx_tmnt; cmp++)
				{
					if (src_encip.IsEqualTo(ref msa_uinfo[cmp].m_encip) == true)
					{
						msa_uinfo[src].mb_multi = true;
						msa_uinfo[cmp].mb_multi = true;
					}
				}
			}
		}

		// ---------------------------------------------------
		public static bool IsMultiUser(string str_eip)
		{
			int idx_uinfo = msa_uinfo.Count;
			while (--idx_uinfo >= 0)
			{
				if (msa_uinfo[idx_uinfo].m_encip.IsEqualTo(str_eip) == true) { break; }
			}

			// str_eip がホストの場合の処理
			if (idx_uinfo == -1) { return false; }

			return msa_uinfo[idx_uinfo].mb_multi;
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////
	// メッセージ id のみを管理するクラス

	static class MsgID_Chkr
	{
		static List<string> msa_msgid = new List<string>();
		static List<bool> msab_flag_on_talks = new List<bool>();

		// ---------------------------------------------------
		public static void Clear_OnTalksFlag()
		{
			for (int idx = msab_flag_on_talks.Count; --idx >= 0; ) { msab_flag_on_talks[idx] &= false; }
		}

		// ---------------------------------------------------
		// 戻り値： ユーザへの注意喚起メッセージを表示した方が良いかどうか
		public static bool IsNew_MsgID(string msgid)
		{
			int idx = msa_msgid.IndexOf(msgid);
			if (idx < 0)
			{
				msa_msgid.Add(msgid);
				msab_flag_on_talks.Add(true);
				return true;
			}

			msab_flag_on_talks[idx] = true;
			return false;
		}

		// ---------------------------------------------------
		public static void Remove_disappeared()
		{
			// idx による削除は後方から行う必要がある
			for (int idx = msab_flag_on_talks.Count; --idx >= 0; )
			{
				if (msab_flag_on_talks[idx] == false)
				{
					msa_msgid.RemoveAt(idx);
					msab_flag_on_talks.RemoveAt(idx);
				}
			}
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////
	// on talks の退室者情報を保持するクラス（encip を軸として記録する）（退室者の BAN 指定にも利用）

	static class ExitEip_onTalks
	{
		enum ExitUsr_Stt : byte { EN_OnTalksFlag = 1, EN_Banned = 2 }

		static List<string> msa_encip_on_talks = new List<string>();
		static List<List<string>> msa_unames_on_talks = new List<List<string>>();  // talk 上の unames
		static List<ExitUsr_Stt> msa_flags = new List<ExitUsr_Stt>();

		// ---------------------------------------------------
		public static void Clear_OnTalksFlag()
		{
			for (int idx = msa_flags.Count; --idx >= 0; ) { msa_flags[idx] &= ~ExitUsr_Stt.EN_OnTalksFlag; }
		}

		// ---------------------------------------------------
		// 戻り値： ユーザ注意喚起メッセージを出力する際に利用可能な eidx
		public static int Regist(string encip, string uname)
		{
			int eidx = msa_encip_on_talks.IndexOf(encip);
			if (eidx < 0)
			{
				msa_encip_on_talks.Add(encip);
				msa_unames_on_talks.Add(DB_cur.NewStringList(uname));
				msa_flags.Add(ExitUsr_Stt.EN_OnTalksFlag);

				return msa_encip_on_talks.Count - 1;
			}
			else
			{
				int name_idx = msa_unames_on_talks[eidx].IndexOf(uname);
				if (name_idx < 0)
				{
					msa_unames_on_talks[eidx].Add(uname);
				}
				msa_flags[eidx] |= ExitUsr_Stt.EN_OnTalksFlag;

				return eidx;
			}
		}

		// ---------------------------------------------------
		public static void Remove_disappeared()
		{
			// idx による削除は後方から行う必要がある
			for (int idx = msa_flags.Count; --idx >= 0; )
			{
				if ((msa_flags[idx] & ExitUsr_Stt.EN_OnTalksFlag) == 0)
				{
					msa_encip_on_talks.RemoveAt(idx);
					msa_unames_on_talks.RemoveAt(idx);
					msa_flags.RemoveAt(idx);
				}
			}
		}

		// ---------------------------------------------------
		public static List<string> GetUnames(int eidx)
		{
			return msa_unames_on_talks[eidx];
		}
	}
	
	///////////////////////////////////////////////////////////////////////////////////////
	// 現セッションの部屋情報を管理するクラス

	static class DB_cur
	{
		static StringBuilder ms_ret_sb = new StringBuilder(256);  // 初期値は後ほど調整、、、

		static List<string> msa_eip = new List<string>();
		static List<List<string>> msa_unames =  new List<List<string>>();
		static List<bool> msab_new_user = new List<bool>();

		static ulong ms_time_for_msgid = 0;

		public static StringBuilder Set_RoomJSON(byte[] buf_utf8)
		{
			// ------------------------------------------------------------
			// users の解析
			var json_reader = new JSON_Reader(buf_utf8);
			json_reader.Search_users_key();

			UInfo_onRoom.Clear_AttendFlag();  // Remove_Absent() の準備

			JsonTokenType type_lead = json_reader.GetNextType();
			if (type_lead == JsonTokenType.StartObject)
			{
				// users がオブジェクト型の場合の処理
				while (true)
				{
					JsonTokenType next_type = json_reader.GetNextType();
					if (next_type == JsonTokenType.EndObject) { break; }

					if (next_type != JsonTokenType.PropertyName)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					if (json_reader.GetNextType() != JsonTokenType.StartObject)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					Chk_NextUser(ref json_reader);
				}
			}
			else
			{
				if (type_lead != JsonTokenType.StartArray)
				{ throw new Exception("!!! JSON のパースに失敗しました。"); }

				// users が配列型の場合の処理
				while (true)
				{
					JsonTokenType next_type = json_reader.GetNextType();
					if (next_type == JsonTokenType.EndArray) { break; }

					if (next_type != JsonTokenType.StartObject)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					Chk_NextUser(ref json_reader);
				}
			}

			UInfo_onRoom.Remove_Absent();  // ここで多窓のチェックも行う

			// ------------------------------------------------------------
			// talks の解析
			json_reader.Search_talks_key();  // JsonTokenType.StartArray の位置に設定される

			// Remove_disappeared() のための準備
			MsgID_Chkr.Clear_OnTalksFlag();
			ExitEip_onTalks.Clear_OnTalksFlag();

			ms_ret_sb.Clear();  // 入室、退室の情報記録用

			while (true)
			{
				JsonTokenType next_type = json_reader.GetNextType();
				if (next_type == JsonTokenType.EndArray) { break; }

				if (next_type != JsonTokenType.StartObject)
				{ throw new Exception("!!! JSON のパースに失敗しました。"); }

				Chk_NextTalk(ref json_reader);
			}

			MsgID_Chkr.Remove_disappeared();
			ExitEip_onTalks.Remove_disappeared();

			return ms_ret_sb;
		}

		// ------------------------------------------------------------------------------------
		// ポジションを JsonTokenType.StartObject にした状態でコールすること
		// JsonTokenType.EndObject のポジションでリターンされる
		static void Chk_NextUser(ref JSON_Reader json_reader)
		{
			var uencip = new Uencip();
			var uid = new Uid();
			string str_uname = null;

			// user 情報の取得
			while (true)
			{
				JsonTokenType next_type = json_reader.GetNextType();
				if (next_type == JsonTokenType.PropertyName)
				{
					// key の取得
					string str_key = json_reader.GetString();

					// value の読み込み
					if (json_reader.GetNextType() != JsonTokenType.String) { continue; }
					string str_value = json_reader.GetString();

					if (str_key == "name") { str_uname = str_value; }
					else if (str_key == "id") { uid.m_str_uid = str_value; }
					else if (str_key == "encip") { uencip.m_str_encip = str_value; }

					continue;
				}
				else if (next_type == JsonTokenType.EndObject)
				{
					if (str_uname == null || uid.m_str_uid == null)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					// ホストの場合、str_encip == null となる
					if (uencip.m_str_encip == null) { return; }
					break;
				}

				throw new Exception("!!! JSON のパースに失敗しました。");
			}

			// encip の調査
			int eidx = msa_eip.IndexOf(uencip.m_str_encip);
			if (eidx < 0)
			{
				// 現セッションで新規登録の eip
				msa_eip.Add(uencip.m_str_encip);
				msa_unames.Add(NewStringList(str_uname));
				msab_new_user.Add(true);
			}
			else
			{
				// 現セッションで登録済みの eip
				// 新規の uname であれば、記録しておく
				var unames = msa_unames[eidx];
				if (unames.IndexOf(str_uname) < 0)
				{
					unames.Add(str_uname);
				}
			}

			UInfo_onRoom.Attend(ref uid, str_uname, ref uencip);
		}

		public static List<string> NewStringList(string str)
		{
			var ret_obj = new List<string>();
			ret_obj.Add(str);
			return ret_obj;
		}

		// ------------------------------------------------------------------------------------
		// ポジションを JsonTokenType.StartObject にした状態でコールすること
		// JsonTokenType.EndObject のポジションでリターンされる
		enum MsgType : byte { EN_None = 0, EN_Enter, EN_Exit }

		static void Chk_NextTalk(ref JSON_Reader json_reader)
		{
			string str_eip = null;
			string str_uname = null;
			string str_msgid = null;
			ulong val_time = 0;
			MsgType msg_type = MsgType.EN_None;

			while (true)
			{
				JsonTokenType next_type = json_reader.GetNextType();
				if (next_type == JsonTokenType.PropertyName)
				{
					// key の取得
					string str_key = json_reader.GetString();

					// value の読み込み
					if (str_key == "time")
					{
						if (json_reader.GetNextType() != JsonTokenType.Number)
						{ throw new Exception("!!! JSON のパースに失敗しました。"); }

						val_time = json_reader.GetUInt64();
						continue;
					}

					if (json_reader.GetNextType() != JsonTokenType.String) { continue; }
					string str_value = json_reader.GetString();

					if (str_key == "encip") { str_eip = str_value; }
					else if (str_key == "name") { str_uname = str_value; }
					else if (str_key == "id" ) { str_msgid = str_value; }
					else if (str_key == "type")
					{
						if (str_value == "enter") { msg_type = MsgType.EN_Enter; }
						else if (str_value == "exit") { msg_type = MsgType.EN_Exit; }
					}

					continue;
				}
				else if (next_type == JsonTokenType.EndObject) { break; }

				throw new Exception("!!! JSON のパースに失敗しました。");
			}

			if (msg_type == MsgType.EN_None) { return; }

			if (str_eip == null || str_uname == null || str_msgid == null || val_time == 0)
			{ throw new Exception("!!! JSON のパースに失敗しました。"); }

			int eidx = msa_eip.IndexOf(str_eip);
			List<string> unames_this_session = null;
			bool b_new_user = false;

			if (eidx < 0)
			{
				// bot がホストであれば、ここには来ないはず？（bot がゲストのときも考慮）
				msa_eip.Add(str_eip);
				unames_this_session = NewStringList(str_uname);
				msa_unames.Add(unames_this_session);
				b_new_user = true;
				msab_new_user.Add(true);

				eidx = msa_eip.Count - 1;
			}
			else
			{
				unames_this_session = msa_unames[eidx];
				if (unames_this_session.IndexOf(str_uname) < 0) { unames_this_session.Add(str_uname); }

				b_new_user = msab_new_user[eidx];
			}

			if (msg_type == MsgType.EN_Enter)
			{
				if (val_time < ms_time_for_msgid) { return; }
				if (MsgID_Chkr.IsNew_MsgID(str_msgid) == false) { return; }

				// Enter メッセージの処理（再入室ユーザ、多窓ユーザの処理を行う）
				if (b_new_user)
				{
					ms_ret_sb.Append("・新規検出");
					msab_new_user[eidx] = false;
				}
				else
				{
					ms_ret_sb.Append("・再入室者");
				}

				if (UInfo_onRoom.IsMultiUser(str_eip))
				{
					ms_ret_sb.Append(" ★★ 多窓ユーザ ★★");
				}

				ms_ret_sb.Append($" [{str_uname}] / [{string.Join(", ", unames_this_session)}]\r\n{str_eip}\r\n\r\n");
			}
			else
			{
				// Exit メッセージの処理（Exit であるため、多窓ユーザかどうかの判定はしない）
				// ExitEip_onTalks のチェックは、ms_time_for_msgid に関わらず必要なことに注意
				int eidx_EUser = ExitEip_onTalks.Regist(str_eip, str_uname);

				if (val_time < ms_time_for_msgid) { return; }
				if (MsgID_Chkr.IsNew_MsgID(str_msgid) == false) { return; }

				ms_ret_sb.Append($"・退室者 [{string.Join(", ", ExitEip_onTalks.GetUnames(eidx_EUser))}] / "
						+ $"[{string.Join(", ", unames_this_session)}]\r\n"
						+ $"{str_eip}\r\n\r\n");
			}

			ms_time_for_msgid = val_time;
		}
	}
}

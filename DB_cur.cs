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
		Utf8JsonReader ms_reader;

		public JSON_Reader(byte[] buf_utf8)
		{
			var options = new JsonReaderOptions
			{
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip
			};
			ms_reader = new Utf8JsonReader(buf_utf8, options);
		}

		public JsonTokenType GetNextType()
		{
			if (ms_reader.Read() == false)
			{ throw new Exception("!!! JSON のパースに失敗しました。"); }

			return ms_reader.TokenType;
		}

		public string GetString()
		{
			return ms_reader.GetString();
		}
	
		public void Search_users_key()
		{
			while (true)
			{
				if (ms_reader.Read() == false)
				{ throw new Exception("!!! JSON のパースに失敗しました。"); }

				if (ms_reader.TokenType != JsonTokenType.PropertyName) { continue; }
				if (ms_reader.GetString() == "users") { return; }
			}
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////
	// 現時点で部屋にいるユーザを管理するクラス（BAN に利用）

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

	///////////////////////////////////////////////////////////////////////////////////////

	static class UInfo_onRoom
	{
		static List<UInfo> msa_uinfo =  new List<UInfo>();
		static List<bool> msab_attend = new List<bool>();  // 情報更新にのみ利用

		public static void Clear_AttendFlag()
		{
			for (int idx = msab_attend.Count; --idx >= 0; ) { msab_attend[idx] = false; }
		}

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
	}

	///////////////////////////////////////////////////////////////////////////////////////
	// 現セッションの部屋情報を管理するクラス

	static class DB_cur
	{
		static StringBuilder ms_ret_sb = new StringBuilder(256);  // 初期値は後ほど調整、、、
		enum UStt : uint { EN_ON = 1, EN_OFF = 2, EN_STAY = 3, EN_ReENT = 4, EN_NEW = 5, EN_Mask_InOut = 0xf
								, EN_MULTI = 16, EN_MULTI_Warned = 32 }

		static List<string> msa_eip = new List<string>();
		static List<List<string>> msa_unames =  new List<List<string>>();
		static List<UStt> msa_ustt = new List<UStt>();

		public static StringBuilder Set_RoomJSON(byte[] buf_utf8)
		{
			var json_reader = new JSON_Reader(buf_utf8);

			// フラグのクリア
			UInfo_onRoom.Clear_AttendFlag();

			// ------------------------------------------------------------
			// users の解析
			json_reader.Search_users_key();

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

			// ------------------------------------------------------------
			// talks 以外の情報をまとめる
			UInfo_onRoom.Remove_Absent();  // ここで多窓のチェックも行う

			ms_ret_sb.Clear();
			for (int idx = msa_ustt.Count; --idx >= 0; )
			{
				bool b_notify = false;  // 報告メッセージを返すかどうかのフラグ

				UStt ustt_old = msa_ustt[idx];  // ustt の一時保存
				switch (ustt_old & UStt.EN_Mask_InOut)
				{
				case UStt.EN_ON:
					msa_ustt[idx] = UStt.EN_OFF;
					break;

				case UStt.EN_STAY:
					msa_ustt[idx] = UStt.EN_ON;
					break;

				case UStt.EN_ReENT:
					b_notify = true;
					ms_ret_sb.Append("・再入室");
					msa_ustt[idx] = UStt.EN_ON;
					break;

				case UStt.EN_NEW:
					b_notify = true;
					ms_ret_sb.Append("・新規入室");
					msa_ustt[idx] = UStt.EN_ON;
					break;
				}

				if ((ustt_old & UStt.EN_MULTI) == UStt.EN_MULTI)
				{
					// まだ未警告であれば、警告を行う
					if ((ustt_old & UStt.EN_MULTI_Warned) == 0)
					{
						b_notify = true;
						ms_ret_sb.Append("・多窓ユーザ");
						msa_ustt[idx] |= UStt.EN_MULTI_Warned;
					}
					msa_ustt[idx] = UStt.EN_ON | UStt.EN_MULTI | UStt.EN_MULTI_Warned;
				}

				if (b_notify)
				{
					ms_ret_sb.Append(" [");
					ms_ret_sb.Append(string.Join(", ", msa_unames[idx]));
					ms_ret_sb.Append($"]\r\n{msa_eip[idx]}\r\n\r\n");
				}
			}

			// ------------------------------------------------------------
			// talks の解析

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
				// 新規登録の eip
				msa_eip.Add(uencip.m_str_encip);
				msa_unames.Add(NewListItem(str_uname));
				msa_ustt.Add(UStt.EN_NEW);
			}
			else
			{
				// 現セッションで登録済みの eip
				UStt ustt_multi_warned = msa_ustt[eidx] & UStt.EN_MULTI_Warned;
				switch (msa_ustt[eidx] & UStt.EN_Mask_InOut)
				{
				case UStt.EN_ON:
					msa_ustt[eidx] = UStt.EN_STAY | ustt_multi_warned;
					break;

				case UStt.EN_OFF:
					msa_ustt[eidx] = UStt.EN_ReENT;
					break;
				
				default:
					msa_ustt[eidx] |= UStt.EN_MULTI | ustt_multi_warned;
					break;
				}

				// 新規の uname であれば、記録しておく（多窓警告済みのフラグも下ろしておく）
				var unames = msa_unames[eidx];
				if (unames.IndexOf(str_uname) < 0)
				{
					unames.Add(str_uname);
					msa_ustt[eidx] &= ~UStt.EN_MULTI_Warned;
				}
			}

			UInfo_onRoom.Attend(ref uid, str_uname, ref uencip);
		}

		static List<string> NewListItem(string str)
		{
			var ret_obj = new List<string>();
			ret_obj.Add(str);
			return ret_obj;
		}
	}
}
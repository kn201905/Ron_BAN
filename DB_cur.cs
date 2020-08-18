using System;
using System.Text.Json;
using System.Collections.Generic;

namespace Ron_BAN
{
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

		public bool GetNext_Check_users_key()
		{
			if (ms_reader.Read() == false)
			{ throw new Exception("!!! JSON のパースに失敗しました。"); }

			if (ms_reader.TokenType != JsonTokenType.PropertyName) { return false; }

			// ms_reader.TokenType == JsonTokenType.PropertyName の場合
			return (ms_reader.GetString() == "users");
		}

		public string GetString()
		{
			return ms_reader.GetString();
		}
	}

	///////////////////////////////////////////////////////////////////////////////////////

	class UInfo_cur
	{
		public string m_encip = null;
		public string m_uid = null;   // BAN する際に必要となる
		public string m_uname = null;

		public override string ToString()
		{
			return $"encip={m_encip}, uid={m_uid}, name={m_uname}";
		}
	}
	
	///////////////////////////////////////////////////////////////////////////////////////

	static class DB_cur
	{
		public static void Set_RoomJSON(byte[] buf_utf8)
		{
			var json_reader = new JSON_Reader(buf_utf8);

			// users プロパティを探す
			while (true)
			{
				if (json_reader.GetNext_Check_users_key() == true) { break; }
			}

			List<UInfo_cur> uinfos_cur = new List<UInfo_cur>();

			JsonTokenType type_begin = json_reader.GetNextType();
			if (type_begin == JsonTokenType.StartObject)
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
					
					var uinfo_cur = Get_UInfo_cur_by_users(ref json_reader);
					if (uinfo_cur == null) { continue; }  // Host の処理
					uinfos_cur.Add(uinfo_cur);
				}
			}
			else
			{
				if (type_begin != JsonTokenType.StartArray)
				{ throw new Exception("!!! JSON のパースに失敗しました。"); }

				// users が配列型の場合の処理
				while (true)
				{
					JsonTokenType next_type = json_reader.GetNextType();
					if (next_type == JsonTokenType.EndArray) { break; }

					if (next_type != JsonTokenType.StartObject)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					var uinfo_cur = Get_UInfo_cur_by_users(ref json_reader);
					if (uinfo_cur == null) { continue; }  // Host の処理
					uinfos_cur.Add(uinfo_cur);
				}
			}
			
			foreach (UInfo_cur uinfo in uinfos_cur)
			{
				Program.WriteStBox(uinfo.ToString() + "\r\n");
			}
		}

		// ------------------------------------------------------------------------------------
		// ポジションを JsonTokenType.StartObject にした状態でコールすること
		// JsonTokenType.EndObject のポジションでリターンされる
		static UInfo_cur Get_UInfo_cur_by_users(ref JSON_Reader json_reader)
		{
			string str_encip = null;
			string str_uid = null;
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
					else if (str_key == "id") { str_uid = str_value; }
					else if (str_key == "encip") { str_encip = str_value; }

					continue;
				}
				else if (next_type == JsonTokenType.EndObject)
				{
					if (str_uname == null || str_uid == null)
					{ throw new Exception("!!! JSON のパースに失敗しました。"); }

					// ホストの場合、str_encip == null となる
					if (str_encip == null) { return null; }

					var ret_uinfo = new UInfo_cur() {
							m_encip = str_encip, m_uid = str_uid, m_uname = str_uname };

					return ret_uinfo;
				}

				throw new Exception("!!! JSON のパースに失敗しました。");
			}
		}
	}
}

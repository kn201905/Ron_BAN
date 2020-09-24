using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Ron_BAN
{
	static class Drrr_Socket
	{
		const int SIZE_BUF_RECV_WND = 8192;          // 8 kbytes（ウィンドウサイズ）
		const int SIZE_MEM_STREAM_RECV = 200 * 1024;  // 200 kbytes
		static int ms_size_buf_send = 4096;          // 4 kbytes


		static TcpClient ms_TcpClient = null;
		static NetworkStream ms_ns = null;

		static Byte[] ms_buf_recv_wnd = new Byte[SIZE_BUF_RECV_WND];
		static MemoryStream ms_mem_stream_recv = new MemoryStream(SIZE_MEM_STREAM_RECV);
		static Byte[] ms_buf_send = null;

		static public void Init(string hostname, int port)
		{
			MainForm.WriteStatus("--- Drrr_Proxy.Init()\r\n");

			ms_buf_send = new byte[ms_size_buf_send];

			ms_TcpClient = new TcpClient(hostname, port);
		}

		static public void Dispose()
		{
			if (ms_ns != null) { ms_ns.Dispose(); }
			if (ms_TcpClient != null) { ms_TcpClient.Close(); }
		}

		static public string Get_index_html()
		{
			if (ms_TcpClient.Connected == false)
			{ throw new Exception("!!! ms_TcpClient.Connected == false on Drrr_Socket.Get_index_html()"); }

			ms_ns = ms_TcpClient.GetStream();

			string str_req = "GET http://drrrkari.com HTTP/1.1\r\n"
				+ "Host: drrrkari.com\r\n"
				+ "Connection: keep-alive\r\n"
				+ "\r\n";

			int bytes_send = Encoding.UTF8.GetByteCount(str_req);
			if (bytes_send > ms_size_buf_send)
			{
				ms_size_buf_send = bytes_send + 256;
				ms_buf_send = new byte[ms_size_buf_send];
			}
			int bytes_encoded = Encoding.UTF8.GetBytes(str_req, 0, str_req.Length, ms_buf_send, 0);

			if (bytes_send != bytes_encoded)
			{ throw new Exception("!!! bytes_send != bytes_encoded となりました。"); }

			MainForm.WriteStatus("--- 接続要求を始めます。\r\n");
			ms_ns.Write(ms_buf_send, 0, bytes_send);

			ms_mem_stream_recv.Position = 0;
			do
			{
				int bytes_read = ms_ns.Read(ms_buf_recv_wnd, 0, SIZE_BUF_RECV_WND);
				MainForm.WriteStatus($"+++ Receive: {bytes_read} bytes\r\n");
				ms_mem_stream_recv.Write(ms_buf_recv_wnd, 0, bytes_read);
			} while (ms_TcpClient.Available > 0);

			{
				int bytes = ms_ns.Read(ms_buf_recv_wnd, 0, SIZE_BUF_RECV_WND);
				MainForm.WriteStatus($"+++ last: {bytes} bytes\r\n");
			}

			MainForm.WriteStatus($"+++ Connected: {ms_TcpClient.Connected.ToString()}\r\n");

			ms_mem_stream_recv.Position = 0;
			var sr = new StreamReader(ms_mem_stream_recv, Encoding.UTF8);
			MainForm.WriteStatus(sr.ReadToEnd());

			return "--- OK\r\n";
		}
	}

	public class DrrrClient
	{
		const int SIZE_BUF_RECV_WND = 8192;          // 8 kbytes（ウィンドウサイズ）
		const int SIZE_MEM_STREAM_RECV = 80 * 1024;  // 80 kbytes
		int m_size_buf_send = 4096;          // 4 kbytes


		TcpClient m_TcpClient = null;
		NetworkStream m_ns_drrr = null;

		Byte[] m_buf_recv_wnd = new Byte[SIZE_BUF_RECV_WND];
		MemoryStream m_mem_stream_recv = new MemoryStream(SIZE_MEM_STREAM_RECV);
		Byte[] m_buf_send = null;

		public DrrrClient()
		{
			m_buf_send = new byte[m_size_buf_send];

			MainForm.WriteStatus("--- drrrkari.com 接続開始\r\n");
			m_TcpClient = new TcpClient("drrrkari.com", 80);
		}

		~DrrrClient()
		{
			if (m_ns_drrr != null) { m_ns_drrr.Dispose(); }
			if (m_TcpClient != null) { m_TcpClient.Close(); }
		}

		public string Get_index_html()
		{
			if (m_TcpClient.Connected == false)
			{ throw new Exception("!!! m_TcpClient.Connected == false on DrrrClient.GetHome_html()"); }

			m_ns_drrr = m_TcpClient.GetStream();

			string str_req = "GET http://drrrkari.com/ HTTP/1.1\r\n" + "Host: drrrkari.com\r\n"
									+ "Connection: keep-alive\r\n\r\n";

			int bytes_send = Encoding.UTF8.GetByteCount(str_req);
			if (bytes_send > m_size_buf_send) { m_buf_send = new byte[m_size_buf_send + 256]; }
			int bytes_wrtn = Encoding.UTF8.GetBytes(str_req, 0, str_req.Length, m_buf_send, 0);

			if (bytes_send != bytes_wrtn)
			{ throw new Exception("!!! bytes_send != bytes_wrtn となりました。"); }

			m_ns_drrr.Write(m_buf_send, 0, bytes_send);
			MainForm.WriteStatus("--- index.html の取得を要求しました。\r\n");

			m_mem_stream_recv.Position = 0;

			do
			{
				int bytes_read = m_ns_drrr.Read(m_buf_recv_wnd, 0, SIZE_BUF_RECV_WND);
				MainForm.WriteStatus($"+++ Receive: {bytes_read}\r\n");
				m_mem_stream_recv.Write(m_buf_recv_wnd, 0, bytes_read);
			}
			while (m_ns_drrr.DataAvailable);

			m_mem_stream_recv.Position = 0;
			var sr = new StreamReader(m_mem_stream_recv, Encoding.UTF8);
			return sr.ReadToEnd();
		}
	}
}

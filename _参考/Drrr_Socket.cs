using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Ron_BAN
{
	public class Drrr_Socket
	{
		const int SIZE_BUF_RECV_WND = 8192;          // 8 kbytes（ウィンドウサイズ）
		const int SIZE_MEM_STREAM_RECV = 200 * 1024;  // 200 kbytes
		int m_size_buf_send = 4096;          // 4 kbytes


		TcpClient m_TcpClient = null;
		NetworkStream m_ns_drrr = null;

		Byte[] m_buf_recv_wnd = new Byte[SIZE_BUF_RECV_WND];
		MemoryStream m_mem_stream_recv = new MemoryStream(SIZE_MEM_STREAM_RECV);
		Byte[] m_buf_send = null;

		public Drrr_Socket()
		{
			m_buf_send = new byte[m_size_buf_send];

			MainForm.WriteStatus("--- drrrkari.com 接続開始\r\n");
			m_TcpClient = new TcpClient("drrrkari.com", 80);
		}

		~Drrr_Socket()
		{
			if (m_ns_drrr != null) { m_ns_drrr.Dispose(); }
			if (m_TcpClient != null) { m_TcpClient.Close(); }
		}

		public string Get_index_html()
		{
			if (m_TcpClient.Connected == false)
			{ throw new Exception("!!! m_TcpClient.Connected == false on DrrrClient.GetHome_html()"); }

			m_ns_drrr = m_TcpClient.GetStream();

			string str_req = "GET http://drrrkari.com/ HTTP/1.1\r\n"
									+ "Host: drrrkari.com\r\n"
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

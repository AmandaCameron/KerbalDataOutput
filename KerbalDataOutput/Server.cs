using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SimpleJSON;
using System.Text;
using System.IO;

namespace KerbalDataOutput
{
	public class Server
	{
		public static int VERSION = 0;

		public delegate void Handler (Client cli);

		private TcpListener mSocket;
		private Dictionary<string, Handler> mHandlers = new Dictionary<string, Handler> ();
		// This is to keep the Client objects in memory. I don't know if
		// it's nessary at all, but I also think it'd be helpful to have a list
		// of pending clients.
		private List<Client> mProcessing = new List<Client> ();

		public Server ()
		{
			mSocket = new TcpListener (8080);

			
			mSocket.Start ();

			mSocket.BeginAcceptTcpClient (new AsyncCallback (OnAccept), mSocket);
		}
	
		public void Hook (string path, Handler new_handler)
		{
			mHandlers.Add (path, new_handler);
		}

		public void Stop ()
		{
			mSocket.Stop ();
		}

		// Internal API.

		private void ClientDone (Client c)
		{
			mProcessing.Remove (c);
		}

		public void Handle (Client cli)
		{
			foreach (var handler in mHandlers) {
				if (cli.Path.StartsWith (handler.Key)) {
					handler.Value(cli);
					if(cli.Done) {
						return;
					}
				}
			}

			cli.Error ("No handler for path \"" + cli.Path + "\"");
		}

		private void OnAccept (IAsyncResult r)
		{
			var sock = (TcpListener)r.AsyncState;
			var client = sock.EndAcceptTcpClient (r);

			sock.BeginAcceptTcpClient (new AsyncCallback (OnAccept), sock);

			mProcessing.Add (new Client (this, client));
		}

		public class Client
		{
			private static byte[] HEADERS = Encoding.ASCII.GetBytes (
				"HTTP/1.1 200 Success\r\n" +
					"Content-Encoding: ascii\r\n" +
					"Content-Type: text/json\r\n\r\n"
				);
			private Server mServer;
			private TcpClient mSocket;
			private NetworkStream mStream;
			private static int BUFF_SIZE = 4096;
			private byte[] mBuffer = new byte[BUFF_SIZE];
			//private StringBuilder mBuilder = new StringBuilder();

			private JSONNode mResponse;

			// Request information
			public string Method;
			public string Path;
			public bool Done;

			public Client (Server s, TcpClient cli)
			{
				Done = false;

				mServer = s;
				mSocket = cli;
				mStream = cli.GetStream ();

				mStream.BeginRead (mBuffer, 0, BUFF_SIZE,
					      new AsyncCallback (OnRead), null);
			}


			// High-level API.

			public void Success (JSONNode data)
			{
				JSONClass resp = new JSONClass ();

				resp ["version"].AsInt = VERSION;
				resp ["success"].AsBool = true;
				resp ["result"] = data;

				WriteResp (resp);
			}

			public void Error (string msg)
			{
				JSONClass resp = new JSONClass ();

				resp ["version"].AsInt = VERSION;
				resp ["success"].AsBool = false;
				resp ["error"] = msg;

				WriteResp(resp);
			}

			// Low Level API.

			public void WriteResp (JSONNode data)
			{
				Done = true;

				mResponse = data;

				mStream.BeginWrite (HEADERS, 0, HEADERS.Length,
					               new AsyncCallback (OnWrittenHeader), null); 
			}

			public void OnRead (IAsyncResult r)
			{
				var len = mStream.EndRead (r);

				// TODO: Make this handle longer payloads than BUFF_SIZE

				if (len > 0) {
					//mServer.Handle (this, Encoding.ASCII.GetString (mBuffer, 0, len));
					var br = new StringReader(Encoding.UTF8.GetString(mBuffer, 0, len));

					var method = br.ReadLine();

					Method = method.Split (' ')[0];
					Path = method.Split (' ')[1];

					// TODO: Read headers and shit.

					mServer.Handle (this);
				}
			}

			// Write Callbacks.

			public void OnWrittenHeader (IAsyncResult r)
			{
				mStream.EndWrite (r);

				var resp = Encoding.UTF8.GetBytes (mResponse.ToString (""));

				mStream.BeginWrite (resp, 0, resp.Length,
				                   new AsyncCallback (OnWrittenBody), null);

			}

			public void OnWrittenBody (IAsyncResult r)
			{
				mStream.EndWrite (r);

				mStream.Flush ();
				mSocket.Close ();

				mServer.ClientDone (this);
			}
		}
	}
}
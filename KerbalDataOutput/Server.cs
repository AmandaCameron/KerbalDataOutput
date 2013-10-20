using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SimpleJSON;
using System.Text;
using System.IO;

namespace KerbalDataOutput
{
	class Server
	{
		public delegate JSONNode Handler(string path);

		private TcpListener mSocket;
		private List<Handler> mHandlers = new List<Handler>();
		// This is to keep the Client objects in memory. I don't know if
		// it's nessary at all, but I also think it'd be helpful to have a list
		// of pending clients.
		private List<Client> mProcessing = new List<Client>();


		public Server()
		{
			mSocket = new TcpListener(8080);

			
			mSocket.Start();

			mSocket.BeginAcceptTcpClient(new AsyncCallback(OnAccept), mSocket);
		}
	
		public void Hook (Handler new_handler)
		{
			mHandlers.Add (new_handler);
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

		public void Handle (Client cli, string req)
		{
			var r = new StringReader(req);

			var head = r.ReadLine();
			var path = head.Split (' ') [1];

			foreach (var handler in mHandlers) {
				var data = handler (path);

				if (data != null) {
					var resp = new JSONClass();

					resp["version"].AsInt = 0;
					resp["success"].AsBool = true;
					resp["result"] = data;

					cli.WriteResp (resp);

					return;
				}
			}

			var err = new JSONClass();

			err["version"].AsInt = 0;
			err["success"].AsBool = false;
			err["error"] = "No handler for path given.";

			cli.WriteResp(err);
		}


		private void OnAccept (IAsyncResult r)
		{
			var sock = (TcpListener)r.AsyncState;
			var client = sock.EndAcceptTcpClient (r);

			sock.BeginAcceptTcpClient(new AsyncCallback(OnAccept), sock);

			mProcessing.Add(new Client(this, client));
		}

		public class Client {
			private static byte[] HEADERS = Encoding.ASCII.GetBytes(
				"HTTP/1.1 200 Success\r\n" +
				"Content-Encoding: ascii\r\n" +
				"Content-Type: text/json\r\n\r\n");
	

			private Server mServer;
			private TcpClient mSocket;
			private NetworkStream mStream;

			private static int BUFF_SIZE = 4096;

			private byte[] mBuffer = new byte[BUFF_SIZE];
			//private StringBuilder mBuilder = new StringBuilder();

			private JSONNode mResponse;

			public Client(Server s, TcpClient cli) {
				mServer = s;
				mSocket = cli;
				mStream = cli.GetStream();

				mStream.BeginRead(mBuffer, 0, BUFF_SIZE,
					      new AsyncCallback(OnRead), null);
			}


			// API.

			public void WriteResp (JSONNode data)
			{
				mResponse = data;

				mStream.BeginWrite(HEADERS, 0, HEADERS.Length,
					               new AsyncCallback(OnWrittenHeader), null); 
			}

			public void OnRead (IAsyncResult r)
			{
				var len = mStream.EndRead (r);

				// TODO: Make this handle longer payloads than BUFF_SIZE

				if (len > 0) {
					mServer.Handle (this, Encoding.ASCII.GetString (mBuffer, 0, len));
				}
			}

			// Write Callbacks.

			public void OnWrittenHeader (IAsyncResult r)
			{
				mStream.EndWrite (r);

				var resp = Encoding.UTF8.GetBytes (mResponse.ToString ());

				mStream.BeginWrite (resp, 0, resp.Length,
				                   new AsyncCallback (OnWrittenBody), null);

			}

			public void OnWrittenBody(IAsyncResult r) {
				mStream.EndWrite (r);

				mStream.Flush ();
				mSocket.Close ();

				mServer.ClientDone(this);
			}
		}
	}
}
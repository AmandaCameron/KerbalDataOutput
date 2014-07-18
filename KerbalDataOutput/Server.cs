using System;
using System.Collections.Generic;
using System.Net.Sockets;
using SimpleJSON;
using System.Text;
using System.IO;
using UnityEngine;

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
					try {
						handler.Value(cli);
						if(cli.Done) {
							return;
						}
					} catch(Exception e) {
						cli.Error ("Error Handling request: " + e.ToString());
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
			private static byte[] JSON_HEADERS = Encoding.ASCII.GetBytes (
				"HTTP/1.1 200 Success\r\n" +
				"Content-Encoding: ascii\r\n" +
				"Content-Type: text/json\r\n\r\n"
				);

			private static byte[] IMAGE_HEADERS = Encoding.ASCII.GetBytes (
				"HTTP/1.1 200 Success\r\n" +
				"Content-Type: image/png\r\n\r\n"
			);

			private Server mServer;
			private TcpClient mSocket;
			private NetworkStream mStream;
			private static int BUFF_SIZE = 4096;
			private byte[] mBuffer = new byte[BUFF_SIZE];
			//private StringBuilder mBuilder = new StringBuilder();

			private byte[] mResponse;

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


			public void Image(Texture2D image) {
				Done = true;

				mResponse = image.EncodeToPNG ();

				mStream.BeginWrite (IMAGE_HEADERS, 0, IMAGE_HEADERS.Length,
					new AsyncCallback (OnWrittenHeader), null); 
			}

			// High-level API.

			public void Success (JSONNode data)
			{
				JSONClass resp = new JSONClass ();

				resp ["version"].AsInt = VERSION;
				resp ["success"].AsBool = true;
				resp ["result"] = data;

				WriteJsonResp (resp);
			}

			public void Error (string msg)
			{
				JSONClass resp = new JSONClass ();

				resp ["version"].AsInt = VERSION;
				resp ["success"].AsBool = false;
				resp ["error"] = msg;

				WriteJsonResp(resp);
			}

			// Low Level API.

			public void WriteJsonResp (JSONNode data)
			{
				Done = true;

				mResponse = Encoding.UTF8.GetBytes (data.ToString (""));

				mStream.BeginWrite (JSON_HEADERS, 0, JSON_HEADERS.Length,
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

				mStream.BeginWrite (mResponse, 0, mResponse.Length,
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
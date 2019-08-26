// State object for receiving data from remote device.  
public class StateObject
{
	// Client socket.  
	public Socket workSocket = null;
	// Size of receive buffer.  
	public const int BufferSize = 1024 * 600;
	// Receive buffer.  
	public byte[] buffer = new byte[BufferSize];
	// Received data string.  
	public StringBuilder sb = new StringBuilder();
}

public class AsynchronousClient
{
	// The port number for the remote device.  
	private const int port = 11000;

	// ManualResetEvent instances signal completion.  
	private static ManualResetEvent connectDone =
		new ManualResetEvent(false);
	private static ManualResetEvent sendDone =
		new ManualResetEvent(false);

	// The response from the remote device.  
	private static String response = String.Empty;

	private static void StartClient()
	{
		// Connect to a remote device.  
		try
		{
			// Establish the remote endpoint for the socket.  
			// The name of the   
			// remote device is "host.contoso.com".  
			IPHostEntry ipHostInfo = Dns.GetHostEntry("DESKTOP-QVLHR6K");
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

			// Create a TCP/IP socket.  
			Socket client = new Socket(ipAddress.AddressFamily,
				SocketType.Stream, ProtocolType.Tcp);

			// Connect to the remote endpoint.  
			client.BeginConnect(remoteEP,
				new AsyncCallback(ConnectCallback), client);
			connectDone.WaitOne();

			// Send test data to the remote device.
			var fileBytes = File.ReadAllBytes(@"C:\Temp\Chunks\App1\foo.jpg");
			
			Send(client, fileBytes);
			sendDone.WaitOne();

			// Release the socket.  
			client.Shutdown(SocketShutdown.Both);
			client.Close();

		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}

	private static void ConnectCallback(IAsyncResult ar)
	{
		try
		{
			// Retrieve the socket from the state object.  
			Socket client = (Socket)ar.AsyncState;

			// Complete the connection.  
			client.EndConnect(ar);

			Console.WriteLine("Socket connected to {0}",
				client.RemoteEndPoint.ToString());

			// Signal that the connection has been made.  
			connectDone.Set();
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}

	private static void Send(Socket client, byte[] data)
	{
		// Begin sending the data to the remote device.  
		client.BeginSend(data, 0, data.Length, 0,
			new AsyncCallback(SendCallback), client);
	}

	private static void SendCallback(IAsyncResult ar)
	{
		try
		{
			// Retrieve the socket from the state object.  
			Socket client = (Socket)ar.AsyncState;

			// Complete sending the data to the remote device.  
			int bytesSent = client.EndSend(ar);
			Console.WriteLine("Sent {0} bytes to server.", bytesSent);

			// Signal that all bytes have been sent.  
			sendDone.Set();
		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
		}
	}

	public static int Main(String[] args)
	{
		StartClient();
		return 0;
	}
}
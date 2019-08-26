// State object for reading client data asynchronously  
public class StateObject
{
	// Client  socket.  
	public Socket workSocket = null;
	// Size of receive buffer.  
	public const int BufferSize = 1024 * 20;
	// Receive buffer.  
	public byte[] buffer = new byte[BufferSize];
	
	public MemoryStream stream = new MemoryStream();
}

public class AsynchronousSocketListener
{
	// Thread signal.  
	public static ManualResetEvent allDone = new ManualResetEvent(false);

	public static void StartListening()
	{
		// Establish the local endpoint for the socket.  
		// The DNS name of the computer  
		// running the listener is "host.contoso.com".  
		IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
		IPAddress ipAddress = ipHostInfo.AddressList[0];
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

		// Create a TCP/IP socket.  
		Socket listener = new Socket(ipAddress.AddressFamily,
			SocketType.Stream, ProtocolType.Tcp);

		// Bind the socket to the local endpoint and listen for incoming connections.  
		try
		{
			listener.Bind(localEndPoint);
			listener.Listen(100);

			while (true)
			{
				// Set the event to nonsignaled state.  
				allDone.Reset();

				// Start an asynchronous socket to listen for connections.  
				Console.WriteLine("Waiting for a connection...");
				listener.BeginAccept(
					new AsyncCallback(AcceptCallback),
					listener);

				// Wait until a connection is made before continuing.  
				allDone.WaitOne();
			}

		}
		catch (Exception e)
		{
			Console.WriteLine(e.ToString());
		}

		Console.WriteLine("\nPress ENTER to continue...");
		Console.Read();

	}

	public static void AcceptCallback(IAsyncResult ar)
	{
		// Signal the main thread to continue.  
		allDone.Set();

		// Get the socket that handles the client request.  
		Socket listener = (Socket)ar.AsyncState;
		Socket handler = listener.EndAccept(ar);

		// Create the state object.  
		StateObject state = new StateObject();
		state.workSocket = handler;
		handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
			new AsyncCallback(ReadCallback), state);
	}

	public static void ReadCallback(IAsyncResult ar)
	{
		// Retrieve the state object and the handler socket  
		// from the asynchronous state object.  
		StateObject state = (StateObject)ar.AsyncState;
		Socket handler = state.workSocket;

		// Read data from the client socket.   
		int bytesRead = handler.EndReceive(ar);

		if (bytesRead > 0)
		{
			// There  might be more data, so store the data received so far. 
			state.stream.Write(state.buffer, 0, state.buffer.Length);
			
			// number here will be the total bytes of the file -> from the rabbitMQ message
			if (state.stream.Length == 163840)
			{
				// All the data has been read from the   
				// client. Display it on the console.
				var fileBytes = state.stream.ToArray();
				File.WriteAllBytes(@"C:\Temp\Chunks\App2\foo.jpg", fileBytes);
				Console.WriteLine($"Read {fileBytes.Length} bytes from socket and wrote file ");
			}
			else
			{
				// Not all data received. Get more.  
				handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
				new AsyncCallback(ReadCallback), state);
			}
		}
	}

	public static int Main(String[] args)
	{
		StartListening();
		return 0;
	}
}
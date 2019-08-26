// State object for reading client data asynchronously  
public class StateObject
{
	// Client  socket.  
	public Socket workSocket = null;
	// Size of receive buffer.  
	public const int BufferSize = 1024 * 20;
	// Receive buffer.  
	public byte[] buffer = new byte[BufferSize];
	
	public byte[] currentBytes {get; set;}

	public int TotalBytesTransferred { get; set; }
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
				listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

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
		
		using(var stream = new FileStream(@"C:\Temp\Chunks\App2\foo.jpg", FileMode.Create, FileAccess.Write))
		{
			while (state.TotalBytesTransferred <= 163840)
			{
				handler.Receive(state.buffer, StateObject.BufferSize, SocketFlags.None);

				if (state.buffer.Length < 0)
					continue;
				
				stream.Write(state.buffer, 0, state.buffer.Length);
				state.TotalBytesTransferred += state.buffer.Length;
			}
		}
		
		Console.WriteLine($"Read {state.TotalBytesTransferred} bytes from socket and wrote file ");
	}

	public static int Main(String[] args)
	{
		StartListening();
		return 0;
	}
}
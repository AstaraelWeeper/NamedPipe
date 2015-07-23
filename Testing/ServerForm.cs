using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;


namespace Testing
{
    public partial class ServerForm : Form
    {
        string message;
        private static int numThreads = 1;
        static NamedPipeServerStream pipeServer;
        Thread serverThread;
        static StreamString ss;
        public ServerForm()
        {
            InitializeComponent();
            serverThread = new Thread(ServerThread);
            serverThread.Start();

        }
        private void button1_Click(System.Object sender, System.EventArgs e)
        {
            message = txtMessage.Text.ToString();
            ss.WriteString(message);
        }

        private static void ServerThread(object data)
        {
            pipeServer =
                new NamedPipeServerStream("testpipe", PipeDirection.InOut, numThreads);

            int threadId = Thread.CurrentThread.ManagedThreadId;

            // Wait for a client to connect
            pipeServer.WaitForConnection();

            Console.WriteLine("Client connected on thread[{0}].", threadId);
            try
            {
                // Read the request from the client. Once the client has 
                // written to the pipe its security token will be available.

                ss = new StreamString(pipeServer);

                // Verify our identity to the connected client using a 
                // string that the client anticipates.

                ss.WriteString("I am the one true server!");
                string filename = ss.ReadString();

                // Read in the contents of the file while impersonating the client.
                ReadFileToStream fileReader = new ReadFileToStream(ss, filename);

                // Display the name of the user we are impersonating.
                Console.WriteLine("Reading file: {0} on thread[{1}] as user: {2}.",
                    filename, threadId, pipeServer.GetImpersonationUserName());
                pipeServer.RunAsClient(fileReader.Start);
            }
            // Catch the IOException that is raised if the pipe is broken 
            // or disconnected. 
            catch (IOException e)
            {
                Console.WriteLine("ERROR: {0}", e.Message);
            }
            
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            pipeServer.Close();
        }
    }
}
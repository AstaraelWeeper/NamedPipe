using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO.Pipes;
using System.Security.Principal;

namespace PipeClient
{
    public partial class ClientForm : Form
    {
        static int numClients = 1;
        bool connectionVerified = false;
        string returnMessage;
        public ClientForm()
        {
            InitializeComponent();
            StartClients();
            Connection();
        }

        void Connection()
        {
            NamedPipeClientStream pipeClient =
                    new NamedPipeClientStream(".", "testpipe",
                        PipeDirection.InOut, PipeOptions.None,
                        TokenImpersonationLevel.Impersonation);

                Console.WriteLine("Connecting to server...\n");
                pipeClient.Connect();

                StreamString ss = new StreamString(pipeClient);
                // Validate the server's signature string 
                if (ss.ReadString() == "I am the one true server!")
                {
                    txtClient.Text += "\r\n" + "connected";
                    connectionVerified = true;
                    // The client security token is sent with the first write. 
                    // Send the name of the file whose contents are returned 
                    // by the server.
                    ss.WriteString("c:\\textfile.txt");

                    // Print the file to the screen.
                    Console.Write(ss.ReadString());
                }
                else if (connectionVerified == true)
                {
                    if (ss.ReadString() == "test")
                    {

                        returnMessage = "message to send back";
                        ss.WriteString(returnMessage);
                        txtClient.Text += "\r\n" + returnMessage + "sent";
                    }
                }
                else
                {
                    txtClient.Text += "\r\n" + "server not verified";
                }
        }

        // Helper function to create pipe client processes 
    private static void StartClients()
    {
        int i;
        string currentProcessName = Environment.CommandLine;
        Process[] plist = new Process[numClients];

        Console.WriteLine("Spawning client processes...\n");

        if (currentProcessName.Contains(Environment.CurrentDirectory))
        {
            currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
        }

        // Remove extra characters when launched from Visual Studio
        currentProcessName = currentProcessName.Replace("\\", String.Empty);
        currentProcessName = currentProcessName.Replace("\"", String.Empty);

        for (i = 0; i < numClients; i++)
        {
            // Start 'this' program but spawn a named pipe client.
            plist[i] = Process.Start(currentProcessName, "spawnclient");
        }
        while (i > 0)
        {
            for (int j = 0; j < numClients; j++)
            {
                if (plist[j] != null)
                {
                    if (plist[j].HasExited)
                    {
                        Console.WriteLine("Client process[{0}] has exited.",
                            plist[j].Id);
                        plist[j] = null;
                        i--;    // decrement the process watch count
                    }
                    else
                    {
                        Thread.Sleep(250);
                    }
                }
            }
        }
        Console.WriteLine("\nClient processes finished, exiting.");
    }

    private void btn_close_Click(object sender, EventArgs e)
    {

    }
}
    }


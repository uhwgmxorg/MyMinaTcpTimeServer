/******************************************************************************/
/*                                                                            */
/*   Program: MyMinaTcpTimeServer                                             */
/*   A Time Tcp MINA Multiconnection Server                                   */
/*                                                                            */
/*   30.01.2017 1.0.0.0 uhwgmxorg Start                                       */
/*                                                                            */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Mina.Core.Service;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Mina.Filter.Codec.TextLine;
using Mina.Filter.Logging;
using Mina.Transport.Socket;
using System.Threading;

namespace MyMinaTcpTimeServer
{
    class Program
    {

        // Object to lock the session list
        static private object _lockObject = new Object();

        static private int _counter;

        static private int Port { get; set; }
        static private List<IoSession> Sessions { get; set; }

        /// <summary>
        /// ScreenOutput
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="foregroundColor"></param>
        /// <param name="backgroundColor"></param>
        static void ScreenOutput(int x, int y, string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.SetCursorPosition(x, y);
            Console.WriteLine(text);
            Console.ResetColor();
        }
        static void ScreenOutput(int x, int y, string text, ConsoleColor foregroundColor)
        {
            ScreenOutput(x, y, text, foregroundColor, ConsoleColor.Black);
        }
        static void ScreenOutput(int x, int y, string text)
        {
            ScreenOutput(x, y, text, ConsoleColor.Gray);
        }
        static void ScreenOutput(string text, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        static void ScreenOutput(string text, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(text);
            Console.ResetColor();
        }
        static void ScreenOutput(string text)
        {
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /******************************/
        /*      Server Events         */
        /******************************/
        #region Server Events

        /// <summary>
        /// ExceptionCaught
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ExceptionCaught(object sender, IoSessionExceptionEventArgs e)
        {
            ScreenOutput(String.Format("Exception on {0}", e.Session.RemoteEndPoint));
            ScreenOutput(e.Exception.Message);
        }

        /// <summary>
        /// SessionIdle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SessionIdle(object sender, Mina.Core.Session.IoSessionIdleEventArgs e)
        {
            ScreenOutput("IDLE " + e.Session.GetIdleCount(e.IdleStatus));
        }

        /// <summary>
        /// SessionCreated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SessionCreated(object sender, Mina.Core.Session.IoSessionEventArgs e)
        {
            ScreenOutput("Connected to " + e.Session.RemoteEndPoint);
            lock (_lockObject)
            {
                Sessions.Add(e.Session);
            }
        }

        /// <summary>
        /// SessionClosed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SessionClosed(object sender, Mina.Core.Session.IoSessionEventArgs e)
        {
            ScreenOutput("Disconnected from " + e.Session.RemoteEndPoint);
            lock (_lockObject)
            {
                Sessions.Remove(e.Session);
            }
        }

        /// <summary>
        /// MessageReceived
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MessageReceived(object sender, Mina.Core.Session.IoSessionMessageEventArgs e)
        {
            String str = e.Message.ToString();

            // Display content if something comes as plain text
            ScreenOutput(String.Format("Received {0} from {1}", str, e.Session.RemoteEndPoint));
        }

        #endregion

        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string Input;

            string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ScreenOutput("Program MyMinaTcpTimeServer Version " + Version + "\n", ConsoleColor.Cyan);
            ScreenOutput("press [Ctrl] C to exit\n", ConsoleColor.Cyan);

            Port = 4711;
            ScreenOutput("We listen on Port " + Port.ToString());
            Sessions = new List<IoSession>();


            IoAcceptor acceptor = new AsyncSocketAcceptor();
            acceptor.FilterChain.AddLast("logger", new LoggingFilter());
            acceptor.FilterChain.AddLast("codec", new ProtocolCodecFilter(new TextLineCodecFactory(Encoding.UTF8)));

            // Set the handlers
            acceptor.ExceptionCaught += ExceptionCaught;
            acceptor.SessionIdle += SessionIdle;
            acceptor.SessionCreated += SessionCreated;
            acceptor.SessionClosed += SessionClosed;
            acceptor.MessageReceived += MessageReceived;


            acceptor.SessionConfig.ReadBufferSize = 2048;
            acceptor.SessionConfig.SetIdleTime(IdleStatus.BothIdle, 10);

            acceptor.Bind(new IPEndPoint(IPAddress.Any, Port));

            // Start the Send-Thread
            Thread SendThread = new Thread(
            () =>
            {
                Thread.CurrentThread.Name = "Server Send Thread";
                while (true)
                {
                    lock (_lockObject)
                    {
                        foreach (var s in Sessions)
                        {
                            string SendString = String.Format("Time: {0}", DateTime.Now.ToString());
                            s.Write(SendString);
                            ScreenOutput(String.Format("Send Time {0} to {1}", DateTime.Now.ToString(), s.RemoteEndPoint));
                        }
                        if(Sessions.Count == 0)
                            ScreenOutput(String.Format("No connection, nothing to do {0} ...",++_counter));
                    }
                    Thread.Sleep(1000);
                }
            });
            SendThread.Start();


            Input = Console.ReadLine();
        }
    }
}

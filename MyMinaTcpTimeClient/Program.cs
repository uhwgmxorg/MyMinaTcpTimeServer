using Mina.Core.Future;
using Mina.Core.Service;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Mina.Filter.Codec.TextLine;
using Mina.Filter.Logging;
using Mina.Transport.Socket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyMinaTcpTimeClient
{
    class Program
    {

        static string IpAddress { get; set; }
        static int Port { get; set; }

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
        
        /// <summary>
        /// Main
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string Input;

            string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ScreenOutput("Program MyMinaTcpTimeClient Version " + Version, ConsoleColor.Green);
            ScreenOutput("use MyMinaTcpTimeClient 192.168.0.222 4712 for another server",ConsoleColor.White);
            ScreenOutput("default is 127.0.0.1:4711\n", ConsoleColor.White);
            ScreenOutput("press [Ctrl] C to exit\n", ConsoleColor.Green);

            IpAddress = "127.0.0.1";
            Port = 4711;

            if (args.Length >= 1)
                IpAddress = args[0];
            if (args.Length >= 2)
                Port = Convert.ToInt32(args[1]);

            ScreenOutput(String.Format("We connect to {0}:{1}",IpAddress,Port.ToString()));

            IoConnector acceptor = new AsyncSocketConnector();
            acceptor.FilterChain.AddLast("logger", new LoggingFilter());
            acceptor.FilterChain.AddLast("codec", new ProtocolCodecFilter(new TextLineCodecFactory(Encoding.UTF8)));

            // Set the handlers
            acceptor.ExceptionCaught += (s, e) => ScreenOutput(e.Exception.Message);
            acceptor.SessionIdle += (s, e) => ScreenOutput("IDLE ");
            acceptor.SessionCreated += (s, e) => ScreenOutput("Connected to " + e.Session.RemoteEndPoint);
            acceptor.SessionClosed += (s, e) => ScreenOutput("Close connected to " + e.Session.RemoteEndPoint);
            acceptor.MessageReceived += (s, e) => ScreenOutput(e.Message.ToString());


            acceptor.SessionConfig.ReadBufferSize = 2048;
            acceptor.SessionConfig.SetIdleTime(IdleStatus.BothIdle, 10);

            IConnectFuture Future = acceptor.Connect(new IPEndPoint(IPAddress.Parse(IpAddress), Port));
            Future.Await();
            try
            {
                IoSession Session = Future.Session;
            }
            catch (Exception ex)
            {
                ScreenOutput(String.Format("{0}", ex.Message), ConsoleColor.Red);
            }

            Input = Console.ReadLine();
        }
    }
}

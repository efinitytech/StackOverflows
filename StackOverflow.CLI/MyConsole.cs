using System;
using System.CommandLine;

namespace StackOverflow
{
    internal class MyConsole
    {
        public static int Verbosity { get; set; } = 0;
        public static Option GetVerbosityOption() => new Option("-v")
        {
            Argument = new Argument<int>("Verbosity")
        };

        public static void Debug(string message)
        {
            if (Verbosity < 2)
            {
                return;
            }
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($@"DBUG   {message}");
            Console.ResetColor();
        }

        public static void Info(string message)
        {
            if (Verbosity < 1)
            {
                return;
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($@"INFO   {message}");
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            if (Verbosity < 0)
            {
                return;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($@"WARN   {message}");
            Console.ResetColor();
        }

        public static void Success(string message)
        {
            if (Verbosity < 0)
            {
                return;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($@"ERR!   {message}");
            Console.ResetColor();
        }
    }
}

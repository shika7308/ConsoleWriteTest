using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWriteTest
{
    class Program
    {
        const int NUM_OF_WRITE = 10000;
        static string buf = "";

        static void measure<T>(Func<T> cb)
        {
            var name = cb.Method.Name;
            var res = cb();
            buf += $"{name}: {res}\n";
        }

        static void Main(string[] args)
        {
            measure(WriteOnMainThread);
            measure(WriteOnSubThread);
            measure(WriteOnMainThreadWithCount);
            measure(WriteOnSubThreadWithCount);
            Console.Write(buf);
            Console.ReadKey();
        }

        static TimeSpan time(Action cb)
        {
            var start = (double)Stopwatch.GetTimestamp();
            cb();
            var end = (double)Stopwatch.GetTimestamp();
            return TimeSpan.FromSeconds((end - start) / (double)Stopwatch.Frequency);
        }

        static TimeSpan WriteOnMainThread()
        {
            return time(() =>
            {
                for (var i = 0; i < NUM_OF_WRITE; i++)
                    Console.Write('a');
                Console.WriteLine();
            });
        }

        static TimeSpan WriteOnSubThread()
        {
            return time(() =>
            {
                Task.Run(() =>
                {
                    for (var i = 0; i < NUM_OF_WRITE; i++)
                        Console.Write('b');
                    Console.WriteLine();
                }).Wait();
            });
        }

        static TimeSpan WriteOnBothThread()
        {
            var num_of_write = NUM_OF_WRITE / 2;
            return time(() =>
            {
                var task = Task.Run(() =>
                {
                    for (var i = 0; i < num_of_write; i++)
                    {
                        Console.Write('c');
                    }
                });
                for (var i = 0; i < num_of_write; i++)
                {
                    Console.Write('c');
                }
                task.Wait();
            });
        }

        static (TimeSpan, double) WriteOnMainThreadWithCount()
        {
            var cnt = 0UL;
            var t = time(() =>
            {
                var f = true;
                Task.Run(() =>
                {
                    while (f)
                        cnt++;
                });
                for (var i = 0; i < NUM_OF_WRITE; i++)
                    Console.Write('c');
                Console.WriteLine();
                f = false;
            });
            return (t, cnt / (double)NUM_OF_WRITE);
        }

        static (TimeSpan, double) WriteOnSubThreadWithCount()
        {
            var cnt = 0UL;
            var t = time(() =>
            {
                var f = true;
                Task.Run(() =>
                {
                    for (var i = 0; i < NUM_OF_WRITE; i++)
                        Console.Write('d');
                    Console.WriteLine();
                    f = false;
                });
                while (f)
                    cnt++;
            });
            return (t, cnt / (double)NUM_OF_WRITE);
        }
    }
}

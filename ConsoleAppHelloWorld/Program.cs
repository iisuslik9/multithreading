using System;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int coreCount = Environment.ProcessorCount;
        Console.WriteLine($"{coreCount} ядер в системе");

        Thread[] threads = new Thread[coreCount];
        for (int i = 0; i < coreCount; i++)
        {
            threads[i] = new Thread(Worker);
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    static void Worker()
    {
        int threadId = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine($"Здравствуй, мир. ID потока: {threadId}");
    }
}

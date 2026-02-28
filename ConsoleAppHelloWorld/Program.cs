using System;
using System.Threading;

class Program
{
    static void Main()
    {
        Thread myThread = new Thread(PrintHello);
        
        myThread.Start();

        Console.WriteLine("главный поток");

        myThread.Join();

    }

    static void PrintHello()
    {
        Console.WriteLine("фоновый поток: Здравствуй, мир");
    }
}

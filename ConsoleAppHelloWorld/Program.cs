using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int n;
        while (!int.TryParse(Console.ReadLine(), out n) || n <= 0)
        {
            Console.Write("введите целое положительное число: ");
        }
        int threadCount = Environment.ProcessorCount;


        var swSeq = Stopwatch.StartNew();
        BigInteger seqResult = SequentialFactorial(n);
        swSeq.Stop();
        Console.WriteLine($"Последовательный: за {swSeq.ElapsedMilliseconds} мс");
        //Console.WriteLine(seqResult);



        var swPar = Stopwatch.StartNew();
        BigInteger parResult = ParallelFactorial(n, threadCount);
        swPar.Stop();        
        Console.WriteLine($"Параллельный ({threadCount} потоков): за {swPar.ElapsedMilliseconds} мс");
        //Console.WriteLine(parResult);
        
    }

    static BigInteger SequentialFactorial(int n)
    {
        BigInteger result = 1;
        for (int i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    static BigInteger ParallelFactorial(int n, int threadCount)
    {
        var localResults = new ThreadLocal<BigInteger>(() => 1, trackAllValues: true); 

        Thread[] threads = new Thread[threadCount];
        int blockSize = n / threadCount;
        int remainder = n % threadCount;

        for (int t = 0; t < threadCount; t++)
        {
            int start = 2 + t * blockSize + (t < remainder ? t : remainder);
            int end = start + blockSize - 1 + (t < remainder ? 1 : 0);
            if (t == threadCount - 1) end = n; 

            int threadStart = start, threadEnd = end;
            threads[t] = new Thread(() =>
            {
                BigInteger local = 1;
                for (int i = threadStart; i <= threadEnd; i++)
                {
                    local *= i;
                }
                localResults.Value = local;


                //int id = Thread.CurrentThread.ManagedThreadId;
                //Console.WriteLine($"Поток {id}: от {threadStart} до {threadEnd}");
                //Console.WriteLine($"Поток {id}: от {threadStart} до {threadEnd}, промежуточный {local}");
            });
            threads[t].Start();
        }

        foreach (Thread t in threads)
            t.Join();

        
        BigInteger total = 1;
        foreach (BigInteger res in localResults.Values)
        {
            total *= res;
        }
        localResults.Dispose();
        return total;
    }
}

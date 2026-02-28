using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Numerics;

class Program
{
    static void Main(string[] args)
    {
        int n = 10000; 
        Console.WriteLine($"Вычисление факториала {n}:");

        // Последовательное 
        var swSeq = Stopwatch.StartNew();
        BigInteger seqResult = SequentialFactorial(n);
        swSeq.Stop();
        //Console.WriteLine($"Последовательно: {seqResult} за {swSeq.ElapsedMilliseconds} мс");
        Console.WriteLine($"Последовательно: за {swSeq.ElapsedMilliseconds} мс");

        // Параллельное 
        var swPar = Stopwatch.StartNew();
        BigInteger parResult = ParallelFactorial(n, Environment.ProcessorCount);
        swPar.Stop();
        //Console.WriteLine($"Параллельно ({Environment.ProcessorCount} потоков): {parResult} за {swPar.ElapsedMilliseconds} мс");
        Console.WriteLine($"Параллельно ({Environment.ProcessorCount} потоков): за {swPar.ElapsedMilliseconds} мс");
        Console.WriteLine($"Ускорение: {swSeq.ElapsedMilliseconds / (double)swPar.ElapsedMilliseconds:F1} раз");
        
        Console.WriteLine("Нажмите Enter для выхода...");
        Console.ReadLine();
    }
    

    static BigInteger SequentialFactorial(BigInteger n)
    {
        if (n <= 1) return 1;
        BigInteger result = 1;
        for (BigInteger i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }

    // каждый поток считает свой блок
    static BigInteger ParallelFactorial(BigInteger n, int threadCount)
    {
        ThreadLocal<BigInteger> localResults = new ThreadLocal<BigInteger>(() => 1L, trackAllValues: true);
        BigInteger blockSize = n / threadCount;
        Thread[] threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            BigInteger start = i * blockSize + 1;
            BigInteger end = (i == threadCount - 1) ? n : (i + 1) * blockSize;
            threads[i] = new Thread(() => ComputeBlock(start, end, localResults));
            threads[i].Start();
        }

        // Сбор результатов
        foreach (var thread in threads)
            thread.Join();

        BigInteger total = 1;
        foreach (var value in localResults.Values)
            total *= value;

        localResults.Dispose(); 
        return total;
    }

    static void ComputeBlock(BigInteger start, BigInteger end, ThreadLocal<BigInteger> results)
    {
        BigInteger localResult = 1;
        for (BigInteger i = start; i <= end; i++)
        {
            localResult *= i;
        }
        results.Value = localResult;
    }
}

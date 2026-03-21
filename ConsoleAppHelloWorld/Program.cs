using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        int num1 = 150000; //~10сек
        int num2 = 300000; //~60сек

        int k1 = Environment.ProcessorCount;
        int k2 = k1 / 2, k3 = 2 * k1;
        int[] threadCounts = {k1, k2, k3};

        Console.WriteLine(" num      k    T1(мс)   Tp(мс)  Ускорение(S)  Эффективность(E)  Стоимость(C)");
        //Сверхлинейное (superlinear) ускорение Sp(n)>p
        PrintMetrics(num1, threadCounts);
        PrintMetrics(num2, threadCounts);

        
        
    }
    static void PrintMetrics(int n, int[] ks)
    {
        double T1 = MeasureSeq(n); 

        foreach (int k in ks)
        {
            double Tp = MeasurePar(n, k);  

            double S = T1 / Tp;                    
            double E = S / k;                              
            double C = k * Tp;                         

            Console.WriteLine($"{n,7} {k,4:F0} {T1,9:F2} {Tp,9:F2} {S,12:F2} {E,15:F3} {C,12:F2}");
        }
        Console.WriteLine();
    }

     static double MeasureSeq(int n)
    {
        var swSeq = Stopwatch.StartNew();
        SequentialFactorial(n);
        swSeq.Stop();
        return swSeq.ElapsedMilliseconds;
    }

    static double MeasurePar(int n, int k)
    {
        var swPar = Stopwatch.StartNew();
        ParallelFactorial(n, k); 
        swPar.Stop();
        return swPar.ElapsedMilliseconds;
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

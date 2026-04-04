using System;
using System.Diagnostics;
//последовательный и параллельный алгоритмы для вычисления  числа Пи с использованием пула потоков. 
//Синхронизацию реализовать через CountdownEvent. Для реализованного параллельного алгоритма 
//вычислить ускорение, эффективность и стоимость.
//методом численного интегрирования 4/(1+x²)dx
//

class Program
{
    static void Main(string[] args)
    {

        int k = Environment.ProcessorCount;
        Console.WriteLine($"{k} потоков");

        long n = 1000000000; 

        double T1 = MeasureSeq(n);
        double Tp = MeasurePar(n, k);
        
        double S = T1 / Tp;
        double E = S / k;
        double C = k * Tp;
        
        Console.WriteLine($"n={n}");
        Console.WriteLine($"Последовательный T1 = {T1:F2}с");
        Console.WriteLine($"Параллельный    Tp = {Tp:F2}с");
        Console.WriteLine($"S={S:F2} E={E:F2} C={C:F2}");
        
    }

    
    static double MeasureSeq(long n)
    {
        var swSeq = Stopwatch.StartNew();
        SequentialPi(n);
        swSeq.Stop();
        return swSeq.Elapsed.TotalSeconds;
    }

    static double MeasurePar(long n, int k)
    {
        var swPar = Stopwatch.StartNew();
        ParallelPi(n, k); 
        swPar.Stop();
        return swPar.Elapsed.TotalSeconds;
    }

    static double SequentialPi(long n)
    {
        double h = 1.0 / n;
        double sum = 0.0;

        for (long i = 0; i < n; i++)
        {
            double x = (i + 0.5) * h;
            sum += 4.0 / (1.0 + x * x);
        }
        //Console.WriteLine($" seq pi {sum*h}");
        return sum * h;
    }
    
    class ThreadData
    {
        public int Index { get; set; }
        public long N { get; set; }
        public double H { get; set; }
        public double[]? PartialSums { get; set; }
        public CountdownEvent? Countdown { get; set; }
    }

    static double ParallelPi(long n, int threadCount)
    {
        double h = 1.0 / n;
        double[] partialSums = new double[threadCount];

        using (var countdownEvent = new CountdownEvent(threadCount)) 
        {
            for (int i = 0; i < threadCount; i++)
            {
                var data = new ThreadData
                {
                    Index = i,
                    N = n,
                    H = h,
                    PartialSums = partialSums,
                    Countdown = countdownEvent
                };
                ThreadPool.QueueUserWorkItem(Work, data);
            }

            countdownEvent.Wait();
        } 

        double totalSum = 0.0;
        foreach (double s in partialSums) totalSum += s;
        double pi = totalSum * h;

        //Console.WriteLine($"Par sum={totalSum:F8}, pi={pi:F10}");

        return pi;
    }

    static void Work(object? obj)
    {
        var data = (ThreadData)obj!;
        int threadIndex = data.Index;
        long nLocal = data.N;  
        double h = data.H;
        double[] partialSums = data.PartialSums!;
        CountdownEvent countdownEvent = data.Countdown!;

        long start = (long)threadIndex * (nLocal / partialSums.Length);
        long end = threadIndex == partialSums.Length - 1 ? nLocal : (long)(threadIndex + 1) * (nLocal / partialSums.Length);

        //int threadId = Thread.CurrentThread.ManagedThreadId;
        //Console.WriteLine($"Поток #{threadId}: шаги [{start}..{end}) ({end-start} шагов)");

        double sum = 0.0;
        for (long j = start; j < end; j++)
        {
            double x = (j + 0.5) * h;
            sum += 4.0 / (1.0 + x * x);
        }
        partialSums[threadIndex] = sum;

        //Console.WriteLine($"Поток #{threadId}: sum={sum}");

        countdownEvent.Signal();
    }
}

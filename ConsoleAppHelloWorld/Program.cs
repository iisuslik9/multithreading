using System;
using System.Diagnostics;

//Написать многопоточное консольное приложение, 
//реализующее параллельные алгоритмы для вычисления 
//числа Пи с использованием библиотеки TPL.Parallel
//методом Монте-Карло

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
        double res = SequentialPi(n);
        swSeq.Stop();
        Console.WriteLine($"Результат последовательного Pi: {res:F10}");
        return swSeq.Elapsed.TotalSeconds;
    }

    static double MeasurePar(long n, int k)
    {
        var swPar = Stopwatch.StartNew();
        double res = ParallelPi(n, k); 
        swPar.Stop();
        Console.WriteLine($"Результат параллельного Pi: {res:F10}");
        return swPar.Elapsed.TotalSeconds;
    }

    static double SequentialPi(long totalPoints)
    {
         long hits = 0;
        Random rand = new Random();

        for (long i = 0; i < totalPoints; i++)
        {
            double x = 2.0 * rand.NextDouble() - 1.0; // -1; 1
            double y = 2.0 * rand.NextDouble() - 1.0; 

            if (x * x + y * y <= 1.0)
                hits++;
        }

        return 4.0 * hits / totalPoints;
    }
    


    static double ParallelPi(long totalPoints, int cores)
    {
        long pointsPerCore = totalPoints / cores;
        long remainder = totalPoints % cores;
        long[] localHits = new long[cores];


        Parallel.For(0, cores, i =>
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode() ^ i);
            
            // осатокк работе последнего ядра
            long pointsToProcess = (i == cores - 1) ? pointsPerCore + remainder : pointsPerCore;
            
            long hits = 0;
            for (long j = 0; j < pointsToProcess; j++)
            {
                double x = 2 * rand.NextDouble() -1;
                double y = 2 * rand.NextDouble() -1;
                if (x * x + y * y <= 1.0)
                {
                    hits++;
                }
            }
            localHits[i] = hits; 
        });

        long totalHits = 0;
        for (int i = 0; i < cores; i++)
        {
            totalHits += localHits[i];
        }

        return 4.0 * totalHits / totalPoints;

    } 

}

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
//Написать многопоточное консольное приложение, реализующее параллельные алгоритмы
// для вычисления числа Пи с использованием библиотеки TPL.Task.
//методом численного интегрирования 4/(1+x²)dx

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
    
    static double PiTask(long s, long e, double h, int taskId)
    {
        //Console.WriteLine($"  task {taskId} на потоке {Thread.CurrentThread.ManagedThreadId} диапазон: {s}..{e}");
        double localSum = 0.0;
        for (long j = s; j < e; j++)
        {
            double x = (j + 0.5) * h;
            localSum += 4.0 / (1.0 + x * x);
        }
        return localSum;
    }


    static double ParallelPi(long n, int threadCount)
    {
        double h = 1.0 / n;
        long chunk = n / threadCount;
        var tasks = new Task<double>[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            int taskId = i;
            long start = i * chunk;
            long end;

            if (i == threadCount - 1)
                end = n;
            else
                end = start + chunk;

            tasks[i] = new Task<double>(() => PiTask(start, end, h, taskId));
            tasks[i].Start();  
        }
        
        Task.WaitAll(tasks);  
        
        double total = 0.0;
        for (int i = 0; i < threadCount; i++)
            total += tasks[i].Result;
            
        return total * h;
    }

}

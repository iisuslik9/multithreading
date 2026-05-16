//Написать многопоточное консольное приложение, реализующее параллельные алгоритмы для вычисления числа 
//Пи с использованием асинхронного программирования

//методом численного интегрирования 4/(1+x^2)dx

using System;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    const long N = 1000000000;

    async static Task Main(string[] args)
    {
        int cores = Environment.ProcessorCount;
        Console.WriteLine($"{cores} потоков");

        var seq = await MeasureAsync(() => Task.Run(() => PiTask(0, N , N)));
        var par = await MeasureAsync(() => ParallelPiAsync(N, cores));

        double S = seq.Time / par.Time;
        double E = S / cores;
        double C = cores * par.Time;

        Console.WriteLine($"n={N}");

        Console.WriteLine($"Последовательный T1 = {seq.Time} с pi = {seq.Result}");
        Console.WriteLine($"Параллельный    Tp = {par.Time} с pi = {par.Result}" );
        Console.WriteLine($"S={S:F2} E={E:F2} C={C:F2}");
    }

    static async Task<double> ParallelPiAsync(long n, int cores)
    {
        long chunk = n / cores;

        Task<double>[] tasks = new Task<double>[cores];

        for (int i = 0; i < cores; i++)
        {
            long start = i * chunk;
            long end = start + chunk;
            if (i == cores - 1)
                end = n;
            long capturedStart = start;
            long capturedEnd = end;

            tasks[i] = Task.Run(() => PiTask(capturedStart, capturedEnd, n));
        }

        double[] partials = await Task.WhenAll(tasks);

        double total = 0.0;
        for (int i = 0; i < partials.Length; i++)
            total += partials[i];

        return total;
    }

    static double PiTask(long start, long end, long n)
    {
        double h = 1.0 / n;
        double sum = 0.0;

        for (long i = start; i < end; i++)
        {
            double x = (i + 0.5) * h;
            sum += 4.0 / (1.0 + x * x);
        }

        return sum * h;
    }

    static async Task<(double Time, double Result)> MeasureAsync(Func<Task<double>> func)
    {
        Stopwatch sw = Stopwatch.StartNew();
        double result = await func();
        sw.Stop();

        return (sw.Elapsed.TotalSeconds, result);
    }
}
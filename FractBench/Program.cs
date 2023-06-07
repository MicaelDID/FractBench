using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractBench
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Simple Fractal Benchmark");
            Console.WriteLine($"Logical processors: {Environment.ProcessorCount}");
            Bench bench;

            // Continue until user press 0 or ESC
            while (true)
            {
                Console.WriteLine();
                Console.Write("Select location [1. Standard 2. Low 3. Medium 4. High] ");
                var loc = ReadNumber(4);

                if (loc == 1)
                    bench = new Bench();
                else if (loc == 2)
                    bench = new Bench(-1.39415229360722, -0.00180321371397862, 0.000000000000454747350886464, 50000);
                else if (loc == 3)
                    bench = new Bench(0.251106774256728, -0.0000724877441406802, 0.000000002, 500000);
                else
                    bench = new Bench(0.339309693454861, -0.570137012708333, 0.00000000625, 1500000);

                Console.Write("Select resolution [1. 480p 2. 720p 3. 1080p 4. 4k, 5. 8k, 6. 3x3 4k 7. memtest1, 8. memtest2] ");
                var num = ReadNumber(8);
                var xres = new List<int> { 640, 1280, 1920, 3840, 7680, 3 * 3840, 16000, 32000 };
                var yres = new List<int> { 480, 720, 1080, 2160, 4320, 3 * 2160, 16000, 32000 };
                var x = xres[num - 1];
                var y = yres[num - 1];

                Console.Write("Select save [1. None 2. Memory 3. File] ");
                var save = ReadNumber(3);

                Console.Write("Select calculation [1. Single threaded 2. Multiple threaded (normal) 3. Multiple threaded (optimal)] ");
                num = ReadNumber(3);
                var mode = new List<string> { "Single threaded", "Multiple threaded (normal)", "Multiple threaded (optimal)" };
                var dteBeg = DateTime.Now;
                bench.FixCorr(x, y);
                object[] data = null;

                if (save > 1)
                {
                    data = new object[y];

                    for (int j = 0; j < y; j++)
                        data[j] = Bench.Repeated(x);
                }

                if (num == 1)
                    bench.DrawNormal(x, y, data);
                else if (num == 2)
                    bench.DrawParallel(x, y, data, -1);
                else
                    bench.DrawParallel(x, y, data, Environment.ProcessorCount * 2);

                var dteEnd = DateTime.Now;
                var saves = new List<string> { "None", "Memory", "File" };
                Console.WriteLine($"Location {loc}, Resolution {x} x {y}, Save {saves[save - 1]}, {mode[num - 1]}, Elapsed {dteEnd.Subtract(dteBeg).TotalMilliseconds:###,###,###,##0} ms");
                bench = null;

                if (data != null)
                {
                    int intMin = -1, intMax = -1;
                    long lngMaxiter = 0, lngAlloc = (long)sizeof(int) * (long)x * (long)y;

                    foreach (var item in data)
                    {
                        var val = ((List<int>)item).Where(z => z < Bench.MAXITER).Max();
                        lngMaxiter += ((List<int>)item).Where(z => z == Bench.MAXITER).Count();

                        if (val > intMax)
                            intMax = val;

                        val = ((List<int>)item).Min();

                        if (intMin == -1 || val < intMin)
                            intMin = val;
                    }

                    var dblAlloc = (double)lngAlloc;
                    var units = new List<string> { "b", "kb", "mb", "gb", "tb" };
                    int i;

                    for (i = 0; i <= 4; i++)
                    {
                        if (i > 0)
                            dblAlloc /= 1024d;

                        if (dblAlloc < 1024 || i == 4)
                            break;
                    }

                    Console.WriteLine($"Alloc {dblAlloc:0.0} {units[i]}, Iteration Min {intMin}, Max {intMax:###,###,###,##0}, MaxIter reached {lngMaxiter:###,###,###,##0}");

                    if (save == 3)
                    {
                        using (var stream = new FileStream("output.dat", FileMode.Create, FileAccess.Write, FileShare.None))
                        using (var writer = new BinaryWriter(stream))
                        {
                            foreach (var item in data)
                            {
                                var bteData = new byte[x * 4];
                                int m = 0;

                                foreach (var col in (List<int>)item)
                                {
                                    var bteCol = BitConverter.GetBytes(col);

                                    for (int n = 0; n < 4; n++)
                                        bteData[m + n] = bteCol[n];

                                    m += 4;
                                }

                                writer.Write(bteData, 0, x * 4);
                            }
                        }
                    }

                    if (save > 1)
                    {
                        for (int j = 0; j < y; j++)
                            data[j] = null;

                        data = null;
                    }
                }
            }
        }

        static int ReadNumber(int max)
        {
            int ret;

            do
            {
                var key = Console.ReadKey(true);

                if (key.KeyChar == (char)27 || key.KeyChar == (char)13)
                    Environment.Exit(0);

                ret = (int)(key.KeyChar - '0');
            } while (!(ret >= 0 && ret <= max));

            Console.WriteLine($"{ret}");

            if (ret == 0)
                Environment.Exit(0);

            return ret;
        }
    }

    public class Bench
    {
        public const int MAXITER = 2000000000;

        private object objLock = new object();
        private int intCurrentMaxIter = 1000;
        private double dblXp = 0, dblYp = 0, dblDiff = 4.0, dblXcorr = 1.0, dblYcorr = 1.0;
        private List<string> progress = new List<string> { " ", ".", "-", "=", "*", "#" }; // 0, 1-9, 10-19, 20-29, 30-38, 39

        public Bench()
        {
        }

        public Bench(double x, double y, double diff, int iter)
        {
            dblXp = x;
            dblYp = y;
            dblDiff = diff;
            intCurrentMaxIter = iter;
        }

        public static List<int> Repeated(int count)
        {
            var ret = new List<int>(count);
            ret.AddRange(Enumerable.Repeat(0, count));
            return ret;
        }

        public void FixCorr(int intPicWidth, int intPicHeight)
        {
            if (intPicWidth == intPicHeight)
                dblXcorr = dblYcorr = 1.0;
            else if (intPicHeight < intPicWidth)
            {
                dblXcorr = (double)intPicWidth / (double)intPicHeight;
                dblYcorr = 1.0;
            }
            else
            {
                dblXcorr = 1.0;
                dblYcorr = (double)intPicHeight / (double)intPicWidth;
            }
        }

        private int MaxMandel(double dblReal, double dblImag)
        {
            int intCount = intCurrentMaxIter;
            double dblRe = dblReal, dblIm = dblImag;
            double dblRe2, dblIm2, dblReIm2;

            // Periodicity Checking
            double dblPrevReal = dblReal, dblPrevImag = dblImag;
            int n1 = 0; int n2 = 8;

            do
            {
                intCount--;
                if (intCount == 0)
                    return MAXITER;

                dblRe2 = dblRe * dblRe;
                dblIm2 = dblIm * dblIm;
                dblReIm2 = 2.0 * dblRe * dblIm;
                dblRe = dblRe2 - dblIm2 + dblReal;
                dblIm = dblReIm2 + dblImag;

                if (dblPrevReal == dblRe && dblPrevImag == dblIm)
                    return MAXITER;

                n1++;

                if (n1 >= n2)
                {
                    dblPrevReal = dblRe;
                    dblPrevImag = dblIm;
                    n1 = 0;
                    n2 *= 2;
                }
            }
            while (dblRe2 + dblIm2 <= 4.0);

            return intCurrentMaxIter - intCount;
        }

        private static IEnumerable<int> SteppedIterator(int startIndex, int endIndex)
        {
            // insanely more fun than: for (int i = startIndex; i < endIndex; i++) yield return i;
            int diff = endIndex - startIndex;

            for (int i = 0, n = 0, m = 0; i < diff; i++)
            {
                int v = (n * 8 + m) % diff;
                yield return v + startIndex;

                if (v + 8 < diff)
                    n++;
                else
                {
                    n = 0;
                    m++;
                }
            }
        }

        // Multiple threaded loop using Parallel.For
        public object[] DrawParallel(int intPicWidth, int intPicHeight, object[] data, int intDegreeOfParallelism)
        {
            var flag = new bool[intPicHeight];
            var pOptions = new ParallelOptions { TaskScheduler = null, MaxDegreeOfParallelism = intDegreeOfParallelism };
            int tot = 0;

            Parallel.ForEach(SteppedIterator(0, intPicHeight), pOptions, (j, loopState) =>
            {
                //if (bolPanicQuit)
                //    loopState.Stop();

                DrawParallelLineThread(intPicWidth, intPicHeight, j, data != null ? (List<int>)data[j] : null);

                flag[j] = true; // should be safe to access j, only one thread will do this

                lock (objLock)
                {
                    tot++;

                    if (0 == (tot % 50))
                        Console.Write($"\r{new string(' ', 41 + 15)}\r{GetProgress(flag, intPicHeight)} Completed {(double)tot * 100d / (double)intPicHeight:0.#}%");
                }
            });

            Console.WriteLine($"\r{new string(' ', 41 + 15)}\r{GetProgress(flag, intPicHeight)} Completed 100%");
            return data;
        }

        private void DrawParallelLineThread(int intPicWidth, int intPicHeight, int j, List<int> row)
        {
            double y = (double)j / (double)intPicHeight * dblDiff * dblYcorr + dblYp - dblDiff * dblYcorr / 2.0;

            for (int i = 0; i < intPicWidth; i++)
            {
                double x = (double)i / (double)intPicWidth * dblDiff * dblXcorr + dblXp - dblDiff * dblXcorr / 2.0;
                int intIter = MaxMandel(x, y);

                // save intIter in allocated buffer
                if (row == null)
                    continue;

                row[i] = intIter;
            }
        }

         // Single threaded loop
        public object[] DrawNormal(int intPicWidth, int intPicHeight, object[] data)
        {
            for (int j = 0; j < intPicHeight; j++)
            {
                double y = (double)j / (double)intPicHeight * dblDiff * dblYcorr + dblYp - dblDiff * dblYcorr / 2.0;

                for (int i = 0; i < intPicWidth; i++)
                {
                    double x = (double)i / (double)intPicWidth * dblDiff * dblXcorr + dblXp - dblDiff * dblXcorr / 2.0;
                    int intIter = MaxMandel(x, y);

                    if (data == null)
                        continue;

                    ((List<int>)data[j])[i] = intIter;
                }

                if (0 == (j % 50))
                    Console.Write($"\r{new string(' ', 41 + 15)}\r{GetProgress(j, intPicHeight)} Completed {(double)j * 100d / (double)intPicHeight:0.#}%");
            }

            Console.WriteLine($"\r{new string(' ', 41 + 15)}\r{GetProgress(intPicHeight, intPicHeight)} Completed 100%");
            return data;
        }

        private string GetProgress(bool[] data, int m)
        {
            int chunksize = m / 40; // assume every height is evenly divided by 40
            var sb = new StringBuilder();

            for (int i = 0, n = 0; i < m; i++)
            {
                if (data[i])
                    n++;

                if ((i % chunksize) + 1 == chunksize)
                {
                    if (n == 0)
                        sb.Append(progress[0]);
                    else if (n == chunksize)
                        sb.Append(progress[5]);
                    else
                        sb.Append(progress[(n % chunksize) * 4 / chunksize + 1]);

                    n = 0;
                }
            }

            return sb.ToString();
        }

        private string GetProgress(int n, int m)
        {
            int chunksize = m / 40; // assume every height is evenly divided by 40
            var sb = new StringBuilder();

            for (int i = 0; i < 40; i++)
            {
                if (n <= i * chunksize)
                    sb.Append(progress[0]);
                else if (n / chunksize > i)
                    sb.Append(progress[5]);
                else
                    sb.Append(progress[(n % chunksize) * 4 / chunksize + 1]);
            }

            return sb.ToString();
        }
    }
}
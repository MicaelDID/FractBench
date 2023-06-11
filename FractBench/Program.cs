using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractBench
{
    internal class Program
    {
        static List<int> xres = new List<int> { 640, 1280, 1920, 3840, 7680, 3 * 3840, 16000, 32000 };
        static List<int> yres = new List<int> { 480, 720, 1080, 2160, 4320, 3 * 2160, 16000, 32000 };
        static List<string> lstModes = new List<string> { "Single threaded", "Multiple threaded (normal)", "Multiple threaded (optimal)" };
        static List<string> lstSaves = new List<string> { "None", "Memory", "None", "Memory", "File" };
        static List<string> lstUnits = new List<string> { "b", "kb", "mb", "gb", "tb" };

        static void Main(string[] args)
        {
            Console.WriteLine($"Simple Fractal Benchmark");
            Console.WriteLine($"Logical processors: {Environment.ProcessorCount}");

            if (args.Length > 1 || (args.Length == 1 && !(args[0].Length == 4 || args[0].Length == 6)))
            {
                Console.WriteLine($"Incorrect number of arguments or incorrect argument");
                return;
            }
            else if (args.Length == 1)
            {
                var intLoc = (int)(args[0][0] - '0');
                var intScrRes = (int)(args[0][1] - '0');
                var intSave = (int)(args[0][2] - '0');
                int intRepeatNum = -1, intNum1 = -1, intNum2 = -1, intIdx = 3;
                if (intSave == 3 || intSave == 4)
                {
                    intNum1 = (int)(args[0][3] - '0');
                    intNum2 = (int)(args[0][4] - '0');

                    if (intNum1 < 0 || intNum1 > 9 || intNum2 < 0 || intNum2 > 9)
                    {
                        Console.WriteLine($"Incorrect argument");
                        return;
                    }

                    intRepeatNum = intNum1 == 0 && intNum2 == 0 ? 100 : intNum1 * 10 + intNum2;
                    intIdx = 5;
                }
                var intCalcMode = (int)(args[0][intIdx] - '0');

                if (intLoc < 1 || intLoc > 4 || intScrRes < 1 || intScrRes > 8 || intSave < 1 || intSave > 5 || intCalcMode < 1 || intCalcMode > 3)
                {
                    Console.WriteLine($"Incorrect argument");
                    return;
                }

                FractalBenchmark(intLoc, xres[intScrRes - 1], yres[intScrRes - 1], intSave, intRepeatNum, intCalcMode);
                return;
            }

            while (true) // Continue until user press 0 or ESC during input or ctrl c at any time
            {
                Console.WriteLine();
                Console.Write("Select location [1. Standard 2. Low 3. Medium 4. High] ");
                var intLoc = ReadNumber(4);

                Console.Write("Select resolution [1. 480p 2. 720p 3. 1080p 4. 4k, 5. 8k, 6. 3x3 4k 7. memtest1, 8. memtest2] ");
                var intScrRes = ReadNumber(8);

                Console.Write("Select save [1. None 2. Memory 3. None with repeat 4. Memory with repeat 5. File] ");
                var intSave = ReadNumber(5);
                var intRepeatNum = -1;

                if (intSave == 3 || intSave == 4)
                {
                    Console.Write("Select repeat number [01-99 or 00 for endless] ");
                    var num1 = ReadNumber(9, false, false);
                    var num2 = ReadNumber(9, false);
                    intRepeatNum = num1 == 0 && num2 == 0 ? 100 : num1 * 10 + num2;
                }

                Console.Write("Select calculation [1. Single threaded 2. Multiple threaded (normal) 3. Multiple threaded (optimal)] ");
                var intCalcMode = ReadNumber(3);

                FractalBenchmark(intLoc, xres[intScrRes - 1], yres[intScrRes - 1], intSave, intRepeatNum, intCalcMode);
            }
        }

        static void FractalBenchmark(int intLoc, int intX, int intY, int intSave, int intRepeatNum, int intCalcMode)
        {
            Bench bench;
            object[] data = null;

            while (true)
            {
                if (intLoc == 1)
                    bench = new Bench();
                else if (intLoc == 2)
                    bench = new Bench(-1.39415229360722, -0.00180321371397862, 0.000000000000454747350886464, 50000);
                else if (intLoc == 3)
                    bench = new Bench(0.251106774256728, -0.0000724877441406802, 0.000000002, 500000);
                else
                    bench = new Bench(0.339309693454861, -0.570137012708333, 0.00000000625, 1500000);

                var dteBeg = DateTime.Now;
                bench.FixCorr(intX, intY);

                if (intSave == 2 || intSave == 4 || intSave == 5)
                {
                    data = new object[intY];

                    for (int j = 0; j < intY; j++)
                        data[j] = Bench.Repeated(intX);
                }

                if (intCalcMode == 1)
                    bench.DrawNormal(intX, intY, data);
                else if (intCalcMode == 2)
                    bench.DrawParallel(intX, intY, data, -1);
                else
                    bench.DrawParallel(intX, intY, data, Environment.ProcessorCount * 2);

                var dteEnd = DateTime.Now;
                Console.WriteLine($"Location {intLoc}, Resolution {intX} x {intY}, Save {lstSaves[intSave - 1]}, {lstModes[intCalcMode - 1]}, Elapsed {dteEnd.Subtract(dteBeg).TotalMilliseconds:###,###,###,##0} ms");
                bench = null;

                if (data != null)
                {
                    int intMin = -1, intMax = -1;
                    long lngMaxiter = 0, lngAlloc = (long)sizeof(int) * (long)intX * (long)intY;

                    foreach (var item in data)
                    {
                        var intVal = ((List<int>)item).Where(z => z < Bench.MAXITER).Max();
                        lngMaxiter += ((List<int>)item).Where(z => z == Bench.MAXITER).Count();

                        if (intVal > intMax)
                            intMax = intVal;

                        intVal = ((List<int>)item).Min();

                        if (intMin == -1 || intVal < intMin)
                            intMin = intVal;
                    }

                    var dblAlloc = (double)lngAlloc;
                    int i;

                    for (i = 0; i <= 4; i++)
                    {
                        if (i > 0)
                            dblAlloc /= 1024d;

                        if (dblAlloc < 1024 || i == 4)
                            break;
                    }

                    Console.WriteLine($"Alloc {dblAlloc:0.0} {lstUnits[i]}, Iteration Min {intMin}, Max {intMax:###,###,###,##0}, MaxIter reached {lngMaxiter:###,###,###,##0}");
                }

                if (intSave == 1 || intSave == 2 || intSave == 5)
                    break;

                if (intRepeatNum < 100 && --intRepeatNum <= 0)
                    break;

                if (data != null && (intSave == 2 || intSave == 4 || intSave == 5))
                    ClearData(data, intY);
            }

            if (data != null && intSave == 5)
                SaveData(data, intX, intY);

            if (data != null && (intSave == 2 || intSave == 4 || intSave == 5))
                ClearData(data, intY);
        }

        static void ClearData(object[] data, int y)
        {
            for (int j = 0; j < y; j++)
                data[j] = null;

            data = null;
        }

        static void SaveData(object[] data, int x, int y)
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

        static int ReadNumber(int intMax, bool bolZeroFlag = true, bool bolNewLine = true)
        {
            int ret;

            do
            {
                var key = Console.ReadKey(true);

                if (key.KeyChar == (char)27 || key.KeyChar == (char)13)
                    Environment.Exit(0);

                ret = (int)(key.KeyChar - '0');
            } while (!(ret >= 0 && ret <= intMax));

            if (bolNewLine)
                Console.WriteLine($"{ret}");
            else
                Console.Write($"{ret}");

            if (bolZeroFlag && ret == 0)
                Environment.Exit(0);

            return ret;
        }
    }

    public class Bench
    {
        public const int MAXITER = 2000000000;
        public bool bolPanicQuit = false;
        private object objLock = new object();
        private int intCurrentMaxIter = 1000;
        private double dblXp = 0, dblYp = 0, dblDiff = 4.0, dblXcorr = 1.0, dblYcorr = 1.0;
        private List<string> lstProgress = new List<string> { " ", ".", "-", "=", "*", "#" }; // 0, 1-9, 10-19, 20-29, 30-38, 39

        public Bench()
        {
        }

        public Bench(double x, double y, double diff, int maxIter)
        {
            dblXp = x;
            dblYp = y;
            dblDiff = diff;
            intCurrentMaxIter = maxIter;
        }

        public static List<int> Repeated(int intCount)
        {
            var ret = new List<int>(intCount);
            ret.AddRange(Enumerable.Repeat(0, intCount));
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

        private static IEnumerable<int> SteppedIterator(int intStartIndex, int intEndIndex)
        {
            // Insanely more fun than: for (int i = intStartIndex; i < intEndIndex; i++) yield return i;
            int intDiff = intEndIndex - intStartIndex;

            for (int i = 0, n = 0, m = 0; i < intDiff; i++)
            {
                int v = (n * 8 + m) % intDiff;
                yield return v + intStartIndex;

                if (v + 8 < intDiff)
                    n++;
                else
                {
                    n = 0;
                    m++;
                }
            }
        }

        // Multiple threaded loop using Parallel.For
        public void DrawParallel(int intPicWidth, int intPicHeight, object[] data, int intDegreeOfParallelism)
        {
            var arrFlag = new bool[intPicHeight];
            var pOptions = new ParallelOptions { TaskScheduler = null, MaxDegreeOfParallelism = intDegreeOfParallelism };
            int intTotal = 0;

            Parallel.ForEach(SteppedIterator(0, intPicHeight), pOptions, (j, loopState) =>
            {
                DrawParallelLineThread(intPicWidth, intPicHeight, j, data != null ? (List<int>)data[j] : null);
                arrFlag[j] = true; // should be safe to access j, only one thread will do this

                lock (objLock)
                {
                    intTotal++;

                    if (0 == (intTotal % 50))
                        Console.Write($"\r{new string(' ', 41 + 15)}\r{GetProgress(arrFlag, intPicHeight)} Completed {(double)intTotal * 100d / (double)intPicHeight:0.#}%");
                }
            });

            Console.WriteLine($"\r{new string(' ', 41 + 15)}\r{GetProgress(arrFlag, intPicHeight)} Completed 100%");
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
        public void DrawNormal(int intPicWidth, int intPicHeight, object[] data)
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
        }

        private string GetProgress(bool[] data, int m)
        {
            int intChunksize = m / 40; // assume every height is evenly divided by 40
            var sb = new StringBuilder();

            for (int i = 0, n = 0; i < m; i++)
            {
                if (data[i])
                    n++;

                if ((i % intChunksize) + 1 == intChunksize)
                {
                    if (n == 0)
                        sb.Append(lstProgress[0]);
                    else if (n == intChunksize)
                        sb.Append(lstProgress[5]);
                    else
                        sb.Append(lstProgress[(n % intChunksize) * 4 / intChunksize + 1]);

                    n = 0;
                }
            }

            return sb.ToString();
        }

        private string GetProgress(int n, int m)
        {
            int intChunksize = m / 40; // assume every height is evenly divided by 40
            var sb = new StringBuilder();

            for (int i = 0; i < 40; i++)
            {
                if (n <= i * intChunksize)
                    sb.Append(lstProgress[0]);
                else if (n / intChunksize > i)
                    sb.Append(lstProgress[5]);
                else
                    sb.Append(lstProgress[(n % intChunksize) * 4 / intChunksize + 1]);
            }

            return sb.ToString();
        }
    }
}
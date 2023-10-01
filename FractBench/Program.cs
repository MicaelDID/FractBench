using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using DataLayer;

namespace FractBench
{
    internal class Program
    {
        static List<int> xres = new List<int> { 640, 1280, 1920, 3840, 7680, 3 * 3840, 16000, 32000 };
        static List<int> yres = new List<int> { 480, 720, 1080, 2160, 4320, 3 * 2160, 16000, 32000 };
        static List<string> lstModes = new List<string> { "Single threaded", "Multiple threaded (normal)", "Multiple threaded (optimal)", "Multiple threaded (free)" };
        static List<string> lstSaves = new List<string> { "None", "Memory", "None", "Memory", "File" };
        static List<string> lstUnits = new List<string> { "b", "kb", "mb", "gb", "tb" };
        static List<string> lstProgress = new List<string> { " ", ".", "-", "=", "*", "#" }; // for example 0, 1-9, 10-19, 20-29, 30-38, 39

        static void Main(string[] args)
        {
            Console.WriteLine($"Simple Fractal Benchmark");
            Console.WriteLine($"Logical processors: {Environment.ProcessorCount}");

            if (args.Length > 1 || (args.Length == 1 && args[0].Length < 4))
            {
                Console.WriteLine($"Incorrect number of arguments or incorrect argument");
                return;
            }
            else if (args.Length == 1)
            {
                var intLoc = (int)(args[0][0] - '0');
                var intScrRes = (int)(args[0][1] - '0');
                var intSave = (int)(args[0][2] - '0');
                int intRepeatNum = -1, intNum1, intNum2, intNum3, intIdx = 3;
                if (args[0].Length >= 6 && (intSave == 3 || intSave == 4))
                {
                    intNum1 = (int)(args[0][3] - '0');
                    intNum2 = (int)(args[0][4] - '0');
                    intRepeatNum = intNum1 == 0 && intNum2 == 0 ? 100 : intNum1 * 10 + intNum2;
                    intIdx = 5;

                    if (intNum1 < 0 || intNum1 > 9 || intNum2 < 0 || intNum2 > 9)
                    {
                        Console.WriteLine($"Incorrect argument");
                        return;
                    }
                }
                int intCalcMode = (int)(args[0][intIdx] - '0'), intCalcFree = 0;

                if (args[0].Length >= (intIdx == 3 ? 7 : 9) && intCalcMode == 4)
                {
                    intNum1 = (int)(args[0][intIdx + 1] - '0');
                    intNum2 = (int)(args[0][intIdx + 2] - '0');
                    intNum3 = (int)(args[0][intIdx + 3] - '0');
                    intCalcFree = intNum1 * 100 + intNum2 * 10 + intNum3;

                    if (intNum1 < 0 || intNum1 > 9 || intNum2 < 0 || intNum2 > 9 || intNum3 < 0 || intNum3 > 9 || intCalcFree <= 1)
                    {
                        Console.WriteLine($"Incorrect argument");
                        return;
                    }
                }

                if (intLoc < 1 || intLoc > 4 || intScrRes < 1 || intScrRes > 8 || intSave < 1 || intSave > 5 || intCalcMode < 1 || intCalcMode > 4)
                {
                    Console.WriteLine($"Incorrect argument");
                    return;
                }

                FractalBenchmark(intLoc, xres[intScrRes - 1], yres[intScrRes - 1], intSave, intRepeatNum, intCalcMode, intCalcFree);
                return;
            }

            while (true) // Continue until user press 0 or ESC during input or ctrl c at any time
            {
                Console.WriteLine();
                Console.Write("Select location [1. Standard, 2. Low, 3. Medium, 4. High, 5. Free] ");
                var intLoc = ReadNumber(5);

                Console.Write("Select resolution [1. 480p, 2. 720p, 3. 1080p, 4. 4k, 5. 8k, 6. 3x3 4k, 7. memtest1, 8. memtest2] ");
                var intScrRes = ReadNumber(8);

                Console.Write("Select save [1. None, 2. Memory, 3. None with repeat, 4. Memory with repeat, 5. File] ");
                int intSave = ReadNumber(5), intRepeatNum = -1;

                if (intSave == 3 || intSave == 4)
                {
                    Console.Write("Select repeat number [1-99 or 00 for endless] ");
                    intRepeatNum = ReadNumber(9, 2);
                    intRepeatNum = intRepeatNum == 0 ? 100 : intRepeatNum;
                }

                Console.Write("Select multithreading [1. Single, 2. Multiple (normal), 3. Multiple (optimal), 4. Multiple (free)] ");
                int intCalcMode = ReadNumber(4), intCalcFree = 0;

                if (intCalcMode == 4)
                {
                    Console.Write("Select number of threads [2-999] ");
                    intCalcFree = ReadNumber(9, 3);

                    if (intCalcFree <= 1)
                        return;
                }

                FractalBenchmark(intLoc, xres[intScrRes - 1], yres[intScrRes - 1], intSave, intRepeatNum, intCalcMode, intCalcFree);
            }
        }

        static void FractalBenchmark(int intLoc, int intX, int intY, int intSave, int intRepeatNum, int intCalcMode, int intCalcFree)
        {
            Fractal bench;
            var disp = new MyProgress();
            object[] data = null;
            int intNum = 1;
            var lstTime = new List<double>();

            while (true)
            {
                if (intLoc == 1)
                    bench = new Fractal();
                else if (intLoc == 2)
                    bench = new Fractal(-1.39415229360722, -0.00180321371397862, 0.000000000000454747350886464, 50000);
                else if (intLoc == 3)
                    bench = new Fractal(0.251106774256728, -0.0000724877441406802, 0.000000002, 500000);
                else if (intLoc == 4)
                    bench = new Fractal(0.339309693454861, -0.570137012708333, 0.00000000625, 1500000);
                else
                {
                    bench = new Fractal(out bool bolReadFile);
                    intLoc = bolReadFile ? 5 : 1;
                }

                var dteBeg = DateTime.Now;
                bench.FixCorr(intX, intY);

                if (intSave == 2 || intSave == 4 || intSave == 5)
                {
                    data = new object[intY];

                    for (int j = 0; j < intY; j++)
                        data[j] = Fractal.Repeated(intX);
                }

                if (intCalcMode == 1)
                    bench.DrawNormal(intX, intY, data, disp);
                else if (intCalcMode == 2)
                    bench.DrawParallel(intX, intY, data, -1, disp);
                else if (intCalcMode == 3)
                    bench.DrawParallel(intX, intY, data, Environment.ProcessorCount * 2, disp);
                else
                    bench.DrawParallel(intX, intY, data, intCalcFree, disp);

                var dteEnd = DateTime.Now;
                string strRepeat = (intRepeatNum > 1 ? $"{intNum++}: ".PadLeft(3 + (int)Math.Log10(intRepeatNum)) : string.Empty);
                string strFree = intCalcMode == 4 ? $", {intCalcFree} threads" : string.Empty;
                double time = dteEnd.Subtract(dteBeg).TotalMilliseconds;
                Console.WriteLine($"{strRepeat}Location {intLoc}, Resolution {intX} x {intY}, Save {lstSaves[intSave - 1]}, {lstModes[intCalcMode - 1]}{strFree}, Elapsed {time:###,###,###,##0} ms");
                lstTime.Add(time);
                bench = null;

                if (data != null)
                {
                    int intMin = -1, intMax = -1;
                    long lngMaxiter = 0, lngAlloc = (long)sizeof(int) * (long)intX * (long)intY;

                    foreach (var item in data)
                    {
                        var intVal = ((List<int>)item).Where(z => z < Fractal.MAXITER).Any() ? ((List<int>)item).Where(z => z < Fractal.MAXITER).Max() : -1;
                        lngMaxiter += ((List<int>)item).Where(z => z == Fractal.MAXITER).Count();

                        if (intVal > intMax)
                            intMax = intVal;

                        intVal = ((List<int>)item).Min();

                        if (intMin == -1 || intVal < intMin)
                            intMin = intVal;
                    }

                    if (intMax == -1)
                        intMax = Fractal.MAXITER;

                    var dblAlloc = (double)lngAlloc;
                    int i;

                    for (i = 0; i <= 4; i++)
                    {
                        if (i > 0)
                            dblAlloc /= 1024d;

                        if (dblAlloc < 1024 || i == 4)
                            break;
                    }

                    Console.WriteLine($"Alloc {dblAlloc:0.0} {lstUnits[i]}, Iteration Min {intMin:###,###,###,##0}, Max {intMax:###,###,###,##0}, MaxIter reached {lngMaxiter:###,###,###,##0}");
                }

                if (intSave == 1 || intSave == 2 || intSave == 5)
                    break;

                if (intRepeatNum < 1000 && intNum > intRepeatNum)
                    break;

                if (data != null && (intSave == 2 || intSave == 4 || intSave == 5))
                    ClearData(data, intY);
            }

            if (lstTime.Count() > 1)
                Console.WriteLine($"Repeat summary: Count {lstTime.Count()}, Elapsed Min {lstTime.Min():###,###,###,##0} ms, Max {lstTime.Max():###,###,###,##0} ms, Average {lstTime.Average():###,###,###,##0} ms");

            if (data != null && intSave == 5)
                SaveData(data, intX);

            if (data != null && (intSave == 2 || intSave == 4 || intSave == 5))
                ClearData(data, intY);
        }

        static void ClearData(object[] data, int y)
        {
            for (int j = 0; j < y; j++)
                data[j] = null;

            data = null;
        }

        static void SaveData(object[] data, int x)
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

        static int ReadNumber(int intMax, int intDigits = 1)
        {
            int i = 1, ret = 0;

            for (int num = 0; i <= intDigits; ret = ret * 10 + num)
            {
                var key = Console.ReadKey(true);

                if (key.KeyChar == (char)27 || (i == 1 && key.KeyChar == (char)13))
                    Environment.Exit(0);

                if (i > 1 && (key.KeyChar == ' ' || key.KeyChar == (char)13))
                {
                    Console.WriteLine();
                    break;
                }

                if (key.KeyChar == (char)8 || (key.KeyChar == ' ' && i == 1) || key.KeyChar < '0' || key.KeyChar > '9')
                    continue;

                num = (int)(key.KeyChar - '0');

                Console.Write($"{num}{(i < intDigits ? "" : Environment.NewLine)}");

                if (num < 0 || num > intMax)
                    return ret;

                i++;
            }

            if (intDigits == 1 && ret == 0)
                Environment.Exit(0);

            return ret;
        }

        public class MyProgress : IProgress
        {
            public void Display(int intTotal, int intPicHeight, bool[] arrFlag)
            {
                if (intTotal == -1)
                {
                    Console.WriteLine($"\r{new string(' ', 41 + 15)}\r{GetProgress(arrFlag, intPicHeight)} Completed 100%");
                    return;
                }

                if (0 == (intTotal % 50))
                    Console.Write($"\r{new string(' ', 41 + 15)}\r{GetProgress(arrFlag, intPicHeight)} Completed {(double)intTotal * 100d / (double)intPicHeight:0.#}%");
            }

            public void Display(int intTotal, int intPicHeight)
            {
                if (intTotal == -1)
                {
                    Console.WriteLine($"\r{new string(' ', 41 + 15)}\r{GetProgress(intPicHeight, intPicHeight)} Completed 100%");
                    return;
                }

                if (0 == (intTotal % 50))
                    Console.Write($"\r{new string(' ', 41 + 15)}\r{GetProgress(intTotal, intPicHeight)} Completed {(double)intTotal * 100d / (double)intPicHeight:0.#}%");
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
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer
{
    public class Fractal
    {
        public const int MAXITER = 2000000000;
        public bool bolPanicQuit = false;
        private object objLock = new object();
        private int intCurrentMaxIter = 1000;
        private double dblXp = 0, dblYp = 0, dblDiff = 4.0, dblXcorr = 1.0, dblYcorr = 1.0;

        public Fractal()
        {
        }

        public Fractal(out bool bolReadFile)
        {
            bolReadFile = false;

            if (!File.Exists("location.txt"))
                return;

            try
            {
                var arrFile = new List<string>();
                using (var sr = new StreamReader("location.txt"))
                {
                    arrFile = sr.ReadToEnd().Replace("\n", "").Split('\r').ToList();
                    sr.Close();
                }

                if (arrFile.Count < 4)
                    return;
                if (!double.TryParse(arrFile[0], out double x))
                    return;
                if (!double.TryParse(arrFile[1], out double y))
                    return;
                if (!double.TryParse(arrFile[2], out double diff))
                    return;
                if (!int.TryParse(arrFile[3], out int maxIter))
                    return;

                bolReadFile = true;
                dblXp = x;
                dblYp = y;
                dblDiff = diff;

                if (intCurrentMaxIter >= 1)
                    intCurrentMaxIter = maxIter;
            }
            catch
            {
            }
        }

        public Fractal(double x, double y, double diff, int maxIter)
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
            double dblRe = dblReal, dblIm = dblImag, dblRe2, dblIm2, dblReIm2;
            double dblPrevReal = dblReal, dblPrevImag = dblImag; // Periodicity Checking
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

        private static IEnumerable<int> SteppedIterator(int intStartIndex, int intEndIndex) // Insanely more fun than: for (int i = intStartIndex; i < intEndIndex; i++) yield return i;
        {
            for (int i = 0, n = 0, m = 0, intDiff = intEndIndex - intStartIndex; i < intDiff; i++)
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

        public void DrawParallel(int intPicWidth, int intPicHeight, object[] data, int intDegreeOfParallelism, IProgress disp) // Multiple threaded loop using Parallel.For
        {
            int intTotal = 0;
            var arrFlag = new bool[intPicHeight];
            var pOptions = new ParallelOptions { TaskScheduler = null, MaxDegreeOfParallelism = intDegreeOfParallelism };
            ThreadPool.GetMinThreads(out int minWorker, out int minIOC);
            var intNewWorker = intDegreeOfParallelism == -1 ? Environment.ProcessorCount : intDegreeOfParallelism;

            if (intNewWorker != minWorker)
                ThreadPool.SetMinThreads(intNewWorker, minIOC); // do we need to set this?

            Parallel.ForEach(SteppedIterator(0, intPicHeight), pOptions, (j, loopState) =>
            {
                DrawParallelLineThread(intPicWidth, intPicHeight, j, data != null ? (List<int>)data[j] : null);

                if (disp != null)
                {
                    arrFlag[j] = true; // should be safe to access j, only one thread will do this

                    lock (objLock)
                    {
                        disp.Display(++intTotal, intPicHeight, arrFlag);
                    }
                }
            });

            disp?.Display(-1, intPicHeight, arrFlag);
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

        public void DrawNormal(int intPicWidth, int intPicHeight, object[] data, IProgress disp) // Single threaded loop
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

                disp?.Display(j, intPicHeight);
            }

            disp?.Display(-1, intPicHeight);
        }
    }
}

using System;
using System.Drawing;
using System.IO;

namespace FractRaw2Png
{
    internal class Program
    {
        static int MAXITER = 2000000000;
  
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Incorrect number of arguments");
                return;
            }

            if (!Int32.TryParse(args[0], out int intX) || !Int32.TryParse(args[1], out int intY))
            {
                Console.WriteLine("Incorrect arguments");
                return;
            }

            Raw2Png(intX, intY);
        }

        static void Raw2Png(int x, int y)
        {
            var dteBeg = DateTime.Now;
            byte[] bteData;

            using (var stream = new FileStream("output.dat", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.BaseStream.Length > int.MaxValue || reader.BaseStream.Length != x * y * 4)
                {
                    Console.WriteLine("File size does not match image size or file too big");
                    return;
                }

                bteData = reader.ReadBytes((int)reader.BaseStream.Length);
            }

            var bmpFractal = new Bitmap(x, y);

            //Somehow the multi threaded version seems slower
            //Parallel.For(0, y, (j, loopState) =>
            //{
            //    int n = j * x * 4;
            //    for (int i = 0; i < x; i++, n += 4)
            //    {
            //        var intIter = BitConverter.ToInt32(bteData, n);
            //        var col = intIter == MAXITER ? Color.Black : GetColor1(intIter);
            //        lock (bteData)
            //        {
            //            bmpFractal.SetPixel(i, j, col);
            //        }
            //    }
            //});

            //While the single threaded is faster
            for (int j = 0, n = 0; j < y; j++)
                for (int i = 0; i < x; i++, n += 4)
                {
                    var intIter = BitConverter.ToInt32(bteData, n);
                    bmpFractal.SetPixel(i, j, intIter == MAXITER ? Color.Black : GetColor1(intIter));
                }

            bmpFractal.Save("output.png", System.Drawing.Imaging.ImageFormat.Png);
            var dteEnd = DateTime.Now;
            Console.WriteLine($"Elapsed {dteEnd.Subtract(dteBeg).TotalMilliseconds:###,###,###,##0} ms");
        }

        static Color Hsb2Rgb(double h, double s, double b)
        {
            int main_colour;
            double sub_colour, var1, var2, var3;

            main_colour = (int)(h / 60.0);
            sub_colour = (h / 60.0) - (double)main_colour;
            var1 = (1.0 - s) * b;
            var2 = (1.0 - (s * sub_colour)) * b;
            var3 = (1.0 - (s * (1 - sub_colour))) * b;

            if (0 == main_colour)
                return Color.FromArgb((int)(b * 255), (int)(var3 * 255), (int)(var1 * 255));
            else if (1 == main_colour)
                return Color.FromArgb((int)(var2 * 255), (int)(b * 255), (int)(var1 * 255));
            else if (2 == main_colour)
                return Color.FromArgb((int)(var1 * 255), (int)(b * 255), (int)(var3 * 255));
            else if (3 == main_colour)
                return Color.FromArgb((int)(var1 * 255), (int)(var2 * 255), (int)(b * 255));
            else if (4 == main_colour)
                return Color.FromArgb((int)(var3 * 255), (int)(var1 * 255), (int)(b * 255));
            else
                return Color.FromArgb((int)(b * 255), (int)(var1 * 255), (int)(var2 * 255));
        }

        // 1 Hsb, Log+Cos+Sin, Blue
        static Color GetColor1(int intIter)
        {
            double dblTmp1, dblTmp2;
            double dblHue, dblSat, dblBri;
            double dblPi2;
            int intTmpIter;

            dblPi2 = Math.PI * 2.0;
            intTmpIter = intIter + 28;      // vill ha blå som första färg :-)

            dblTmp1 = (Math.Log(((double)intTmpIter) * ((double)intTmpIter) * ((double)intTmpIter))) / 2.0;
            dblTmp2 = Math.Cos(dblTmp1);
            dblHue = (dblTmp2 + 1.0) * 180.0;
            dblTmp1 = dblPi2 * (((double)intIter) / 105.0);
            dblTmp2 = Math.Sin(dblTmp1);
            dblSat = dblTmp2 / 4.0 + 0.7;   // varierar mellan 0.45 och 0.95
            dblTmp1 = dblPi2 * (((double)intIter) / 130.0);
            dblTmp2 = Math.Sin(dblTmp1);
            dblBri = dblTmp2 / 5.0 + 0.79;  // varierar mellan 0.59 och 0.99
            return Hsb2Rgb(dblHue, dblSat, dblBri);
        }
    }
}
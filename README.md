# FractBench

Simple Fractal Benchmark program, console based, .NET Framework 4.7.x (project mono compatible?)

Using
- either command line arguments or console key press
- integer / double math operations for fractal calculations
- singel- or multithreaded code
- with / without allocating memory
- save to file option (raw dump of iterations in each coordinate, needs reprocessing for generating an image)
- with / without repeats (1-99 or endless)

Included utilty (FractRaw2Png) to convert the saved raw dump to PNG

ToDo: enter x / y location, diff and max iteration

## Example of usage

Starting FractBench.exe without arguments is very descriptive. Just enter from the various variations available.

Of the selectable starting locations 1 to 4 they need different processing time to be calculated, 1 the fastest and 4 the slowest.
The size of the generated fractal would also affect total processing time, as well as single threaded (slower) compared to multi threaded (faster).
Memtest1 and Memtest2 are included to generate resolution of 16000x16000 and 32000x32000, use it you want longer running elapsed times.
Memtest1 in combination with multi threaded and location 2 on a AMD Ryzen 9 5950X could take about 3-4 minutes.
When using save to memory the code will allocate enough memory to store the whole image (and can also save this to a file) or if you select none
then memory usage will be less of a factor comparing different computers total execution times.
The idea of iterate the same selected options is to find an average or a lowest execution time, for example running it 10 times.

Using FractBench.exe with arguments is using the same notation like the input entered with the program without arguments.

FractBench.exe 1353

Will create a 1080p image of the famous starting location of Mandelbrot fractal as output.dat file.

FractRaw2Png.exe 1920 1080

In the same directory after running the previous command will create output.png from output.dat
but typically to get better quality one would downsample (biqubic, bilinear, adaptive, ..) a larger image.
To get 1080p better quality instead generate 4k image then downsample (feature not included in FractRaw2Png).

FractBench.exe 223103

Will run the same calculations 10 times, in this case location 2, 720p, not saved to memory, multi threaded.

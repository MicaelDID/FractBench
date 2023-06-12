# FractBench

Simple Fractal Benchmark program, console based, .NET Framework 4.7.x (project mono compatible?)

## Features

Using
- either command line arguments or console key press
- integer / double math operations for Mandelbrot fractal calculations
- singel- or multithreaded code
- with / without allocating memory
- save to file option (raw dump of iterations in each coordinate, needs reprocessing for generating an image)
- with / without repeats (1-99 or endless)

## Utility

Included utilty (FractRaw2Png) to convert the saved raw dump to PNG.

## ToDo

- somehow be able to enter x / y location, diff and max iteration other than the 4 included with the program

## Example of usage

Starting FractBench without arguments is very descriptive. Just enter your selections from the presented alternatives.

Of the selectable starting locations 1 to 4 they need different processing time to be calculated, with 1 the fastest and 4 the slowest.

The size of the generated fractal would also affect total processing time, as well as single threaded (slower) compared to multi threaded (faster).

Memtest1 and Memtest2 are included to generate resolution of 16k x 16k and 32k x 32k, use it if you want longer running elapsed times.
Memtest1 in combination with multi threaded and location 2 on a AMD Ryzen 9 5950X could take about 3-4 minutes.

When using save to memory the code will allocate enough memory to store the whole image (and can also save this to a file) or if you select none
then memory usage will be less of a factor comparing different computers total execution times.

The idea of repeating the same selected options several times is to find an average or perhaps a lowest execution time, for example running it 10 times.

Using FractBench with arguments is using the same notation like the input entered with the program without arguments.

### Example 1

FractBench 1353

'1' Location 1 (normal / fast calculation intensity; famous Mandelbrot fractal starting location)

'3' a 1080p image (1920 x 1080)

'5' create output.dat file

'3' multi threaded calculation

FractRaw2Png 1920 1080

From the same directory after running the previous command (or move the output.dat to the same directory as FractRaw2Png executable is in)
will create output.png from output.dat using the same resolution.

To get 1080p better quality instead generate 4k or 8k image (3840 x 2160 or 7680 x 4320) then downsample (biqubic, bilinear, adaptive or similar) to 1080p (feature not included in FractRaw2Png).

### Example 2

FractBench 223103

'2' Location 2 (medium calculation intensity)

'2' a 720p image (1280 x 720)

'3' not saved to memory but with repeat

'10' repeat 10 times

'3' multi threaded calculation

## Results

```
AMD Ryzen9 5950X 32 Logical cores, RAM 2x16 GB, DIMM, 2133 Mhz
 2111,  5 845 ms, Location 2, Resolution 640 x 480, Save None, Single threaded
 2313,  1 712 ms, Location 2, Resolution 1920 x 1080, Save None, Multiple threaded (optimal)
 3313, 12 424 ms, Location 3, Resolution 1920 x 1080, Save None, Multiple threaded (optimal)
 4313, 43 204 ms, Location 4, Resolution 1920 x 1080, Save None, Multiple threaded (optimal)
 1723,  3 871 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
 1823, 11 781 ms, Location 1, Resolution 32000 x 32000, Save Memory, Multiple threaded (optimal)
 2523, 27 565 ms, Location 2, Resolution 7680 x 4320, Save Memory, Multiple threaded (optimal)
```

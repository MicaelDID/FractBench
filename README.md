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

## Example of usage

Starting FractBench without arguments is very descriptive. Just enter your selections from the presented alternatives.

Of the selectable starting locations 1 to 4 they need different processing time to be calculated, with 1 the fastest and 4 the slowest.
Location 5 makes it possible to read from location.txt what to process. If the file could be read its presented as location 5 otherwise 1.

Example of location.txt that on each new line contains: center x location, center y location, diff or width / height at the specified location, max iterations
```
-0,545205229166666
-0,601145622083333
0,0000005
5000
```

Note: depending on your windows localization settings it maybe be . instead of , that is used as decimal point.

The size of the generated fractal would also affect total processing time, as well as single threaded (slower) compared to multi threaded (faster).

Memtest1 and Memtest2 are included to generate resolution of 16k x 16k and 32k x 32k, use it if you want longer running elapsed times.
Memtest1 in combination with multi threaded and location 2 on a AMD Ryzen 9 5950X could take about 3-4 minutes.

When using save to memory the code will allocate enough memory to store the whole image (and can also save iterations to a file, raw format)
or if you select none then memory usage will be less of a factor comparing different computers total execution times. Extended information about
about allocated memory, min / max iterations and how many times max iterations was reached are displayed.

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

Build for Release, create and run a batch file like this (or just paste this into the command prompt).
```
FractBench 2111
FractBench 2313
FractBench 3313
FractBench 4313
FractBench 1723
FractBench 1823
FractBench 2523
```

### Desktop, AMD Ryzen9 5950X, 32 Logical Cores (16+16), RAM 2x16 GB, DIMM, 2133 Mhz
```
 2111,  5 845 ms, Location 2, Resolution   640 x   480, Save None, Single threaded
 2313,  1 712 ms, Location 2, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
 3313, 12 424 ms, Location 3, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
 4313, 43 204 ms, Location 4, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
 1723,  3 871 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
 1823, 11 781 ms, Location 1, Resolution 32000 x 32000, Save Memory, Multiple threaded (optimal)
 2523, 27 565 ms, Location 2, Resolution  7680 x  4320, Save Memory, Multiple threaded (optimal)
```

### Desktop, Intel Core i7 8700K, 12 Logical Cores (6+6), RAM 4x8 GB, DIMM, 2133 Mhz
```
2111,   9 872 ms, Location 2, Resolution   640 x   480, Save None, Single threaded
2313,   6 198 ms, Location 2, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
3313,  43 802 ms, Location 3, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
4313, 141 861 ms, Location 4, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
1723,   5 573 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
1823,  20 878 ms, Location 1, Resolution 32000 x 32000, Save Memory, Multiple threaded (optimal)
2523,  98 860 ms, Location 2, Resolution  7680 x  4320, Save Memory, Multiple threaded (optimal)
```

### Laptop (Lenovo), Intel Core i7 1255U, 12 Logical Cores (2+2 P, 8 E), RAM 2x16 GB, SODIMM, 3200 Mhz
```
 2111,  10 238 ms, Location 2, Resolution   640 x   480, Save None, Single threaded
 2313,   7 624 ms, Location 2, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
 3313,  63 975 ms, Location 3, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
 4313, 160 577 ms, Location 4, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
 1723,   7 437 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
 1823,  24 268 ms, Location 1, Resolution 32000 x 32000, Save Memory, Multiple threaded (optimal)
 2523, 113 855 ms, Location 2, Resolution  7680 x  4320, Save Memory, Multiple threaded (optimal)
```

### Laptop (MSI), Intel Core i7 9750H, 12 Logical Cores (6+6), RAM 2x8 GB, SODIMM, 2667 Mhz
```
2111,   9 061 ms, Location 2, Resolution   640 x   480, Save None, Single threaded
2313,   6 193 ms, Location 2, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
3313,  46 220 ms, Location 3, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
4313, 150 894 ms, Location 4, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
1723,   5 683 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
1823,  22 105 ms, Location 1, Resolution 32000 x 32000, Save Memory, Multiple threaded (optimal)
2523, 105 188 ms, Location 2, Resolution  7680 x  4320, Save Memory, Multiple threaded (optimal)
```

### Laptop (MSI), Intel Core i7 6700HQ, 8 Logical (4+4) , RAM 2x8 GB, SODIMM, 2133 Mhz
```
2111,  12 122 ms, Location 2, Resolution   640 x   480, Save None, Single threaded
2313,  12 294 ms, Location 2, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
3313,  92 506 ms, Location 3, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
4313, 282 490 ms, Location 4, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
1723,   9 354 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
1823,  35 796 ms, Location 1, Resolution 32000 x 32000, Save Memory, Multiple threaded (optimal)
2523, 192 768 ms, Location 2, Resolution  7680 x  4320, Save Memory, Multiple threaded (optimal)
```

### Laptop (Asus), Intel Core i5 2450M, 4 Logical Cores, RAM 6 GB
```
2111,  15 740 ms, Location 2, Resolution   640 x   480, Save None, Single threaded
2313,  32 698 ms, Location 2, Resolution  1920 x  1080, Save None, Multiple threaded (optimal)
1723,  21 029 ms, Location 1, Resolution 16000 x 16000, Save Memory, Multiple threaded (optimal)
2523, 492 494 ms, Location 2, Resolution  7680 x  4320, Save Memory, Multiple threaded (optimal)
```

### General return values
```
1223, Alloc   3,5 mb, Iteration Min      1, Max       989, MaxIter reached     49 045
2323, Alloc   7,9 mb, Iteration Min  9 043, Max    23 452, MaxIter reached          0
3223, Alloc   3,5 mb, Iteration Min 42 104, Max   301 127, MaxIter reached          0
4223, Alloc   3,5 mb, Iteration Min  7 748, Max 1 499 907, MaxIter reached     94 484
1723, Alloc 976,6 mb, Iteration Min      1, Max       999, MaxIter reached 24 166 805
1823, Alloc   3,8 gb, Iteration Min      1, Max       999, MaxIter reached 96 665 047
2523, Alloc 126,6 mb, Iteration Min  9 043, Max    31 782, MaxIter reached          0
```

Original code for calculating regression and correlation comes from here: https://gist.github.com/NikolayIT/d86118a3a0cb3f5ed63d674a350d75f2
I only changed it to use Vector instructions here and see what happens.

Results are a bit disappointing - the gains were not as impressive as expected - it is far from 4x expected from arithmetic operations only.
Perhaps memory management is still more expensive than arithmetic?
Using single precision numbers would speed it up further because we could fit twice as many numbers into each vector but it would make the numerical errors enormous
so here we only test double precision numbers.
Maybe AVX-512 instructions could do better but they are not supported by this not-so-old laptop... (I think they are aimed at server CPUs market?)
So here we only test 256 bit vectors from AVX2.

Output from Benchmark.Net:

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
12th Gen Intel Core i9-12950HX, 1 CPU, 24 logical and 16 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2


| Method                   | N       | Mean         | Error        | StdDev       | Allocated |
|------------------------- |-------- |-------------:|-------------:|-------------:|----------:|
| SimdVersion              | 1000    |     512.3 ns |     10.19 ns |     12.89 ns |         - |
| NonSimd_original_Version | 1000    |     612.3 ns |      7.54 ns |      7.05 ns |         - |
| SimdVersion              | 100000  |  60,216.7 ns |  1,128.81 ns |  1,055.89 ns |         - |
| NonSimd_original_Version | 100000  |  61,022.4 ns |    757.84 ns |    671.80 ns |         - |
| SimdVersion              | 1000000 | 766,346.8 ns | 12,409.13 ns | 11,000.37 ns |         - |
| NonSimd_original_Version | 1000000 | 630,424.7 ns | 11,195.85 ns | 10,472.61 ns |         - |

// * Hints *
Outliers
  Calculations.SimdVersion: Default              -> 1 outlier  was  removed (557.39 ns)
  Calculations.SimdVersion: Default              -> 1 outlier  was  removed (64.49 us)
  Calculations.NonSimd_original_Version: Default -> 1 outlier  was  removed (65.36 us)
  Calculations.SimdVersion: Default              -> 3 outliers were removed (797.58 us..803.95 us)
```

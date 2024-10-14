Original code for calculating regression and correlation comes from here: https://gist.github.com/NikolayIT/d86118a3a0cb3f5ed63d674a350d75f2
I only changed it to use Vector instructions here and see what happens.

Results are a bit disappointing - the gains were not as impressive as expected - it is far from 4x expected from arithmetic operations only.
Perhaps memory management is still more expensive than arithmetic? (which could explain worse result for 1M input array maybe?)
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
| SimdVersion              | 1000    |     521.5 ns |     10.34 ns |      9.67 ns |         - |
| NonSimd_original_Version | 1000    |     607.7 ns |      6.05 ns |      5.66 ns |         - |
| SimdVersion              | 100000  |  64,540.9 ns |  1,230.71 ns |  2,311.56 ns |         - |
| NonSimd_original_Version | 100000  |  67,312.9 ns |  1,337.35 ns |  2,197.31 ns |         - |
| SimdVersion              | 1000000 | 860,309.7 ns | 17,018.77 ns | 18,916.32 ns |         - |
| NonSimd_original_Version | 1000000 | 660,854.5 ns | 10,298.79 ns |  9,129.61 ns |         - |

// * Hints *
Outliers
  Calculations.SimdVersion: Default              -> 1 outlier  was  removed (570.37 ns)
  Calculations.SimdVersion: Default              -> 2 outliers were removed (70.99 us, 71.66 us)
  Calculations.NonSimd_original_Version: Default -> 2 outliers were removed (684.40 us, 693.58 us)
```

Original code for calculating regression and correlation comes from here: https://gist.github.com/NikolayIT/d86118a3a0cb3f5ed63d674a350d75f2
I only changed it to use Vector instructions here and see what happens.

Results are a bit disappointing - the gains were not as impressive as expected.
I don't want to use single precision numbers because the regression results are really bad but it could be faster with more numbers in each vector.
Maybe AVX-512 instructions could do better but they are not supported by this not-so-old laptop... (I guess they are targeting server CPUs)

Output forom Benchmark.Net:
```
// * Summary *

BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
12th Gen Intel Core i9-12950HX, 1 CPU, 24 logical and 16 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2


| Method                   | N      | Mean     | Error     | StdDev    | Median   | Allocated |
|------------------------- |------- |---------:|----------:|----------:|---------:|----------:|
| SimdVersion              | 1000   | 6.089 us | 0.1353 us | 0.3988 us | 5.960 us |         - |
| NonSimd_original_Version | 1000   | 6.151 us | 0.0805 us | 0.0753 us | 6.127 us |         - |
| SimdVersion              | 100000 | 5.330 us | 0.0336 us | 0.0314 us | 5.342 us |         - |
| NonSimd_original_Version | 100000 | 6.065 us | 0.0484 us | 0.0429 us | 6.070 us |         - |
```

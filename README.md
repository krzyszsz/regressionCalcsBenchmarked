Original code for calculating regression and correlation comes from here: https://gist.github.com/NikolayIT/d86118a3a0cb3f5ed63d674a350d75f2
I only changed it to use Vector instructions here and see what happens.

Results are a bit disappointing - the gains were not as impressive as expected - it is maybe 2 times faster but far from 4x.
Using single precision numbers would speed it up because we could fit twice as many numbers into each vector but it would make the numerical errors are enormous
so here we only have double precision numbers.
Maybe AVX-512 instructions could do better but they are not supported by this not-so-old laptop... (I think they are targeting server CPUs market?)

Output from Benchmark.Net:

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.22631.4317/23H2/2023Update/SunValley3)
12th Gen Intel Core i9-12950HX, 1 CPU, 24 logical and 16 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  Job-QPRFAL : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1

| Method                   | N       | Mean         | Error       | StdDev     | Allocated |
|------------------------- |-------- |-------------:|------------:|-----------:|----------:|
| SimdVersion              | 1000    |    12.069 us |   0.8817 us |   2.600 us |     400 B |
| NonSimd_original_Version | 1000    |     6.324 us |   0.3685 us |   1.027 us |     400 B |
| SimdVersion              | 100000  |   138.372 us |  13.2240 us |  38.991 us |     400 B |
| NonSimd_original_Version | 100000  |    83.707 us |   5.0455 us |  14.798 us |     400 B |
| SimdVersion              | 1000000 | 1,662.514 us | 110.1418 us | 317.784 us |     400 B |
| NonSimd_original_Version | 1000000 |   954.945 us |  51.3414 us | 149.765 us |     400 B |
```

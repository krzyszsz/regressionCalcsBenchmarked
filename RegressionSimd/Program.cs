/*
Original code for calculating regression and correlation comes from here: https://gist.github.com/NikolayIT/d86118a3a0cb3f5ed63d674a350d75f2
I only changed it to use Vector instructions here and see what happens.

Results are a bit disappointing - the gains were not as impressive as expected.
I don't want to use single precision numbers because the regression results are really bad but it could be faster with more numbers in each vector.
Maybe AVX-512 instructions could do better but they are not supported by this not-so-old laptop... (I guess they are targeting server CPUs)

Output forom Benchmark.Net:

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
*/

using System.Runtime.Intrinsics.X86;
using System.Runtime.Intrinsics;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;

Calculations.SelfTestEasyCase();
BenchmarkRunner.Run<Calculations>();

[MemoryDiagnoser]
public class Calculations
{
    private double[]? _firstDataSeriesForTest;
    private double[]? _secondDataSeriesForTest;

    [Params(1000, 100000)]
    public int N;

    [GlobalSetup]
    public void GlobalSetup()
    {
        const int n = 10000;
        var rand = new Random();
        _firstDataSeriesForTest = Enumerable.Range(0, n).Select(x => rand.NextDouble()).ToArray();
        _secondDataSeriesForTest = Enumerable.Range(0, n).Select(x => rand.NextDouble()).ToArray();
    }

    [Benchmark]
    public void SimdVersion()
    {
        LinearRegression(
            _firstDataSeriesForTest!,
            _secondDataSeriesForTest!,
            out float correlation,
            out float yIntercept,
            out float slope);
    }

    [Benchmark]
    public void NonSimd_original_Version()
    {
        NonSimd_original_LinearRegression(
            _firstDataSeriesForTest!,
            _secondDataSeriesForTest!,
            out double correlation,
            out double yIntercept,
            out double slope);
    }

    public static void LinearRegression(
        Memory<double> xValsMem,
        Memory<double> yValsMem,
        out float correlation,
        out float yIntercept,
        out float slope)
    {
        var xVals = xValsMem.Span;
        var yVals = yValsMem.Span;
        if (xVals.Length != yVals.Length)
        {
            throw new Exception("Input values should be with the same length.");
        }

        double sumOfX = SumVectorizedAvx2(xVals);
        double sumOfY = SumVectorizedAvx2(yVals);
        double sumOfXSq = SumSquaresVectorizedAvx2(xVals);
        double sumOfYSq = SumSquaresVectorizedAvx2(yVals);
        double sumCodeviates = SumOfMultiplicationsVectorizedAvx2(xVals, yVals);

        var count = xVals.Length;
        var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
        //var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

        var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
        var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
        var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

        var meanX = sumOfX / count;
        var meanY = sumOfY / count;
        correlation = (float)(double.IsFinite(rDenom) && rDenom != 0.0 ? rNumerator / Math.Sqrt(rDenom) : 0.0);

        //rSquared = dblR * dblR;
        yIntercept = (float)(meanY - ((sCo / ssX) * meanX));
        slope = (float)(sCo / ssX);

        // Warning: numerical errors creep up when you calculate this for a lot of data. For me it works good enough but the errors may be too big for you! Please double test.
    }

    public static void SelfTestEasyCase()
    {
        var rand = new Random(0);
        double[] exampleLinearInput;
        double[] exampleLinearOutput;
        for (var i = 2; i < 50; i++)
        {
            exampleLinearInput = Enumerable.Range(0, i).Select(x => rand.NextDouble()).ToArray();
            exampleLinearOutput = Enumerable.Range(0, i).Select(x => rand.NextDouble()).ToArray();

            var result = SumVectorizedAvx2(exampleLinearInput);
            var expectedResult = exampleLinearInput.Sum();
            if (Math.Abs(result - expectedResult) > 0.001)
            {
                throw new InvalidOperationException("SumSquaresVectorizedAvx2 is wrong.");
            }

            result = SumSquaresVectorizedAvx2(exampleLinearInput);
            expectedResult = exampleLinearInput.Sum(x => x * x);
            if (Math.Abs(result - expectedResult) > 0.001)
            {
                throw new InvalidOperationException("SumSquaresVectorizedAvx2 is wrong.");
            }

            result = SumOfMultiplicationsVectorizedAvx2(exampleLinearInput, exampleLinearOutput);
            expectedResult = exampleLinearInput.Zip(exampleLinearOutput).Select(x => x.First * x.Second).Sum();
            if (Math.Abs(result - expectedResult) > 0.001)
            {
                throw new InvalidOperationException("SumSquaresVectorizedAvx2 is wrong.");
            }
        }

        exampleLinearInput = new[] { 1.0, 2.0, 3.0 };
        exampleLinearOutput = new[] { -2.0 + 7.0, -4.0 + 7.0, -6.0 + 7.0 };

        LinearRegression(exampleLinearInput, exampleLinearOutput,
            out var correlation,
            out var yIntercept,
            out var slope);

        if (correlation != -1.0 || yIntercept != 7.0 || slope != -2.0)
        {
            throw new InvalidOperationException("Correlations incorrectly calculated.");
        }
    }

    static unsafe double SumVectorizedAvx2(ReadOnlySpan<double> source) // Based on https://gist.github.com/tannergooding/ed5783418a857c1cbeb95b5b0f95754e#file-sumvectorized-cs
    {
        double result;

        fixed (double* pSource = source)
        {
            Vector256<double> vresult = Vector256<double>.Zero;

            int i = 0;
            int lastBlockIndex = source.Length - (source.Length % 4);

            while (i < lastBlockIndex)
            {
                vresult = Avx2.Add(vresult, Avx2.LoadVector256(pSource + i));
                i += 4;
            }

            result = vresult[0] + vresult[1] + vresult[2] + vresult[3];

            while (i < source.Length)
            {
                result += pSource[i];
                i += 1;
            }
        }

        return result;
    }

    static unsafe double SumSquaresVectorizedAvx2(ReadOnlySpan<double> source)
    {
        double result;

        fixed (double* pSource = source)
        {
            Vector256<double> loaded = Vector256<double>.Zero;
            Vector256<double> vresult = Vector256<double>.Zero;

            int i = 0;
            int lastBlockIndex = source.Length - (source.Length % 4);

            while (i < lastBlockIndex)
            {
                loaded = Avx2.LoadVector256(pSource + i);
                loaded = Avx2.Multiply(loaded, loaded);
                vresult = Avx2.Add(vresult, loaded);

                i += 4;
            }

            result = vresult[0] + vresult[1] + vresult[2] + vresult[3];

            while (i < source.Length)
            {
                result += pSource[i] * pSource[i];
                i += 1;
            }
        }

        return result;
    }

    static unsafe double SumOfMultiplicationsVectorizedAvx2(ReadOnlySpan<double> source1, ReadOnlySpan<double> source2)
    {
        if (source1.Length != source2.Length)
        {
            throw new ArgumentOutOfRangeException("source1 vs source2");
        }
        double result;

        fixed (double* pSource1 = source1)
        fixed (double* pSource2 = source2)
        {
            Vector256<double> loaded1 = Vector256<double>.Zero;
            Vector256<double> loaded2 = Vector256<double>.Zero;
            Vector256<double> vresult = Vector256<double>.Zero;

            int i = 0;
            int lastBlockIndex = source1.Length - (source1.Length % 4);

            while (i < lastBlockIndex)
            {
                loaded1 = Avx2.LoadVector256(pSource1 + i);
                loaded2 = Avx2.LoadVector256(pSource2 + i);
                loaded1 = Avx2.Multiply(loaded1, loaded2);
                vresult = Avx2.Add(vresult, loaded1);

                i += 4;
            }

            result = vresult[0] + vresult[1] + vresult[2] + vresult[3];

            while (i < source1.Length)
            {
                result += pSource1[i] * pSource2[i];
                i += 1;
            }
        }

        return result;
    }

    public static void NonSimd_original_LinearRegression(
            double[] xVals,
            double[] yVals,
            out double rSquared,
            out double yIntercept,
            out double slope)
    {
        if (xVals.Length != yVals.Length)
        {
            throw new Exception("Input values should be with the same length.");
        }

        double sumOfX = 0;
        double sumOfY = 0;
        double sumOfXSq = 0;
        double sumOfYSq = 0;
        double sumCodeviates = 0;

        for (var i = 0; i < xVals.Length; i++)
        {
            var x = xVals[i];
            var y = yVals[i];
            sumCodeviates += x * y;
            sumOfX += x;
            sumOfY += y;
            sumOfXSq += x * x;
            sumOfYSq += y * y;
        }

        var count = xVals.Length;
        var ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
        var ssY = sumOfYSq - ((sumOfY * sumOfY) / count);

        var rNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
        var rDenom = (count * sumOfXSq - (sumOfX * sumOfX)) * (count * sumOfYSq - (sumOfY * sumOfY));
        var sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

        var meanX = sumOfX / count;
        var meanY = sumOfY / count;
        var dblR = rNumerator / Math.Sqrt(rDenom);

        rSquared = dblR * dblR;
        yIntercept = meanY - ((sCo / ssX) * meanX);
        slope = sCo / ssX;
    }
}

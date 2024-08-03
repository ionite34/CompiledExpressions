using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.NativeAot;
using CompiledExpressions.Benchmarks;

var baseJob = Job.Default.WithWarmupCount(30).WithIterationCount(1000);

var config = DefaultConfig
    .Instance
    .AddJob(baseJob.WithJit(Jit.RyuJit))
    .AddJob(
        baseJob.WithToolchain(
            NativeAotToolchain.CreateBuilder().UseNuGet().IlcInstructionSet("native").ToToolchain()
        )
    )
    .CreateImmutableConfig();

// var summaries = BenchmarkRunner.Run([typeof(AccessorBenchmark), typeof(AccessorInvokeBenchmark)], config);
var summaries = new[] { BenchmarkRunner.Run<AccessorInvokeBenchmark>(config) };

var compositeAnalyser = config.GetCompositeAnalyser();
var logger = ConsoleLogger.Default;

foreach (var summary in summaries)
{
    logger.WriteLineHeader("// * Summary *");
    MarkdownExporter.Console.ExportToLog(summary, logger);
    ConclusionHelper.Print(logger, compositeAnalyser.Analyse(summary).ToList());
}

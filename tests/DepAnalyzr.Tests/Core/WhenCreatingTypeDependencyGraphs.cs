﻿using DepAnalyzr.Core;
using Xunit;
using Xunit.Abstractions;

namespace DepAnalyzr.Tests.Core;

[Collection(nameof(LibCAnalyzedCollection))]
public class WhenCreatingTypeDependencyGraphs
{
    private readonly LibCAnalyzedScenario _libCAnalyzedScenario;
    private readonly ITestOutputHelper _output;

    public WhenCreatingTypeDependencyGraphs(LibCAnalyzedScenario libCAnalyzedScenario, ITestOutputHelper output)
    {
        _libCAnalyzedScenario = libCAnalyzedScenario;
        _output = output;
    }

    // Note: dotnet test --logger "console;verbosity=detailed"

    [Fact]
    public void NonFilteredRelationshipsAreDetected()
    {
        var analysisResult = _libCAnalyzedScenario.AnalysisResult;
        var depGraph = DependencyGraph.CreateForTypes(analysisResult, null);
        var graphVizDotFormat = depGraph.ToGraphvizDotFormat();

        _output.WriteLine(graphVizDotFormat);

        const string expectedGraphVizDotFormat = @"digraph G {
0 [shape=box, label=""DepAnalyzr.Tests.LibA.LibAType01""];
1 [shape=box, label=""DepAnalyzr.Tests.LibB.LibBType01""];
2 [shape=box, label=""DepAnalyzr.Tests.LibC.LibCType01""];
1 -> 0 [];
2 -> 0 [];
2 -> 1 [];
}
";

        Assert.Equal(graphVizDotFormat, expectedGraphVizDotFormat);
    }

    [Fact]
    public void FilteredRelationshipsAreDetected()
    {
        var analysisResult = _libCAnalyzedScenario.AnalysisResult;
        const string pattern = "(DepAnalyzr.Tests.LibA|DepAnalyzr.Tests.LibC)";
        var depGraph = DependencyGraph.CreateForTypes(analysisResult, pattern);
        var graphvizDotFormat = depGraph.ToGraphvizDotFormat();

        _output.WriteLine(graphvizDotFormat);

        const string expectedGraphVizDotFormat = @"digraph G {
0 [shape=box, label=""DepAnalyzr.Tests.LibA.LibAType01""];
1 [shape=box, label=""DepAnalyzr.Tests.LibC.LibCType01""];
1 -> 0 [];
}
";

        Assert.Equal(graphvizDotFormat, expectedGraphVizDotFormat);
    }

    [Fact]
    public void SvgGraphCanBeGenerated()
    {
        var analysisResult = _libCAnalyzedScenario.AnalysisResult;
        var depGraph = DependencyGraph.CreateForTypes(analysisResult, null);
        var graphvizSvgFormat = depGraph.ToGraphvizSvgFormat();

        _output.WriteLine(graphvizSvgFormat);
        Assert.Contains("<!-- Generated by graphviz", graphvizSvgFormat);
    }
}
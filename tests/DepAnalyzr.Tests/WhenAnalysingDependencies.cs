﻿using System.Collections.Generic;
using System.Linq;
using DepAnalyzr.Domain.Models;
using Mono.Cecil;
using Xunit;
using Xunit.Abstractions;

namespace DepAnalyzr.Tests;

public class WhenAnalysingDependencies : IClassFixture<LibCBuiltScenario>
{
    private readonly LibCBuiltScenario _libCBuiltScenario;
    private readonly ITestOutputHelper _testOutputHelper;

    public WhenAnalysingDependencies(LibCBuiltScenario libCBuiltScenario, ITestOutputHelper testOutputHelper)
    {
        _libCBuiltScenario = libCBuiltScenario;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ExploreMonoCecilDefinitions()
    {
        var libAType01 = _libCBuiltScenario.AssemblyADefinition.MainModule.Types.Single(x => x.Name == "LibAType01");
        var libBType01 = _libCBuiltScenario.AssemblyBDefinition.MainModule.Types.Single(x => x.Name == "LibBType01");
        var libCType01 = _libCBuiltScenario.AssemblyCDefinition.MainModule.Types.Single(x => x.Name == "LibCType01");
        var typeDefSet = new HashSet<TypeDefinition>(new[] { libAType01, libBType01, libCType01 });
        var methodDefDependenciesByMethodDef = Analyser.AnalyseMethodDependencies(typeDefSet);

        Assert.NotEmpty(methodDefDependenciesByMethodDef);
        _testOutputHelper.WriteLine($"num: {methodDefDependenciesByMethodDef.Count}");

        foreach (var kvp in methodDefDependenciesByMethodDef)
        {
            _testOutputHelper.WriteLine(new string('=', 80));
            _testOutputHelper.WriteLine(kvp.Key);
            _testOutputHelper.WriteLine(kvp.Value.Count.ToString());

            foreach (var dependencyMethodDef in kvp.Value)
            {
                _testOutputHelper.WriteLine(new string('-', 80));
                _testOutputHelper.WriteLine(dependencyMethodDef.ToString());
            }
        }
    }
}
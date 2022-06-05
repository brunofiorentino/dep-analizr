using DepAnalyzr.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace DepAnalyzr.Core;

public sealed class Analyzer
{
    private readonly IndexedDefinitions _indexedDefinitions;

    public Analyzer(IndexedDefinitions indexedDefinitions) =>
        _indexedDefinitions =
            indexedDefinitions ?? throw new ArgumentNullException(nameof(indexedDefinitions));

    public AnalysisResult Analyze()
    {
        Dictionary<string, IReadOnlySet<string>>
            methodDefDependenciesByKey = new(),
            typeDefDependenciesByKey = new(),
            assemblyDefDependenciesByKey = new();

        foreach (var assemblyDef in _indexedDefinitions.AssemblyDefsByKey.Values)
        {
            var assemblyDefDependencies = new HashSet<string>();
            assemblyDefDependenciesByKey[assemblyDef.Key()] = assemblyDefDependencies;

            foreach (var typeDef in assemblyDef
                         .MainModule.Types
                         .Where(x => IndexedDefinitions.NotModuleTypeDefinitionKey(x.Key())))
            {
                var typeDefDependencies = new HashSet<string>();
                typeDefDependenciesByKey[typeDef.Key()] = typeDefDependencies;

                foreach (var methodDef in typeDef.Methods)
                {
                    var methodDefDependencies = new HashSet<string>();
                    methodDefDependenciesByKey[methodDef.Key()] = methodDefDependencies;

                    foreach (var instruction in methodDef.Body.Instructions)
                    {
                        var (hasDependency, dependencyMethodDef) = AnalyseInstruction(instruction);
                        if (!hasDependency) continue;

                        methodDefDependencies.Add(dependencyMethodDef!.Key());
                        typeDefDependencies.Add(dependencyMethodDef!.DeclaringType.Key());
                        assemblyDefDependencies.Add(dependencyMethodDef.DeclaringType.Module.Assembly.Key());
                    }
                }
            }
        }

        return new AnalysisResult(
            _indexedDefinitions, methodDefDependenciesByKey,
            typeDefDependenciesByKey, assemblyDefDependenciesByKey);
    }

    private (bool hasDependency, MethodDefinition? dependencyMethodDef) AnalyseInstruction(Instruction instruction)
    {
        Func<string, bool> containsByteCode = instruction.ToString().Contains;
        var hasDependencyByteCode =
            containsByteCode(": call ") || containsByteCode(": callvirt ") || containsByteCode(": newobj ");

        if (!hasDependencyByteCode) return (false, null);

        var depMethodRef = (MethodReference)instruction.Operand;
        var depMethodDef = depMethodRef.Resolve();

        if (!IndexedDefinitions.NotModuleTypeDefinitionKey(depMethodDef.DeclaringType.Key()))
            return (false, null);

        var depMethodAssemblyDef = depMethodDef.Module.Assembly;
        var isDefinedInAnalysisAssembliesSet = _indexedDefinitions.AssemblyDefsByKey
            .ContainsKey(depMethodAssemblyDef.Key());

        return isDefinedInAnalysisAssembliesSet ? (true, depMethodDef) : (false, null);
    }
}
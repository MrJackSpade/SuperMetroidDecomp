using Microsoft.CodeAnalysis;

namespace SuperMetroid.Generators;

/// <summary>
/// #2602: the members a debugger state reads, writes and restores the game's objects through, written at compile time so
/// a state names every field, type and method it holds in code instead of reaching them by reflection. In an assembly
/// marked <c>[assembly: StateGraphAssembly]</c> every class, struct and record is partial, and each gets: a constructor
/// restoring it from a state, methods writing its fields and handing the fields that can reach other objects to a visitor,
/// and a registration naming it, the collections, arrays and enums its fields are declared with, and the methods delegates
/// are made from. With <c>Catalogs</c> the assembly also registers its static read-only members as definition catalogs and
/// what each may hold (#1623, #1716). See <see cref="StateGraphRun"/>.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class StateGraphGenerator : IIncrementalGenerator
{
    /// <summary>The attribute marking an assembly whose types get the members.</summary>
    internal const string AttributeName = "SuperMetroid.Core.StateGraphAssemblyAttribute";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterSourceOutput(context.CompilationProvider, static (output, compilation) => Execute(output, compilation));

    /// <summary>Writes the members of every type of a marked compilation.</summary>
    private static void Execute(SourceProductionContext output, Compilation compilation)
    {
        AttributeData? marker = compilation.Assembly.GetAttributes()
            .FirstOrDefault(attribute => attribute.AttributeClass?.ToDisplayString() == AttributeName);
        if (marker is null)
            return;
        new StateGraphRun(compilation, output, Named(marker, "PartialTypesOnly"), Named(marker, "Catalogs")).Generate();
    }

    /// <summary>The value of a named Boolean argument of an attribute, false when it is not given.</summary>
    private static bool Named(AttributeData attribute, string name) =>
        attribute.NamedArguments.Any(argument => argument.Key == name && argument.Value.Value is true);
}

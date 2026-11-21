using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// The constructors the game's code declares and calls. A type of a state-graph assembly also has the constructor the
/// compiler writes to restore it from a debugger state (#2602), taking an <c>IStateGraphReader</c>; no frame comes from
/// it, so the walks never follow it.
/// </summary>
internal static class GameConstructors
{
    /// <summary>The type's instance constructors, without the restore constructor the compiler writes for a state.</summary>
    internal static IEnumerable<IMethodSymbol> Of(INamedTypeSymbol type) => type.InstanceConstructors.Where(constructor => !IsRestore(constructor));

    /// <summary>Whether <paramref name="reference"/> lies in code the compiler writes for debugger states, which makes no frame.</summary>
    internal static bool IsCompilerWritten(SyntaxReference reference) =>
        reference.SyntaxTree.FilePath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether <paramref name="constructor"/> is the restore constructor the compiler writes for a state.</summary>
    internal static bool IsRestore(IMethodSymbol constructor) =>
        constructor.Parameters is [{ Type: { Name: "IStateGraphReader", ContainingNamespace: { Name: "Core", ContainingNamespace.Name: "SuperMetroid" } } }, ..];
}

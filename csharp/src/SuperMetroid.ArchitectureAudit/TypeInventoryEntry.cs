using Microsoft.CodeAnalysis;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>A declared type with every assembly and file that compiles it, and the compilation it was first found in.</summary>
/// <param name="Symbol">The symbol.</param>
/// <param name="Assemblies">The assemblies.</param>
/// <param name="Files">The files.</param>
/// <param name="Compilation">The compilation.</param>
internal readonly record struct TypeInventoryEntry(INamedTypeSymbol Symbol, SortedSet<string> Assemblies, SortedSet<string> Files, Compilation Compilation);

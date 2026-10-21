using Microsoft.CodeAnalysis;

namespace SuperMetroid.ArchitectureAudit;

/// <summary>A declared type and the compilation that declares it.</summary>
/// <param name="Symbol">The symbol.</param>
/// <param name="Compilation">The compilation.</param>
internal readonly record struct CompiledType(INamedTypeSymbol Symbol, Compilation Compilation);

using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>What a unit of reached code is.</summary>
internal enum UnitKind
{
    /// <summary>One member: a method, constructor, property, field or event, with its declaration's code.</summary>
    Member,
    /// <summary>A type's static initialization: its static field and property initializers and static constructor.</summary>
    StaticInitialization,
    /// <summary>A type's instance field and property initializers, which every non-chaining constructor runs.</summary>
    InstanceInitialization,
}

/// <summary>One node of Core's call graph: a member, or a type's static or instance initialization.</summary>
internal readonly struct Unit : IEquatable<Unit>
{
    /// <summary>Creates a unit.</summary>
    private Unit(UnitKind kind, ISymbol symbol)
    {
        Kind = kind;
        Symbol = symbol;
    }

    /// <summary>What the unit is.</summary>
    public UnitKind Kind { get; }

    /// <summary>The member, or for an initialization the type.</summary>
    public ISymbol Symbol { get; }

    /// <summary>The type whose code the unit is.</summary>
    public INamedTypeSymbol Type => Kind == UnitKind.Member
        ? Symbol.ContainingType ?? throw new InvalidOperationException($"{Symbol} has no containing type.")
        : (INamedTypeSymbol)Symbol;

    /// <summary>Whether the unit is an instance or static constructor.</summary>
    public bool IsConstructor => Kind == UnitKind.Member && Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor };

    /// <summary>The unit of one member.</summary>
    public static Unit Member(ISymbol member) => new(UnitKind.Member, member);

    /// <summary>The unit of a type's static initialization.</summary>
    public static Unit StaticInitialization(INamedTypeSymbol type) => new(UnitKind.StaticInitialization, type.OriginalDefinition);

    /// <summary>The unit of a type's instance initializers.</summary>
    public static Unit InstanceInitialization(INamedTypeSymbol type) => new(UnitKind.InstanceInitialization, type.OriginalDefinition);

    /// <inheritdoc/>
    public bool Equals(Unit other) => Kind == other.Kind && SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Unit other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, SymbolEqualityComparer.Default.GetHashCode(Symbol));

    /// <inheritdoc/>
    public override string ToString() => Kind switch
    {
        UnitKind.Member => Symbol.ToDisplayString(),
        UnitKind.StaticInitialization => $"{Symbol.ToDisplayString()} (static initialization)",
        _ => $"{Symbol.ToDisplayString()} (instance initializers)",
    };
}

using System.Collections.Frozen;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The authored values of a parameter that select one variant: a population parameter, a spawn argument or the
/// selector a projectile carries. The data names a variant by a small value; each owner keeps one of these tables
/// pairing its values with its variants, so the variant identities carry no numeric meaning of their own.
/// </summary>
/// <typeparam name="TVariant">The species' variant identity.</typeparam>
internal sealed class ParameterVariants<TVariant>
    where TVariant : struct, Enum
{
    /// <summary>Each authored parameter value's variant.</summary>
    private readonly FrozenDictionary<int, TVariant> _variants;

    /// <summary>Each variant's authored parameter value.</summary>
    private readonly FrozenDictionary<TVariant, int> _parameters;

    /// <summary>Keeps <paramref name="variants"/>, the variant each authored parameter value selects; each variant has one value.</summary>
    internal ParameterVariants(IReadOnlyDictionary<int, TVariant> variants)
    {
        _variants = variants.ToFrozenDictionary();
        _parameters = variants.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);
    }

    /// <summary>The variant an authored parameter value selects.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value selects no variant.</exception>
    internal TVariant Decode(int parameter) => _variants.TryGetValue(parameter, out TVariant variant)
        ? variant
        : throw new ArgumentOutOfRangeException(nameof(parameter), parameter,
            $"Parameter {parameter} selects no {typeof(TVariant).Name}.");

    /// <summary>The variant an authored parameter value selects, when it selects one.</summary>
    internal bool TryDecode(int parameter, out TVariant variant) => _variants.TryGetValue(parameter, out variant);

    /// <summary>The authored parameter value that selects <paramref name="variant"/>.</summary>
    internal int Parameter(TVariant variant) => _parameters[variant];
}

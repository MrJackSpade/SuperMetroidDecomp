using System.Collections.Frozen;

namespace SuperMetroid.AssetExtraction.Enemies;

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

    /// <summary>Keeps <paramref name="variants"/>, the variant each authored parameter value selects.</summary>
    internal ParameterVariants(IReadOnlyDictionary<int, TVariant> variants) => _variants = variants.ToFrozenDictionary();

    /// <summary>The variant an authored parameter value selects, when it selects one.</summary>
    internal bool TryDecode(int parameter, out TVariant variant) => _variants.TryGetValue(parameter, out variant);
}

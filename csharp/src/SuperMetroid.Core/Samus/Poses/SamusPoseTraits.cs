using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What each pose defines (#2323), from its definition (<see cref="SamusPoseDefinitions"/>).</summary>
internal static partial class SamusPoseTraits
{
    /// <summary>The definition of <paramref name="pose"/>.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseDefinition Of(SamusPoseId pose) =>
        SamusPoseDefinitions.Poses.TryGetValue(pose, out SamusPoseDefinition? definition) ? definition
        : throw new InvalidOperationException($"Undefined {nameof(SamusPoseId)} {pose}.");

    /// <summary>The poses of each family (<see cref="SamusPoseFamilyDefinitions.Members"/>), each family's as a set.</summary>
    private static readonly FrozenDictionary<SamusPoseFamily, FrozenSet<SamusPoseId>> Families =
        SamusPoseFamilyDefinitions.Members.ToFrozenDictionary(entry => entry.Key, entry => entry.Value.ToFrozenSet());

    /// <summary>Whether <paramref name="pose"/> belongs to <paramref name="family"/>.</summary>
    /// <exception cref="KeyNotFoundException">The family lists no poses.</exception>
    internal static bool IsIn(SamusPoseId pose, SamusPoseFamily family) => Families[family].Contains(pose);
}

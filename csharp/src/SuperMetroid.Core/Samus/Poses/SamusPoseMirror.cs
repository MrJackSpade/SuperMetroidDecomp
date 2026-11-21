using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Reflects a pose across the vertical axis, so a rule written for one facing serves the other.</summary>
internal static class SamusPoseMirror
{
    /// <summary>Each facing pose's mirror, both ways round.</summary>
    private static readonly FrozenDictionary<SamusPoseId, SamusPoseId> Opposites = SamusPoseMirrorDefinitions.Pairs
        .SelectMany(pair => new[] { KeyValuePair.Create(pair.First, pair.Second), KeyValuePair.Create(pair.Second, pair.First) })
        .ToFrozenDictionary();

    /// <summary>The same pose facing the other way; a pose without a facing is its own mirror.</summary>
    internal static SamusPoseId Of(SamusPoseId pose) => Opposites.TryGetValue(pose, out SamusPoseId opposite) ? opposite : pose;
}

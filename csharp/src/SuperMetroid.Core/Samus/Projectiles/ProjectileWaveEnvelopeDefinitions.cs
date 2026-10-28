namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed native Wave-lobe placement basis, shared by stock artwork and physical envelopes.
/// Independent imported artwork edits never change collision mechanics. The five source offsets
/// remain required under ProjectileSpriteCatalog.frames; sharing them grants no art exemption.</summary>
internal static class ProjectileWaveEnvelopeDefinitions
{
    /// <summary>The value each key selects; AxialLobeDistance looks it up.</summary>
    internal static readonly IReadOnlyDictionary<int, int> AxialLobeDistances = new Dictionary<int, int>
    {
        [0] = 8,
        [1] = 13,
        [2] = 15,
        [3] = 16,
    };

    /// <summary>$93:D64A: selected initial Spazer lane distance 4, preceding the four Wave distances.
    /// REQUIRED artwork input; its numerical equality with a half-tile does not dispose of its selection.</summary>
    internal const int SpazerInitialAxialSpread = 4;
}

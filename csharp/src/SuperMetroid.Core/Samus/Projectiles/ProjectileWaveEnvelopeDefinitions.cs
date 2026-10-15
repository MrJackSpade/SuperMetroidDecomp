namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Fixed native Wave-lobe placement basis, shared by stock artwork and physical envelopes.
/// Independent imported artwork edits never change collision mechanics. The five source offsets
/// remain required under ProjectileSpriteCatalog.frames; sharing them grants no art exemption.</summary>
internal static class ProjectileWaveEnvelopeDefinitions
{

    /// <summary>$93:AE70/AE77/AE7E/AE85: four outward axial lobe offsets 8/13/15/16.
    /// These selected geometric source inputs remain REQUIRED under the artwork owner.</summary>
    internal static int AxialLobeDistance(int outwardStep) => outwardStep switch
    {
        0 => 8, 1 => 13, 2 => 15, 3 => 16,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$93:D64A: selected initial Spazer lane distance 4, preceding the four Wave distances.
    /// REQUIRED artwork input; its numerical equality with a half-tile does not dispose of its selection.</summary>
    internal const int SpazerInitialAxialSpread = 4;
}

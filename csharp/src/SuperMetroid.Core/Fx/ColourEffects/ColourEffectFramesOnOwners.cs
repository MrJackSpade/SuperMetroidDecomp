namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The frames the colour effects showed on their owners' draws during one update, in order (#1512, #1783, #2288, #2544):
/// Samus's suit's, the escape's shutters' and each glowing enemy's. The room's and a screen's pictures take their shades
/// from the effects' frames directly, so none of theirs is kept here.
/// </summary>
internal sealed class ColourEffectFramesOnOwners
{
    /// <summary>The frames Samus's suit groups showed, in order.</summary>
    internal List<ShownColourEffectFrame> Suit { get; } = [];

    /// <summary>The frames the escape's shutter flash showed, in order.</summary>
    internal List<ColourEffectFrame> ShutterFlash { get; } = [];

    /// <summary>The frames the effects enemies show showed, in order, each for the enemy its start handed it.</summary>
    internal List<ShownEnemyGlow> Glows { get; } = [];

    /// <summary>Forgets the previous update's frames as a new update's turns begin.</summary>
    internal void Clear()
    {
        Suit.Clear();
        ShutterFlash.Clear();
        Glows.Clear();
    }
}

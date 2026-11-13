namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Zoomer, Ripper and Skree corpses' rot-trigger reactions.</summary>
internal static class DeadTourianCorpseProfileDefinitions
{
    /// <summary>Dead Zoomer's rot-trigger reactions.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        TouchAndShot: EnemyReaction.DeadZoomerTouchAndShot,
        PowerBomb: EnemyReaction.DeadZoomerPowerBomb);

    /// <summary>Dead Ripper's rot-trigger reactions.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        TouchAndShot: EnemyReaction.DeadRipperTouchAndShot,
        PowerBomb: EnemyReaction.DeadRipperPowerBomb);

    /// <summary>Dead Skree's rot-trigger reactions.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadSkreeProfile = new(
        DeadTourianCorpseSpecies.Skree,
        TouchAndShot: EnemyReaction.DeadSkreeTouchAndShot,
        PowerBomb: EnemyReaction.DeadSkreePowerBomb);
}

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Zoomer, Ripper and Skree corpses' header rot-trigger callbacks and their numbers of corpse variants.</summary>
internal static class DeadTourianCorpseProfileDefinitions
{
    /// <summary>Dead Zoomer's rot-trigger callbacks and three corpse variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        TouchAndShot: EnemyReaction.DeadZoomerTouchAndShot,
        PowerBomb: EnemyReaction.DeadZoomerPowerBomb,
        VariantCount: 3);

    /// <summary>Dead Ripper's rot-trigger callbacks and two corpse variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        TouchAndShot: EnemyReaction.DeadRipperTouchAndShot,
        PowerBomb: EnemyReaction.DeadRipperPowerBomb,
        VariantCount: 2);

    /// <summary>Dead Skree's rot-trigger callbacks and three corpse variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadSkreeProfile = new(
        DeadTourianCorpseSpecies.Skree,
        TouchAndShot: EnemyReaction.DeadSkreeTouchAndShot,
        PowerBomb: EnemyReaction.DeadSkreePowerBomb,
        VariantCount: 3);
}

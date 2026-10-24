namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Zoomer, Ripper and Skree corpses' header rot-trigger callbacks and their numbers of corpse variants.</summary>
internal static class DeadTourianCorpseProfileDefinitions
{
    /// <summary>Dead Zoomer's rot-trigger callbacks and three corpse variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadZoomerProfile = new(
        DeadTourianCorpseSpecies.Zoomer,
        TouchAndShot: EnemyInteractionCallback.DeadZoomerTouchAndShot,
        PowerBomb: EnemyInteractionCallback.DeadZoomerPowerBomb,
        VariantCount: 3);

    /// <summary>Dead Ripper's rot-trigger callbacks and two corpse variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadRipperProfile = new(
        DeadTourianCorpseSpecies.Ripper,
        TouchAndShot: EnemyInteractionCallback.DeadRipperTouchAndShot,
        PowerBomb: EnemyInteractionCallback.DeadRipperPowerBomb,
        VariantCount: 2);

    /// <summary>Dead Skree's rot-trigger callbacks and three corpse variants.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadSkreeProfile = new(
        DeadTourianCorpseSpecies.Skree,
        TouchAndShot: EnemyInteractionCallback.DeadSkreeTouchAndShot,
        PowerBomb: EnemyInteractionCallback.DeadSkreePowerBomb,
        VariantCount: 3);
}

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The dead Zoomer, Ripper and Skree corpses' profiles. Each corpse's definition leaves its touch, shot and Power Bomb
/// reactions to the species, whose entity starts the rot (#1759).
/// </summary>
internal static class DeadTourianCorpseProfileDefinitions
{
    /// <summary>Dead Zoomer's profile.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadZoomerProfile = new(DeadTourianCorpseSpecies.Zoomer);

    /// <summary>Dead Ripper's profile.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadRipperProfile = new(DeadTourianCorpseSpecies.Ripper);

    /// <summary>Dead Skree's profile.</summary>
    internal static readonly DeadTourianBehavior.DeadTourianCorpseProfile DeadSkreeProfile = new(DeadTourianCorpseSpecies.Skree);
}

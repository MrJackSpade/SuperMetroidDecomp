namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Fixed generation and respawn definitions for Tourian's Zebetites.</summary>
internal static class ZebetiteDefinitions
{
    /// <summary>
    /// The four barrier generations, indexed by how many Zebetites have been destroyed (#2012): large barriers and split
    /// pairs alternate twelve blocks apart from the right, all on one centre line.
    /// </summary>
    internal static IReadOnlyList<ZebetiteGenerationDefinition> Generations { get; } =
    [
        new(ZebetiteBarrierForm.Single, X: 824, CentreY: 111, HalfHeight: 24),
        new(ZebetiteBarrierForm.SplitPair, X: 632, CentreY: 111, HalfHeight: 8),
        new(ZebetiteBarrierForm.Single, X: 440, CentreY: 111, HalfHeight: 24),
        new(ZebetiteBarrierForm.SplitPair, X: 248, CentreY: 111, HalfHeight: 8),
    ];

    /// <summary>How far each half of a split pair stands from its generation's centre line: the primary above, the linked below.</summary>
    internal const int SplitHalfOffset = 40;

    /// <summary>The health an active Zebetite regenerates up to, one each update, and starts its picture from.</summary>
    internal const int MaximumHealth = 1000;

    /// <summary>The sound a Zebetite makes when a shot hits it.</summary>
    internal static readonly SoundEffectId ShotSound = RoomSound.MissileDoorHit;

}

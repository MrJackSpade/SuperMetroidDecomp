namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How an ordinary Metroid flees when bombed off Samus, and which cry it plays.</summary>
internal static class MetroidReactions
{
    /// <summary>The escape displacement at <paramref name="countdown"/>'s phase.</summary>
    internal static Offset16<RoomPixels> EscapeDisplacement(int countdown) =>
        MetroidBehaviorDefinitions.EscapeDisplacements[MetroidCadenceDefinitions.EscapeDisplacementStep.Of(countdown)];

    /// <summary>The cry <paramref name="random"/>'s <see cref="MetroidBehaviorDefinitions.CrySlot"/> selects.</summary>
    internal static SoundEffectId RandomCrySoundEffect(int random) =>
        MetroidBehaviorDefinitions.RandomCries[MetroidBehaviorDefinitions.CrySlot.Of(random)];
}

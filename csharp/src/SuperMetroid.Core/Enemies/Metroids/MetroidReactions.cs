namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>How an ordinary Metroid flees when bombed off Samus, and which cry it plays.</summary>
internal static class MetroidReactions
{
    /// <summary>The escape displacement at <paramref name="countdown"/>'s phase.</summary>
    internal static Offset16<RoomPixels> EscapeDisplacement(int countdown) =>
        MetroidBehaviorDefinitions.EscapeDisplacements[countdown & 3];

    /// <summary>The cry <paramref name="random"/>'s low three bits select.</summary>
    internal static SoundEffectId RandomCrySoundEffect(int random) => MetroidBehaviorDefinitions.RandomCries[random & 7];
}

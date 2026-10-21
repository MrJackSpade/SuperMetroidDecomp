namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Mochtroid flight/attachment pose identities; independent artwork and timing stay separate.</summary>
internal static class MochtroidVisualDefinitions
{
    /// <summary>$A3: Mochtroid instruction and spritemap bank.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>$A3:A9B0: first free-flight composition; the first two records each have six parts.</summary>
    private const ushort FlightFrame = 0xa9b0;
    /// <summary>$A3:AA06: first attached composition; each preceding record has four parts.</summary>
    private const ushort AttachedFrame = 0xaa06;

    /// <summary>Publishes three free-flight and three attached poses in their native pointer order.</summary>
    internal static IEnumerable<NativeEnemySpritemapDefinition> Frames()
    {
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(FlightFrame, FlightParts, pose), $"mochtroid_flight_{pose}");
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(AttachedFrame, AttachedParts, pose), $"mochtroid_attached_{pose}");
    }

    /// <summary>Parts in each free-flight pose record.</summary>
    private const int FlightParts = 6;

    /// <summary>Parts in each attached pose record.</summary>
    private const int AttachedParts = 4;

    /// <summary>Steps through pose records of a two-byte count followed by five bytes per part.</summary>
    private static ushort PosePointer(ushort first, int parts, int pose) =>
        checked((ushort)(first + pose * (2 + 5 * parts)));
}

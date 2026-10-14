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
    internal static IEnumerable<EnemySpritemapDefinition> Frames()
    {
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(false, pose), $"mochtroid_flight_{pose}");
        for (int pose = 0; pose < 3; pose++)
            yield return new(Bank, PosePointer(true, pose), $"mochtroid_attached_{pose}");
    }

    /// <summary>Uses each pose's native record size: six parts in flight or four while attached.</summary>
    private static ushort PosePointer(bool attached, int pose) =>
        checked((ushort)((attached ? AttachedFrame : FlightFrame) + pose * (2 + 5 * (attached ? 4 : 6))));
}

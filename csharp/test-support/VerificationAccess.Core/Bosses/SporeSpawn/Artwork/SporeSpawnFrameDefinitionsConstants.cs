using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>Cartridge values of Spore Spawn's body frames that only verification reads.</summary>
internal static class SporeSpawnFrameDefinitionsConstants
{
    /// <summary>$A5:EE65: the hardened husk.</summary>
    internal const ushort Dead = 0xee65;
    /// <summary>$A5:EE6F: the closed head.</summary>
    internal const ushort Closed = 0xee6f;
    /// <summary>$A5:EE79: the first of seven opening poses.</summary>
    internal const ushort FirstOpening = 0xee79;
    /// <summary>$A5:EF3D: the first of three fully open poses.</summary>
    internal const ushort FirstOpen = 0xef3d;
    /// <summary>The size of one opening or open root.</summary>
    internal const int RootBytes = 18;
    /// <summary>The extended spritemap <paramref name="frame"/>, one of Spore Spawn's body frames, is stored at.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is not one of Spore Spawn's body frames.</exception>
    internal static ushort Spritemap(EnemyFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Not a Spore Spawn body frame.");

    /// <summary>Each frame by the cartridge frame it shows.</summary>
    private static readonly Dictionary<EnemyFrameId, ushort> Spritemaps = new()
    {
        [EnemyFrameId.SporeSpawnShutTwin] = Dead,
        [EnemyFrameId.SporeSpawnShut] = Closed,
        [EnemyFrameId.SporeSpawnParted8] = Opening(0),
        [EnemyFrameId.SporeSpawnParted16] = Opening(1),
        [EnemyFrameId.SporeSpawnParted24] = Opening(2),
        [EnemyFrameId.SporeSpawnParted32] = Opening(3),
        [EnemyFrameId.SporeSpawnParted40] = Opening(4),
        [EnemyFrameId.SporeSpawnParted44] = Opening(5),
        [EnemyFrameId.SporeSpawnParted48] = Opening(6),
        [EnemyFrameId.SporeSpawnParted48CoreGlowing] = Open(0),
        [EnemyFrameId.SporeSpawnParted48CoreBright] = Open(1),
        [EnemyFrameId.SporeSpawnParted48Twin] = Open(2),
    };

    /// <summary>The opening pose stored <paramref name="pose"/> roots into the opening run.</summary>
    private static ushort Opening(int pose) => (ushort)(FirstOpening + RootBytes * pose);

    /// <summary>The open pose stored <paramref name="pose"/> roots into the open run.</summary>
    private static ushort Open(int pose) => (ushort)(FirstOpen + RootBytes * pose);
}

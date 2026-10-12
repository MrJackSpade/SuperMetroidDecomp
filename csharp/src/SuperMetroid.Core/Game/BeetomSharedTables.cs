using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The per-slot Beetom tables shared with the enemy system (#633).</summary>
internal sealed record BeetomSharedTables
{
    /// <summary>Installed instruction lists.</summary>
    public required ushort[] InstalledInstructionLists { get; init; }

    /// <summary>Initial short leap y speed indexes.</summary>
    public required ushort[] InitialShortLeapYSpeedIndexes { get; init; }

    /// <summary>Initial long leap y speed indexes.</summary>
    public required ushort[] InitialLongLeapYSpeedIndexes { get; init; }

    /// <summary>Initial lunge y speed indexes.</summary>
    public required ushort[] InitialLungeYSpeedIndexes { get; init; }

    /// <summary>Falling flags.</summary>
    public required ushort[] FallingFlags { get; init; }

    /// <summary>Attached flags.</summary>
    public required ushort[] AttachedFlags { get; init; }

    /// <summary>Directions.</summary>
    public required ushort[] Directions { get; init; }

    /// <summary>Initial attachment offsets.</summary>
    public required Offset16<RoomPixels>[] InitialAttachmentOffsets { get; init; }
}

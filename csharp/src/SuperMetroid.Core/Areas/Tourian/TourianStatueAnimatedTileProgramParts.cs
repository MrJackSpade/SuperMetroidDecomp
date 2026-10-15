using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue animated-tile program's native operands (#633).</summary>
internal sealed record TourianStatueAnimatedTileProgramParts
{
    /// <summary>The animated-tile object header address in bank $87.</summary>
    public required AnimatedTileObject ObjectPointer { get; init; }

    /// <summary>The first instruction of the object's fixed 104-byte program.</summary>
    public required ushort ProgramStart { get; init; }

    /// <summary>The byte count uploaded by each timed frame.</summary>
    public required ushort TransferByteCount { get; init; }

    /// <summary>The hardware-encoded VRAM destination for each timed frame.</summary>
    public required ushort EncodedVramDestination { get; init; }

    /// <summary>The statue-specific bit manipulated by $87:8349/$8352.</summary>
    public required ushort StatueStateBit { get; init; }

    /// <summary>The event set once this statue has turned grey.</summary>
    public required ushort GreyEventNumber { get; init; }

    /// <summary>The statue-specific first-frame duration.</summary>
    public required ushort FirstFrameDuration { get; init; }

    /// <summary>The area byte and boss mask consumed by $87:8303.</summary>
    public required ushort PackedBossTest { get; init; }

    /// <summary>The byte offset of the first of three colors cleared by $87:835B.</summary>
    public required ushort ClearPaletteByteIndex { get; init; }

    /// <summary>The even statue selector passed to the eye-glow and soul spawners.</summary>
    public required ushort UnlockEffectParameter { get; init; }

    /// <summary>The bank-$8D palette-FX definition spawned during release.</summary>
    public required ushort PaletteFxDefinition { get; init; }

    /// <summary>The byte offset receiving the eight common grey target colors.</summary>
    public required ushort TargetPaletteByteIndex { get; init; }
}

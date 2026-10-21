using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One Tourian boss statue's animated-tile definition: the artwork transfer each frame makes, the typed program the
/// statue runs and the operands that program's cues act on.
/// </summary>
public sealed class TourianStatueAnimatedTileProgramDefinition
{

    /// <summary>Only the statue catalog creates definitions.</summary>
    internal TourianStatueAnimatedTileProgramDefinition()
    {
    }

    /// <summary>The animated-tile object this statue's program belongs to.</summary>
    public required AnimatedTileObject AnimatedObject { get; init; }

    /// <summary>The bytes of statue artwork each shown frame transfers.</summary>
    public required int TransferByteCount { get; init; }

    /// <summary>The hardware-encoded VRAM destination for each timed frame.</summary>
    public required ushort EncodedVramDestination { get; init; }

    /// <summary>The queued-DMA destination <see cref="EncodedVramDestination"/> decodes to.</summary>
    [PpuLayoutBoundary("codec: Tourian statue animated-tile program's native VRAM destination")]
    public VramDmaDestination Destination => VramDmaDestination.FromNative(EncodedVramDestination);

    /// <summary>The statue's own flag in the shared statue animation state.</summary>
    public required TourianStatueAnimationState AnimatingFlag { get; init; }

    /// <summary>The event set once this statue has turned grey, and tested to skip straight to the grey picture.</summary>
    public required EventNumber GreyEvent { get; init; }

    /// <summary>The updates the first idle frame lasts after the statue spawns.</summary>
    public required int FirstFrameDuration { get; init; }

    /// <summary>The area whose boss bits decide whether the statue releases.</summary>
    public required AreaId BossArea { get; init; }

    /// <summary>The boss bits of <see cref="BossArea"/> any one of which starts the release.</summary>
    public required BossBits DefeatedBossBits { get; init; }

    /// <summary>The CGRAM colour index of the first of the three colours blacked out when the release starts.</summary>
    public required int ClearedColorIndex { get; init; }

    /// <summary>The even statue selector passed to the eye-glow and soul spawners.</summary>
    public required ushort UnlockEffectParameter { get; init; }

    /// <summary>The palette-FX object spawned during release.</summary>
    public required PaletteFxObject PaletteFx { get; init; }

    /// <summary>The CGRAM colour index that receives the eight common grey colours.</summary>
    public required int GreyColorIndex { get; init; }


    /// <summary>The statue's typed animated-tile programs.</summary>
    internal AnimationProgramSet<TourianStatueTileProgram, TourianStatueTilePicture, TourianStatueTileCue> Program =>
        field ??= TourianStatueTileAnimations.Create(FirstFrameDuration);
}

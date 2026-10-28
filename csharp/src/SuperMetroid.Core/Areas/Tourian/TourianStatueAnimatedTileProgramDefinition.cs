using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.PaletteFx;

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

    /// <summary>The bytes of statue artwork each shown frame shows.</summary>
    public required int FrameByteCount { get; init; }

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

    /// <summary>The statue's three colours blacked out when the release starts.</summary>
    public required IColourSetRun ClearedColours { get; init; }

    /// <summary>The even statue selector passed to the eye-glow and soul spawners.</summary>
    public required int UnlockEffectParameter { get; init; }

    /// <summary>The palette-FX object spawned during release.</summary>
    public required PaletteFxObject PaletteFx { get; init; }

    /// <summary>The statue's eight colours that receive the common grey colours.</summary>
    public required IColourSetRun GreyColours { get; init; }
}

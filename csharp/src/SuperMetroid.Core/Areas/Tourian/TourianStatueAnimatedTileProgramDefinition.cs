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
    public required int FrameArtworkSize { get; init; }

    /// <summary>The statue this program animates, as the shared statue animation state names it.</summary>
    public required TourianGreyStatues Statue { get; init; }

    /// <summary>The event set once this statue has turned grey, and tested to skip straight to the grey picture.</summary>
    public required StoryEvent GreyEvent { get; init; }

    /// <summary>The updates the first idle frame lasts after the statue spawns.</summary>
    public required int FirstFrameDuration { get; init; }

    /// <summary>The area whose boss bits decide whether the statue releases.</summary>
    public required AreaId BossArea { get; init; }

    /// <summary>The boss bits of <see cref="BossArea"/> any one of which starts the release.</summary>
    public required BossBits DefeatedBossBits { get; init; }

    /// <summary>The statue with its three eye colours blacked out, which it shows from the start of its release (#1512).</summary>
    public required PictureShade ClearedShade { get; init; }

    /// <summary>The statue's eye, where its release's eye glow and soul appear, and the glow it lights.</summary>
    public required TourianStatueEye Eye { get; init; }

    /// <summary>The palette-FX object spawned during release.</summary>
    public required PaletteFxObject PaletteFx { get; init; }

    /// <summary>The statue greyed, which it shows once its eight colours take the common grey colours (#1512).</summary>
    public required PictureShade GreyShade { get; init; }
}

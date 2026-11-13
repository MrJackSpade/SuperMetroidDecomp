using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Kraid's three eyes as they glow and fade after a hit (#1512, #1552), counted on the game's fade scale
/// (<see cref="FadeLevelColour"/>), the levels the game authored and steps them in.
/// </summary>
/// <remarks>
/// <para>
/// Gameplay behaviour reproduced: a glow raises each eye's red and green one level an update, a glow that starts mid-fade
/// starting from the levels the fade left, and has peaked on the update every red and green reaches full; the fade lowers
/// each red and green that differs from his health band's eyes one level an update, a channel at none wrapping to full,
/// and ends on the first update no channel differs. When the glow peaks ends his eye glow, and when the eyes show the
/// band's again starts his roar, so the levels, the wrap and both tests time the roar. A health colour update shows the
/// band's eyes at once.
/// </para>
/// <para>
/// Consulted on every update of Kraid's eye glow and of its fade, the two head behaviours his head's cues run after
/// a body hit, and set by his health colour updates during his enemy update. Evidence: the original glow and fade
/// routines step the eye colours' channels one level an update, test them for the peak and for the band's colours, and
/// wrap a channel decremented from none; the Kraid replays confirm the roar's timing.
/// </para>
/// <para>
/// A value Kraid's colours own; each operation gives the eyes after it.
/// </para>
/// </remarks>
[CompatibilityService("Kraid's eyes glow up and fade back a level an update, a fading channel at none wrapping to full; the glow's peak and the eyes' return to the band's time his roar")]
public readonly struct KraidEyeGlow
{
    /// <summary>The eyes Kraid has.</summary>
    public const int Eyes = 3;

    /// <summary>The level from which one glow step reaches full.</summary>
    private const int NearPeak = FadeLevelColour.FullLevel - 1;

    /// <summary>Each eye's levels now, one per eye.</summary>
    private readonly ImmutableArray<FadeLevelColour> shown;

    /// <summary>Holds <paramref name="levels"/>, one per eye.</summary>
    private KraidEyeGlow(ImmutableArray<FadeLevelColour> levels) => shown = levels;

    /// <summary>The eyes at none in every channel, as his colours hold them before his first health colour update.</summary>
    public static KraidEyeGlow Unlit { get; } = new([.. Enumerable.Repeat(FadeLevelColour.Of(RgbColor.Black), Eyes)]);

    /// <summary>The eyes showing <paramref name="bandEyes"/>, his health band's eye colours, as a health colour update writes them.</summary>
    /// <exception cref="ArgumentException">The colours are not one per eye, or one lies off the fade scale.</exception>
    public static KraidEyeGlow Showing(ReadOnlySpan<RgbColor> bandEyes)
    {
        Ensure.LengthEqual(bandEyes, Eyes);
        var levels = ImmutableArray.CreateBuilder<FadeLevelColour>(Eyes);
        foreach (RgbColor colour in bandEyes)
            levels.Add(FadeLevelColour.Of(colour));
        return new(levels.MoveToImmutable());
    }

    /// <summary>The eye colours shown now, one per eye.</summary>
    public ImmutableArray<RgbColor> Shown => [.. shown.Select(levels => levels.ToColour())];

    /// <summary>Whether every eye's red and green stand at full, which a glow step reaches on the update it peaks.</summary>
    public bool HasPeaked => shown.All(levels => levels.Red == FadeLevelColour.FullLevel && levels.Green == FadeLevelColour.FullLevel);

    /// <summary>The eyes after one glow step: each eye's red and green one level higher, a channel near full reaching full.</summary>
    public KraidEyeGlow Glowed() => new([.. shown.Select(levels => levels.WithRed(Raised(levels.Red)).WithGreen(Raised(levels.Green)))]);

    /// <summary>
    /// Whether every eye's red and green equal those of <paramref name="bandEyes"/>, his health band's eye colours, which
    /// ends the fade.
    /// </summary>
    /// <exception cref="ArgumentException">The colours are not one per eye, or one lies off the fade scale.</exception>
    public bool ShowsBand(ReadOnlySpan<RgbColor> bandEyes)
    {
        Ensure.LengthEqual(bandEyes, Eyes);
        for (int eye = 0; eye < Eyes; eye++)
        {
            FadeLevelColour target = FadeLevelColour.Of(bandEyes[eye]);
            if (shown[eye].Red != target.Red || shown[eye].Green != target.Green)
                return false;
        }
        return true;
    }

    /// <summary>
    /// The eyes after one fade step toward <paramref name="bandEyes"/>, his health band's eye colours: each red and green
    /// that differs one level lower, none wrapping to full.
    /// </summary>
    /// <exception cref="ArgumentException">The colours are not one per eye, or one lies off the fade scale.</exception>
    public KraidEyeGlow Unglowed(ReadOnlySpan<RgbColor> bandEyes)
    {
        Ensure.LengthEqual(bandEyes, Eyes);
        var levels = ImmutableArray.CreateBuilder<FadeLevelColour>(Eyes);
        for (int eye = 0; eye < Eyes; eye++)
        {
            FadeLevelColour current = shown[eye];
            FadeLevelColour target = FadeLevelColour.Of(bandEyes[eye]);
            int red = current.Red == target.Red ? current.Red : LoweredWrapping(current.Red);
            int green = current.Green == target.Green ? current.Green : LoweredWrapping(current.Green);
            levels.Add(current.WithRed(red).WithGreen(green));
        }
        return new(levels.MoveToImmutable());
    }

    /// <summary>A glowing channel's level one step on: one higher, or full from near full or full.</summary>
    private static int Raised(int level) => level >= NearPeak ? FadeLevelColour.FullLevel : level + 1;

    /// <summary>A fading channel's level one step on: one lower, none wrapping to full.</summary>
    private static int LoweredWrapping(int level) => level == 0 ? FadeLevelColour.FullLevel : level - 1;
}

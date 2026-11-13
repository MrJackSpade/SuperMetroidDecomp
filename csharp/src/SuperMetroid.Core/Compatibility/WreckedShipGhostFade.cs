using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The Wrecked Ship ghost's sixteen colours as its brightening and fades move them (#1512, #1544, #1552), counted on the
/// game's fade scale (<see cref="FadeLevelColour"/>), the levels the game authored and steps them in.
/// </summary>
/// <remarks>
/// <para>
/// Gameplay behaviour reproduced: the ghost's colours are darkened to black when it initializes; each brightening step
/// raises every colour whose red is below full by one level in each channel, a channel that passes full overflowing into the
/// next (red into green, green into blue, blue's overflow lost), and the brightening ends on the first step that changes no
/// colour; each fade step moves every channel of every colour one level toward the fade's target, the ghost's own colours
/// as it materializes or white as it vanishes, and the fade ends on the first step that moves no channel. All sixteen
/// colours take part, colour zero too, which no picture draws, so how many steps each takes and when each ends are decided
/// over all of them, and that decides when the ghost's AI moves on.
/// </para>
/// <para>
/// Consulted on every update of the ghost's brightening, materializing and vanishing phases, during its enemy update;
/// darkened by its initialization. Evidence: the original game's brightening and fade routines for the ghost, which add
/// one level to all three channels of a colour in one addition, so a channel passing full carries into the next, and step
/// all sixteen of the ghost's colours; the Wrecked Ship ghost replays confirm their end conditions.
/// </para>
/// <para>
/// A value the ghost's state owns; each operation gives the colours after it. The import pre-colours the ghost's shades by
/// running the same value along the ghost's one path, so each step's colours are named by the effect that last changed
/// them and the steps it took (<see cref="ChangedBy"/>, <see cref="StepsChanged"/>).
/// </para>
/// </remarks>
[CompatibilityService("the Wrecked Ship ghost's brightening carries a channel passing full into the next, and its fades step all sixteen of its colours a level at a time, so when each ends decides when its AI moves on")]
public readonly struct WreckedShipGhostFade
{
    /// <summary>The colours the ghost's fades move: the fifteen its pictures draw and colour zero, which none draws.</summary>
    public const int ColourCount = 16;

    /// <summary>The channels a fade step moves, in the order the game steps them.</summary>
    private static readonly ColorChannel[] Channels = [ColorChannel.Red, ColorChannel.Green, ColorChannel.Blue];

    /// <summary>The ghost's colours on the fade scale, colour zero first: what each step's end is decided by.</summary>
    private readonly ImmutableArray<FadeLevelColour> shown;

    /// <summary>The colours the current fade moves toward, colour zero first.</summary>
    internal readonly ImmutableArray<FadeLevelColour> target;

    /// <summary>The fade the next step toward <see cref="target"/> belongs to.</summary>
    private readonly Effect fade;

    /// <summary>Holds the colours, the fade's target and the effects that move and last changed them.</summary>
    private WreckedShipGhostFade(
        ImmutableArray<FadeLevelColour> shown, ImmutableArray<FadeLevelColour> target, Effect fade, Effect changedBy, int stepsChanged)
    {
        this.shown = shown;
        this.target = target;
        this.fade = fade;
        ChangedBy = changedBy;
        StepsChanged = stepsChanged;
    }

    /// <summary>The effects that change the ghost's colours.</summary>
    public enum Effect
    {
        /// <summary>The ghost's colours darkened to black, as its initialization leaves them.</summary>
        Dark,

        /// <summary>The ghost's brightening from black.</summary>
        Brightening,

        /// <summary>The ghost's fade into its own colours, which materializes it.</summary>
        Materializing,

        /// <summary>The ghost's fade to white, which vanishes it.</summary>
        Vanishing,
    }

    /// <summary>
    /// Every colour, shown and targeted, darkened to black, as the ghost's initialization leaves its colours: its room's
    /// fade has long settled them on the black target its initialization writes, so its first brightening starts from black.
    /// </summary>
    public static WreckedShipGhostFade Dark { get; } = new(Black(), Black(), Effect.Dark, Effect.Dark, 0);

    /// <summary>The ghost's colours now, colour zero first.</summary>
    public ImmutableArray<RgbColor> Shown => [.. shown.Select(levels => levels.ToColour())];

    /// <summary>The effect that last changed the colours; <see cref="Effect.Dark"/> until a step changes them.</summary>
    public Effect ChangedBy { get; }

    /// <summary>
    /// The steps of <see cref="ChangedBy"/> that have changed the colours since another effect last did; a step that changes
    /// nothing leaves the count, as it leaves the colours.
    /// </summary>
    public int StepsChanged { get; }

    /// <summary>
    /// One brightening step: every colour whose red is below full raised one level in each channel, a channel passing full
    /// carrying into the next; the step's count is the colours it changed, and none ends the brightening.
    /// </summary>
    public WreckedShipGhostFadeStep Brightened()
    {
        var next = ImmutableArray.CreateBuilder<FadeLevelColour>(ColourCount);
        int changed = 0;
        foreach (FadeLevelColour colour in shown)
        {
            if (colour.Red >= FadeLevelColour.FullLevel)
            {
                next.Add(colour);
                continue;
            }
            next.Add(RaisedOneLevel(colour));
            changed++;
        }
        return Stepped(next.MoveToImmutable(), changed, Effect.Brightening);
    }

    /// <summary>The fade toward <paramref name="ownColours"/>, the ghost's own sixteen colours, begun; it materializes the ghost.</summary>
    /// <exception cref="ArgumentException">The colours are not sixteen, or one lies off the fade scale.</exception>
    public WreckedShipGhostFade Materializing(IReadOnlyList<RgbColor> ownColours)
    {
        Ensure.NotNull(ownColours);
        if (ownColours.Count != ColourCount)
            throw new ArgumentException($"The Wrecked Ship ghost has {ColourCount} colours, not {ownColours.Count}.", nameof(ownColours));
        return new(shown, [.. ownColours.Select(FadeLevelColour.Of)], Effect.Materializing, ChangedBy, StepsChanged);
    }

    /// <summary>The fade toward white begun; it vanishes the ghost.</summary>
    public WreckedShipGhostFade Vanishing() =>
        new(shown, [.. Enumerable.Repeat(FadeLevelColour.Of(RgbColor.White), ColourCount)], Effect.Vanishing, ChangedBy, StepsChanged);

    /// <summary>
    /// One fade step: every channel of every colour moved one level toward the fade's target; the step's count is the
    /// channels it moved, and none ends the fade.
    /// </summary>
    public WreckedShipGhostFadeStep SteppedTowardTarget()
    {
        var next = ImmutableArray.CreateBuilder<FadeLevelColour>(ColourCount);
        int moved = 0;
        for (int colour = 0; colour < ColourCount; colour++)
        {
            FadeLevelColour levels = shown[colour];
            FadeLevelColour goal = target[colour];
            foreach (ColorChannel channel in Channels)
            {
                int level = levels[channel];
                int toward = goal[channel];
                if (level == toward)
                    continue;
                levels = levels.With(channel, level + (level > toward ? -1 : 1));
                moved++;
            }
            next.Add(levels);
        }
        return Stepped(next.MoveToImmutable(), moved, fade);
    }

    /// <summary>
    /// The step that leaves <paramref name="next"/> after <paramref name="changes"/> changes by <paramref name="change"/>:
    /// a step that changed the colours counts as one more of that effect's, its first if another effect changed them last.
    /// </summary>
    private WreckedShipGhostFadeStep Stepped(ImmutableArray<FadeLevelColour> next, int changes, Effect change)
    {
        if (changes == 0)
            return new(this, changes);
        int steps = ChangedBy == change ? StepsChanged + 1 : 1;
        return new(new WreckedShipGhostFade(next, target, fade, change, steps), changes);
    }

    /// <summary>Sixteen black colours.</summary>
    private static ImmutableArray<FadeLevelColour> Black() => [.. Enumerable.Repeat(FadeLevelColour.Of(RgbColor.Black), ColourCount)];

    /// <summary>
    /// <paramref name="colour"/> one level brighter in each channel, a channel passing full wrapping to zero and adding
    /// one level to the next, the last channel's overflow lost.
    /// </summary>
    private static FadeLevelColour RaisedOneLevel(FadeLevelColour colour)
    {
        const int levels = FadeLevelColour.FullLevel + 1;
        int red = colour.Red + 1;
        int green = colour.Green + 1 + red / levels;
        int blue = colour.Blue + 1 + green / levels;
        return new FadeLevelColour(red % levels, green % levels, blue % levels);
    }
}

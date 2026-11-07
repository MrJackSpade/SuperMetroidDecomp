using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// The shades of Kraid's room backdrop (#1512), which the import pre-colours his room's pictures in: his backdrop
/// colours over the room colour group his growth recolours, black, and every fade's colours the encounter can show, as
/// <c>AdvanceKraidRoomBackgroundFade</c> moves each colour by its remaining distance over the steps left.
/// </summary>
/// <remarks>
/// The fades the encounter makes, and what each can start from:
/// <list type="bullet">
/// <item>His growth (<c>KraidRise</c>) fades the room's colours into his. Its redraw of the room's background zeroes the
/// shared counter first, so its steps run from zero.</item>
/// <item>His death (<c>KraidDeath</c>) fades to black from what the backdrop shows when it begins. Kraid's colours when
/// his growth fade completed, which zeroed the counter. The room's colours when he dies before his growth fade (it waits
/// after his health falls), at whatever value the shared counter then holds. The growth fade's colours when he dies
/// during it: the death fade continues that fade's counter, its first step at the value after the growth fade's last.
/// When he dies with the counter past the growth fade's last value, the death fade takes no step and leaves his colours.</item>
/// <item>Once his body has sunk, the death zeroes the counter and the backdrop fades back into his colours from what it
/// shows: black after a death fade that stepped, his colours after one that took none or in his defeated room (a fade
/// that changes nothing), or the room's colours after a death fade that took none before his growth (the growth fade's
/// own shades).</item>
/// </list>
/// The death fade runs until it completes, which follows the step at the last moving value and lands on black, so no fade
/// starts from its colours partway; only the growth fade is interrupted. The room's doors open once Kraid is dead, so
/// Samus can leave during the fade back: the door's fade then advances the shared counter in the same updates, and the
/// fade back's later steps take every other counter value. It completes when the counter passes the last moving value,
/// on his colours or, when its steps skipped that value, on its colours partway.
/// </remarks>
internal static class KraidBackdropShades
{
    /// <summary>The first ink of a room picture the backdrop recolours: the fifth tile colour group's first.</summary>
    private const int FirstInk = 96;

    /// <summary>The fades' length: their last moving step.</summary>
    private const int Denominator = KraidBackgroundDefinitions.BackdropFadeLastStep - 1;

    /// <summary>The last counter value of a fade's colours partway, before the step that lands on its goal.</summary>
    private const int LastPartway = KraidBackgroundDefinitions.BackdropFadeLastStep - 1;

    /// <summary>The room's own backdrop colours.</summary>
    private static readonly KraidBackdropShown ShowsRoomColours = new KraidBackdropShown.Settled(KraidBackdropSettledColours.RoomColours);

    /// <summary>Kraid's backdrop colours.</summary>
    private static readonly KraidBackdropShown ShowsKraidColours = new KraidBackdropShown.Settled(KraidBackdropSettledColours.KraidColours);

    /// <summary>Black.</summary>
    private static readonly KraidBackdropShown ShowsBlack = new KraidBackdropShown.Settled(KraidBackdropSettledColours.Black);

    /// <summary>Every backdrop shade of a room whose standard colours are <paramref name="standard"/>, to Kraid's <paramref name="kraid"/>.</summary>
    internal static IReadOnlyList<SheetShade> Of(ReadOnlySpan<RgbColor> standard, KraidColorCatalog kraid)
    {
        Ensure.NotNull(kraid);
        var palettes = new KraidBackdropPalettes(
            new RgbColor[KraidPalettes.BandColors], new RgbColor[KraidPalettes.BandColors], new RgbColor[KraidPalettes.BandColors]);
        for (int colour = 0; colour < KraidPalettes.BandColors; colour++)
        {
            palettes.Room[colour] = standard[FirstInk + colour];
            palettes.Kraid[colour] = kraid.Resolve(KraidPaletteSource.RoomBackdrop, colour);
        }
        var shades = new List<SheetShade>();
        foreach (KraidBackdropShown shown in Reachable().Distinct())
            shades.Add(SheetShade.Run(KraidShades.BackdropShowing(shown), FirstInk, Colours(shown, palettes)));
        return shades;
    }

    /// <summary>What the encounter's fades can show besides the room's own colours, as the remarks derive it.</summary>
    private static IEnumerable<KraidBackdropShown> Reachable()
    {
        yield return ShowsKraidColours;
        yield return ShowsBlack;
        // His growth and the death fade from his colours, each from a zeroed counter.
        foreach (KraidBackdropShown shown in Partway(ShowsRoomColours, KraidBackdropSettledColours.KraidColours, Consecutive(0)))
            yield return shown;
        foreach (KraidBackdropShown shown in Partway(ShowsKraidColours, KraidBackdropSettledColours.Black, Consecutive(0)))
            yield return shown;
        // The fade back from black or from the room's colours, from a zeroed counter, with or without a door's fade.
        foreach (KraidBackdropShown from in new[] { ShowsBlack, ShowsRoomColours })
        {
            for (int lastAlone = -1; lastAlone <= KraidBackgroundDefinitions.BackdropFadeLastStep; lastAlone++)
            {
                foreach (KraidBackdropShown shown in Partway(from, KraidBackdropSettledColours.KraidColours, DoorFadeFrom(lastAlone)))
                    yield return shown;
            }
        }
        // The death fade from the room's colours, at whatever value the shared counter holds.
        for (int first = 0; first <= LastPartway; first++)
        {
            foreach (KraidBackdropShown shown in Partway(ShowsRoomColours, KraidBackdropSettledColours.Black, Consecutive(first)))
                yield return shown;
        }
        // The death fade from the growth fade's colours partway, continuing its counter.
        foreach (KraidBackdropShown.Fading growth in Partway(ShowsRoomColours, KraidBackdropSettledColours.KraidColours, Consecutive(0)))
        {
            foreach (KraidBackdropShown shown in Partway(growth, KraidBackdropSettledColours.Black, Consecutive(growth.Steps.Latest + 1)))
                yield return shown;
        }
    }

    /// <summary>A fade's steps at each counter value from <paramref name="first"/> through the last moving value.</summary>
    private static IEnumerable<int> Consecutive(int first) =>
        Enumerable.Range(first, KraidBackgroundDefinitions.BackdropFadeLastStep - first + 1);

    /// <summary>
    /// A fade back's steps from a zeroed counter when a door's fade begins advancing the counter after its step at
    /// <paramref name="lastAlone"/> (minus one: before its first): each value through that one, then every other value.
    /// </summary>
    private static IEnumerable<int> DoorFadeFrom(int lastAlone)
    {
        for (int step = 0; step <= lastAlone; step++)
            yield return step;
        for (int step = lastAlone + 2; step <= KraidBackgroundDefinitions.BackdropFadeLastStep; step += 2)
            yield return step;
    }

    /// <summary>
    /// A fade from <paramref name="from"/> toward <paramref name="goal"/>'s colours partway, its steps at the counter's
    /// <paramref name="values"/>: what each of its steps leaves shown that is neither what it started from nor its goal.
    /// </summary>
    private static IEnumerable<KraidBackdropShown.Fading> Partway(KraidBackdropShown from, KraidBackdropSettledColours goal, IEnumerable<int> values)
    {
        KraidBackdropFadeSteps? steps = null;
        foreach (int value in values)
        {
            steps = steps is { } taken ? taken.Then(value) : KraidBackdropFadeSteps.At(value);
            if (KraidBackdropShown.Fade(from, goal, steps.Value) is KraidBackdropShown.Fading fading)
                yield return fading;
        }
    }

    /// <summary>The colours <paramref name="shown"/> names, from the room's, Kraid's and black.</summary>
    private static RgbColor[] Colours(KraidBackdropShown shown, KraidBackdropPalettes palettes) => shown switch
    {
        KraidBackdropShown.Settled settled => Colours(settled.Colours, palettes),
        KraidBackdropShown.Fading fading => Faded(Colours(fading.From, palettes), Colours(fading.Goal, palettes), fading.Steps),
        _ => throw new ArgumentOutOfRangeException(nameof(shown), shown, "Undefined Kraid backdrop colours."),
    };

    /// <summary>The settled <paramref name="colours"/>.</summary>
    private static RgbColor[] Colours(KraidBackdropSettledColours colours, KraidBackdropPalettes palettes) => colours switch
    {
        KraidBackdropSettledColours.RoomColours => palettes.Room,
        KraidBackdropSettledColours.KraidColours => palettes.Kraid,
        KraidBackdropSettledColours.Black => palettes.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(colours), colours, "Undefined Kraid backdrop colours."),
    };

    /// <summary>
    /// Each of <paramref name="from"/>'s colours after the fade's steps toward <paramref name="to"/>'s at the shared
    /// counter's values <paramref name="steps"/>.
    /// </summary>
    private static RgbColor[] Faded(RgbColor[] from, RgbColor[] to, KraidBackdropFadeSteps steps)
    {
        var faded = new RgbColor[from.Length];
        for (int colour = 0; colour < faded.Length; colour++)
        {
            RgbColor current = from[colour];
            foreach (int step in Values(steps))
                current = GradualColourFade.Step(step, current, to[colour], Denominator);
            faded[colour] = current;
        }
        return faded;
    }

    /// <summary>The shared counter's values <paramref name="steps"/> took, in rising order.</summary>
    private static IEnumerable<int> Values(KraidBackdropFadeSteps steps)
    {
        for (int step = 0; step <= KraidBackgroundDefinitions.BackdropFadeLastStep; step++)
        {
            if (steps.Took(step))
                yield return step;
        }
    }

    /// <summary>The settled colours of the backdrop's ink run.</summary>
    /// <param name="Room">The room's own colours.</param>
    /// <param name="Kraid">Kraid's backdrop colours.</param>
    /// <param name="Black">Black.</param>
    private sealed record KraidBackdropPalettes(RgbColor[] Room, RgbColor[] Kraid, RgbColor[] Black);
}

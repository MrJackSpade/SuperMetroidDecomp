using Engine.Shared;
using Godot;

namespace Game.Compatibility;

/// <summary>
/// The cartridge's arithmetic on a picture's colours, for a boss that works out the colours it shows from changes of
/// colours written as data: one change laid over another, and a fade part of the way from one to another, each of a
/// colour's red, green and blue moved by the cartridge's own steps of brightness, thirty-two to a part.
/// </summary>
/// <remarks>
/// A change here is the list of colours it changes on one thing (<see cref="Engine.Ui.ColourChange.On"/>); a colour it
/// leaves alone is shown as the picture has it. A colour is told apart by its colour in the picture and the region it
/// holds in. Nothing is kept from one call to the next.
/// </remarks>
public static class CartridgeColours
{
    /// <summary>The highest of a part's steps of brightness.</summary>
    private const int FullStep = 31;

    /// <summary>
    /// <paramref name="over"/> laid over <paramref name="under"/>: for every colour the later names, the later's; for
    /// the rest, the earlier's.
    /// </summary>
    public static IReadOnlyList<ColourSwap> LaidOver(IReadOnlyList<ColourSwap> under, IReadOnlyList<ColourSwap> over)
    {
        ArgumentNullException.ThrowIfNull(under);
        ArgumentNullException.ThrowIfNull(over);
        HashSet<(int, int)> named = [.. over.Select(Key)];
        return [.. under.Where(swap => !named.Contains(Key(swap))), .. over];
    }

    /// <summary>
    /// A fade <paramref name="step"/> of <paramref name="steps"/> of the way from <paramref name="from"/> to
    /// <paramref name="to"/>: each colour either names is shown so far from what the first shows it as toward what the
    /// second does, each of its three parts a step of brightness plus the difference times the fade's step over its
    /// steps, cut off toward nothing. A colour shown as the picture has it is left out.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The fade has no steps, or its step lies outside them.</exception>
    /// <exception cref="ArgumentException">A colour is see-through, or lies off the cartridge's steps of brightness.</exception>
    public static IReadOnlyList<ColourSwap> PartWay(IReadOnlyList<ColourSwap> from, IReadOnlyList<ColourSwap> to, int step, int steps)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);
        ArgumentOutOfRangeException.ThrowIfLessThan(steps, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(step);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(step, steps);
        Dictionary<(int, int), ColourSwap> starts = from.ToDictionary(Key), ends = to.ToDictionary(Key);
        var shown = new List<ColourSwap>();
        foreach ((int, int) key in starts.Keys.Union(ends.Keys))
        {
            bool started = starts.TryGetValue(key, out ColourSwap start), ended = ends.TryGetValue(key, out ColourSwap end);
            // A colour a change leaves alone it shows as the picture has it.
            Color own = started ? start.From : end.From;
            Color begins = started ? start.To : own, finishes = ended ? end.To : own;
            if (!IsOpaque(begins) || !IsOpaque(finishes))
                throw new ArgumentException($"A fade of colours is asked to fade a see-through colour, {begins.ToHtml()} or {finishes.ToHtml()}, which has no steps of brightness.");
            Color now = Color.Color8(
                (byte)Part(begins.R8, finishes.R8, step, steps), (byte)Part(begins.G8, finishes.G8, step, steps), (byte)Part(begins.B8, finishes.B8, step, steps));
            if (!SameColour(now, own))
                shown.Add(new ColourSwap(own, now, key.Item2));
        }

        return shown;
    }

    /// <summary>One part of a colour, as a channel's value, so far from one to another by the cartridge's steps of brightness.</summary>
    private static int Part(int from, int to, int step, int steps)
    {
        int start = StepOf(from), end = StepOf(to);
        return Shown(start + ((end - start) * step / steps));
    }

    /// <summary>The step of brightness a channel's value is, which must be one of the cartridge's.</summary>
    private static int StepOf(int value)
    {
        int step = value >> 3;
        return step <= FullStep && Shown(step) == value
            ? step
            : throw new ArgumentException($"The value {value} of a colour's part is none of the cartridge's thirty-two steps of brightness.", nameof(value));
    }

    /// <summary>A channel's value for a step of brightness: the steps spread evenly from none to full, both ends included.</summary>
    private static int Shown(int step) => (step * 8) + (step / 4);

    /// <summary>What tells a changed colour apart: its colour in the picture, and the region it holds in.</summary>
    private static (int, int) Key(ColourSwap swap) => ((swap.From.R8 << 16) | (swap.From.G8 << 8) | swap.From.B8, swap.Region);

    private static bool SameColour(Color one, Color other) => one.R8 == other.R8 && one.G8 == other.G8 && one.B8 == other.B8;

    private static bool IsOpaque(Color colour) => colour.A8 == 255;
}

using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// What a room's characters show for each value of the appearance inputs that reach them (#1466): which inputs reach a
/// run of characters, every assignment of values to them, and what shows at the run under one, as the cartridge's
/// bindings and animation uploads would leave it. Extraction pictures each block, background cell and written word for
/// every assignment and names the picture by it, so the room chooses a picture by the inputs alone.
/// </summary>
internal static class RoomAppearanceComposition
{
    /// <summary>The inputs some option of which shows over any of <paramref name="characters"/> of <paramref name="set"/>, in token order.</summary>
    internal static ImmutableArray<AppearanceInput> InputsOver(BgCharacterSet set, IEnumerable<int> characters, IReadOnlyList<AppearanceOption> options)
    {
        int[] reached = [.. Ensure.NotNull(characters)];
        return [.. Ensure.NotNull(options).Where(option => reached.Any(character => option.Upload.Covers(set, character)))
            .Select(option => option.Input).Distinct().OrderBy(input => input.Token, StringComparer.Ordinal)];
    }

    /// <summary>
    /// Every assignment of a value to each of <paramref name="inputs"/>, in order: the room's load, when
    /// <paramref name="withLoaded"/>, and each value an option gives the input.
    /// </summary>
    internal static IEnumerable<string[]> Assignments(ImmutableArray<AppearanceInput> inputs, IReadOnlyList<AppearanceOption> options, bool withLoaded)
    {
        Ensure.NotNull(options);
        IEnumerable<string[]> assignments = [new string[inputs.Length]];
        for (int index = 0; index < inputs.Length; index++)
        {
            int place = index;
            string[] values = [.. (withLoaded ? [RoomAppearanceValues.Loaded] : Array.Empty<string>()),
                .. options.Where(option => option.Input == inputs[place]).Select(option => option.Value).Distinct()];
            assignments = [.. assignments.SelectMany(earlier => values.Select(value =>
            {
                string[] chosen = [.. earlier];
                chosen[place] = value;
                return chosen;
            }))];
        }
        return assignments;
    }

    /// <summary>
    /// What shows over <paramref name="count"/> characters of <paramref name="set"/> from <paramref name="first"/> while
    /// <paramref name="inputs"/> take <paramref name="values"/>: an animation that paints them shows its selected frame
    /// (#1406); otherwise the newest artwork shown there, Kraid's backdrop over the collectibles the room bound as it
    /// loaded and a later encounter look over an earlier; otherwise what <paramref name="load"/> binds there. False where
    /// nothing shows, or where an animation's selected frame does not reach them.
    /// </summary>
    internal static bool TryShowing(BgCharacterSet set, int first, int count, ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values,
        IReadOnlyList<AppearanceOption> options, BgArtworkBindings? load, out BgArtworkShown shown)
    {
        Ensure.LengthEqual(values, inputs.Length);
        // The first animation by name that paints the characters and has selected a frame shows that frame there.
        foreach (AppearanceInput animation in Ensure.NotNull(options)
            .Where(option => PrecedenceOf(option.Input) == AppearancePrecedence.Animation && option.Upload.Covers(set, first))
            .Select(option => option.Input).Distinct().OrderBy(input => input.Token, StringComparer.Ordinal))
        {
            int index = inputs.IndexOf(animation);
            if (index < 0 || values[index] == RoomAppearanceValues.Loaded)
                continue;
            string value = values[index];
            AppearanceOption? frame = options.FirstOrDefault(option => option.Input == animation && option.Value == value);
            if (frame is null || !frame.Upload.Covers(set, first))
            {
                shown = default;
                return false;
            }
            shown = frame.Upload.ShownAt(first);
            return true;
        }
        AppearanceOption? bound = null;
        foreach (AppearanceOption option in options)
        {
            AppearancePrecedence precedence = PrecedenceOf(option.Input);
            int index = inputs.IndexOf(option.Input);
            if (precedence == AppearancePrecedence.Animation || index < 0 || values[index] != option.Value || !option.Upload.Covers(set, first))
                continue;
            if (bound is null || precedence <= PrecedenceOf(bound.Input))
                bound = option;
        }
        if (bound is not null)
        {
            shown = bound.Upload.ShownAt(first);
            return true;
        }
        if (load is not null)
            return load.TryShowing(set, first, count, out shown);
        shown = default;
        return false;
    }

    /// <summary>Whether <paramref name="input"/>'s artwork shows over another's where both reach the same characters.</summary>
    /// <exception cref="InvalidDataException">The input is of no kind extraction pictures.</exception>
    private static AppearancePrecedence PrecedenceOf(AppearanceInput input) => input switch
    {
        AnimationAppearance => AppearancePrecedence.Animation,
        KraidBackdropAppearance => AppearancePrecedence.KraidBackdrop,
        EncounterLookAppearance => AppearancePrecedence.EncounterLook,
        CollectibleSlotAppearance => AppearancePrecedence.CollectibleSlot,
        _ => throw new InvalidDataException($"No precedence is defined for appearance input {input.Token}."),
    };
}

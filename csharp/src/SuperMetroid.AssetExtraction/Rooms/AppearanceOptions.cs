using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The appearance options one composition pictures over (#1466), indexed once: which options reach each character, each
/// input's values in the order the options give them, and the option an input takes for each value. Composition asks
/// these for every character of every assignment, so none of them scans the options again.
/// </summary>
internal sealed class AppearanceOptions
{
    /// <summary>The options reaching each character asked about so far, by character set and then character.</summary>
    private readonly Dictionary<BgCharacterSet, Dictionary<int, AppearanceOption[]>> reaching = [];

    /// <summary>Each input's distinct values, in the order the options give them.</summary>
    private readonly Dictionary<AppearanceInput, List<string>> values = [];

    /// <summary>The first option giving each input each of its values, by input and then value.</summary>
    private readonly Dictionary<AppearanceInput, Dictionary<string, AppearanceOption>> byValue = [];

    /// <summary>Where the first option naming each input stands among the options.</summary>
    private readonly Dictionary<AppearanceInput, int> firstOption = [];

    /// <summary>Indexes <paramref name="options"/>, keeping their order.</summary>
    internal AppearanceOptions(IReadOnlyList<AppearanceOption> options)
    {
        All = Ensure.NotNull(options);
        for (int index = 0; index < options.Count; index++)
        {
            AppearanceOption option = options[index];
            if (firstOption.TryAdd(option.Input, index))
            {
                values.Add(option.Input, []);
                byValue.Add(option.Input, new Dictionary<string, AppearanceOption>(StringComparer.Ordinal));
            }
            if (byValue[option.Input].TryAdd(option.Value, option))
                values[option.Input].Add(option.Value);
        }
    }

    /// <summary>The options, in the order given.</summary>
    internal IReadOnlyList<AppearanceOption> All { get; }

    /// <summary>The options whose upload covers <paramref name="character"/> of <paramref name="set"/>, in the order given.</summary>
    internal AppearanceOption[] Reaching(BgCharacterSet set, int character)
    {
        if (!reaching.TryGetValue(set, out Dictionary<int, AppearanceOption[]>? ofSet))
            reaching.Add(set, ofSet = []);
        if (!ofSet.TryGetValue(character, out AppearanceOption[]? found))
        {
            found = [.. All.Where(option => option.Upload.Covers(set, character))];
            ofSet.Add(character, found);
        }
        return found;
    }

    /// <summary>The distinct values the options give <paramref name="input"/>, in the order given; none for an input no option names.</summary>
    internal IReadOnlyList<string> ValuesOf(AppearanceInput input) => values.TryGetValue(input, out List<string>? given) ? given : [];

    /// <summary>The first option giving <paramref name="input"/> <paramref name="value"/>, or null where none does.</summary>
    internal AppearanceOption? Option(AppearanceInput input, string value) =>
        byValue.TryGetValue(input, out Dictionary<string, AppearanceOption>? ofInput) ? ofInput.GetValueOrDefault(value) : null;

    /// <summary>Where the first option naming <paramref name="input"/> stands among the options.</summary>
    internal int FirstOptionOf(AppearanceInput input) => firstOption[input];
}

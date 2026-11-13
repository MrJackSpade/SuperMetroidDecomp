using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The rules an enemy frame display binding keeps (#1638), wherever it is authored: in the installed display bindings the
/// game reads, or in the compositions only the import composes the enemy sheets from. A binding names, for each named frame,
/// the frame displayed for it.
/// </summary>
internal static class EnemyFrameBindingRules
{
    /// <summary>The enemies showing each frame.</summary>
    private static readonly Lazy<ILookup<EnemyFrameId, EnemyDefinitionId>> Showing = new(() =>
        EnemyDefinitionFrameDefinitions.Frames.SelectMany(entry => entry.Value, (entry, frame) => KeyValuePair.Create(frame, entry.Key))
            .ToLookup(shown => shown.Key, shown => shown.Value));

    /// <summary>Each frame of <paramref name="frames"/> displayed as itself, as the original displays every frame.</summary>
    internal static Dictionary<string, string> Identity(IEnumerable<EnemyFrameName> frames) =>
        frames.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal);

    /// <summary>
    /// The ordinary frames' bindings <paramref name="authored"/> names: one for every named ordinary frame
    /// (<see cref="EnemySpritemapDefinitions"/>), each selecting a named ordinary frame every enemy showing it shows.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame has no binding, or a binding selects a frame it may not.</exception>
    internal static Dictionary<EnemyFrameId, EnemyFrameId> Ordinary(Dictionary<string, string>? authored)
    {
        Dictionary<string, EnemyFrameId> identities = Identities(EnemySpritemapDefinitions.Frames);
        if (authored is null || authored.Count != identities.Count)
            throw new InvalidDataException(
                "Enemy display bindings require every named native frame.");
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        foreach ((string name, EnemyFrameId identity) in identities)
        {
            if (!authored.TryGetValue(name, out string? selectedName) ||
                selectedName is null ||
                !identities.TryGetValue(selectedName, out EnemyFrameId selected))
                throw new InvalidDataException(
                    $"Enemy display binding {name} must select a named frame.");
            // Each enemy's sheet composes the selected frame over that enemy's own characters, which only the
            // frames it shows were authored against (#1392).
            if (Showing.Value[identity].Except(Showing.Value[selected]).Any())
                throw new InvalidDataException(
                    $"Enemy display binding {name} must select a frame every enemy showing it shows.");
            displayFrames.Add(identity, selected);
        }
        return displayFrames;
    }

    /// <summary>
    /// The extended frames' bindings <paramref name="authored"/> names: one for every named extended frame
    /// (<see cref="EnemyExtendedFrameDefinitions"/>), each selecting a named frame of the same enemy family.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame has no binding, or a binding selects a frame of another family.</exception>
    internal static Dictionary<EnemyFrameId, EnemyFrameId> Extended(Dictionary<string, string>? authored)
    {
        Dictionary<string, EnemyFrameId> identities = Identities(EnemyExtendedFrameDefinitions.Frames);
        if (authored is null || authored.Count != identities.Count)
            throw new InvalidDataException(
                "Extended enemy display bindings require every named frame.");
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        foreach ((string name, EnemyFrameId identity) in identities)
        {
            if (!authored.TryGetValue(name, out string? selectedName) ||
                selectedName is null ||
                !identities.TryGetValue(selectedName, out EnemyFrameId selected) ||
                EnemyExtendedFrameFamilies.Of(identity) != EnemyExtendedFrameFamilies.Of(selected))
                throw new InvalidDataException(
                    $"Extended enemy display binding {name} must select a frame of the same enemy family.");
            displayFrames.Add(identity, selected);
        }
        return displayFrames;
    }

    /// <summary>Each named frame of <paramref name="frames"/>, by its name.</summary>
    /// <exception cref="InvalidDataException">Two frames share a name or an identity.</exception>
    private static Dictionary<string, EnemyFrameId> Identities(IEnumerable<EnemyFrameName> frames)
    {
        var identities = new Dictionary<string, EnemyFrameId>(StringComparer.Ordinal);
        var seen = new HashSet<EnemyFrameId>();
        foreach (EnemyFrameName frame in frames)
        {
            if (!seen.Add(frame.Frame) || !identities.TryAdd(frame.Name, frame.Frame))
                throw new InvalidDataException($"Enemy frame {frame.Name} repeats a visual identity.");
        }
        return identities;
    }
}

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One authored look of an encounter's artwork (#1538): the encounter and the look's name. Every encounter shows its
/// <see cref="LoadedName"/> look as its room loads, until it shows another. A frame the encounter's looks change is a
/// finished picture of each look it is drawn in, named by <see cref="PictureName"/>.
/// </summary>
/// <param name="Encounter">The encounter whose look it is.</param>
/// <param name="Name">The look's name.</param>
public readonly record struct EncounterLook(EncounterArtwork Encounter, string Name)
{
    /// <summary>The name of the look every encounter shows as its room loads.</summary>
    public const string LoadedName = "Loaded";

    /// <summary>The look's name.</summary>
    public string Name { get; } = string.IsNullOrEmpty(Name) || Name.Contains('|', StringComparison.Ordinal) || Name.Contains('=', StringComparison.Ordinal)
        ? throw new ArgumentException($"An encounter look's name \"{Name}\" is empty or holds a picture name's separator.", nameof(Name))
        : Name;

    /// <summary>The look <paramref name="encounter"/> shows as its room loads.</summary>
    public static EncounterLook Loaded(EncounterArtwork encounter) => new(encounter, LoadedName);

    /// <summary>
    /// The name of the finished picture of <paramref name="frame"/> that shows each look of <paramref name="shown"/>, the
    /// looks of the encounters that change the frame, in their encounters' order: the frame's own name when none does.
    /// </summary>
    public static string PictureName(string frame, IEnumerable<EncounterLook> shown)
    {
        ArgumentException.ThrowIfNullOrEmpty(frame);
        return frame + string.Concat(Ensure.NotNull(shown).Select(look => $"|{look.Encounter}={look.Name}"));
    }

    /// <summary>Names the look for diagnostics.</summary>
    public override string ToString() => $"{Encounter} {Name}";
}

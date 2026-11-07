using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The look each of the room's encounters shows (#1538): the frames the encounter's looks change show their finished
/// picture of that look. A room starts with every encounter showing the look its room's load gives it.
/// </summary>
public sealed class EncounterLooks : IEncounterLookView
{
    /// <summary>The look each encounter that has shown another than its loaded look shows now.</summary>
    private readonly Dictionary<EncounterArtwork, EncounterLook> shown = [];

    /// <summary>A room load: every encounter of the new room shows the look its room's load gives it.</summary>
    internal void Restart() => shown.Clear();

    /// <summary>Records that the encounter whose look <paramref name="look"/> is shows it from this update on.</summary>
    internal void Show(EncounterLook look) => shown[look.Encounter] = look;

    /// <summary>The look <paramref name="encounter"/> shows now: its loaded look until it shows another.</summary>
    internal EncounterLook Current(EncounterArtwork encounter) =>
        shown.TryGetValue(encounter, out EncounterLook look) ? look : EncounterLook.Loaded(encounter);

    /// <inheritdoc />
    public bool Shows(EncounterLook look) => Current(look.Encounter) == look;
}

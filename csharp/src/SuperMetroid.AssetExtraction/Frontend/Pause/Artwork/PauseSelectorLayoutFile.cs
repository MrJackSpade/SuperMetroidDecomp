using System.Text.Json;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed equipment-selector layout (#1638): the anchors, frame sequences and timing the game binds. The frames'
/// compositions, whose parts are the original's packed sprite placements, are import data
/// (<see cref="PauseSelectorFormat.ArtworkFileName"/>).
/// </summary>
public static class PauseSelectorLayoutFile
{
    /// <summary>Supported layout revision.</summary>
    public const int Version = 1;

    /// <summary>The layout of the extracted artwork document <paramref name="artwork"/>, as the installation holds it.</summary>
    /// <exception cref="InvalidDataException">The artwork document does not decode.</exception>
    internal static byte[] Extract(byte[] artwork)
    {
        PauseSelectorDocument document = JsonAssetDocument.Read<PauseSelectorDocument>(artwork, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause selector document is null.");
        return JsonSerializer.SerializeToUtf8Bytes(new PauseSelectorLayoutDocument
        {
            Version = Version, InitialDurationTicks = document.InitialDurationTicks, Anchors = document.Anchors, Animation = document.Animation,
        }, global::SuperMetroid.AssetExtraction.AssetExtractionJson.Info<global::SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseSelectorLayoutDocument>(ArtworkJson.Options));
    }

    /// <summary>Reads and validates the installed layout: the anchors, frame sequences and timing the game binds.</summary>
    /// <exception cref="InvalidDataException">The JSON, version, anchors, phases or timing are invalid.</exception>
    public static PauseSelectorPresentation Read(Stream json)
    {
        PauseSelectorLayoutDocument document;
        try { document = JsonAssetDocument.Read<PauseSelectorLayoutDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause selector layout is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause selector layout JSON.", error); }
        if (document.Version != Version)
            throw new InvalidDataException($"Pause selector layout requires version {Version}.");
        // The frames each phase names are the pause sheet's; the sheet's own check finds a frame it does not hold.
        return Presentation(document.InitialDurationTicks, document.Anchors, document.Animation, name => !string.IsNullOrWhiteSpace(name));
    }

    /// <summary>
    /// The presentation <paramref name="anchors"/>, <paramref name="animation"/> and <paramref name="initialDuration"/> name,
    /// each phase's frames among those <paramref name="isFrame"/> accepts.
    /// </summary>
    /// <exception cref="InvalidDataException">An anchor, a phase or a duration is invalid.</exception>
    internal static PauseSelectorPresentation Presentation(int initialDuration, Dictionary<string, Offset16<ScreenPixels>>? anchors,
        PauseSelectorPhase[]? animation, Func<string, bool> isFrame)
    {
        if (initialDuration < 1 || anchors is null || anchors.Count != PauseSelectorFormat.AnchorCount ||
            animation is null || animation.Length < 1)
            throw new InvalidDataException("Pause selectors require a positive initial dwell, sixteen anchors and at least one phase.");
        var named = new Dictionary<string, Offset16<ScreenPixels>>(StringComparer.Ordinal);
        foreach (PauseEquipmentControl control in PauseEquipmentControls.All)
        {
            string name = control.Anchor;
            if (!anchors.TryGetValue(name, out var point) || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {name} requires screen coordinates X=0..255/Y=0..223.");
            named.Add(name, point);
        }
        var steps = new List<PauseSelectorPresentation.PauseSelectorStep>(animation.Length);
        for (int i = 0; i < animation.Length; i++)
        {
            var phase = animation[i];
            if (phase is null || phase.DurationTicks < 1 ||
                phase.Reserve is null || !isFrame(phase.Reserve) ||
                phase.Beam is null || !isFrame(phase.Beam) ||
                phase.Equipment is null || !isFrame(phase.Equipment))
                throw new InvalidDataException($"Pause selector phase {i} has invalid timing or an unknown frame.");
            steps.Add(new(new PauseSelectorLook(phase.Reserve, phase.Beam, phase.Equipment), phase.DurationTicks));
        }
        return new PauseSelectorPresentation(new PauseSelectorPresentation.PauseSelectorParts { Anchors = named, Steps = steps,
            InitialDuration = initialDuration });
    }
}

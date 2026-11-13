using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
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
        }, ArtworkJson.Options);
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
    internal static PauseSelectorPresentation Presentation(int initialDuration, Dictionary<string, Position16<SpritePixels>>? anchors,
        PauseSelectorPhase[]? animation, Func<string, bool> isFrame)
    {
        if (initialDuration is < 1 or > PauseSelectorFormat.MaximumDuration || anchors is null || anchors.Count != PauseSelectorFormat.AnchorCount ||
            animation is null || animation.Length is < 1 or > PauseSelectorFormat.MaximumPhases)
            throw new InvalidDataException("Pause selectors require version 1, sixteen anchors, palette 0..7, valid named frames and positive bounded timing.");
        var named = new Dictionary<string, Position16<SpritePixels>>(StringComparer.Ordinal);
        foreach (var definition in PauseSelectorAnchors.Anchors())
        {
            string name = PauseSelectorAnchors.Anchor(definition.Category, definition.Item);
            if (!anchors.TryGetValue(name, out var point) || point.X is < 0 or > 255 || point.Y is < 0 or > 223)
                throw new InvalidDataException($"Pause selector {name} requires screen coordinates X=0..255/Y=0..223.");
            named.Add(name, point);
        }
        var reserve = new string[animation.Length];
        var beam = new string[animation.Length];
        var equipment = new string[animation.Length];
        var durations = new int[animation.Length];
        for (int i = 0; i < animation.Length; i++)
        {
            var phase = animation[i];
            if (phase is null || phase.DurationTicks is < 1 or > PauseSelectorFormat.MaximumDuration ||
                phase.Reserve is null || !isFrame(phase.Reserve) ||
                phase.Beam is null || !isFrame(phase.Beam) ||
                phase.Equipment is null || !isFrame(phase.Equipment))
                throw new InvalidDataException($"Pause selector phase {i} has invalid timing or an unknown frame.");
            reserve[i] = phase.Reserve;
            beam[i] = phase.Beam;
            equipment[i] = phase.Equipment;
            durations[i] = phase.DurationTicks;
        }
        return new PauseSelectorPresentation(new PauseSelectorPresentation.PauseSelectorParts { Anchors = named, Reserve = reserve,
            Beam = beam, Equipment = equipment, Durations = durations, InitialDuration = initialDuration });
    }
}

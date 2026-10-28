using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain room colour document.</summary>
public static class MotherBrainRoomColorFile
{
    /// <summary>Validates and compiles fake-death flash, room-entry, phase-two and recovery color artwork without retaining the document's mutable RGB5 arrays.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <param name="currentStock">Explicit compatibility source required for version 1 or 2 documents: version 1 inherits its room-entry and recovery colors; version 2 inherits only recovery colors.</param>
    /// <returns>Selected native color words, with calculated representations used only where their complete output matches the supplied artwork.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, the version is unsupported without stock fallback, array sizes are wrong, or a required color is null or outside RGB5 range.</exception>
    public static MotherBrainRoomColorPresentation Read(Stream json,
        MotherBrainRoomColorPresentation? currentStock = null)
    {
        MotherBrainRoomColorDocument document = JsonAssetDocument.Read<MotherBrainRoomColorDocument>(
            json, ArtworkJson.Options, "Mother Brain room colors");
        int version = document.Version;
        if (version is not (MotherBrainRoomColorFormat.PreRoomEntryVersion or MotherBrainRoomColorFormat.PreRecoveryLightsVersion or
            MotherBrainRoomColorFormat.Version))
            throw new InvalidDataException("Mother Brain room colors require the supported version and fourteen flash rows.");
        bool inheritsRoomEntry = version == MotherBrainRoomColorFormat.PreRoomEntryVersion;
        bool inheritsRecoveryLights = version is MotherBrainRoomColorFormat.PreRoomEntryVersion or
            MotherBrainRoomColorFormat.PreRecoveryLightsVersion;
        if ((inheritsRecoveryLights && currentStock is null) ||
            document.Flash is null ||
            document.Flash.Length != MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException("Mother Brain room colors require the supported version and fourteen flash rows.");
        var flash = new RgbColor[document.Flash.Length][];
        for (int index = 0; index < flash.Length; index++)
            flash[index] = Compile(document.Flash[index], MotherBrainRoomColorDefinitions.SliceColors * 2,
                $"flash row {index}");
        RgbColor[] finalRoom = Compile(document.FinalRoom, MotherBrainRoomColorDefinitions.SliceColors * 2, "final room");
        // A legacy override inherits the families it predates from the current stock the
        // version check above required; a null family says so.
        return MotherBrainRoomColorPresentation.Create(new MotherBrainRoomColours
        {
            Flash = flash,
            FinalRoom = finalRoom,
            PhaseTwoAttack = Compile(document.PhaseTwoAttack, MotherBrainRoomColorDefinitions.PhaseTwoColors,
                "phase-two attack"),
            PhaseTwoRearLeg = Compile(document.PhaseTwoRearLeg, MotherBrainRoomColorDefinitions.PhaseTwoColors,
                "phase-two rear leg"),
            InitialGlassShard = inheritsRoomEntry
                ? null
                : Compile(document.InitialGlassShard, MotherBrainRoomColorDefinitions.InitialColors,
                    "room-entry glass shard"),
            InitialTubeProjectile = inheritsRoomEntry
                ? null
                : Compile(document.InitialTubeProjectile, MotherBrainRoomColorDefinitions.InitialColors,
                    "room-entry tube projectile"),
            RecoveryLights = inheritsRecoveryLights
                ? null
                : CompileRecoveryLights(document.RecoveryLights),
        }, currentStock);
    }

    /// <summary>Validates the seven recovery frames and compiles their RGB5 words.</summary>
    private static RgbColor[][] CompileRecoveryLights(PaletteRgb5[][]? frames)
    {
        if (frames is null || frames.Length != MotherBrainRoomColorDefinitions.RecoveryLightsFrames)
            throw new InvalidDataException("Mother Brain room-light recovery requires seven frames.");
        var compiled = new RgbColor[frames.Length][];
        for (int frame = 0; frame < frames.Length; frame++)
            compiled[frame] = Compile(frames[frame],
                MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors * 2,
                $"room-light recovery frame {frame}");
        return compiled;
    }

    /// <summary>Rejects missing or out-of-range RGB5 components before packing an authored palette row.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? colors, int expectedCount, string name)
    {
        if (colors is null || colors.Length != expectedCount)
            throw new InvalidDataException($"Mother Brain {name} requires {expectedCount} colors.");
        var compiled = new RgbColor[colors.Length];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Mother Brain {name} color {index} requires RGB5 components.");
            compiled[index] = color.ToRgbColor();
        }
        return compiled;
    }

    /// <summary>Serializes and validates a complete current-version room-color document before writing its UTF-8 JSON bytes; older versions have no stock fallback here.</summary>
    /// <param name="json">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored RGB5 arrays read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Read"/>'s current-version schema or color validation.</exception>
    public static void Write(Stream json, MotherBrainRoomColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

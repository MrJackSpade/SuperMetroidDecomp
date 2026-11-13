using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Mother Brain room colour document.</summary>
public static class MotherBrainRoomColorFile
{
    /// <summary>Validates and compiles fake-death flash, room-entry, phase-two and recovery color artwork without retaining the document's mutable RGB5 arrays.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <returns>Every installed colour set, kept as installed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, the version is unsupported, array sizes are wrong, or a required color is null or outside RGB5 range.</exception>
    public static MotherBrainRoomColorPresentation Read(Stream json)
    {
        MotherBrainRoomColorDocument document = JsonAssetDocument.Read<MotherBrainRoomColorDocument>(
            json, ArtworkJson.Options, "Mother Brain room colors");
        if (document.Version != MotherBrainRoomColorFormat.Version ||
            document.Flash is null ||
            document.Flash.Length != MotherBrainRoomPaletteAnimationDefinitions.FlashFrameCount)
            throw new InvalidDataException("Mother Brain room colors require the supported version and fourteen flash rows.");
        var flash = new RgbColor[document.Flash.Length][];
        for (int index = 0; index < flash.Length; index++)
            flash[index] = Compile(document.Flash[index], MotherBrainRoomColorGeometry.SliceColors * 2,
                $"flash row {index}");
        RgbColor[] finalRoom = Compile(document.FinalRoom, MotherBrainRoomColorGeometry.SliceColors * 2, "final room");
        return new MotherBrainRoomColorPresentation(new MotherBrainRoomColours
        {
            Flash = flash,
            FinalRoom = finalRoom,
            PhaseTwoAttack = Compile(document.PhaseTwoAttack, MotherBrainRoomColorGeometry.PhaseTwoColors,
                "phase-two attack"),
            PhaseTwoRearLeg = Compile(document.PhaseTwoRearLeg, MotherBrainRoomColorGeometry.PhaseTwoColors,
                "phase-two rear leg"),
            InitialGlassShard = Compile(document.InitialGlassShard, MotherBrainRoomColorGeometry.InitialColors,
                "room-entry glass shard"),
            InitialTubeProjectile = Compile(document.InitialTubeProjectile, MotherBrainRoomColorGeometry.InitialColors,
                "room-entry tube projectile"),
            RecoveryLights = CompileRecoveryLights(document.RecoveryLights),
        });
    }

    /// <summary>Validates the seven recovery frames and compiles their RGB5 words.</summary>
    private static RgbColor[][] CompileRecoveryLights(PaletteRgb5[][]? frames)
    {
        if (frames is null || frames.Length != MotherBrainRoomColorDefinitions.RecoveryLightsFrames)
            throw new InvalidDataException("Mother Brain room-light recovery requires seven frames.");
        var compiled = new RgbColor[frames.Length][];
        for (int frame = 0; frame < frames.Length; frame++)
            compiled[frame] = Compile(frames[frame],
                MotherBrainRoomColorGeometry.RecoveryLightsSliceColors * 2,
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

    /// <summary>Serializes a room-color document and writes its UTF-8 JSON bytes.</summary>
    /// <param name="json">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored RGB5 arrays read for serialization, not retained by the writer.</param>
    public static void Write(Stream json, MotherBrainRoomColorDocument document)
    {
        json.Write(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
    }
}

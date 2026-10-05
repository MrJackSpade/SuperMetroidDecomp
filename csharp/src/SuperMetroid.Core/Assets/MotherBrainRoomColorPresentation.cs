using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Mother Brain fake-death room flash and phase-two initial colors.</summary>
public sealed class MotherBrainRoomColorPresentation
{
    private readonly ushort[][] flash;
    private readonly ushort[] finalRoom;
    private readonly ushort[] phaseTwoAttack;
    private readonly ushort[] phaseTwoRearLeg;
    private readonly ushort[] initialGlassShard;
    private readonly ushort[] initialTubeProjectile;
    private readonly RecoveryLightFade recoveryLights;

    /// <summary>Timed-entry identities installed by the validated flash rows; exposes no color payload.</summary>
    internal IEnumerable<ushort> FlashEntryPointers => Enumerable.Range(0, flash.Length)
        .Select(index => checked((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart +
            index * MotherBrainRoomColorRomData.TimedEntryByteCount)));

    private MotherBrainRoomColorPresentation(ushort[][] flash, ushort[] finalRoom,
        ushort[] phaseTwoAttack, ushort[] phaseTwoRearLeg,
        ushort[] initialGlassShard, ushort[] initialTubeProjectile,
        RecoveryLightFade recoveryLights)
    {
        this.flash = flash;
        this.finalRoom = finalRoom;
        this.phaseTwoAttack = phaseTwoAttack;
        this.phaseTwoRearLeg = phaseTwoRearLeg;
        this.initialGlassShard = initialGlassShard;
        this.initialTubeProjectile = initialTubeProjectile;
        this.recoveryLights = recoveryLights;
    }

    /// <summary>Applies a color row selected by the compiled $A9:D046 timing program.</summary>
    public void ApplyFlash(SnesCgram cgram, ushort timedEntryPointer)
    {
        int offset = timedEntryPointer - MotherBrainRoomPaletteProgramDefinitions.FlashStart;
        int stride = MotherBrainRoomColorRomData.TimedEntryByteCount;
        if (offset < 0 || offset % stride != 0 || (uint)(offset / stride) >= flash.Length)
            throw new InvalidDataException($"Mother Brain room-flash entry $A9:{timedEntryPointer:X4} is not authored.");
        ApplyRoom(cgram, flash[offset / stride]);
    }

    /// <summary>Applies the final grey room colors when the flash program is stopped.</summary>
    public void ApplyFinal(SnesCgram cgram) => ApplyRoom(cgram, finalRoom);

    /// <summary>Installs the two nontransparent OBJ palettes before phase two starts.</summary>
    public void ApplyPhaseTwoInitial(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.PhaseTwoColors; index++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.PhaseTwoAttackColor + index,
                phaseTwoAttack[index]);
            cgram.SetColor(MotherBrainRoomColorRomData.PhaseTwoRearLegColor + index,
                phaseTwoRearLeg[index]);
        }
    }

    /// <summary>Installs room-entry glass-shard and tube-projectile sprite colors.</summary>
    public void ApplyRoomEntry(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.InitialColors; index++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.InitialGlassShardColor + index,
                initialGlassShard[index]);
            cgram.SetColor(MotherBrainRoomColorRomData.InitialTubeProjectileColor + index,
                initialTubeProjectile[index]);
        }
    }

    /// <summary>Applies one room-light image after the Baby Metroid cutscene.</summary>
    public void ApplyRecoveryLights(SnesCgram cgram, int frame)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)frame >= MotherBrainRoomColorRomData.RecoveryLightsFrames)
            throw new InvalidDataException($"Mother Brain room-light recovery frame {frame} is not authored.");

        for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination;
             index++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.RecoveryLightsFirstColor + index,
                recoveryLights.Color(frame, index));
            cgram.SetColor(MotherBrainRoomColorRomData.RecoveryLightsSecondColor + index,
                recoveryLights.Color(frame, MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination + index));
        }
    }

    private static void ApplyRoom(SnesCgram cgram, ushort[] colors)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.SliceColors; index++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.FirstColor + index, colors[index]);
            ushort second = colors[MotherBrainRoomColorRomData.SliceColors + index];
            cgram.SetColor(MotherBrainRoomColorRomData.SecondColor + index, second);
            cgram.SetColor(MotherBrainRoomColorRomData.MirroredSecondColor + index, second);
        }
    }

    public static MotherBrainRoomColorPresentation Load(Stream json,
        MotherBrainRoomColorPresentation? currentStock = null)
    {
        MotherBrainRoomColorDocument document = JsonAssetDocument.Read<MotherBrainRoomColorDocument>(
            json, MapPresentationFormat.JsonOptions, "Mother Brain room colors");
        bool previousWithStock = currentStock is not null &&
            document.Version is MotherBrainRoomColorFormat.PreRoomEntryVersion or
                MotherBrainRoomColorFormat.PreRecoveryLightsVersion;
        if ((document.Version != MotherBrainRoomColorFormat.Version && !previousWithStock) ||
            document.Flash is null ||
            document.Flash.Length != MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException("Mother Brain room colors require the supported version and fourteen flash rows.");
        var flash = new ushort[document.Flash.Length][];
        for (int index = 0; index < flash.Length; index++)
            flash[index] = Compile(document.Flash[index], MotherBrainRoomColorRomData.SliceColors * 2,
                $"flash row {index}");
        return new(flash,
            Compile(document.FinalRoom, MotherBrainRoomColorRomData.SliceColors * 2, "final room"),
            Compile(document.PhaseTwoAttack, MotherBrainRoomColorRomData.PhaseTwoColors,
                "phase-two attack"),
            Compile(document.PhaseTwoRearLeg, MotherBrainRoomColorRomData.PhaseTwoColors,
                "phase-two rear leg"),
            document.Version == MotherBrainRoomColorFormat.PreRoomEntryVersion
                ? currentStock!.initialGlassShard
                : Compile(document.InitialGlassShard, MotherBrainRoomColorRomData.InitialColors,
                    "room-entry glass shard"),
            document.Version == MotherBrainRoomColorFormat.PreRoomEntryVersion
                ? currentStock!.initialTubeProjectile
                : Compile(document.InitialTubeProjectile, MotherBrainRoomColorRomData.InitialColors,
                    "room-entry tube projectile"),
            document.Version < MotherBrainRoomColorFormat.Version
                ? currentStock!.recoveryLights
                : new RecoveryLightFade(CompileRecoveryLights(document.RecoveryLights)));
    }

    /// <summary>Seven equal light-intensity steps evaluated before RGB5 quantization.</summary>
    private sealed class RecoveryLightFade
    {
        private readonly LightChannel[] channels;

        public RecoveryLightFade(ushort[][] rows)
        {
            int last = MotherBrainRoomColorRomData.RecoveryLightsFrames - 1;
            channels = new LightChannel[rows[last].Length * 3];
            for (int color = 0; color < rows[last].Length; color++)
                for (int component = 0; component < 3; component++)
                {
                    int endpoint = (rows[last][color] >> (component * 5)) & 31;
                    int basis = endpoint * 8;
                    for (; basis < endpoint * 8 + 8; basis++)
                    {
                        bool matches = true;
                        for (int frame = 0; frame <= last; frame++)
                            if (Intensity(basis, frame) != ((rows[frame][color] >> (component * 5)) & 31))
                            { matches = false; break; }
                        if (matches) break;
                    }
                    byte[]? supplied = null;
                    if (basis == endpoint * 8 + 8)
                    {
                        supplied = new byte[last + 1];
                        for (int frame = 0; frame <= last; frame++)
                            supplied[frame] = (byte)((rows[frame][color] >> (component * 5)) & 31);
                    }
                    channels[color * 3 + component] = new LightChannel(basis, supplied);
                }
        }

        public ushort Color(int frame, int color)
        {
            int result = 0;
            for (int component = 0; component < 3; component++)
            {
                LightChannel channel = channels[color * 3 + component];
                int value = channel.Supplied is { } supplied ? supplied[frame] : Intensity(channel.Basis, frame);
                result |= value << (component * 5);
            }
            return (ushort)result;
        }

        private static int Intensity(int basis, int frame) =>
            basis * (frame + 1) / (MotherBrainRoomColorRomData.RecoveryLightsFrames * 8);

        private readonly record struct LightChannel(int Basis, byte[]? Supplied);
    }
    private static ushort[][] CompileRecoveryLights(PaletteRgb5[][]? frames)
    {
        if (frames is null || frames.Length != MotherBrainRoomColorRomData.RecoveryLightsFrames)
            throw new InvalidDataException("Mother Brain room-light recovery requires seven frames.");
        var compiled = new ushort[frames.Length][];
        for (int frame = 0; frame < frames.Length; frame++)
            compiled[frame] = Compile(frames[frame],
                MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination * 2,
                $"room-light recovery frame {frame}");
        return compiled;
    }

    private static ushort[] Compile(PaletteRgb5[]? colors, int expectedCount, string name)
    {
        if (colors is null || colors.Length != expectedCount)
            throw new InvalidDataException($"Mother Brain {name} requires {expectedCount} colors.");
        var compiled = new ushort[colors.Length];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Mother Brain {name} color {index} requires RGB5 components.");
            compiled[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return compiled;
    }

    public static void Write(Stream json, MotherBrainRoomColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MotherBrainRoomColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Flash { get; init; }
    public required PaletteRgb5[] FinalRoom { get; init; }
    public required PaletteRgb5[] PhaseTwoAttack { get; init; }
    public required PaletteRgb5[] PhaseTwoRearLeg { get; init; }
    public PaletteRgb5[]? InitialGlassShard { get; init; }
    public PaletteRgb5[]? InitialTubeProjectile { get; init; }
    public PaletteRgb5[][]? RecoveryLights { get; init; }
}

public static class MotherBrainRoomColorFormat
{
    public const string FileName = "mother-brain-room-colors.json";
    public const int Version = 3;
    public const int PreRoomEntryVersion = 1;
    public const int PreRecoveryLightsVersion = 2;
}

using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Mother Brain fake-death room flash and phase-two initial colors.</summary>
public sealed class MotherBrainRoomColorPresentation
{
    private readonly RoomFlash flash;
    private readonly FinalRoomPalette finalRoom;
    private readonly AttackPalette phaseTwoAttack;
    private readonly ushort[]? phaseTwoRearLeg;
    private readonly ushort[] initialGlassShard;
    private readonly ushort[] initialTubeProjectile;
    private readonly RecoveryLightFade recoveryLights;

    /// <summary>Timed-entry identities installed by the validated flash rows; exposes no color payload.</summary>
    internal IEnumerable<ushort> FlashEntryPointers => Enumerable.Range(0, flash.FrameCount)
        .Select(index => checked((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart +
            index * MotherBrainRoomColorRomData.TimedEntryByteCount)));

    private MotherBrainRoomColorPresentation(ushort[][] flash, FinalRoomPalette finalRoom,
        ushort[] phaseTwoAttack, ushort[] phaseTwoRearLeg,
        ushort[] initialGlassShard, ushort[] initialTubeProjectile,
        RecoveryLightFade recoveryLights)
    {
        this.flash = new RoomFlash(flash, finalRoom);
        this.finalRoom = finalRoom;
        this.phaseTwoAttack = new AttackPalette(phaseTwoAttack);
        this.phaseTwoRearLeg = phaseTwoRearLeg.Where((word, color) =>
            word != MotherBrainHealthPalettePresentation.StockBaseColor(backLeg: true, color)).Any() ? phaseTwoRearLeg : null;
        this.initialGlassShard = initialGlassShard;
        this.initialTubeProjectile = initialTubeProjectile;
        this.recoveryLights = recoveryLights;
    }

    /// <summary>Applies a color row selected by the compiled $A9:D046 timing program.</summary>
    public void ApplyFlash(SnesCgram cgram, ushort timedEntryPointer)
    {
        int offset = timedEntryPointer - MotherBrainRoomPaletteProgramDefinitions.FlashStart;
        int stride = MotherBrainRoomColorRomData.TimedEntryByteCount;
        if (offset < 0 || offset % stride != 0 || (uint)(offset / stride) >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException($"Mother Brain room-flash entry $A9:{timedEntryPointer:X4} is not authored.");
        ArgumentNullException.ThrowIfNull(cgram);
        int frame = offset / stride;
        for (int color = 0; color < MotherBrainRoomColorRomData.SliceColors; color++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.FirstColor + color, flash.Color(frame, color));
            ushort second = flash.Color(frame, MotherBrainRoomColorRomData.SliceColors + color);
            cgram.SetColor(MotherBrainRoomColorRomData.SecondColor + color, second);
            cgram.SetColor(MotherBrainRoomColorRomData.MirroredSecondColor + color, second);
        }
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
                phaseTwoAttack.Color(index));
            cgram.SetColor(MotherBrainRoomColorRomData.PhaseTwoRearLegColor + index,
                phaseTwoRearLeg?[index] ?? MotherBrainHealthPalettePresentation.StockBaseColor(backLeg: true, index));
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

    private static void ApplyRoom(SnesCgram cgram, FinalRoomPalette colors)
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
        var finalRoom = new FinalRoomPalette(Compile(document.FinalRoom, MotherBrainRoomColorRomData.SliceColors * 2, "final room"));
        return new(flash, finalRoom,
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
                : new RecoveryLightFade(CompileRecoveryLights(document.RecoveryLights), finalRoom));
    }

    private sealed class FinalRoomPalette
    {
        private readonly ushort[]? supplied;
        public int Length { get; }
        public FinalRoomPalette(ushort[] colors)
        {
            Length = colors.Length;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        public ushort this[int color] => (uint)color < Length
            ? supplied is null ? Calculate(color) : supplied[color] : throw new IndexOutOfRangeException();
        private static ushort Calculate(int color)
        {
            if (color < MotherBrainRoomColorRomData.RoomShadowFirst)
                return InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.WallLight, MotherBrainFinalRoomPaintDefinitions.WallDark, color - MotherBrainRoomColorRomData.RoomWallFirst,
                    MotherBrainRoomColorRomData.RoomWallCount - 1);
            // Native shadow red is5,4,3,1: ceiling interpolation. Green7,5,4,2
            // and blue8,6,4,2 use nearest interpolation. This is an exact sequence
            // relationship, not a claim about a historical palette tool.
            if (color < MotherBrainRoomColorRomData.RoomShadowFirst + MotherBrainRoomColorRomData.RoomShadowCount)
                return InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight, InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight, 0,
                    MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor - 1,
                    MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor), color - MotherBrainRoomColorRomData.RoomShadowFirst,
                    MotherBrainRoomColorRomData.RoomShadowCount - 1, ceilingRed: MotherBrainFinalRoomPaintDefinitions.RecessedRedCeiling);
            if (color < MotherBrainRoomColorRomData.RoomOutlineColor)
                return color == MotherBrainRoomColorRomData.RoomAmberColor ? MotherBrainFinalRoomPaintDefinitions.Amber : (ushort)0;
            if (color == MotherBrainRoomColorRomData.RoomOutlineColor) return MotherBrainFinalRoomPaintDefinitions.PanelField;
            if (color < MotherBrainRoomColorRomData.RoomDarkGrayColor)
                return InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLight, MotherBrainFinalRoomPaintDefinitions.MetalLow, color - MotherBrainRoomColorRomData.RoomGrayFirst,
                    MotherBrainRoomColorRomData.RoomGrayCount - 1);
            return color switch
            {
                // Metal contour shadow is half the low gray surface intensity.
                MotherBrainRoomColorRomData.RoomDarkGrayColor or MotherBrainRoomColorRomData.RoomRepeatedDarkGrayColor =>
                    InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLow, 0, MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor - 1, MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor),
                MotherBrainRoomColorRomData.RoomRedFirst => MotherBrainFinalRoomPaintDefinitions.RedLens,
                MotherBrainRoomColorRomData.RoomRedFirst + 1 => (ushort)Math.Max(0, (MotherBrainFinalRoomPaintDefinitions.RedLens & 31) - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop),
                MotherBrainRoomColorRomData.RoomBlueFirst => MotherBrainFinalRoomPaintDefinitions.BlueLens,
                MotherBrainRoomColorRomData.RoomBlueFirst + 1 => (ushort)(Math.Max(0, (MotherBrainFinalRoomPaintDefinitions.BlueLens >> 10 & 31) - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop) << 10),
                MotherBrainRoomColorRomData.RoomGlowColor => MotherBrainFinalRoomPaintDefinitions.WarmAccent,
                _ => MotherBrainHealthPalettePresentation.StockBaseColor(false, MotherBrainRoomColorRomData.WhiteColor),
            };
        }
    }
    private static ushort InterpolateRgb5(ushort start, ushort end, int step, int intervals, bool ceilingRed = false)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((start >> shift & 31) * (intervals - step) + (end >> shift & 31) * step + (ceilingRed && shift == 0 ? intervals - 1 : intervals / 2)) / intervals) << shift;
        return (ushort)result;
    }
    // Shared shades use their definitions only after the installed output matches.
    // The shared recovery highlight is reviewed paint; supplied edits remain independent.
    private sealed class AttackPalette
    {
        private readonly ushort[]? supplied;
        public AttackPalette(ushort[] colors)
        {
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        public ushort Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        private static ushort Calculate(int color)
        {
            if (color < MotherBrainRoomColorRomData.AttackFlameFirst)
                return InterpolateShade(MotherBrainAttackPaintDefinitions.RingHighlight, MotherBrainAttackPaintDefinitions.RingEdge,
                    color - MotherBrainRoomColorRomData.AttackCyanFirst,
                    MotherBrainRoomColorRomData.AttackCyanCount - 1);
            if (color < MotherBrainRoomColorRomData.AttackTissueFirst)
                return InterpolateShade(MotherBrainAttackPaintDefinitions.CoreHighlight, MotherBrainAttackPaintDefinitions.CoreEdge,
                    color - MotherBrainRoomColorRomData.AttackFlameFirst,
                    MotherBrainRoomColorRomData.AttackFlameCount - 1);
            if (color < MotherBrainRoomColorRomData.AttackGrayFirst)
                return MotherBrainHealthPalettePresentation.StockBaseColor(false,
                    MotherBrainRoomColorRomData.AttackBodyTissueFirst + color - MotherBrainRoomColorRomData.AttackTissueFirst);
            if (color == MotherBrainRoomColorRomData.WhiteColor)
                return MotherBrainHealthPalettePresentation.StockBaseColor(false, MotherBrainRoomColorRomData.WhiteColor);
            return InterpolateShade(MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight, MotherBrainAttackPaintDefinitions.ShellOutline,
                color - MotherBrainRoomColorRomData.AttackGrayFirst
                - (color > MotherBrainRoomColorRomData.WhiteColor ? 1 : 0),
                MotherBrainRoomColorRomData.AttackGrayLast - MotherBrainRoomColorRomData.AttackGrayFirst - 1);
        }
        private static ushort InterpolateShade(ushort first, ushort last, int shade, int intervals)
        {
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((first >> shift & 31) * (intervals - shade)
                    + (last >> shift & 31) * shade + intervals / 2) / intervals) << shift;
            return (ushort)result;
        }
    }
    /// <summary>Background highlight interpolation paired with darkening level colors.</summary>
    private sealed class RoomFlash
    {
        private readonly FinalRoomPalette basis;
        private readonly ushort highlight;
        private readonly ushort[][]? supplied;

        public int FrameCount { get; }

        public RoomFlash(ushort[][] rows, FinalRoomPalette finalRoom)
        {
            FrameCount = rows.Length;
            basis = finalRoom;
            highlight = rows[3][0];
            for (int frame = 0; frame < rows.Length; frame++)
                for (int color = 0; color < basis.Length; color++)
                    if (Calculate(frame, color) != rows[frame][color])
                    { supplied = rows; return; }
        }

        public ushort Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        private ushort Calculate(int frame, int color)
        {
            int strength = MotherBrainRoomPaletteProgramDefinitions.FlashStrength(frame);
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
            {
                int channel = (basis[color] >> shift) & 31;
                // The first level color joins the background highlight; the remaining
                // level colors dim by one quarter per strength step, rounding nearest.
                int value = color <= MotherBrainRoomColorRomData.SliceColors
                    ? (channel * (3 - strength) + ((highlight >> shift) & 31) * strength + 1) / 3
                    : (channel * (4 - strength) + 2) / 4;
                result |= value << shift;
            }
            return (ushort)result;
        }
    }
    /// <summary>Seven equal RGB5 intensity steps with shared room-palette endpoints.</summary>
    private sealed class RecoveryLightFade
    {
        private readonly FinalRoomPalette finalRoom;
        private readonly ushort backgroundLight;
        private readonly ushort backgroundMiddle;
        private readonly ushort backgroundDark;
        private readonly ushort neutralHighlight;
        private readonly ushort[][]? supplied;

        public RecoveryLightFade(ushort[][] rows, FinalRoomPalette finalRoom)
        {
            this.finalRoom = finalRoom;
            ushort[] full = rows[^1];
            backgroundLight = full[0];
            backgroundMiddle = full[1];
            backgroundDark = full[2];
            neutralHighlight = full[14];
            for (int frame = 0; frame < rows.Length; frame++)
                for (int color = 0; color < rows[frame].Length; color++)
                    if (Calculate(frame, color) != rows[frame][color])
                    { supplied = rows; return; }
        }

        public ushort Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        private ushort Calculate(int frame, int color)
        {
            ushort endpoint = color switch
            {
                0 => backgroundLight,
                1 => backgroundMiddle,
                2 => backgroundDark,
                >= 3 and <= 13 => finalRoom[color - 3],
                14 or 15 => neutralHighlight,
                _ => finalRoom[color - 4],
            };
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((endpoint >> shift) & 31) * (frame + 1) /
                    MotherBrainRoomColorRomData.RecoveryLightsFrames) << shift;
            return (ushort)result;
        }
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

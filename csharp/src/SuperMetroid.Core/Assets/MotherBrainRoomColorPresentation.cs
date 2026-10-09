using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Mother Brain fake-death room flash and phase-two initial colors.</summary>
public sealed class MotherBrainRoomColorPresentation
{
    /// <summary>Compiled fourteen-row fake-death room palette program.</summary>
    internal readonly RoomFlash flash;
    /// <summary>Compiled final room palette used as the basis for related color ramps.</summary>
    private readonly FinalRoomPalette finalRoom;
    /// <summary>Phase-two Mother Brain attack sprite colors.</summary>
    private readonly AttackPalette phaseTwoAttack;
    /// <summary>Optional authored rear-leg colors; null selects the stock room-compatible palette.</summary>
    private readonly ushort[]? phaseTwoRearLeg;
    /// <summary>Room-entry glass-shard palette, calculated from the room where it matches.</summary>
    private readonly GlassPalette initialGlassShard;
    /// <summary>Room-entry tube-projectile palette, calculated from the room where it matches.</summary>
    private readonly TubePalette initialTubeProjectile;
    /// <summary>Seven-frame light recovery palette program after the Baby Metroid scene.</summary>
    private readonly RecoveryLightFade recoveryLights;

    /// <summary>Stores compiled palette data and selects calculated or supplied colors by exact equality.</summary>
    /// <param name="flash">Fourteen compiled rows for the native fake-death palette program.</param>
    /// <param name="finalRoom">Compiled final room colors used by derived palettes.</param>
    /// <param name="phaseTwoAttack">Compiled phase-two attack sprite colors.</param>
    /// <param name="phaseTwoRearLeg">Compiled rear-leg colors, compared with stock before retention.</param>
    /// <param name="initialGlassShard">Room-entry shard palette.</param>
    /// <param name="initialTubeProjectile">Room-entry tube-projectile palette.</param>
    /// <param name="recoveryLights">Compiled post-cutscene room-light frames.</param>
    private MotherBrainRoomColorPresentation(ushort[][] flash, FinalRoomPalette finalRoom,
        ushort[] phaseTwoAttack, ushort[] phaseTwoRearLeg,
        GlassPalette initialGlassShard, TubePalette initialTubeProjectile,
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
    /// <param name="cgram">Destination color memory; twelve colors at indices 52..63 and 83..94 are replaced, with the second slice mirrored to 115..126.</param>
    /// <param name="timedEntryPointer">Bank-$A9 timed-entry address $D046 + row * 4 for row 0..13, pointing at the duration word, not its palette operand.</param>
    /// <exception cref="InvalidDataException"><paramref name="timedEntryPointer"/> is not one of the fourteen compiled entry addresses.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
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
    /// <param name="cgram">Destination color memory; the same 52..63, 83..94 and mirrored 115..126 slices as <see cref="ApplyFlash"/> are replaced.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyFinal(SnesCgram cgram) => ApplyRoom(cgram, finalRoom);

    /// <summary>Installs the two nontransparent OBJ palettes before phase two starts.</summary>
    /// <param name="cgram">Destination color memory; attack colors 161..175 and rear-leg colors 177..191 are replaced while palette color zero is preserved.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
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
    /// <param name="cgram">Destination color memory; glass colors 177..191 and tube-projectile colors 241..255 are replaced without their transparent color-zero entries.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyRoomEntry(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.InitialColors; index++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.InitialGlassShardColor + index,
                initialGlassShard.Color(index));
            cgram.SetColor(MotherBrainRoomColorRomData.InitialTubeProjectileColor + index,
                initialTubeProjectile.Color(index));
        }
    }

    /// <summary>$AD:F209, FadeOutBackgroundForBabyMetroidDeathSequence: immediately blacks
    /// out colors 1..E of BG palettes 3 and 5, the same slices later restored by $AD:F24B.</summary>
    /// <param name="cgram">Destination color memory; all entries outside indices 49..62 and 81..94 remain unchanged.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public static void ApplyBabyMetroidDeathBlackout(SnesCgram cgram)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination; index++)
        {
            cgram.SetColor(MotherBrainRoomColorRomData.RecoveryLightsFirstColor + index, 0);
            cgram.SetColor(MotherBrainRoomColorRomData.RecoveryLightsSecondColor + index, 0);
        }
    }

    /// <summary>Applies one room-light image after the Baby Metroid cutscene.</summary>
    /// <param name="cgram">Destination color memory receiving fourteen-color slices at indices 49..62 and 81..94.</param>
    /// <param name="frame">Recovery image 0..6 in playback order, corresponding to native source $AD:F3D3 - frame * $38; this call does not advance the cutscene's timer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is outside the seven authored images.</exception>
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

    /// <summary>Writes final room colors into their two CGRAM ranges and mirrors the second range.</summary>
    /// <param name="cgram">Destination color memory.</param>
    /// <param name="colors">Palette providing the two source slices.</param>
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

    /// <summary>Validates and compiles fake-death flash, room-entry, phase-two and recovery color artwork without retaining the document's mutable RGB5 arrays.</summary>
    /// <param name="json">UTF-8 JSON read from its current position to the end and left open.</param>
    /// <param name="currentStock">Explicit compatibility source required for version 1 or 2 documents: version 1 inherits its room-entry and recovery colors; version 2 inherits only recovery colors.</param>
    /// <returns>Selected native color words, with calculated representations used only where their complete output matches the supplied artwork.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">JSON is invalid or ambiguous, the version is unsupported without stock fallback, array sizes are wrong, or a required color is null or outside RGB5 range.</exception>
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
                : new GlassPalette(Compile(document.InitialGlassShard, MotherBrainRoomColorRomData.InitialColors,
                    "room-entry glass shard"), finalRoom),
            document.Version == MotherBrainRoomColorFormat.PreRoomEntryVersion
                ? currentStock!.initialTubeProjectile
                : new TubePalette(Compile(document.InitialTubeProjectile, MotherBrainRoomColorRomData.InitialColors,
                    "room-entry tube projectile"), finalRoom),
            document.Version < MotherBrainRoomColorFormat.Version
                ? currentStock!.recoveryLights
                : new RecoveryLightFade(CompileRecoveryLights(document.RecoveryLights), finalRoom));
    }

    /// <summary>Provides final room colors from supplied artwork or its exact RGB5 paint calculation.</summary>
    internal sealed class FinalRoomPalette
    {
        /// <summary>Explicit color words retained when at least one supplied value differs from the formula.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Number of room colors available through the indexer.</summary>
        public int Length { get; }
        /// <summary>Checks whether the complete supplied palette matches the calculated room paint.</summary>
        /// <param name="colors">Compiled RGB5 words in room-palette order.</param>
        public FinalRoomPalette(ushort[] colors)
        {
            Length = colors.Length;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Gets a room color, calculating it when the authored palette exactly matches the formula.</summary>
        /// <param name="color">Index in the room palette.</param>
        /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="Length"/>.</exception>
        public ushort this[int color] => (uint)color < Length
            ? supplied is null ? Calculate(color) : supplied[color] : throw new IndexOutOfRangeException();
        /// <summary>Calculates one wall, recess, metal, lens, or accent color from reviewed paint endpoints.</summary>
        /// <param name="color">Room palette index to calculate.</param>
        /// <returns>The packed RGB5 color at that index.</returns>
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
    /// <summary>Interpolates packed RGB5 channels independently, with an optional ceiling rule for red.</summary>
    /// <param name="start">Color at the beginning of the ramp.</param>
    /// <param name="end">Color at the end of the ramp.</param>
    /// <param name="step">Current zero-based position in the ramp.</param>
    /// <param name="intervals">Number of steps between endpoints.</param>
    /// <param name="ceilingRed">When set, rounds the red channel upward to match the authored sequence.</param>
    /// <returns>The interpolated packed RGB5 color.</returns>
    private static ushort InterpolateRgb5(ushort start, ushort end, int step, int intervals, bool ceilingRed = false)
    {
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
            result |= (((start >> shift & 31) * (intervals - step) + (end >> shift & 31) * step + (ceilingRed && shift == 0 ? intervals - 1 : intervals / 2)) / intervals) << shift;
        return (ushort)result;
    }
    // Shared shades use their definitions only after the installed output matches.
    // The shared recovery highlight is reviewed paint; supplied edits remain independent.
    /// <summary>Provides phase-two attack colors, retaining the supplied array if any value differs from the paint formula.</summary>
    private sealed class AttackPalette
    {
        /// <summary>Explicit colors retained when the full array does not match calculated paint.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Tests whether all attack colors match their calculated representation.</summary>
        /// <param name="colors">Compiled nontransparent attack colors.</param>
        public AttackPalette(ushort[] colors)
        {
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Gets one supplied or calculated attack color.</summary>
        /// <param name="color">Index in the phase-two attack palette.</param>
        /// <returns>The packed RGB5 color.</returns>
        public ushort Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Calculates the attack palette's ring, flame, tissue, and casing colors.</summary>
        /// <param name="color">Index in the nontransparent attack palette.</param>
        /// <returns>The packed RGB5 color at that index.</returns>
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
        /// <summary>Interpolates each RGB5 channel between the attack palette's shade endpoints.</summary>
        /// <param name="first">Highlight color at the start of the shade ramp.</param>
        /// <param name="last">Edge color at the end of the shade ramp.</param>
        /// <param name="shade">Current zero-based shade position.</param>
        /// <param name="intervals">Number of steps between endpoint colors.</param>
        /// <returns>The interpolated packed RGB5 shade.</returns>
        private static ushort InterpolateShade(ushort first, ushort last, int shade, int intervals)
        {
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((first >> shift & 31) * (intervals - shade)
                    + (last >> shift & 31) * shade + intervals / 2) / intervals) << shift;
            return (ushort)result;
        }
    }
    /// <summary>Derives the entry glass-shard ramp from final room colors and reviewed highlights.</summary>
    private sealed class GlassPalette
    {
        /// <summary>Room palette supplying the shard's gray and dark-gray colors.</summary>
        private readonly FinalRoomPalette room;
        /// <summary>Explicit shard colors retained if any entry differs from the derived ramp.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Chooses calculated colors only when the supplied shard palette fully matches them.</summary>
        /// <param name="colors">Compiled room-entry shard colors.</param>
        /// <param name="room">Final room palette used for shared gray colors.</param>
        public GlassPalette(ushort[] colors, FinalRoomPalette room)
        {
            this.room = room;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Gets one supplied or room-derived shard color.</summary>
        /// <param name="color">Index in the nontransparent shard palette.</param>
        /// <returns>The packed RGB5 color.</returns>
        public ushort Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Calculates a shard color from the room grays, glass highlight, and stock fallback.</summary>
        /// <param name="color">Index in the nontransparent shard palette.</param>
        /// <returns>The packed RGB5 color at that index.</returns>
        private ushort Calculate(int color)
        {
            if (color < MotherBrainRoomColorRomData.GlassRampFirst)
                return room[MotherBrainRoomColorRomData.RoomGrayFirst + color];
            if (color < MotherBrainRoomColorRomData.GlassDarkGrayColor)
                return InterpolateRgb5(MotherBrainGlassPaintDefinitions.FaceHighlight, room[MotherBrainRoomColorRomData.RoomOutlineColor],
                    color - MotherBrainRoomColorRomData.GlassRampFirst, MotherBrainRoomColorRomData.GlassRampCount - 1);
            return color switch
            {
                MotherBrainRoomColorRomData.GlassDarkGrayColor => room[MotherBrainRoomColorRomData.RoomDarkGrayColor],
                MotherBrainRoomColorRomData.GlassNeutralColor => MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight,
                _ => MotherBrainHealthPalettePresentation.StockBaseColor(false, color),
            };
        }
    }
    /// <summary>Derives the room-entry tube projectile colors from shared highlights and the room outline.</summary>
    private sealed class TubePalette
    {
        /// <summary>Room palette supplying the outline used in the tube's dark ramp.</summary>
        private readonly FinalRoomPalette room;
        /// <summary>Explicit tube colors retained if any entry differs from the derived colors.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Chooses calculated colors only when the supplied tube palette fully matches them.</summary>
        /// <param name="colors">Compiled room-entry tube-projectile colors.</param>
        /// <param name="room">Final room palette used for the dark ramp.</param>
        public TubePalette(ushort[] colors, FinalRoomPalette room)
        {
            this.room = room;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Gets one supplied or room-derived tube color.</summary>
        /// <param name="color">Index in the nontransparent tube palette.</param>
        /// <returns>The packed RGB5 color.</returns>
        public ushort Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Calculates a tube color from the shared neutral, black, or room-outline color.</summary>
        /// <param name="color">Index in the nontransparent tube palette.</param>
        /// <returns>The packed RGB5 color at that index.</returns>
        private ushort Calculate(int color) => color < MotherBrainRoomColorRomData.TubeNeutralCount
            ? MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight : color == MotherBrainRoomColorRomData.RoomEntryBlackColor
                ? MotherBrainHealthPalettePresentation.StockBaseColor(false, color)
                : room[MotherBrainRoomColorRomData.RoomOutlineColor + color - MotherBrainRoomColorRomData.TubeNeutralCount];
    }
    /// <summary>Background highlight interpolation paired with darkening level colors.</summary>
    internal sealed class RoomFlash
    {
        /// <summary>Final room palette used as the source image for each flash frame.</summary>
        private readonly FinalRoomPalette basis;
        /// <summary>Explicit flash rows retained when any row differs from calculated colors.</summary>
        private readonly ushort[][]? supplied;

        /// <summary>Number of timed palette rows in the fake-death flash program.</summary>
        public int FrameCount { get; }

        /// <summary>Uses calculated rows only when every supplied flash color matches the formula.</summary>
        /// <param name="rows">Fourteen compiled flash images, each containing the two room palette slices.</param>
        /// <param name="finalRoom">Final room palette used as the flash source.</param>
        public RoomFlash(ushort[][] rows, FinalRoomPalette finalRoom)
        {
            FrameCount = rows.Length;
            basis = finalRoom;
            for (int frame = 0; frame < rows.Length; frame++)
                for (int color = 0; color < basis.Length; color++)
                    if (Calculate(frame, color) != rows[frame][color])
                    { supplied = rows; return; }
        }

        /// <summary>Gets one color from a supplied or calculated flash image.</summary>
        /// <param name="frame">Timed flash row index.</param>
        /// <param name="color">Color index within that row.</param>
        /// <returns>The packed RGB5 color.</returns>
        public ushort Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Applies the native flash strength to the room basis or warm highlight.</summary>
        /// <param name="frame">Timed flash row whose authored strength is applied.</param>
        /// <param name="color">Color index in the room palette slices.</param>
        /// <returns>The packed RGB5 color for that frame and index.</returns>
        private ushort Calculate(int frame, int color)
        {
            int strength = MotherBrainRoomPaletteProgramDefinitions.FlashStrength(frame);
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
            {
                int channel = (basis[color] >> shift) & 31;
                // The first level color joins the background highlight; the remaining
                // level colors dim by one quarter per strength step, rounding nearest.
                int value = color < MotherBrainRoomFlashPaintDefinitions.HighlightedColorCount
                    ? (channel * (MotherBrainRoomFlashPaintDefinitions.BrightenIntervals - strength)
                        + ((MotherBrainRoomFlashPaintDefinitions.WarmFlood >> shift) & 31) * strength
                        + MotherBrainRoomFlashPaintDefinitions.BrightenIntervals / 2) / MotherBrainRoomFlashPaintDefinitions.BrightenIntervals
                    : (channel * (MotherBrainRoomFlashPaintDefinitions.DimIntervals - strength)
                        + MotherBrainRoomFlashPaintDefinitions.DimIntervals / 2) / MotherBrainRoomFlashPaintDefinitions.DimIntervals;
                result |= value << shift;
            }
            return (ushort)result;
        }
    }
    /// <summary>Seven equal RGB5 intensity steps with shared room-palette endpoints.</summary>
    private sealed class RecoveryLightFade
    {
        /// <summary>Final room palette supplying the recovery fade's room-colored endpoints.</summary>
        private readonly FinalRoomPalette finalRoom;
        /// <summary>Explicit recovery images retained when any supplied color differs from the ramp.</summary>
        private readonly ushort[][]? supplied;

        /// <summary>Uses calculated intensity steps only when every supplied recovery color matches.</summary>
        /// <param name="rows">Seven ordered recovery images containing two destination slices per frame.</param>
        /// <param name="finalRoom">Final room palette used for the fade endpoints.</param>
        public RecoveryLightFade(ushort[][] rows, FinalRoomPalette finalRoom)
        {
            this.finalRoom = finalRoom;
            for (int frame = 0; frame < rows.Length; frame++)
                for (int color = 0; color < rows[frame].Length; color++)
                    if (Calculate(frame, color) != rows[frame][color])
                    { supplied = rows; return; }
        }

        /// <summary>Gets one supplied or calculated recovery-light color.</summary>
        /// <param name="frame">Recovery image index in playback order.</param>
        /// <param name="color">Color index within the destination slices.</param>
        /// <returns>The packed RGB5 color.</returns>
        public ushort Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Scales the selected room or doorway endpoint for one recovery image.</summary>
        /// <param name="frame">Recovery image index, from darkest to brightest.</param>
        /// <param name="color">Index in the combined recovery palette slices.</param>
        /// <returns>The packed RGB5 color at that point in the fade.</returns>
        private ushort Calculate(int frame, int color)
        {
            ushort endpoint = color switch
            {
                0 => MotherBrainRecoveryPaintDefinitions.DoorwayRimLight,
                1 => MotherBrainRecoveryPaintDefinitions.DoorwayRimMiddle,
                2 => MotherBrainRecoveryPaintDefinitions.DoorwayRimDark,
                >= 3 and <= 13 => finalRoom[color - 3],
                14 or 15 => MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight,
                _ => finalRoom[color - 4],
            };
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= (((endpoint >> shift) & 31) * (frame + 1) /
                    MotherBrainRoomColorRomData.RecoveryLightsFrames) << shift;
            return (ushort)result;
        }
    }
    /// <summary>Validates and packs all seven recovery images into native RGB15 words.</summary>
    /// <param name="frames">Authored rows, each containing both fourteen-color destination slices.</param>
    /// <returns>Compiled words indexed by frame and color.</returns>
    /// <exception cref="InvalidDataException">The frame count or any frame's color count or components are invalid.</exception>
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

    /// <summary>Checks an authored palette's dimensions and RGB5 channels, then packs each color as RGB15.</summary>
    /// <param name="colors">Palette entries to validate and compile.</param>
    /// <param name="expectedCount">Required number of entries.</param>
    /// <param name="name">Palette label included in validation errors.</param>
    /// <returns>Packed color words in the same order as the input.</returns>
    /// <exception cref="InvalidDataException">The array has the wrong size or an entry is null or outside RGB5 range.</exception>
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

    /// <summary>Serializes and validates a complete current-version room-color document before writing its UTF-8 JSON bytes; older versions have no stock fallback here.</summary>
    /// <param name="json">Destination stream written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Authored RGB5 arrays read for serialization, not retained by the writer.</param>
    /// <exception cref="InvalidDataException">The serialized document fails <see cref="Load"/>'s current-version schema or color validation.</exception>
    public static void Write(Stream json, MotherBrainRoomColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable Mother Brain room-color schema; every supplied entry is a nonnull RGB5 color with channels 0..31, and nested arrays remain caller-mutable until compilation.</summary>
public sealed record MotherBrainRoomColorDocument
{
    /// <summary>Current revision 3, or revision 1/2 only when loading with explicit current-stock compatibility data.</summary>
    public required int Version { get; init; }
    /// <summary>Fourteen rows for native timed entries $A9:D046..D07A. Each row has 24 colors: twelve for CGRAM 52..63, then twelve for 83..94 mirrored to 115..126; timing is owned by the compiled program.</summary>
    public required PaletteRgb5[][] Flash { get; init; }
    /// <summary>Twenty-four final-room colors corresponding to $A9:D082: the same two twelve-color source slices and destinations as <see cref="Flash"/>.</summary>
    public required PaletteRgb5[] FinalRoom { get; init; }
    /// <summary>Fifteen nontransparent attack colors sourced natively from $A9:94B4 and installed at CGRAM 161..175 before phase two.</summary>
    public required PaletteRgb5[] PhaseTwoAttack { get; init; }
    /// <summary>Fifteen nontransparent rear-leg colors sourced natively from $A9:9494 and installed at CGRAM 177..191 before phase two, replacing the room-entry glass palette.</summary>
    public required PaletteRgb5[] PhaseTwoRearLeg { get; init; }
    /// <summary>Fifteen glass-shard colors from $A9:9514 for CGRAM 177..191; required in versions 2/3, but inherited from current stock when loading version 1.</summary>
    public PaletteRgb5[]? InitialGlassShard { get; init; }
    /// <summary>Fifteen tube-projectile colors from $A9:94F4 for CGRAM 241..255; required in versions 2/3, but inherited from current stock when loading version 1.</summary>
    public PaletteRgb5[]? InitialTubeProjectile { get; init; }
    /// <summary>Seven ordered 28-color images: fourteen for CGRAM 49..62 followed by fourteen for 81..94. Required in version 3; versions 1/2 inherit current-stock recovery artwork.</summary>
    public PaletteRgb5[][]? RecoveryLights { get; init; }
}

/// <summary>Installation filename and compatible schema revisions for Mother Brain's room and cutscene color artwork.</summary>
public static class MotherBrainRoomColorFormat
{
    /// <summary>Installed JSON filename selecting room-entry, fake-death, phase-two and post-Baby recovery colors.</summary>
    public const string FileName = "mother-brain-room-colors.json";
    /// <summary>Revision 3, requiring room-entry palettes and seven recovery-light images in addition to flash and phase-two colors.</summary>
    public const int Version = 3;
    /// <summary>Revision 1, lacking room-entry and recovery-light artwork; loading requires an explicit current-stock fallback for those fields.</summary>
    public const int PreRoomEntryVersion = 1;
    /// <summary>Revision 2, including room-entry palettes but lacking recovery-light artwork; loading requires an explicit current-stock fallback for recovery images.</summary>
    public const int PreRecoveryLightsVersion = 2;
}

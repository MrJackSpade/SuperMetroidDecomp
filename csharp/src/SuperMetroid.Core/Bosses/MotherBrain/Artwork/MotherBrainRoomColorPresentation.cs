using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable Mother Brain fake-death room flash and phase-two initial colors.</summary>
public sealed class MotherBrainRoomColorPresentation
{
    /// <summary>Timed room-color flash derived from the final room palette unless edited.</summary>
    internal readonly RoomFlash flash;
    /// <summary>Steady gray room colors used after the fake-death flash ends.</summary>
    internal readonly FinalRoomPalette finalRoom;
    /// <summary>Initial phase-two attack sprite colors.</summary>
    internal readonly AttackPalette phaseTwoAttack;
    /// <summary>Optional independent rear-leg colors; stock values come from the health palette.</summary>
    internal readonly Bgr555[]? phaseTwoRearLeg;
    /// <summary>Room-entry glass-shard colors related to the final room palette.</summary>
    internal readonly GlassPalette initialGlassShard;
    /// <summary>Room-entry tube-projectile colors related to the final room palette.</summary>
    internal readonly TubePalette initialTubeProjectile;
    /// <summary>Seven-frame doorway and room-light recovery sequence.</summary>
    internal readonly RecoveryLightFade recoveryLights;

    /// <summary>Retains independent color edits while keeping stock-derived relationships implicit.</summary>
    private MotherBrainRoomColorPresentation(Bgr555[][] flash, FinalRoomPalette finalRoom,
        Bgr555[] phaseTwoAttack, Bgr555[] phaseTwoRearLeg,
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

    /// <summary>Applies the color row of one room-flash frame.</summary>
    /// <param name="cgram">Destination color memory; twelve colors at indices 52..63 and 83..94 are replaced, with the second slice mirrored to 115..126.</param>
    /// <param name="frame">The flash frame, 0..13.</param>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of the fourteen authored frames.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyFlash(SnesCgram cgram, int frame)
    {
        if ((uint)frame >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException($"Mother Brain room-flash frame {frame} is not authored.");
        Ensure.NotNull(cgram);
        for (int color = 0; color < MotherBrainRoomColorRomData.SliceColors; color++)
        {
            new MotherBrainPalettes(cgram).RoomFirst.Write(color, flash.Color(frame, color));
            Bgr555 second = flash.Color(frame, MotherBrainRoomColorRomData.SliceColors + color);
            new MotherBrainPalettes(cgram).RoomSecond.Write(color, second);
            new MotherBrainPalettes(cgram).RoomSecondMirror.Write(color, second);
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
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.PhaseTwoColors; index++)
        {
            new MotherBrainPalettes(cgram).PhaseTwoAttack.Write(index,
                phaseTwoAttack.Color(index));
            new MotherBrainPalettes(cgram).PhaseTwoRearLegs.Write(index,
                phaseTwoRearLeg?[index] ?? MotherBrainHealthPalettePresentation.StockBaseColor(backLeg: true, index));
        }
    }

    /// <summary>Installs room-entry glass-shard and tube-projectile sprite colors.</summary>
    /// <param name="cgram">Destination color memory; glass colors 177..191 and tube-projectile colors 241..255 are replaced without their transparent color-zero entries.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyRoomEntry(SnesCgram cgram)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.InitialColors; index++)
        {
            new MotherBrainPalettes(cgram).GlassShards.Write(index,
                initialGlassShard.Color(index));
            new MotherBrainPalettes(cgram).TubeProjectiles.Write(index,
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
            new MotherBrainPalettes(cgram).RecoveryLightsFirst.Write(index, Bgr555.Black);
            new MotherBrainPalettes(cgram).RecoveryLightsSecond.Write(index, Bgr555.Black);
        }
    }

    /// <summary>Applies one room-light image after the Baby Metroid cutscene.</summary>
    /// <param name="cgram">Destination color memory receiving fourteen-color slices at indices 49..62 and 81..94.</param>
    /// <param name="frame">Recovery image 0..6 in playback order, corresponding to native source $AD:F3D3 - frame * $38; this call does not advance the cutscene's timer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is outside the seven authored images.</exception>
    public void ApplyRecoveryLights(SnesCgram cgram, int frame)
    {
        Ensure.NotNull(cgram);
        if ((uint)frame >= MotherBrainRoomColorRomData.RecoveryLightsFrames)
            throw new InvalidDataException($"Mother Brain room-light recovery frame {frame} is not authored.");

        for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination;
             index++)
        {
            new MotherBrainPalettes(cgram).RecoveryLightsFirst.Write(index,
                recoveryLights.Color(frame, index));
            new MotherBrainPalettes(cgram).RecoveryLightsSecond.Write(index,
                recoveryLights.Color(frame, MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination + index));
        }
    }

    /// <summary>Writes final room colors into both room slices and mirrors the second slice.</summary>
    private static void ApplyRoom(SnesCgram cgram, FinalRoomPalette colors)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.SliceColors; index++)
        {
            new MotherBrainPalettes(cgram).RoomFirst.Write(index, colors[index]);
            Bgr555 second = colors[MotherBrainRoomColorRomData.SliceColors + index];
            new MotherBrainPalettes(cgram).RoomSecond.Write(index, second);
            new MotherBrainPalettes(cgram).RoomSecondMirror.Write(index, second);
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
        if (!Enum.IsDefined((MotherBrainRoomColorRevision)document.Version))
            throw new InvalidDataException("Mother Brain room colors require the supported version and fourteen flash rows.");
        var revision = (MotherBrainRoomColorRevision)document.Version;
        bool inheritsRoomEntry = revision == MotherBrainRoomColorRevision.PreRoomEntry;
        bool inheritsRecoveryLights = revision switch
        {
            MotherBrainRoomColorRevision.PreRoomEntry or MotherBrainRoomColorRevision.PreRecoveryLights => true,
            MotherBrainRoomColorRevision.Current => false,
            _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainRoomColorRevision)} {(int)revision}."),
        };
        if ((inheritsRecoveryLights && currentStock is null) ||
            document.Flash is null ||
            document.Flash.Length != MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException("Mother Brain room colors require the supported version and fourteen flash rows.");
        var flash = new Bgr555[document.Flash.Length][];
        for (int index = 0; index < flash.Length; index++)
            flash[index] = Compile(document.Flash[index], MotherBrainRoomColorRomData.SliceColors * 2,
                $"flash row {index}");
        var finalRoom = new FinalRoomPalette(Compile(document.FinalRoom, MotherBrainRoomColorRomData.SliceColors * 2, "final room"));
        return new(flash, finalRoom,
            Compile(document.PhaseTwoAttack, MotherBrainRoomColorRomData.PhaseTwoColors,
                "phase-two attack"),
            Compile(document.PhaseTwoRearLeg, MotherBrainRoomColorRomData.PhaseTwoColors,
                "phase-two rear leg"),
            inheritsRoomEntry
                ? (currentStock ?? throw new InvalidDataException("A legacy Mother Brain room color override requires the current stock presentation.")).initialGlassShard
                : new GlassPalette(Compile(document.InitialGlassShard, MotherBrainRoomColorRomData.InitialColors,
                    "room-entry glass shard"), finalRoom),
            inheritsRoomEntry
                ? (currentStock ?? throw new InvalidDataException("A legacy Mother Brain room color override requires the current stock presentation.")).initialTubeProjectile
                : new TubePalette(Compile(document.InitialTubeProjectile, MotherBrainRoomColorRomData.InitialColors,
                    "room-entry tube projectile"), finalRoom),
            inheritsRecoveryLights
                ? (currentStock ?? throw new InvalidDataException("A legacy Mother Brain room color override requires the current stock presentation.")).recoveryLights
                : new RecoveryLightFade(CompileRecoveryLights(document.RecoveryLights), finalRoom));
    }

    /// <summary>Derives wall, recess, panel, metal, and lens shades from selected room paint.</summary>
    internal sealed class FinalRoomPalette
    {
        /// <summary>Independent supplied colors retained only when they differ from the derivation.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Number of room colors available to the flash and recovery programs.</summary>
        public int Length { get; }
        /// <summary>Checks whether all supplied colors match the paint-derived room palette.</summary>
        public FinalRoomPalette(Bgr555[] colors)
        {
            Length = colors.Length;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Uses edited paint when present and rejects colors outside the room palette.</summary>
        public Bgr555 this[int color] => (uint)color < Length
            ? supplied is null ? Calculate(color) : supplied[color] : throw new IndexOutOfRangeException();
        /// <summary>Builds room wall and shadow ramps plus the selected panel and lens accents.</summary>
        private static Bgr555 Calculate(int color)
        {
            if (color < MotherBrainRoomColorRomData.RoomShadowFirst)
                return InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.WallLight, MotherBrainFinalRoomPaintDefinitions.WallDark, color - MotherBrainRoomColorRomData.RoomWallFirst,
                    MotherBrainRoomColorRomData.RoomWallCount - 1);
            // Native shadow red is5,4,3,1: ceiling interpolation. Green7,5,4,2
            // and blue8,6,4,2 use nearest interpolation. This is an exact sequence
            // relationship, not a claim about a historical palette tool.
            if (color < MotherBrainRoomColorRomData.RoomShadowFirst + MotherBrainRoomColorRomData.RoomShadowCount)
                return InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight, InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight, Bgr555.Black,
                    MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor - 1,
                    MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor), color - MotherBrainRoomColorRomData.RoomShadowFirst,
                    MotherBrainRoomColorRomData.RoomShadowCount - 1, ceilingRed: MotherBrainFinalRoomPaintDefinitions.RecessedRedCeiling);
            if (color <= MotherBrainRoomColorRomData.RoomOutlineColor)
                return PanelColorAt(color);
            if (color < MotherBrainRoomColorRomData.RoomDarkGrayColor)
                return InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLight, MotherBrainFinalRoomPaintDefinitions.MetalLow, color - MotherBrainRoomColorRomData.RoomGrayFirst,
                    MotherBrainRoomColorRomData.RoomGrayCount - 1);
            if (!Enum.IsDefined((AccentColor)color))
                return MotherBrainHealthPalettePresentation.StockBaseColor(false, MotherBrainRoomColorRomData.WhiteColor);
            return (AccentColor)color switch
            {
                // Metal contour shadow is half the low gray surface intensity.
                AccentColor.DarkGray or AccentColor.RepeatedDarkGray =>
                    InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLow, Bgr555.Black, MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor - 1, MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor),
                AccentColor.RedLens => MotherBrainFinalRoomPaintDefinitions.RedLens,
                AccentColor.RedLensShade => new Bgr555(Math.Max(0, MotherBrainFinalRoomPaintDefinitions.RedLens.Red - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop), 0, 0),
                AccentColor.BlueLens => MotherBrainFinalRoomPaintDefinitions.BlueLens,
                AccentColor.BlueLensShade => new Bgr555(0, 0, Math.Max(0, MotherBrainFinalRoomPaintDefinitions.BlueLens.Blue - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop)),
                AccentColor.Glow => MotherBrainFinalRoomPaintDefinitions.WarmAccent,
                _ => throw new InvalidOperationException($"Undefined {nameof(AccentColor)} {color}."),
            };
        }

        /// <summary>Selects authored amber and outline panel colors; other panel indexes are black.</summary>
        private static Bgr555 PanelColorAt(int color)
        {
            if (!Enum.IsDefined((PanelColor)color))
                return Bgr555.Black;
            return (PanelColor)color switch
            {
                PanelColor.Amber => MotherBrainFinalRoomPaintDefinitions.Amber,
                PanelColor.Outline => MotherBrainFinalRoomPaintDefinitions.PanelField,
                _ => throw new InvalidOperationException($"Undefined {nameof(PanelColor)} {color}."),
            };
        }

        /// <summary>The final-room color indexes between the shadow ramp and the gray ramp that own individual paint; the rest are black.</summary>
        private enum PanelColor
        {
            /// <summary>Amber panel light.</summary>
            Amber = MotherBrainRoomColorRomData.RoomAmberColor,
            /// <summary>Panel outline field.</summary>
            Outline = MotherBrainRoomColorRomData.RoomOutlineColor,
        }

        /// <summary>The final-room color indexes past the gray ramp that own individual paint; the rest are stock white.</summary>
        private enum AccentColor
        {
            /// <summary>Metal contour shadow.</summary>
            DarkGray = MotherBrainRoomColorRomData.RoomDarkGrayColor,
            /// <summary>Red lens.</summary>
            RedLens = MotherBrainRoomColorRomData.RoomRedFirst,
            /// <summary>Red lens shade.</summary>
            RedLensShade = MotherBrainRoomColorRomData.RoomRedFirst + 1,
            /// <summary>Blue lens.</summary>
            BlueLens = MotherBrainRoomColorRomData.RoomBlueFirst,
            /// <summary>Blue lens shade.</summary>
            BlueLensShade = MotherBrainRoomColorRomData.RoomBlueFirst + 1,
            /// <summary>Repeated metal contour shadow.</summary>
            RepeatedDarkGray = MotherBrainRoomColorRomData.RoomRepeatedDarkGrayColor,
            /// <summary>Warm glow accent.</summary>
            Glow = MotherBrainRoomColorRomData.RoomGlowColor,
        }
    }
    /// <summary>Interpolates RGB5 channels with nearest rounding, or ceiling rounding for shadow red.</summary>
    private static Bgr555 InterpolateRgb5(Bgr555 start, Bgr555 end, int step, int intervals, bool ceilingRed = false)
    {
        return start.Zip(end, (channel, a, b) => ((a * (intervals - step) + b * step + (ceilingRed && channel == ColorChannel.Red ? intervals - 1 : intervals / 2)) / intervals));
    }
    /// <summary>Derives ring, core, tissue, and casing shades while preserving independent edits.</summary>
    internal sealed class AttackPalette
    {
        /// <summary>Supplied attack colors when any word differs from the shared-shade derivation.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Uses paint-derived shades only when the entire supplied row matches.</summary>
        public AttackPalette(Bgr555[] colors)
        {
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Selects the edited or calculated phase-two attack color.</summary>
        public Bgr555 Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Builds cyan and flame ramps and reuses health-palette tissue colors.</summary>
        private static Bgr555 Calculate(int color)
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
        /// <summary>Interpolates an attack shade between its painted endpoints with nearest rounding.</summary>
        private static Bgr555 InterpolateShade(Bgr555 first, Bgr555 last, int shade, int intervals)
        {
            return first.Zip(last, (_, a, b) => ((a * (intervals - shade)
                    + b * shade + intervals / 2) / intervals));
        }
    }
    /// <summary>Relates glass-shard metal shades to room colors and selected glass paint.</summary>
    internal sealed class GlassPalette
    {
        /// <summary>Room palette supplying shared gray, outline, and contour colors.</summary>
        private readonly FinalRoomPalette room;
        /// <summary>Independent shard colors retained only when derivation differs.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Checks whether shard colors can be calculated from room and glass paint.</summary>
        public GlassPalette(Bgr555[] colors, FinalRoomPalette room)
        {
            this.room = room;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Selects a supplied or room-derived shard color.</summary>
        public Bgr555 Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Builds the glass face ramp and reuses the room's metal accents.</summary>
        private Bgr555 Calculate(int color)
        {
            if (color < MotherBrainRoomColorRomData.GlassRampFirst)
                return room[MotherBrainRoomColorRomData.RoomGrayFirst + color];
            if (color < MotherBrainRoomColorRomData.GlassDarkGrayColor)
                return InterpolateRgb5(MotherBrainGlassPaintDefinitions.FaceHighlight, room[MotherBrainRoomColorRomData.RoomOutlineColor],
                    color - MotherBrainRoomColorRomData.GlassRampFirst, MotherBrainRoomColorRomData.GlassRampCount - 1);
            if (!Enum.IsDefined((AccentColor)color))
                return MotherBrainHealthPalettePresentation.StockBaseColor(false, color);
            return (AccentColor)color switch
            {
                AccentColor.DarkGray => room[MotherBrainRoomColorRomData.RoomDarkGrayColor],
                AccentColor.Neutral => MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight,
                _ => throw new InvalidOperationException($"Undefined {nameof(AccentColor)} {color}."),
            };
        }

        /// <summary>The glass color indexes past the ramp that own individual paint; the rest are stock colors.</summary>
        private enum AccentColor
        {
            /// <summary>Glass dark gray, shared with the room's metal contour shadow.</summary>
            DarkGray = MotherBrainRoomColorRomData.GlassDarkGrayColor,
            /// <summary>Glass neutral casing highlight.</summary>
            Neutral = MotherBrainRoomColorRomData.GlassNeutralColor,
        }
    }
    /// <summary>Relates tube-projectile casing and outline shades to the final room palette.</summary>
    internal sealed class TubePalette
    {
        /// <summary>Room palette supplying tube outline colors.</summary>
        private readonly FinalRoomPalette room;
        /// <summary>Independent tube colors retained only when derivation differs.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Checks whether tube colors can be calculated from room paint.</summary>
        public TubePalette(Bgr555[] colors, FinalRoomPalette room)
        {
            this.room = room;
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color]) { supplied = colors; return; }
        }
        /// <summary>Selects a supplied or room-derived tube-projectile color.</summary>
        public Bgr555 Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Uses shared casing highlight, stock black, and room outline colors.</summary>
        private Bgr555 Calculate(int color) => color < MotherBrainRoomColorRomData.TubeNeutralCount
            ? MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight : color == MotherBrainRoomColorRomData.RoomEntryBlackColor
                ? MotherBrainHealthPalettePresentation.StockBaseColor(false, color)
                : room[MotherBrainRoomColorRomData.RoomOutlineColor + color - MotherBrainRoomColorRomData.TubeNeutralCount];
    }
    /// <summary>Background highlight interpolation paired with darkening level colors.</summary>
    internal sealed class RoomFlash
    {
        /// <summary>Final room colors that the timed flash brightens or dims.</summary>
        internal readonly FinalRoomPalette basis;
        /// <summary>Independent flash rows retained only when the calculated rows differ.</summary>
        internal readonly Bgr555[][]? supplied;

        /// <summary>Number of timed flash entries selected by the palette program.</summary>
        public int FrameCount { get; }

        /// <summary>Checks supplied flash rows against the final-room color derivation.</summary>
        public RoomFlash(Bgr555[][] rows, FinalRoomPalette finalRoom)
        {
            FrameCount = rows.Length;
            basis = finalRoom;
            for (int frame = 0; frame < rows.Length; frame++)
                for (int color = 0; color < basis.Length; color++)
                    if (Calculate(frame, color) != rows[frame][color])
                    { supplied = rows; return; }
        }

        /// <summary>Selects the edited or calculated room color at one timed entry.</summary>
        public Bgr555 Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Blends the first room color toward warm flood and dims the remaining colors by flash strength.</summary>
        private Bgr555 Calculate(int frame, int color)
        {
            int strength = MotherBrainRoomPaletteProgramDefinitions.FlashStrength(frame);
            // The first level color joins the background highlight; the remaining
            // level colors dim by one quarter per strength step, rounding nearest.
            return basis[color].Zip(MotherBrainRoomFlashPaintDefinitions.WarmFlood, (_, channel, warm) =>
                color < MotherBrainRoomFlashPaintDefinitions.HighlightedColorCount
                    ? (channel * (MotherBrainRoomFlashPaintDefinitions.BrightenIntervals - strength)
                        + warm * strength
                        + MotherBrainRoomFlashPaintDefinitions.BrightenIntervals / 2) / MotherBrainRoomFlashPaintDefinitions.BrightenIntervals
                    : (channel * (MotherBrainRoomFlashPaintDefinitions.DimIntervals - strength)
                        + MotherBrainRoomFlashPaintDefinitions.DimIntervals / 2) / MotherBrainRoomFlashPaintDefinitions.DimIntervals);
        }
    }
    /// <summary>Seven equal RGB5 intensity steps with shared room-palette endpoints.</summary>
    internal sealed class RecoveryLightFade
    {
        /// <summary>Final room colors reused as recovery endpoints.</summary>
        internal readonly FinalRoomPalette finalRoom;
        /// <summary>Independent recovery rows retained only when the calculated rows differ.</summary>
        internal readonly Bgr555[][]? supplied;

        /// <summary>Checks supplied recovery colors against seven intensity steps to shared endpoints.</summary>
        public RecoveryLightFade(Bgr555[][] rows, FinalRoomPalette finalRoom)
        {
            this.finalRoom = finalRoom;
            for (int frame = 0; frame < rows.Length; frame++)
                for (int color = 0; color < rows[frame].Length; color++)
                    if (Calculate(frame, color) != rows[frame][color])
                    { supplied = rows; return; }
        }

        /// <summary>Selects edited or calculated doorway and room-light color for one recovery frame.</summary>
        public Bgr555 Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Scales selected doorway, casing, and room endpoints by the recovery frame's intensity.</summary>
        private Bgr555 Calculate(int frame, int color)
        {
            Bgr555 endpoint = color switch
            {
                0 => MotherBrainRecoveryPaintDefinitions.DoorwayRimLight,
                1 => MotherBrainRecoveryPaintDefinitions.DoorwayRimMiddle,
                2 => MotherBrainRecoveryPaintDefinitions.DoorwayRimDark,
                >= 3 and <= 13 => finalRoom[color - 3],
                14 or 15 => MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight,
                _ => finalRoom[color - 4],
            };
            return endpoint.Map((_, a) => (a * (frame + 1) /
                    MotherBrainRoomColorRomData.RecoveryLightsFrames));
        }
    }
    /// <summary>Validates the seven recovery frames and compiles their RGB5 words.</summary>
    private static Bgr555[][] CompileRecoveryLights(PaletteRgb5[][]? frames)
    {
        if (frames is null || frames.Length != MotherBrainRoomColorRomData.RecoveryLightsFrames)
            throw new InvalidDataException("Mother Brain room-light recovery requires seven frames.");
        var compiled = new Bgr555[frames.Length][];
        for (int frame = 0; frame < frames.Length; frame++)
            compiled[frame] = Compile(frames[frame],
                MotherBrainRoomColorRomData.RecoveryLightsColorsPerDestination * 2,
                $"room-light recovery frame {frame}");
        return compiled;
    }

    /// <summary>Rejects missing or out-of-range RGB5 components before packing an authored palette row.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? colors, int expectedCount, string name)
    {
        if (colors is null || colors.Length != expectedCount)
            throw new InvalidDataException($"Mother Brain {name} requires {expectedCount} colors.");
        var compiled = new Bgr555[colors.Length];
        for (int index = 0; index < colors.Length; index++)
        {
            PaletteRgb5? color = colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Mother Brain {name} color {index} requires RGB5 components.");
            compiled[index] = color.ToBgr555();
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

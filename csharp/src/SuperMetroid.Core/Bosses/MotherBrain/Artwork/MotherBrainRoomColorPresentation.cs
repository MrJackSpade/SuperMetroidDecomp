using SuperMetroid.Core.Assets;

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

    /// <summary>Builds the selected flash, room, phase-two, room-entry and recovery colours.</summary>
    /// <param name="flash">The fourteen flash rows.</param>
    /// <param name="finalRoom">The final room colours.</param>
    /// <param name="phaseTwoAttack">The phase-two attack colours.</param>
    /// <param name="phaseTwoRearLeg">The phase-two rear-leg colours.</param>
    /// <param name="initialGlassShard">The room-entry glass shard colours, or null to inherit them from <paramref name="currentStock"/>.</param>
    /// <param name="initialTubeProjectile">The room-entry tube projectile colours, or null to inherit them from <paramref name="currentStock"/>.</param>
    /// <param name="recoveryLights">The room-light recovery frames, or null to inherit them from <paramref name="currentStock"/>.</param>
    /// <param name="currentStock">The current stock presentation a missing family is inherited from.</param>
    internal static MotherBrainRoomColorPresentation Create(Bgr555[][] flash, Bgr555[] finalRoom,
        Bgr555[] phaseTwoAttack, Bgr555[] phaseTwoRearLeg, Bgr555[]? initialGlassShard, Bgr555[]? initialTubeProjectile,
        Bgr555[][]? recoveryLights, MotherBrainRoomColorPresentation? currentStock)
    {
        var room = new FinalRoomPalette(finalRoom);
        return new(flash, room, phaseTwoAttack, phaseTwoRearLeg,
            initialGlassShard is null ? Ensure.NotNull(currentStock).initialGlassShard : new GlassPalette(initialGlassShard, room),
            initialTubeProjectile is null ? Ensure.NotNull(currentStock).initialTubeProjectile : new TubePalette(initialTubeProjectile, room),
            recoveryLights is null ? Ensure.NotNull(currentStock).recoveryLights : new RecoveryLightFade(recoveryLights, room));
    }

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
            word != MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, color)).Any() ? phaseTwoRearLeg : null;
        this.initialGlassShard = initialGlassShard;
        this.initialTubeProjectile = initialTubeProjectile;
        this.recoveryLights = recoveryLights;
    }

    /// <summary>Applies the color row of one room-flash frame.</summary>
    /// <param name="cgram">Destination color memory; twelve colors at indices 52..63 and 83..94 are replaced, with the second slice mirrored to 115..126.</param>
    /// <param name="frame">The flash frame, 0..13.</param>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of the fourteen authored frames.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyFlash(GameplayColours cgram, int frame)
    {
        if ((uint)frame >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException($"Mother Brain room-flash frame {frame} is not authored.");
        Ensure.NotNull(cgram);
        for (int color = 0; color < MotherBrainRoomColorRomData.SliceColors; color++)
        {
            new MotherBrainColours(cgram).RoomFirst.Write(color, flash.Color(frame, color));
            Bgr555 second = flash.Color(frame, MotherBrainRoomColorRomData.SliceColors + color);
            new MotherBrainColours(cgram).RoomSecond.Write(color, second);
            new MotherBrainColours(cgram).RoomSecondMirror.Write(color, second);
        }
    }

    /// <summary>Applies the final grey room colors when the flash program is stopped.</summary>
    /// <param name="cgram">Destination color memory; the same 52..63, 83..94 and mirrored 115..126 slices as <see cref="ApplyFlash"/> are replaced.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyFinal(GameplayColours cgram) => ApplyRoom(cgram, finalRoom);

    /// <summary>Installs the two nontransparent OBJ palettes before phase two starts.</summary>
    /// <param name="cgram">Destination color memory; attack colors 161..175 and rear-leg colors 177..191 are replaced while palette color zero is preserved.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyPhaseTwoInitial(GameplayColours cgram)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.PhaseTwoColors; index++)
        {
            new MotherBrainColours(cgram).PhaseTwoAttack.Write(index,
                phaseTwoAttack.Color(index));
            new MotherBrainColours(cgram).PhaseTwoRearLegs.Write(index,
                phaseTwoRearLeg?[index] ?? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, index));
        }
    }

    /// <summary>Installs room-entry glass-shard and tube-projectile sprite colors.</summary>
    /// <param name="cgram">Destination color memory; glass colors 177..191 and tube-projectile colors 241..255 are replaced without their transparent color-zero entries.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public void ApplyRoomEntry(GameplayColours cgram)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.InitialColors; index++)
        {
            new MotherBrainColours(cgram).GlassShards.Write(index,
                initialGlassShard.Color(index));
            new MotherBrainColours(cgram).TubeProjectiles.Write(index,
                initialTubeProjectile.Color(index));
        }
    }

    /// <summary>$AD:F209, FadeOutBackgroundForBabyMetroidDeathSequence: immediately blacks
    /// out colors 1..E of BG palettes 3 and 5, the same slices later restored by $AD:F24B.</summary>
    /// <param name="cgram">Destination color memory; all entries outside indices 49..62 and 81..94 remain unchanged.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    public static void ApplyBabyMetroidDeathBlackout(GameplayColours cgram)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsSliceColors; index++)
        {
            new MotherBrainColours(cgram).RecoveryLightsFirst.Write(index, Bgr555.Black);
            new MotherBrainColours(cgram).RecoveryLightsSecond.Write(index, Bgr555.Black);
        }
    }

    /// <summary>Applies one room-light image after the Baby Metroid cutscene.</summary>
    /// <param name="cgram">Destination color memory receiving fourteen-color slices at indices 49..62 and 81..94.</param>
    /// <param name="frame">Recovery image 0..6 in playback order, corresponding to native source $AD:F3D3 - frame * $38; this call does not advance the cutscene's timer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is outside the seven authored images.</exception>
    public void ApplyRecoveryLights(GameplayColours cgram, int frame)
    {
        Ensure.NotNull(cgram);
        if ((uint)frame >= MotherBrainRoomColorRomData.RecoveryLightsFrames)
            throw new InvalidDataException($"Mother Brain room-light recovery frame {frame} is not authored.");

        for (int index = 0; index < MotherBrainRoomColorRomData.RecoveryLightsSliceColors;
             index++)
        {
            new MotherBrainColours(cgram).RecoveryLightsFirst.Write(index,
                recoveryLights.Color(frame, index));
            new MotherBrainColours(cgram).RecoveryLightsSecond.Write(index,
                recoveryLights.Color(frame, MotherBrainRoomColorRomData.RecoveryLightsSliceColors + index));
        }
    }

    /// <summary>Writes final room colors into both room slices and mirrors the second slice.</summary>
    private static void ApplyRoom(GameplayColours cgram, FinalRoomPalette colors)
    {
        Ensure.NotNull(cgram);
        for (int index = 0; index < MotherBrainRoomColorRomData.SliceColors; index++)
        {
            new MotherBrainColours(cgram).RoomFirst.Write(index, colors[index]);
            Bgr555 second = colors[MotherBrainRoomColorRomData.SliceColors + index];
            new MotherBrainColours(cgram).RoomSecond.Write(index, second);
            new MotherBrainColours(cgram).RoomSecondMirror.Write(index, second);
        }
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
            ? supplied is null ? Calculate(color) : supplied[color] : throw new ArgumentOutOfRangeException(nameof(color));
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
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, MotherBrainRoomColorRomData.WhiteColor);
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
            /// <summary>Amber panel light, $A9:D096 between black slots at D092/D094/D098; its pixel role remains unresolved.</summary>
            Amber = 10,
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
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body,
                    MotherBrainRoomColorRomData.AttackBodyTissueFirst + color - MotherBrainRoomColorRomData.AttackTissueFirst);
            if (color == MotherBrainRoomColorRomData.WhiteColor)
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, MotherBrainRoomColorRomData.WhiteColor);
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
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color);
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
            Neutral = 12,
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
                ? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color)
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
}

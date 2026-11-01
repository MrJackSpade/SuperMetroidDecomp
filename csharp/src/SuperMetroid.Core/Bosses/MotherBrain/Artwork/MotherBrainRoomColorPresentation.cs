using System.Collections.Immutable;
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
    internal readonly RgbColor[]? phaseTwoRearLeg;
    /// <summary>Room-entry glass-shard colors related to the final room palette.</summary>
    internal readonly GlassPalette initialGlassShard;
    /// <summary>Room-entry tube-projectile colors related to the final room palette.</summary>
    internal readonly TubePalette initialTubeProjectile;
    /// <summary>Seven-frame doorway and room-light recovery sequence.</summary>
    internal readonly RecoveryLightFade recoveryLights;

    /// <summary>Keeps the recognized flash, room, phase-two, room-entry and recovery colours.</summary>
    /// <param name="flash">The room flash.</param>
    /// <param name="finalRoom">The final room colours.</param>
    /// <param name="phaseTwoAttack">The phase-two attack colours.</param>
    /// <param name="phaseTwoRearLeg">Edited phase-two rear-leg colours, or null when they are the stock rear legs.</param>
    /// <param name="initialGlassShard">The room-entry glass-shard colours.</param>
    /// <param name="initialTubeProjectile">The room-entry tube-projectile colours.</param>
    /// <param name="recoveryLights">The room-light recovery fade.</param>
    internal MotherBrainRoomColorPresentation(RoomFlash flash, FinalRoomPalette finalRoom, AttackPalette phaseTwoAttack,
        RgbColor[]? phaseTwoRearLeg, GlassPalette initialGlassShard, TubePalette initialTubeProjectile,
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

    /// <summary>Applies the colors of one room-flash frame.</summary>
    /// <param name="gameplayColours">Destination color memory; twelve colors at indices 52..63 and 83..94 are replaced, with the second slice mirrored to 115..126.</param>
    /// <param name="frame">The flash frame, 0..13.</param>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of the fourteen authored frames.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    public void ApplyFlash(GameplayColours gameplayColours, int frame)
    {
        if ((uint)frame >= MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount)
            throw new InvalidDataException($"Mother Brain room-flash frame {frame} is not authored.");
        Ensure.NotNull(gameplayColours);
        for (int color = 0; color < MotherBrainRoomColorDefinitions.SliceColors; color++)
        {
            new MotherBrainColours(gameplayColours).RoomFirst.Write(color, flash.Color(frame, color));
            RgbColor second = flash.Color(frame, MotherBrainRoomColorDefinitions.SliceColors + color);
            new MotherBrainColours(gameplayColours).RoomSecond.Write(color, second);
            new MotherBrainColours(gameplayColours).RoomSecondMirror.Write(color, second);
        }
    }

    /// <summary>Applies the final grey room colors when the flash program is stopped.</summary>
    /// <param name="gameplayColours">Destination color memory; the same 52..63, 83..94 and mirrored 115..126 slices as <see cref="ApplyFlash"/> are replaced.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    public void ApplyFinal(GameplayColours gameplayColours) => ApplyRoom(gameplayColours, finalRoom);

    /// <summary>Installs the two nontransparent OBJ palettes before phase two starts.</summary>
    /// <param name="gameplayColours">Destination color memory; attack colors 161..175 and rear-leg colors 177..191 are replaced while palette color zero is preserved.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    public void ApplyPhaseTwoInitial(GameplayColours gameplayColours)
    {
        Ensure.NotNull(gameplayColours);
        for (int index = 0; index < MotherBrainRoomColorDefinitions.PhaseTwoColors; index++)
        {
            new MotherBrainColours(gameplayColours).PhaseTwoAttack.Write(index,
                phaseTwoAttack.Color(index));
            new MotherBrainColours(gameplayColours).PhaseTwoRearLegs.Write(index,
                phaseTwoRearLeg?[index] ?? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, index));
        }
    }

    /// <summary>Installs room-entry glass-shard and tube-projectile sprite colors.</summary>
    /// <param name="gameplayColours">Destination color memory; glass colors 177..191 and tube-projectile colors 241..255 are replaced without their transparent color-zero entries.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    public void ApplyRoomEntry(GameplayColours gameplayColours)
    {
        Ensure.NotNull(gameplayColours);
        for (int index = 0; index < MotherBrainRoomColorDefinitions.InitialColors; index++)
        {
            new MotherBrainColours(gameplayColours).GlassShards.Write(index,
                initialGlassShard.Color(index));
            new MotherBrainColours(gameplayColours).TubeProjectiles.Write(index,
                initialTubeProjectile.Color(index));
        }
    }

    /// <summary>FadeOutBackgroundForBabyMetroidDeathSequence: immediately blacks
    /// out colors 1..14 of background palettes 3 and 5, the same slices the recovery lights later restore.</summary>
    /// <param name="gameplayColours">Destination color memory; all entries outside indices 49..62 and 81..94 remain unchanged.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    public static void ApplyBabyMetroidDeathBlackout(GameplayColours gameplayColours)
    {
        Ensure.NotNull(gameplayColours);
        for (int index = 0; index < MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors; index++)
        {
            new MotherBrainColours(gameplayColours).RecoveryLightsFirst.Write(index, RgbColor.Black);
            new MotherBrainColours(gameplayColours).RecoveryLightsSecond.Write(index, RgbColor.Black);
        }
    }

    /// <summary>Applies one room-light image after the Baby Metroid cutscene.</summary>
    /// <param name="gameplayColours">Destination color memory receiving fourteen-color slices at indices 49..62 and 81..94.</param>
    /// <param name="frame">Recovery image 0..6 in playback order; this call does not advance the cutscene's timer.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is outside the seven authored images.</exception>
    public void ApplyRecoveryLights(GameplayColours gameplayColours, int frame)
    {
        Ensure.NotNull(gameplayColours);
        if ((uint)frame >= MotherBrainRoomColorDefinitions.RecoveryLightsFrames)
            throw new InvalidDataException($"Mother Brain room-light recovery frame {frame} is not authored.");

        for (int index = 0; index < MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors;
             index++)
        {
            new MotherBrainColours(gameplayColours).RecoveryLightsFirst.Write(index,
                recoveryLights.Color(frame, index));
            new MotherBrainColours(gameplayColours).RecoveryLightsSecond.Write(index,
                recoveryLights.Color(frame, MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors + index));
        }
    }

    /// <summary>Writes final room colors into both room slices and mirrors the second slice.</summary>
    private static void ApplyRoom(GameplayColours gameplayColours, FinalRoomPalette colors)
    {
        Ensure.NotNull(gameplayColours);
        for (int index = 0; index < MotherBrainRoomColorDefinitions.SliceColors; index++)
        {
            new MotherBrainColours(gameplayColours).RoomFirst.Write(index, colors[index]);
            RgbColor second = colors[MotherBrainRoomColorDefinitions.SliceColors + index];
            new MotherBrainColours(gameplayColours).RoomSecond.Write(index, second);
            new MotherBrainColours(gameplayColours).RoomSecondMirror.Write(index, second);
        }
    }

    /// <summary>Derives wall, recess, panel, metal, and lens shades from selected room paint.</summary>
    internal sealed class FinalRoomPalette
    {
        /// <summary>Independent supplied colors retained only when they differ from the derivation.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>Number of room colors available to the flash and recovery programs.</summary>
        public int Length { get; }
        /// <summary>Keeps the room colour count and, when one differs from the derivation, every supplied colour.</summary>
        /// <param name="length">The number of room colours.</param>
        /// <param name="supplied">Every supplied colour when one differs from the paint-derived palette, otherwise null.</param>
        internal FinalRoomPalette(int length, RgbColor[]? supplied)
        {
            Length = length;
            this.supplied = supplied;
        }
        /// <summary>Uses edited paint when present and rejects colors outside the room palette.</summary>
        public RgbColor this[int color] => (uint)color < Length
            ? supplied is null ? Calculate(color) : supplied[color] : throw new ArgumentOutOfRangeException(nameof(color));
        /// <summary>The paint-derived room colours, one per room colour index of both slices.</summary>
        private static readonly ImmutableArray<RgbColor> StockColors = BuildStockColors();

        /// <summary>The body's white, which every room colour past the accents repeats.</summary>
        private static RgbColor StockWhite =>
            MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, MotherBrainRoomColorDefinitions.WhiteColor);

        /// <summary>Selects the paint-derived room colour; colours past the two slices repeat the stock white.</summary>
        private static RgbColor Calculate(int color) => color < StockColors.Length ? StockColors[color] : StockWhite;

        /// <summary>Builds room wall and shadow ramps plus the selected panel and lens accents.</summary>
        private static ImmutableArray<RgbColor> BuildStockColors()
        {
            RgbColor[] stock = new RgbColor[2 * MotherBrainRoomColorDefinitions.SliceColors];
            Array.Fill(stock, StockWhite);
            for (int step = 0; step < MotherBrainRoomColorDefinitions.RoomWallCount; step++)
                stock[MotherBrainRoomColorDefinitions.RoomWallFirst + step] = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.WallLight,
                    MotherBrainFinalRoomPaintDefinitions.WallDark, step, MotherBrainRoomColorDefinitions.RoomWallCount - 1);
            // Shadow red is5,4,3,1: ceiling interpolation. Green7,5,4,2
            // and blue8,6,4,2 use nearest interpolation. This is an exact sequence
            // relationship, not a claim about a historical palette tool.
            RgbColor recessedShadow = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight, RgbColor.Black,
                MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor - 1, MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor);
            for (int step = 0; step < MotherBrainRoomColorDefinitions.RoomShadowCount; step++)
                stock[MotherBrainRoomColorDefinitions.RoomShadowFirst + step] = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight,
                    recessedShadow, step, MotherBrainRoomColorDefinitions.RoomShadowCount - 1,
                    ceilingRed: MotherBrainFinalRoomPaintDefinitions.RecessedRedCeiling);
            // The panel colours between the shadow ramp and the outline are black except the amber light.
            for (int color = MotherBrainRoomColorDefinitions.RoomShadowFirst + MotherBrainRoomColorDefinitions.RoomShadowCount;
                 color < MotherBrainRoomColorDefinitions.RoomOutlineColor; color++)
                stock[color] = RgbColor.Black;
            stock[PanelAmberColor] = MotherBrainFinalRoomPaintDefinitions.Amber;
            stock[MotherBrainRoomColorDefinitions.RoomOutlineColor] = MotherBrainFinalRoomPaintDefinitions.PanelField;
            for (int step = 0; step < MotherBrainRoomColorDefinitions.RoomGrayCount; step++)
                stock[MotherBrainRoomColorDefinitions.RoomGrayFirst + step] = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLight,
                    MotherBrainFinalRoomPaintDefinitions.MetalLow, step, MotherBrainRoomColorDefinitions.RoomGrayCount - 1);
            // Metal contour shadow is half the low gray surface intensity.
            RgbColor contour = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLow, RgbColor.Black,
                MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor - 1, MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor);
            stock[MotherBrainRoomColorDefinitions.RoomDarkGrayColor] = contour;
            stock[MotherBrainRoomColorDefinitions.RoomRepeatedDarkGrayColor] = contour;
            stock[MotherBrainRoomColorDefinitions.RoomRedFirst] = MotherBrainFinalRoomPaintDefinitions.RedLens;
            stock[MotherBrainRoomColorDefinitions.RoomRedFirst + 1] = new RgbColor(
                Math.Max(0, MotherBrainFinalRoomPaintDefinitions.RedLens.Red - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop), 0, 0);
            stock[MotherBrainRoomColorDefinitions.RoomBlueFirst] = MotherBrainFinalRoomPaintDefinitions.BlueLens;
            stock[MotherBrainRoomColorDefinitions.RoomBlueFirst + 1] = new RgbColor(0, 0,
                Math.Max(0, MotherBrainFinalRoomPaintDefinitions.BlueLens.Blue - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop));
            stock[MotherBrainRoomColorDefinitions.RoomGlowColor] = MotherBrainFinalRoomPaintDefinitions.WarmAccent;
            return [.. stock];
        }

        /// <summary>The amber panel light between the shadow ramp and the gray ramp; its pixel role remains unresolved.</summary>
        private const int PanelAmberColor = 10;
    }
    /// <summary>Interpolates RGB5 channels with nearest rounding, or ceiling rounding for shadow red.</summary>
    private static RgbColor InterpolateRgb5(RgbColor start, RgbColor end, int step, int intervals, bool ceilingRed = false)
    {
        return start.Zip(end, (channel, a, b) => ((a * (intervals - step) + b * step + (ceilingRed && channel == ColorChannel.Red ? intervals - 1 : intervals / 2)) / intervals));
    }
    /// <summary>Derives ring, core, tissue, and casing shades while preserving independent edits.</summary>
    internal sealed class AttackPalette
    {
        /// <summary>Supplied attack colors when any word differs from the shared-shade derivation.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>Keeps every supplied colour when one differs from the paint-derived shades.</summary>
        /// <param name="supplied">Every supplied colour when one differs, otherwise null.</param>
        internal AttackPalette(RgbColor[]? supplied) => this.supplied = supplied;
        /// <summary>Selects the edited or calculated phase-two attack color.</summary>
        public RgbColor Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Builds cyan and flame ramps and reuses health-palette tissue colors.</summary>
        private static RgbColor Calculate(int color)
        {
            if (color < MotherBrainRoomColorDefinitions.AttackFlameFirst)
                return InterpolateShade(MotherBrainAttackPaintDefinitions.RingHighlight, MotherBrainAttackPaintDefinitions.RingEdge,
                    color - MotherBrainRoomColorDefinitions.AttackCyanFirst,
                    MotherBrainRoomColorDefinitions.AttackCyanCount - 1);
            if (color < MotherBrainRoomColorDefinitions.AttackTissueFirst)
                return InterpolateShade(MotherBrainAttackPaintDefinitions.CoreHighlight, MotherBrainAttackPaintDefinitions.CoreEdge,
                    color - MotherBrainRoomColorDefinitions.AttackFlameFirst,
                    MotherBrainRoomColorDefinitions.AttackFlameCount - 1);
            if (color < MotherBrainRoomColorDefinitions.AttackGrayFirst)
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body,
                    MotherBrainRoomColorDefinitions.AttackBodyTissueFirst + color - MotherBrainRoomColorDefinitions.AttackTissueFirst);
            if (color == MotherBrainRoomColorDefinitions.WhiteColor)
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, MotherBrainRoomColorDefinitions.WhiteColor);
            return InterpolateShade(MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight, MotherBrainAttackPaintDefinitions.ShellOutline,
                color - MotherBrainRoomColorDefinitions.AttackGrayFirst
                - (color > MotherBrainRoomColorDefinitions.WhiteColor ? 1 : 0),
                MotherBrainRoomColorDefinitions.AttackGrayLast - MotherBrainRoomColorDefinitions.AttackGrayFirst - 1);
        }
        /// <summary>Interpolates an attack shade between its painted endpoints with nearest rounding.</summary>
        private static RgbColor InterpolateShade(RgbColor first, RgbColor last, int shade, int intervals)
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
        internal readonly RgbColor[]? supplied;
        /// <summary>Keeps the room palette and, when one shard colour differs from the derivation, every supplied colour.</summary>
        /// <param name="room">The room palette supplying shared colours.</param>
        /// <param name="supplied">Every supplied colour when one differs, otherwise null.</param>
        internal GlassPalette(FinalRoomPalette room, RgbColor[]? supplied)
        {
            this.room = room;
            this.supplied = supplied;
        }
        /// <summary>Selects a supplied or room-derived shard color.</summary>
        public RgbColor Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Builds the glass face ramp and reuses the room's metal accents.</summary>
        private RgbColor Calculate(int color)
        {
            if (color < MotherBrainRoomColorDefinitions.GlassRampFirst)
                return room[MotherBrainRoomColorDefinitions.RoomGrayFirst + color];
            if (color < MotherBrainRoomColorDefinitions.GlassDarkGrayColor)
                return InterpolateRgb5(MotherBrainGlassPaintDefinitions.FaceHighlight, room[MotherBrainRoomColorDefinitions.RoomOutlineColor],
                    color - MotherBrainRoomColorDefinitions.GlassRampFirst, MotherBrainRoomColorDefinitions.GlassRampCount - 1);
            // The glass colours past the ramp that own individual paint; the rest are stock colours.
            return color switch
            {
                MotherBrainRoomColorDefinitions.GlassDarkGrayColor => room[MotherBrainRoomColorDefinitions.RoomDarkGrayColor],
                GlassNeutralColor => MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight,
                _ => MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color),
            };
        }

        /// <summary>The glass's neutral casing highlight.</summary>
        private const int GlassNeutralColor = 12;
    }
    /// <summary>Relates tube-projectile casing and outline shades to the final room palette.</summary>
    internal sealed class TubePalette
    {
        /// <summary>Room palette supplying tube outline colors.</summary>
        private readonly FinalRoomPalette room;
        /// <summary>Independent tube colors retained only when derivation differs.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>Keeps the room palette and, when one tube colour differs from the derivation, every supplied colour.</summary>
        /// <param name="room">The room palette supplying outline colours.</param>
        /// <param name="supplied">Every supplied colour when one differs, otherwise null.</param>
        internal TubePalette(FinalRoomPalette room, RgbColor[]? supplied)
        {
            this.room = room;
            this.supplied = supplied;
        }
        /// <summary>Selects a supplied or room-derived tube-projectile color.</summary>
        public RgbColor Color(int color) => supplied is null ? Calculate(color) : supplied[color];
        /// <summary>Uses shared casing highlight, stock black, and room outline colors.</summary>
        private RgbColor Calculate(int color) => color < MotherBrainRoomColorDefinitions.TubeNeutralCount
            ? MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight : color == MotherBrainRoomColorDefinitions.RoomEntryBlackColor
                ? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color)
                : room[MotherBrainRoomColorDefinitions.RoomOutlineColor + color - MotherBrainRoomColorDefinitions.TubeNeutralCount];
    }
    /// <summary>Background highlight interpolation paired with darkening level colors.</summary>
    internal sealed class RoomFlash
    {
        /// <summary>Final room colors that the timed flash brightens or dims.</summary>
        internal readonly FinalRoomPalette basis;
        /// <summary>Independent flash frames retained only when the calculated frames differ.</summary>
        internal readonly RgbColor[][]? supplied;

        /// <summary>Number of timed flash entries selected by the palette program.</summary>
        public int FrameCount { get; }

        /// <summary>Keeps the flash's frame count and basis, and every supplied frame when one differs from the derivation.</summary>
        /// <param name="frameCount">The number of flash entries.</param>
        /// <param name="finalRoom">The final room colours the flash brightens or dims.</param>
        /// <param name="supplied">Every supplied frame when one differs, otherwise null.</param>
        internal RoomFlash(int frameCount, FinalRoomPalette finalRoom, RgbColor[][]? supplied)
        {
            FrameCount = frameCount;
            basis = finalRoom;
            this.supplied = supplied;
        }

        /// <summary>Selects the edited or calculated room color at one timed entry.</summary>
        public RgbColor Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Blends the first room color toward warm flood and dims the remaining colors by flash strength.</summary>
        private RgbColor Calculate(int frame, int color)
        {
            int strength = MotherBrainRoomFlashes.FlashStrength(frame);
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
        /// <summary>Independent recovery frames retained only when the calculated frames differ.</summary>
        internal readonly RgbColor[][]? supplied;

        /// <summary>Keeps the shared endpoints and every supplied frame when one differs from the intensity steps.</summary>
        /// <param name="finalRoom">The final room colours reused as endpoints.</param>
        /// <param name="supplied">Every supplied frame when one differs, otherwise null.</param>
        internal RecoveryLightFade(FinalRoomPalette finalRoom, RgbColor[][]? supplied)
        {
            this.finalRoom = finalRoom;
            this.supplied = supplied;
        }

        /// <summary>Selects edited or calculated doorway and room-light color for one recovery frame.</summary>
        public RgbColor Color(int frame, int color) => supplied is null ? Calculate(frame, color) : supplied[frame][color];

        /// <summary>Scales selected doorway, casing, and room endpoints by the recovery frame's intensity.</summary>
        private RgbColor Calculate(int frame, int color)
        {
            RgbColor endpoint = color switch
            {
                0 => MotherBrainRecoveryPaintDefinitions.DoorwayRimLight,
                1 => MotherBrainRecoveryPaintDefinitions.DoorwayRimMiddle,
                2 => MotherBrainRecoveryPaintDefinitions.DoorwayRimDark,
                >= 3 and <= 13 => finalRoom[color - 3],
                14 or 15 => MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight,
                _ => finalRoom[color - 4],
            };
            return endpoint.Map((_, a) => (a * (frame + 1) /
                    MotherBrainRoomColorDefinitions.RecoveryLightsFrames));
        }
    }
}

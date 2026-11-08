using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

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
            MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, MotherBrainRoomColorGeometry.WhiteColor);

        /// <summary>Selects the paint-derived room colour; colours past the two slices repeat the stock white.</summary>
        private static RgbColor Calculate(int color) => color < StockColors.Length ? StockColors[color] : StockWhite;

        /// <summary>Builds room wall and shadow ramps plus the selected panel and lens accents.</summary>
        private static ImmutableArray<RgbColor> BuildStockColors()
        {
            RgbColor[] stock = new RgbColor[2 * MotherBrainRoomColorGeometry.SliceColors];
            Array.Fill(stock, StockWhite);
            for (int step = 0; step < MotherBrainRoomColorGeometry.RoomWallCount; step++)
                stock[MotherBrainRoomColorGeometry.RoomWallFirst + step] = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.WallLight,
                    MotherBrainFinalRoomPaintDefinitions.WallDark, step, MotherBrainRoomColorGeometry.RoomWallCount - 1);
            // Shadow red is5,4,3,1: ceiling interpolation. Green7,5,4,2
            // and blue8,6,4,2 use nearest interpolation. This is an exact sequence
            // relationship, not a claim about a historical palette tool.
            RgbColor recessedShadow = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight, RgbColor.Black,
                MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor - 1, MotherBrainFinalRoomPaintDefinitions.RecessedShadowDivisor);
            for (int step = 0; step < MotherBrainRoomColorGeometry.RoomShadowCount; step++)
                stock[MotherBrainRoomColorGeometry.RoomShadowFirst + step] = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.RecessedLight,
                    recessedShadow, step, MotherBrainRoomColorGeometry.RoomShadowCount - 1,
                    ceilingRed: MotherBrainFinalRoomPaintDefinitions.RecessedRedCeiling);
            // The panel colours between the shadow ramp and the outline are black except the amber light.
            for (int color = MotherBrainRoomColorGeometry.RoomShadowFirst + MotherBrainRoomColorGeometry.RoomShadowCount;
                 color < MotherBrainRoomColorGeometry.RoomOutlineColor; color++)
                stock[color] = RgbColor.Black;
            stock[PanelAmberColor] = MotherBrainFinalRoomPaintDefinitions.Amber;
            stock[MotherBrainRoomColorGeometry.RoomOutlineColor] = MotherBrainFinalRoomPaintDefinitions.PanelField;
            for (int step = 0; step < MotherBrainRoomColorGeometry.RoomGrayCount; step++)
                stock[MotherBrainRoomColorGeometry.RoomGrayFirst + step] = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLight,
                    MotherBrainFinalRoomPaintDefinitions.MetalLow, step, MotherBrainRoomColorGeometry.RoomGrayCount - 1);
            // Metal contour shadow is half the low gray surface intensity.
            RgbColor contour = InterpolateRgb5(MotherBrainFinalRoomPaintDefinitions.MetalLow, RgbColor.Black,
                MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor - 1, MotherBrainFinalRoomPaintDefinitions.MetalContourDivisor);
            stock[MotherBrainRoomColorGeometry.RoomDarkGrayColor] = contour;
            stock[MotherBrainRoomColorGeometry.RoomRepeatedDarkGrayColor] = contour;
            stock[MotherBrainRoomColorGeometry.RoomRedFirst] = MotherBrainFinalRoomPaintDefinitions.RedLens;
            stock[MotherBrainRoomColorGeometry.RoomRedFirst + 1] = new FadeLevelColour(
                Math.Max(0, MotherBrainFinalRoomPaintDefinitions.RedLens.FadeLevels.Red - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop), 0, 0).ToColour();
            stock[MotherBrainRoomColorGeometry.RoomBlueFirst] = MotherBrainFinalRoomPaintDefinitions.BlueLens;
            stock[MotherBrainRoomColorGeometry.RoomBlueFirst + 1] = new FadeLevelColour(0, 0,
                Math.Max(0, MotherBrainFinalRoomPaintDefinitions.BlueLens.FadeLevels.Blue - MotherBrainFinalRoomPaintDefinitions.LensShadeDrop)).ToColour();
            stock[MotherBrainRoomColorGeometry.RoomGlowColor] = MotherBrainFinalRoomPaintDefinitions.WarmAccent;
            return [.. stock];
        }

        /// <summary>The amber panel light between the shadow ramp and the gray ramp; its pixel role remains unresolved.</summary>
        private const int PanelAmberColor = 10;
    }
    /// <summary>Interpolates RGB5 channels with nearest rounding, or ceiling rounding for shadow red.</summary>
    private static RgbColor InterpolateRgb5(RgbColor start, RgbColor end, int step, int intervals, bool ceilingRed = false)
    {
        return start.FadeLevels.Zip(end.FadeLevels, (channel, a, b) => ((a * (intervals - step) + b * step + (ceilingRed && channel == ColorChannel.Red ? intervals - 1 : intervals / 2)) / intervals)).ToColour();
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
            if (color < MotherBrainRoomColorGeometry.AttackFlameFirst)
                return InterpolateShade(MotherBrainAttackPaintDefinitions.RingHighlight, MotherBrainAttackPaintDefinitions.RingEdge,
                    color - MotherBrainRoomColorGeometry.AttackCyanFirst,
                    MotherBrainRoomColorGeometry.AttackCyanCount - 1);
            if (color < MotherBrainRoomColorGeometry.AttackTissueFirst)
                return InterpolateShade(MotherBrainAttackPaintDefinitions.CoreHighlight, MotherBrainAttackPaintDefinitions.CoreEdge,
                    color - MotherBrainRoomColorGeometry.AttackFlameFirst,
                    MotherBrainRoomColorGeometry.AttackFlameCount - 1);
            if (color < MotherBrainRoomColorGeometry.AttackGrayFirst)
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body,
                    MotherBrainRoomColorGeometry.AttackBodyTissueFirst + color - MotherBrainRoomColorGeometry.AttackTissueFirst);
            if (color == MotherBrainRoomColorGeometry.WhiteColor)
                return MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, MotherBrainRoomColorGeometry.WhiteColor);
            return InterpolateShade(MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight, MotherBrainAttackPaintDefinitions.ShellOutline,
                color - MotherBrainRoomColorGeometry.AttackGrayFirst
                - (color > MotherBrainRoomColorGeometry.WhiteColor ? 1 : 0),
                MotherBrainRoomColorGeometry.AttackGrayLast - MotherBrainRoomColorGeometry.AttackGrayFirst - 1);
        }
        /// <summary>Interpolates an attack shade between its painted endpoints with nearest rounding.</summary>
        private static RgbColor InterpolateShade(RgbColor first, RgbColor last, int shade, int intervals)
        {
            return first.FadeLevels.Zip(last.FadeLevels, (_, a, b) => ((a * (intervals - shade)
                    + b * shade + intervals / 2) / intervals)).ToColour();
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
            if (color < MotherBrainRoomColorGeometry.GlassRampFirst)
                return room[MotherBrainRoomColorGeometry.RoomGrayFirst + color];
            if (color < MotherBrainRoomColorGeometry.GlassDarkGrayColor)
                return InterpolateRgb5(MotherBrainGlassPaintDefinitions.FaceHighlight, room[MotherBrainRoomColorGeometry.RoomOutlineColor],
                    color - MotherBrainRoomColorGeometry.GlassRampFirst, MotherBrainRoomColorGeometry.GlassRampCount - 1);
            // The glass colours past the ramp that own individual paint; the rest are stock colours.
            return color switch
            {
                MotherBrainRoomColorGeometry.GlassDarkGrayColor => room[MotherBrainRoomColorGeometry.RoomDarkGrayColor],
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
        private RgbColor Calculate(int color) => color < MotherBrainRoomColorGeometry.TubeNeutralCount
            ? MotherBrainRecoveryPaintDefinitions.SharedCasingHighlight : color == MotherBrainRoomColorGeometry.RoomEntryBlackColor
                ? MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.Body, color)
                : room[MotherBrainRoomColorGeometry.RoomOutlineColor + color - MotherBrainRoomColorGeometry.TubeNeutralCount];
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
            return basis[color].FadeLevels.Zip(MotherBrainRoomFlashPaintDefinitions.WarmFlood.FadeLevels, (_, channel, warm) =>
                color < MotherBrainRoomFlashPaintDefinitions.HighlightedColorCount
                    ? (channel * (MotherBrainRoomFlashPaintDefinitions.BrightenIntervals - strength)
                        + warm * strength
                        + MotherBrainRoomFlashPaintDefinitions.BrightenIntervals / 2) / MotherBrainRoomFlashPaintDefinitions.BrightenIntervals
                    : (channel * (MotherBrainRoomFlashPaintDefinitions.DimIntervals - strength)
                        + MotherBrainRoomFlashPaintDefinitions.DimIntervals / 2) / MotherBrainRoomFlashPaintDefinitions.DimIntervals).ToColour();
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
            return endpoint.FadeLevels.Map((_, a) => (a * (frame + 1) /
                    MotherBrainRoomColorDefinitions.RecoveryLightsFrames)).ToColour();
        }
    }
}

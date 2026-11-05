using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using Presentation = SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRoomColorPresentation;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Recognizes Mother Brain's supplied room colours as their paint derivations: each family keeps its supplied colours
/// only when one of them differs from the colours derived from the final room palette and the selected paint.
/// </summary>
internal static class MotherBrainRoomColorRecognition
{
    /// <summary>Builds the selected flash, room, phase-two, room-entry and recovery colours.</summary>
    /// <param name="colours">The selected colours.</param>
    internal static Presentation Create(MotherBrainRoomColours colours)
    {
        Ensure.NotNull(colours);
        Presentation.FinalRoomPalette room = FinalRoom(colours.FinalRoom);
        return new(Flash(colours.Flash, room), room, Attack(colours.PhaseTwoAttack), RearLeg(colours.PhaseTwoRearLeg),
            Glass(colours.InitialGlassShard, room), Tube(colours.InitialTubeProjectile, room), Recovery(colours.RecoveryLights, room));
    }

    /// <summary>Whether <paramref name="resolve"/> gives every supplied colour.</summary>
    private static bool Reproduces(RgbColor[] colors, Func<int, RgbColor> resolve)
    {
        for (int color = 0; color < colors.Length; color++)
            if (resolve(color) != colors[color]) return false;
        return true;
    }

    /// <summary>Whether <paramref name="resolve"/> gives every supplied colour of every frame.</summary>
    private static bool Reproduces(RgbColor[][] frames, Func<int, int, RgbColor> resolve)
    {
        for (int frame = 0; frame < frames.Length; frame++)
            if (!Reproduces(frames[frame], color => resolve(frame, color))) return false;
        return true;
    }

    /// <summary>Checks whether all supplied colors match the paint-derived room palette.</summary>
    private static Presentation.FinalRoomPalette FinalRoom(RgbColor[] colors)
    {
        var calculated = new Presentation.FinalRoomPalette(colors.Length, supplied: null);
        return Reproduces(colors, color => calculated[color]) ? calculated : new(colors.Length, colors);
    }

    /// <summary>Uses paint-derived attack shades only when every supplied colour matches.</summary>
    private static Presentation.AttackPalette Attack(RgbColor[] colors)
    {
        var calculated = new Presentation.AttackPalette(supplied: null);
        return Reproduces(colors, calculated.Color) ? calculated : new(colors);
    }

    /// <summary>The supplied phase-two rear legs when one differs from the stock rear legs, otherwise null.</summary>
    private static RgbColor[]? RearLeg(RgbColor[] colors) =>
        Reproduces(colors, color => MotherBrainHealthPalettePresentation.StockBaseColor(MotherBrainPalettePart.BackLeg, color))
            ? null : colors;

    /// <summary>Checks whether shard colors can be calculated from room and glass paint.</summary>
    private static Presentation.GlassPalette Glass(RgbColor[] colors, Presentation.FinalRoomPalette room)
    {
        var calculated = new Presentation.GlassPalette(room, supplied: null);
        return Reproduces(colors, calculated.Color) ? calculated : new(room, colors);
    }

    /// <summary>Checks whether tube colors can be calculated from room paint.</summary>
    private static Presentation.TubePalette Tube(RgbColor[] colors, Presentation.FinalRoomPalette room)
    {
        var calculated = new Presentation.TubePalette(room, supplied: null);
        return Reproduces(colors, calculated.Color) ? calculated : new(room, colors);
    }

    /// <summary>Checks supplied flash frames against the final-room color derivation.</summary>
    private static Presentation.RoomFlash Flash(RgbColor[][] frames, Presentation.FinalRoomPalette room)
    {
        var calculated = new Presentation.RoomFlash(frames.Length, room, supplied: null);
        for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < room.Length; color++)
                if (calculated.Color(frame, color) != frames[frame][color])
                    return new(frames.Length, room, frames);
        return calculated;
    }

    /// <summary>Checks supplied recovery colors against seven intensity steps to shared endpoints.</summary>
    private static Presentation.RecoveryLightFade Recovery(RgbColor[][] frames, Presentation.FinalRoomPalette room)
    {
        var calculated = new Presentation.RecoveryLightFade(room, supplied: null);
        return Reproduces(frames, calculated.Color) ? calculated : new(room, frames);
    }
}

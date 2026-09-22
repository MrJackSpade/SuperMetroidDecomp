using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyEffectPatterns(byte[] rom)
    {
        int[] background = Oracle(rom, EffectResearchData.BackgroundShake, 144, 2, "BGShakeDisplacements");
        int[] projectiles = Oracle(rom, EffectResearchData.ProjectileShake, 72, 2, "Get_Values_for_Screen_Shaking");
        for (int type = 0; type < 36; type++)
        {
            var expected = new RoomShakeDefinition((short)background[4 * type], (short)background[4 * type + 1],
                (short)background[4 * type + 2], (short)background[4 * type + 3],
                (short)projectiles[2 * type], (short)projectiles[2 * type + 1]);
            Equal(expected, Shake(type), $"shake generator {type}");
            Equal(expected, RoomShakeDefinitions.ForType((ushort)type), $"shake compiled {type}");
        }
        int[] radii = Oracle(rom, EffectResearchData.FirefleaRadii, 8, 2);
        for (int i = 0; i < radii.Length; i++)
        {
            Equal(radii[i], FirefleaRadius(i), $"Fireflea radius {i}");
            Equal(radii[i], (int)FirefleaMovementDefinitions.Radius((byte)i), $"compiled Fireflea radius {i}");
        }
        int[] shades = Oracle(rom, EffectResearchData.FirefleaFlashing, 12, 2);
        for (int i = 0; i < shades.Length; i++)
        {
            Equal(shades[i], FirefleaFlash(i), $"Fireflea flash {i}");
            Equal(shades[i], (int)FirefleaFxDefinitions.FlashingShade((ushort)i), $"compiled Fireflea flash {i}");
        }
        int[] masks = Oracle(rom, EffectResearchData.CrocomireMasks, 8, 1, "TilePixelColumnBitmasks");
        for (int i = 0; i < 49; i++)
        {
            Equal(masks[i & 7], MeltMask(i), $"Crocomire mask {i}");
            Equal(masks[i & 7], (int)CrocomireMeltingDefinitions.SelectMask(i), $"compiled Crocomire mask {i}");
        }
        int[] brakes = Oracle(rom, EffectResearchData.GunshipBrakes, 17, 2, "ShipBrakesMovementData");
        for (int i = 0; i < brakes.Length; i++)
        {
            Equal((int)unchecked((short)brakes[i]), GunshipBrake(i), $"gunship brake {i}");
            Equal(GunshipBrake(i), Math.Clamp((8 - i) / 3, -1, 1), $"gunship clamped ramp {i}");
            Equal(unchecked((short)brakes[i]), GunshipMotionDefinitions.LandingBrakeYDelta((ushort)i), $"compiled brake {i}");
        }
        int[] bob = Oracle(rom, EffectResearchData.GunshipBob, 8, 1, "ProcessShipHover");
        for (int i = 0; i < 4; i++)
        {
            var compiled = GunshipMotionDefinitions.IdleBob((ushort)i);
            Equal(16, bob[2 * i], $"gunship bob timer {i}");
            Equal((int)unchecked((sbyte)bob[2 * i + 1]), GunshipBob(i), $"gunship bob delta {i}");
            Equal(bob[2 * i], (int)compiled.Timer, $"compiled gunship bob timer {i}");
            Equal(unchecked((sbyte)bob[2 * i + 1]), compiled.YDelta, $"compiled gunship bob delta {i}");
        }
        CheckBounds(i => Shake(i).Bg1X, 35);
        CheckBounds(FirefleaRadius, 7);
        CheckBounds(FirefleaFlash, 11);
        CheckBounds(MeltMask, 48);
        CheckBounds(GunshipBrake, 16);
        CheckBounds(GunshipBob, 3);
        Console.WriteLine("PASS: 269/269 effect values: 216 earthquake words, 20 Fireflea words, 8 melt masks, 25 gunship values.");
    }

    private static RoomShakeDefinition Shake(int type)
    {
        Bound(type, 35);
        int group = type / 9, amplitude = type / 3 % 3 + 1, direction = type % 3;
        short x = (short)(direction == 1 ? 0 : amplitude), y = (short)(direction == 0 ? 0 : amplitude);
        return new(group == 3 ? (short)0 : x, group == 3 ? (short)0 : y,
            group == 0 ? (short)0 : x, group == 0 ? (short)0 : y,
            group < 2 ? (short)0 : x, group < 2 ? (short)0 : y);
    }
    private static int FirefleaRadius(int i) { Bound(i, 7); return 8 * (i + 1); }
    private static int FirefleaFlash(int i) { Bound(i, 11); return (6 - Math.Abs(i - 6)) << 8; }
    private static int MeltMask(int cursor) { Bound(cursor, 48); return 255 ^ (128 >> (cursor & 7)); }
    private static int GunshipBrake(int i) { Bound(i, 16); return i < 6 ? 1 : i < 11 ? 0 : -1; }
    private static int GunshipBob(int i) { Bound(i, 3); return 1 - 2 * ((i ^ (i >> 1)) & 1); }
}

internal static class EffectResearchData
{
    /// <summary>$A0:872D, BGShakeDisplacements, 36 records of four words.</summary>
    internal const int BackgroundShake = 0xa0872d;
    /// <summary>$86:846B, Get_Values_for_Screen_Shaking displacements, 36 XY word pairs.</summary>
    internal const int ProjectileShake = 0x86846b;
    /// <summary>$A3:8D1D, Fireflea radii, eight words.</summary>
    internal const int FirefleaRadii = 0xa38d1d;
    /// <summary>$88:B058, Fireflea_Flashing_Shades, twelve words.</summary>
    internal const int FirefleaFlashing = 0x88b058;
    /// <summary>$A4:9BBD, TilePixelColumnBitmasks, eight bytes.</summary>
    internal const int CrocomireMasks = 0xa49bbd;
    /// <summary>$A2:A622, ShipBrakesMovementData, seventeen words.</summary>
    internal const int GunshipBrakes = 0xa2a622;
    /// <summary>$A2:A7CF, ProcessShipHover timer/YVelocity, four byte pairs.</summary>
    internal const int GunshipBob = 0xa2a7cf;
}

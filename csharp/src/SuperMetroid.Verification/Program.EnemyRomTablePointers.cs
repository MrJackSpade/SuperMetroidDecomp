using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Protects the enemy-data catalog from accidentally acquiring WRAM addresses,
    /// duplicate names, or pointers outside mapped cartridge space. With the private
    /// retail ROM present, this also reads both ends of representative multi-byte ranges.
    /// </summary>
    static void VerifyEnemyRomTablePointerCatalog()
    {
        Type[] families = typeof(EnemyRomTablePointers).GetNestedTypes(
            BindingFlags.Public | BindingFlags.NonPublic);
        AssertTrue(families.Length >= 14, "enemy ROM data is grouped by owning family");

        FieldInfo[] fields = families
            .SelectMany(family => family.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.IsLiteral && field.FieldType == typeof(int))
            .ToArray();
        AssertTrue(fields.Length >= 50, "enemy ROM data catalog covers fixed data tables");
        foreach (FieldInfo field in fields)
        {
            int address = (int)field.GetRawConstantValue()!;
            AssertTrue(
                address is >= 0x808000 and <= 0xffffff,
                $"{field.DeclaringType!.Name}.{field.Name} is a mapped SNES ROM address");
        }

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine(
                $"  Enemy ROM data: {fields.Length} named ranges are structurally valid; " +
                "retail ROM reads skipped (private input absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach ((int start, int byteLength) in RepresentativeEnemyRomRanges())
        {
            _ = bus.ReadByte(start);
            _ = bus.ReadByte(start + byteLength - 1);
        }

        Console.WriteLine(
            $"  Enemy ROM data: {fields.Length} named ranges and " +
            $"{RepresentativeEnemyRomRanges().Length} representative retail ranges verified.");
    }

    /// <summary>
    /// Uses one nontrivial range from every represented storage shape: byte arrays,
    /// word arrays, palettes, pointer arrays, interleaved records, and transfer tables.
    /// </summary>
    private static (int Start, int ByteLength)[] RepresentativeEnemyRomRanges() =>
    [
        (EnemyRomTablePointers.Common.SignedSineCosineWords, 512),
        (EnemyRomTablePointers.Torizo.WakeXPositions, 4),
        (EnemyRomTablePointers.ChozoStatue.CarryVelocityWords, 64),
        (EnemyRomTablePointers.Ceres.FallingDebrisInstructionPointers, 12),
        (EnemyRomTablePointers.Crocomire.DeathGraphicsSourceWords, 14),
        (EnemyRomTablePointers.DeadSidehopper.HorizontalVelocityWords, 8),
        (EnemyRomTablePointers.Gunship.LiftoffVramDestinationWords, 10),
        (EnemyRomTablePointers.Kraid.RoomBackgroundPaletteWords, 32),
        (EnemyRomTablePointers.Phantoon.FirstRoundHidingTimerWords, 16),
        (EnemyRomTablePointers.Ridley.HealthPaletteWords, 84),
        (EnemyRomTablePointers.TourianStatue.StatuePaletteWords, 32),
        (EnemyRomTablePointers.WorkRobot.InitialInstructionListWords, 4),
    ];
}

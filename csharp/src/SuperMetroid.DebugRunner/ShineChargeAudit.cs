using System.Globalization;
using System.Security.Cryptography;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Compares the runtime palette dispatcher to the original CPU's charged-glow/storage interaction.</summary>
internal static class ShineChargeAudit
{
    public static int Run(string rom, string trace)
    {
        if (Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(trace))) !=
            "7A829405877C9D40FCEB1C6792D20FADD0CF4C51B40396ED1EF43B3098C4FFE7")
            throw new InvalidDataException("Use accepted shine-charge capture.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int records = 0, differences = 0;
        foreach (var group in File.ReadLines(trace).Skip(1).Select(line => line.Split(','))
            .GroupBy(row => string.Join(',', row[..2])))
        {
            var seed = group.First();
            int shots = int.Parse(seed[0]);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            samus.EquippedItems = seed[1] switch { "0" => 0,
                "1" => (ushort)SamusEquipmentFlags.VariaSuit, _ => (ushort)SamusEquipmentFlags.GravitySuit };
            samus.Shinespark.TryStoreFromSpeedBooster(SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter);
            int failures = 0;
            foreach (var row in group)
            {
                int frame = int.Parse(row[2]);
                // Set only the post-shot seed. Production StepFrame owns all palette ordering and countdowns.
                for (int shot = 0; shot < shots; shot++)
                    if (frame == 20 + 50 * shot)
                        typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ChargedShotGlowTimer))!
                            .SetValue(runtime.Projectiles, (ushort)4);
                runtime.StepFrame(0);
                string palette = string.Concat(runtime.Cgram.Colors.Slice(SamusPaletteRomData.Common.SamusObjPaletteStart, 16)
                    .ToArray().Select(color => $"{color:X4}"));
                string actual = $"{samus.Shinespark.ShineTimer:X4},{runtime.Projectiles.ChargedShotGlowTimer:X4}," +
                    $"{samus.Shinespark.PaletteType:X4},{samus.Shinespark.PaletteFrameOffset:X4},{palette}";
                if (actual != string.Join(',', row[3..]))
                {
                    if (failures == 0) Console.WriteLine($"First mismatch {group.Key} frame {frame}: {actual}; native {string.Join(',', row[3..])}");
                    failures++;
                }
                records++;
                // Stop after the native storage expiry, including its normal-palette restoration.
                // Handler zero's subsequent Screw/Speed palette state is a different fixture.
                if (row[3] == "0000") break;
            }
            differences += failures;
            Console.WriteLine($"Charge/storage {group.Key}: {failures} mismatches.");
        }
        if (records != 2178) throw new InvalidDataException("Incomplete charge/storage matrix.");
        Console.WriteLine($"Charge/storage: {records} records, {differences} mismatches.");
        return differences == 0 ? 0 : 1;
    }
}

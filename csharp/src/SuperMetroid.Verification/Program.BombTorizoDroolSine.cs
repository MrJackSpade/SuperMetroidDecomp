using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static MotherBrainNeckSineVerification;
using static Program;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class BombTorizoDroolSineVerification
{
    internal static void VerifyBombTorizoDroolSine(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Action<RoomEnemySystem, RoomEnemySlot> spawn =
            (enemies, torizo) => enemies.BombTorizoBehavior.SpawnBombTorizoLowHealthDrool(torizo);

        for (int random = byte.MinValue; random <= byte.MaxValue; random++)
        {
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                random,
                parameter1: 0x4000,
                expectedIndex: random & 0x01fe,
                "full-circle left-facing");
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                random,
                parameter1: 0xc000,
                expectedIndex: random & 0x01fe,
                "full-circle right-facing");
        }

        for (int randomNibble = 0; randomNibble < 16; randomNibble++)
        {
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                randomNibble,
                parameter1: 0,
                expectedIndex: (224 + randomNibble - 8) << 1,
                "left-facing cone");
            VerifyBombTorizoDroolSpawn(
                rom,
                spawn,
                randomNibble,
                parameter1: 0x8000,
                expectedIndex: (32 + randomNibble - 8) << 1,
                "right-facing cone");
        }

        Console.WriteLine(
            "Bomb Torizo drool sine: 544 real random/facing spawns preserve native XY velocity with the entire signed-sine ROM table forbidden.");
    }

    private static void VerifyBombTorizoDroolSpawn(
        SuperMetroidAddressSpace rom,
        Action<RoomEnemySystem, RoomEnemySlot> spawn,
        int random,
        ushort parameter1,
        int expectedIndex,
        string scenario)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewBombTorizoDroolSineReadGuard(rom));
        enemies.VerificationServices().NextRandom = (Func<ushort>)(new Queue<ushort>([0, unchecked((ushort)random)]).Dequeue);

        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(0x0400, 0x0200));
        torizo.Parameter1 = parameter1;

        spawn(enemies, torizo);

        RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles[^1];
        AssertTrue(projectile.IsActive,
            $"Bomb Torizo {scenario} random {random:X2} allocation");
        // $86:A621/$A628 index the sign-extended sine and negative-cosine tables with the
        // same word offset: X is the sine and Y the negative cosine of the drool angle.
        AssertEqual(
            ReadBombTorizoDroolWord(rom, EnemyMathReferenceData.SignedSine, expectedIndex),
            projectile.Velocity.X.RawValue,
            $"Bomb Torizo {scenario} random {random:X2} X velocity");
        AssertEqual(
            ReadBombTorizoDroolWord(rom, EnemyMathReferenceData.SignedNegativeCosine, expectedIndex),
            projectile.Velocity.Y.RawValue,
            $"Bomb Torizo {scenario} random {random:X2} Y velocity");
        AssertEqual(
            (parameter1 & 0x8000) != 0 ? (ushort)0x0408 : (ushort)0x03f8,
            projectile.Position.X.Whole,
            $"Bomb Torizo {scenario} random {random:X2} X origin");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Bomb Torizo drool words")]
    private static ushort ReadBombTorizoDroolWord(
        SuperMetroidAddressSpace rom,
        int table,
        int wordOffset)
    {
        int address = table + wordOffset;
        return CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
    }

    private static CartridgeReadGuard NewBombTorizoDroolSineReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address is >= 0xa0b3c3 and < 0xa0b643,
            address => $"Bomb Torizo drool attempted migrated sine or negative-cosine read ${address:X6}.");

    [VerificationCommand("--remaining-signed-sine-consumers")]
    internal static int RemainingSignedSineConsumersCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBombTorizoDroolSine), () => VerifyBombTorizoDroolSine(rom));
        Suite(nameof(VerifyMotherBrainNeckSine), () => VerifyMotherBrainNeckSine(rom));
        return 0;
    }
}

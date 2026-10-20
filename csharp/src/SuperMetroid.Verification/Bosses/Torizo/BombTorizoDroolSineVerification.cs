using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainNeckSineVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Bomb Torizo's drool velocity against native sine indexes for both facing modes and all random bytes.</summary>
internal static class BombTorizoDroolSineVerification
{
    /// <summary>Checks both facing modes and native velocity table indexes for every drool random value.</summary>
    internal static void VerifyBombTorizoDroolSine(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySystem, RoomEnemySlot> spawn =
            (enemies, torizo) => enemies.BombTorizoBehavior.Attacks.SpawnBombTorizoLowHealthDrool(torizo);

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

    /// <summary>Spawns drool through production behavior with sine ROM reads forbidden and compares its velocity and origin to cartridge words.</summary>
    private static void VerifyBombTorizoDroolSpawn(
        SuperMetroidAddressSpace rom,
        Action<RoomEnemySystem, RoomEnemySlot> spawn,
        int random,
        ushort parameter1,
        int expectedIndex,
        string scenario)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = new Queue<ushort>([0, unchecked((ushort)random)]).Dequeue;

        RoomEnemySlot torizo = enemies.SlotTable.Slots[0];
        torizo.Position = torizo.Position.WithWhole(new(0x0400, 0x0200));
        enemies.BombTorizoBehavior.BindFixtureState(torizo, parameter1);

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

    /// <summary>Reads the pinned retail sine or negative-cosine word used solely as this verifier's oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Bomb Torizo drool words")]
    private static ushort ReadBombTorizoDroolWord(
        SuperMetroidAddressSpace rom,
        int table,
        int wordOffset)
    {
        int address = table + wordOffset;
        return CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
    }


    /// <summary>Runs the two signed-sine consumer checks against the retail cartridge image.</summary>
    [VerificationCommand("--remaining-signed-sine-consumers")]
    internal static int RemainingSignedSineConsumersCommand()
    {
        SuperMetroidAddressSpace rom =
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyBombTorizoDroolSine), () => VerifyBombTorizoDroolSine(rom));
        Suite(nameof(VerifyMotherBrainNeckSine), () => VerifyMotherBrainNeckSine(rom));
        return 0;
    }
}

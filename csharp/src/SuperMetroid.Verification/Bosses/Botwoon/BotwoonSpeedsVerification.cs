using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>Checks Botwoon movement, spit, and body-spacing values against native phase tables.</summary>
internal static class BotwoonSpeedsVerification
{
    /// <summary>Runs separate speed and segment-spacing checks for the three health phases.</summary>
    internal static void VerifyCompiledBotwoonSpeeds(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBotwoonSpeedAlgorithm), () => VerifyBotwoonSpeedAlgorithm(rom));
        Suite(nameof(VerifyBotwoonSpacingAlgorithm), () => VerifyBotwoonSpacingAlgorithm(rom));
    }

    /// <summary>Checks native speeds and production health-phase transitions, including phase retention inside a hole.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Botwoon speed algorithm")]
    private static void VerifyBotwoonSpeedAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (byte phase = 0; phase < 3; phase++)
        {
            var speed = BotwoonSpeedDefinitions.ForHealthPhase(phase);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb394bb + phase * 4)), speed.MovementSpeed, "Botwoon native movement speed");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb39e77 + phase * 2)), speed.SpitSpeed, "Botwoon native spit speed");
        }
        Action<RoomEnemySlot, BotwoonEnemyState> update = BotwoonBehavior.UpdateBotwoonHealthPhase;
        var head = new RoomEnemySystem().SlotTable.Slots[0];
        var state = new BotwoonEnemyState(head) { HalfHealth = 1500, QuarterHealth = 750 };
        for (int health = 0; health <= 3000; health++)
        {
            head.Health = (ushort)health;
            byte phase = health is 0 or >= 1500 ? (byte)0 : health >= 750 ? (byte)1 : (byte)2;
            state.InsideHole = false;
            update(head, state);
            AssertEqual(phase, state.HealthPhase, "Botwoon real phase selection without bus");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb394bb + phase * 4)), state.Speed, "Botwoon real phase speed");
            state.InsideHole = true;
            head.Health = (ushort)(3000 - health);
            update(head, state);
            AssertEqual(phase, state.HealthPhase, "Botwoon preserves phase inside hole");
        }
        AssertThrows<InvalidDataException>(() => BotwoonSpeedDefinitions.ForHealthPhase(3), "Botwoon invalid phase");
        AssertThrows<InvalidDataException>(() => BotwoonSpeedDefinitions.ForHealthPhase(byte.MaxValue), "Botwoon maximum invalid phase");
        Console.WriteLine("Botwoon speed algorithm: both three-word native views and 3,001 real health transitions match; hidden phases remain held.");
    }

    /// <summary>Checks native body-history spacing for each health phase and rejects unsupported phases.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Botwoon spacing algorithm")]
    private static void VerifyBotwoonSpacingAlgorithm(SuperMetroidAddressSpace rom)
    {
        for (byte phase = 0; phase < 3; phase++)
        {
            int address = BotwoonSpeedDefinitions.MovementReferenceAddress + phase * 4 + 2;
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, BotwoonSpeedDefinitions.ForHealthPhase(phase).SegmentSpacingBytes,
                $"Botwoon native body spacing phase {phase}");
        }
        AssertThrows<InvalidDataException>(() => BotwoonSpeedDefinitions.ForHealthPhase(3), "Botwoon spacing invalid phase");
        AssertThrows<InvalidDataException>(() => BotwoonSpeedDefinitions.ForHealthPhase(byte.MaxValue), "Botwoon spacing maximum invalid phase");
        Console.WriteLine("Botwoon spacing algorithm: all three native body-history spacings and phase bounds match.");
    }
}

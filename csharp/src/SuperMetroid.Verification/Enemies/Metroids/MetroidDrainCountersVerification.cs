using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.SpriteObjects;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Metroid's energy drain and the Mochtroid's attachment count, the counts the species own.</summary>
internal static class MetroidDrainCountersVerification
{
    /// <summary>
    /// A Metroid's drain takes an energy exactly when its fraction exceeds what is held over, which then holds the rest
    /// of the energy taken; a Mochtroid's attachment count wraps after 65535, and its damage check compares the signed word
    /// difference, so a count past the sign bit does not reach the period until it wraps.
    /// </summary>
    internal static void VerifyMetroidDrainCounters()
    {
        var metroid = new MetroidEnemyState(new RoomSpriteObject(RoomSpriteObjectKind.MetroidElectricity, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard), new RoomSpriteObject(RoomSpriteObjectKind.MetroidShell, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
        {
            Drain = FractionalEnergyDrain.HoldingOver(3 * FractionalEnergyDrain.PartsPerEnergy / 16),
        };
        AssertTrue(!metroid.DrainTakesEnergy(3 * FractionalEnergyDrain.PartsPerEnergy / 16), "a fraction equal to what is held over takes no energy");
        AssertEqual(0, metroid.Drain.HeldOver, "nothing is held over after it");
        AssertTrue(metroid.DrainTakesEnergy(3 * FractionalEnergyDrain.PartsPerEnergy / 16), "a fraction above what is held over takes an energy");
        AssertEqual(13 * FractionalEnergyDrain.PartsPerEnergy / 16, metroid.Drain.HeldOver, "the rest of the energy taken is held over");
        AssertEqual(65536, FractionalEnergyDrain.PartsPerEnergy, "an energy is drained in the original's 65536 parts");
        AssertTrue(FractionalEnergyDrain.Start.TakesEnergy(1), "a drain that has taken nothing takes an energy on its first fraction");

        var mochtroid = new MochtroidEnemyState() { AttachmentDamageTimer = 79 };
        AssertTrue(!mochtroid.HasReachedDamagePeriod(80), "79 attached updates do not reach the period");
        mochtroid.CountAttachedUpdate();
        AssertTrue(mochtroid.HasReachedDamagePeriod(80), "the 80th attached update reaches the period");
        mochtroid.AttachmentDamageTimer = 0x8050;
        AssertTrue(!mochtroid.HasReachedDamagePeriod(80), "a count past the sign bit compares as negative");
        mochtroid.AttachmentDamageTimer = ushort.MaxValue;
        mochtroid.CountAttachedUpdate();
        AssertEqual(0, mochtroid.AttachmentDamageTimer, "the count wraps after 65535");
        Console.WriteLine("Metroid drain counters: the drain borrow, the attachment wrap and its signed check match.");
    }

    /// <summary>Runs the focused Metroid drain-counter check.</summary>
    [VerificationCommand("--metroid-drain-counters")]
    internal static int MetroidDrainCountersCommand()
    {
        VerifyMetroidDrainCounters();
        return 0;
    }
}

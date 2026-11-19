using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.EffectActors;
namespace SuperMetroid.Verification.Enemies.Metroids;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Metroid's energy drain and the Mochtroid's attachment count, the counts the species own.</summary>
internal static class MetroidDrainCountersVerification
{
    /// <summary>
    /// A Metroid's drain takes an energy exactly when its fraction exceeds what is held over, which then holds the rest
    /// of the energy taken; a Mochtroid's attachment count reaches its period on the 80th attached update, and every body
    /// attack, the only touch that counts without the check, deals at least the Mochtroid's whole health, so the count
    /// cannot grow far past the period (#2169).
    /// </summary>
    internal static void VerifyMetroidDrainCounters()
    {
        var metroid = new MetroidEnemyState(new RoomEffectActor(RoomEffectActorKind.MetroidElectricity, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard), new RoomEffectActor(RoomEffectActorKind.MetroidShell, default, SuperMetroid.Core.Enemies.SpriteGraphics.SharedStandard))
        {
            Drain = FractionalEnergyDrain.HoldingOver(3 * FractionalEnergyDrain.PartsPerEnergy / 16),
        };
        AssertTrue(!metroid.DrainTakesEnergy(3 * FractionalEnergyDrain.PartsPerEnergy / 16), "a fraction equal to what is held over takes no energy");
        AssertEqual(0, metroid.Drain.HeldOver, "nothing is held over after it");
        AssertTrue(metroid.DrainTakesEnergy(3 * FractionalEnergyDrain.PartsPerEnergy / 16), "a fraction above what is held over takes an energy");
        AssertEqual(13 * FractionalEnergyDrain.PartsPerEnergy / 16, metroid.Drain.HeldOver, "the rest of the energy taken is held over");
        AssertEqual(65536, FractionalEnergyDrain.PartsPerEnergy, "an energy is drained in the original's 65536 parts");
        AssertTrue(FractionalEnergyDrain.Start.TakesEnergy(1), "a drain that has taken nothing takes an energy on its first fraction");

        AssertEqual(80, new MochtroidEnemyState().DrainPeriod.Length, "the drain period lasts 80 attached updates");
        RoomEnemyDefinition definition = RoomEnemyDefinitionCatalog.ById[EnemyDefinitionId.Mochtroid];
        EnemyContactVulnerability contact = EnemyVulnerabilities.Of(definition).Contact;
        (string Attack, int Damage)[] bodyAttacks =
        [
            ("Speed Booster", contact.SpeedBooster.DamageFrom(EnemyCollisionDefinitions.SpeedBoosterContactDamage)),
            ("Shinespark", contact.Shinespark.DamageFrom(EnemyCollisionDefinitions.ShinesparkContactDamage)),
            ("Screw Attack", contact.ScrewAttack.DamageFrom(EnemyCollisionDefinitions.ScrewAttackContactDamage)),
            ("Pseudo Screw", contact.PseudoScrew.DamageFrom(EnemyCollisionDefinitions.PseudoScrewContactDamage)),
        ];
        foreach ((string attack, int damage) in bodyAttacks)
            AssertTrue(damage >= definition.Health, $"{attack} contact deals {damage}, at least the Mochtroid's {definition.Health} health");
        Console.WriteLine("Metroid drain counters: the drain borrow and the attachment period match, and every body attack kills a Mochtroid.");
    }

    /// <summary>Runs the focused Metroid drain-counter check.</summary>
    [VerificationCommand("--metroid-drain-counters")]
    internal static int MetroidDrainCountersCommand()
    {
        VerifyMetroidDrainCounters();
        return 0;
    }
}

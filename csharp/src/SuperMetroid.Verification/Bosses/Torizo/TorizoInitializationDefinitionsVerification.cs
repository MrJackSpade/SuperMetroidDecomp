using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Torizo;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks both Torizo wake records and their use by the actual enemy initializer.</summary>
internal static class TorizoInitializationDefinitionsVerification
{
    /// <summary>Matches twelve compiled record words to ROM, then verifies live initialization preserves population state and skips defeated actors.</summary>
    internal static void VerifyCompiledTorizoInitialization(CartridgeImportAddressSpace rom)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            ushort[] native = [rom.ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeXPositions + variant * 2), rom.ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeYPositions + variant * 2),
                rom.ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeAnimationScripts + variant * 2), rom.ReadCartridgeWord(EnemyRomTablePointersTorizo.WakePropertyMasks + variant * 2),
                rom.ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeXRadii + variant * 2), rom.ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeYRadii + variant * 2)];
            var compiled = variant == 0 ? TorizoInitializationDefinitions.Bomb : TorizoInitializationDefinitions.Golden;
            ushort[] actual = [compiled.Position.X, compiled.Position.Y, TorizoNativeListAddresses.Entry(compiled.Animation), checked((ushort)EnemyPropertyWordCodec.EncodePropertyWord(new EnemyPropertyState(EnemyContact.None, compiled.Processing, EnemyPresence.None, CollidesByFrameHitboxes: false, NewInstructionFrame: false))), compiled.Radius.X, compiled.Radius.Y];
            AssertTrue(native.SequenceEqual(actual), "Every Torizo initialization record word matches ROM");

            EnemyDefinitionId species = variant == 0 ? EnemyDefinitionId.BombTorizo : EnemyDefinitionId.GoldenTorizo;
            // Each initialization is the setup the room's population load runs for a Torizo it places with the entry's
            // properties, in a room of its own whose Torizo is beaten or not.
            RoomEnemyPlace initialize(bool defeated, RoomEnemyPlacement entry)
            {
                var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
                enemies.VerificationServices().IsAreaTorizoDefeated = () => defeated;
                return enemies.LoadPopulation(entry)[0];
            }
            // Every combination of engine properties (#1472: the property word's low byte is species settings, not
            // an engine property), with both additional properties varying across the combinations.
            for (int properties = 0; properties <= ushort.MaxValue; properties += 256)
            {
                EnemyPropertyState population = EnemyPropertyWordCodec.Decode(properties, 0);
                bool populationCollidesByFrameHitboxes = population.Contact.HasAny(EnemyContact.BlocksPlasmaBeam);
                RoomEnemyPlace actor = initialize(defeated: false, PopulationPlacementAccess.Entry(species, new(0xffff, 0xffff)) with
                {
                    Contact = population.Contact, Processing = population.Processing, Presence = population.Presence,
                    CollidesByFrameHitboxes = populationCollidesByFrameHitboxes,
                });
                AssertEqual(native[0], actor.Position.X.Whole, "Real Torizo initial X");
                AssertEqual(native[1], actor.Position.Y.Whole, "Real Torizo initial Y");
                TorizoEnemyState started = actor.EntityOf<TorizoEntity>().State;
                AssertEqual(native[2], TorizoNativeListAddresses.Entry(TorizoAnimations.Playing(started.BodyPlayback)), "Real Torizo initial program selection");
                AssertEqual(TorizoAnimations.Start(TorizoAnimations.Playing(started.BodyPlayback)), started.BodyPlayback, "Real Torizo starts its initial animation");
                EnemyPropertyState initialized = EnemyPropertyWordCodec.Decode(properties | native[3], 0);
                AssertEqual(initialized.Contact, actor.Contact, "All population contact preserved by native OR");
                AssertEqual(initialized.Processing, actor.Processing, "All population processing preserved by native OR");
                AssertEqual(initialized.Presence, actor.Presence, "All population presence preserved by native OR");
                AssertEqual(native[4], actor.Radius.X, "Real Torizo X hitbox radius");
                AssertEqual(native[5], actor.Radius.Y, "Real Torizo Y hitbox radius");
                AssertTrue(actor.CollidesByFrameHitboxes, "Torizo initialization collides by frame hitboxes");
                // The population places a Torizo on whole pixels, which his initialization keeps beneath his wake position.
                AssertEqual((ushort)0, actor.Position.X.Fraction, "Torizo initialization preserves X fraction");
                AssertEqual((ushort)0, actor.Position.Y.Fraction, "Torizo initialization preserves Y fraction");
                AssertEqual(actor.Definition.Health, actor.Health, "Torizo initialization preserves population health");
                AssertEqual(1, actor.AnimationClock.Hold, "Initial instruction timer");
                AssertEqual((ushort)0, started.Velocity.X.RawValue, "Initial horizontal velocity");
                AssertEqual(started.Colours.Shading, actor.Shading, "Initial colours: the shading his colours name for his starting look");
                AssertTrue(!actor.Shading.IsStandard, "Initial colours: his starting look is a shade over his sheet");
                AssertEqual((ushort)0x100, started.Velocity.Y.RawValue, "Initial falling velocity");
            }
            RoomEnemyPlace beaten = initialize(defeated: true, PopulationPlacementAccess.Entry(species, new(0x1111, 0x2222)) with
            {
                Contact = EnemyContact.BlocksPlasmaBeam | EnemyContact.IgnoresSamus,
            });
            AssertEqual(EnemyContact.BlocksPlasmaBeam | EnemyContact.IgnoresSamus, beaten.Contact, "Defeated Torizo keeps its contact");
            AssertEqual(EnemyProcessing.None, beaten.Processing, "Defeated Torizo gains no processing");
            AssertEqual(EnemyPresence.Deleted, beaten.Presence, "Defeated Torizo is deleted before initialization writes");
            AssertEqual((ushort)0x1111, beaten.Position.X.Whole, "Defeated Torizo preserves X");
            AssertEqual((ushort)0x2222, beaten.Position.Y.Whole, "Defeated Torizo preserves Y");
            AssertEqual(default(AnimationPlayback<TorizoBodySegment>), beaten.EntityOf<TorizoEntity>().State.BodyPlayback,
                "Defeated Torizo leaves its animation untouched");
        }
        Console.WriteLine("Torizo initialization: all 12 native words, 131072 real live initializations and both defeated early returns pass without ROM reads.");
    }
}

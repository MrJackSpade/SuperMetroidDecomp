using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Torizo;

using SuperMetroid.Verification.Harness;

/// <summary>Checks both Torizo wake records and their use by the actual enemy initializer.</summary>
internal static class TorizoInitializationDefinitionsVerification
{
    /// <summary>Matches twelve compiled record words to ROM, then verifies live initialization preserves population state and skips defeated actors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Torizo initialization")]
    internal static void VerifyCompiledTorizoInitialization(SuperMetroidAddressSpace rom)
    {
        for (int variant = 0; variant < 2; variant++)
        {
            ushort[] native = [CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeXPositions + variant * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeYPositions + variant * 2),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeInstructionLists + variant * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakePropertyMasks + variant * 2),
                CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeXRadii + variant * 2), CartridgeImportSource.Require(rom).ReadCartridgeWord(EnemyRomTablePointersTorizo.WakeYRadii + variant * 2)];
            var compiled = variant == 0 ? TorizoInitializationDefinitions.Bomb : TorizoInitializationDefinitions.Golden;
            ushort[] actual = [compiled.Position.X, compiled.Position.Y, TorizoNativeListAddresses.Entry(compiled.Program), (ushort)compiled.PropertyMask, compiled.Radius.X, compiled.Radius.Y];
            AssertTrue(native.SequenceEqual(actual), "Every Torizo initialization record word matches ROM");

            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.Presentation.BindCgram(new SnesCgram());
            bool defeated = false;
            enemies.VerificationServices().IsAreaTorizoDefeated = () => defeated;
            Action<RoomEnemySlot, SamusState?, SnesButton> initializeWithInput = enemies.BombTorizoBehavior.Movement.InitializeBombTorizo;
            void initialize(RoomEnemySlot slot) => initializeWithInput(slot, null, 0);
            var actor = enemies.SlotTable.Slots[0];
            actor.Bind(variant == 0 ? EnemyDefinitionId.BombTorizo : EnemyDefinitionId.GoldenTorizo, actor.Definition);
            for (int properties = 0; properties <= ushort.MaxValue; properties++)
            {
                actor.Properties = EnemyPropertyWord.FromNativeWord((ushort)properties);
                actor.ExtraProperties = EnemyExtraPropertyWord.FromNativeWord((ushort)~properties);
                actor.Radius = new(0xffff, 0xffff);
                actor.Position = new(new(0xffff, 0x1234), new(0xffff, 0xabcd));
                actor.Health = 0x3456;
                actor.PaletteIndex = 0xffff;
                actor.Timer = NativeCountdown.Start(0xffff);
                actor.InstructionTimer = NativeCountdown.Start(0xffff);
                actor.CurrentInstruction = 0xffff;
                initialize(actor);
                AssertEqual(native[0], actor.Position.X.Whole, "Real Torizo initial X");
                AssertEqual(native[1], actor.Position.Y.Whole, "Real Torizo initial Y");
                TorizoEnemyState started = enemies.BombTorizoBehavior.Access.TorizoState
                    ?? throw new InvalidOperationException("Verification requires the initialized Torizo state.");
                AssertEqual(native[2], TorizoNativeListAddresses.Entry(started.BodyProgram.Program), "Real Torizo initial program selection");
                AssertEqual(AnimationProgramCursor<TorizoBodyProgram>.Start(started.BodyProgram.Program), started.BodyProgram, "Real Torizo starts its initial program");
                AssertEqual((ushort)(properties | native[3]), actor.Properties.ToNativeWord(), "All population property bits preserved by native OR");
                AssertEqual(native[4], actor.Radius.X, "Real Torizo X hitbox radius");
                AssertEqual(native[5], actor.Radius.Y, "Real Torizo Y hitbox radius");
                AssertEqual((ushort)((ushort)~properties | (ushort)EnemyExtraProperties.UsesExtendedSpritemap), actor.ExtraProperties.ToNativeWord(), "Extended map flag preserves population extra properties");
                AssertEqual((ushort)0x1234, actor.Position.X.Fraction, "Torizo initialization preserves X fraction");
                AssertEqual((ushort)0xabcd, actor.Position.Y.Fraction, "Torizo initialization preserves Y fraction");
                AssertEqual(0x3456, actor.Health, "Torizo initialization preserves population health");
                AssertEqual((ushort)1, actor.InstructionTimer.RawValue, "Initial instruction timer");
                AssertEqual((ushort)0, actor.Timer.RawValue, "Initial actor timer");
                AssertEqual(0, actor.PaletteIndex, "Initial palette index");
                var state = variant == 0 ? enemies.BombTorizo : enemies.BombTorizoBehavior.Attacks.GoldenTorizo;
                AssertEqual((ushort)0, (state ?? throw new InvalidOperationException("Verification requires state.")).Velocity.X.RawValue, "Initial horizontal velocity");
                AssertEqual((ushort)0x100, state.Velocity.Y.RawValue, "Initial falling velocity");
            }
            defeated = true;
            actor.Properties = EnemyPropertyWord.FromNativeWord(0x1234);
            actor.Position = actor.Position.WithWhole(new(0x1111, 0x2222));
            actor.CurrentInstruction = 0x3333;
            initialize(actor);
            AssertEqual((ushort)(0x1234 | (ushort)EnemyProperties.Deleted), actor.Properties.ToNativeWord(), "Defeated Torizo is deleted before initialization writes");
            AssertEqual((ushort)0x1111, actor.Position.X.Whole, "Defeated Torizo preserves X");
            AssertEqual((ushort)0x2222, actor.Position.Y.Whole, "Defeated Torizo preserves Y");
            AssertEqual(0x3333, actor.CurrentInstruction, "Defeated Torizo preserves instruction");
        }
        Console.WriteLine("Torizo initialization: all 12 native words, 131072 real live initializations and both defeated early returns pass without ROM reads.");
    }
}

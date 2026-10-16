using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Cacatac's idle loops and the sound, spikes and patrol hand-off of its attacks.</summary>
internal static class CacatacBehaviorVerification
{
    /// <summary>
    /// The real initializer starts the upright or inverted idle animation, whose cue restores patrol and whose eight
    /// poses loop on time; each attack plays the spike sound, spits its five spikes in their authored directions and
    /// returns to the idle loop with patrol restored.
    /// </summary>
    internal static void VerifyCacatacBehavior(SuperMetroidAddressSpace rom)
    {
        foreach (bool upsideUp in new[] { true, false })
        {
            string orientation = upsideUp ? "upright" : "inverted";
            CacatacProgram idle = upsideUp ? CacatacProgram.UpsideUpIdle : CacatacProgram.UpsideDownIdle;
            CacatacProgram loop = upsideUp ? CacatacProgram.UpsideUpIdle : CacatacProgram.UpsideDownIdleLoop;
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) = Create(rom, upsideUp);
            AssertEqual(AnimationProgramCursor<CacatacProgram>.Start(idle), state.Program, $"Cacatac {orientation} starts idling");
            state.Function = CacatacEnemyFunction.Stopped;
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(CacatacAnimations.Programs, loop, CacatacFrameDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(CacatacAnimations.Programs, loop)), $"Cacatac {orientation} idle loop");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function, $"Cacatac {orientation} idle restores patrol");

            (enemies, slot, state) = Create(rom, upsideUp);
            state.Function = CacatacEnemyFunction.Stopped;
            CacatacBehavior.SetCacatacProgram(slot, state, upsideUp ? CacatacProgram.UpsideUpAttack : CacatacProgram.UpsideDownAttack);
            EnemyInstructionRunner.ShownFrames(slot, 53);
            AssertEqual(loop, state.Program.Program, $"Cacatac {orientation} attack returns to idle");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function, $"Cacatac {orientation} attack resumes patrol");
            AssertEqual((ushort?)CacatacNativeValues.SpikeSound, enemies.CacatacBehavior.LastCacatacSoundEffect, $"Cacatac {orientation} attack plays the spike sound");
            CacatacSpikeDirection[] directions = upsideUp
                ? [CacatacSpikeDirection.LeftFacingUp, CacatacSpikeDirection.UpLeft, CacatacSpikeDirection.Up, CacatacSpikeDirection.UpRight, CacatacSpikeDirection.RightFacingUp]
                : [CacatacSpikeDirection.LeftFacingDown, CacatacSpikeDirection.DownLeft, CacatacSpikeDirection.Down, CacatacSpikeDirection.DownRight, CacatacSpikeDirection.RightFacingDown];
            AssertEqual(string.Join(",", directions.Select(direction => (ushort)direction).Order()),
                string.Join(",", enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
                    .Select(projectile => projectile.Variable0).Order()),
                $"Cacatac {orientation} spike directions");
        }
        Console.WriteLine("  Cacatac behavior: both idles restore patrol and loop on time; both attacks play the sound, spit five spikes and resume patrol.");
    }

    /// <summary>A Cacatac initialized upright or inverted.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, CacatacEnemyState State) Create(SuperMetroidAddressSpace rom, bool upsideUp)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(rom);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = CacatacFrameToolingDefinitions.Bank });
        slot.Parameter1 = upsideUp ? CacatacNativeValues.UpsideUpParameter : (ushort)0;
        CacatacBehavior.InitializeCacatac(slot);
        return (enemies, slot, enemies.CacatacStates[0] ?? throw new InvalidOperationException("Cacatac has no state."));
    }

    /// <summary>Runs the Cacatac behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--cacatac-instruction-mechanics")]
    internal static int CacatacBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCacatacBehavior), () => VerifyCacatacBehavior(rom));
        Suite(nameof(CacatacAnimationsVerification.VerifyCacatacAnimations), () => CacatacAnimationsVerification.VerifyCacatacAnimations(rom));
        return 0;
    }
}

using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
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
    internal static void VerifyCacatacBehavior()
    {
        foreach (bool upsideUp in new[] { true, false })
        {
            string orientation = upsideUp ? "upright" : "inverted";
            CacatacProgram idle = upsideUp ? CacatacProgram.UpsideUpIdle : CacatacProgram.UpsideDownIdle;
            CacatacProgram loop = upsideUp ? CacatacProgram.UpsideUpIdle : CacatacProgram.UpsideDownIdleLoop;
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) = Create(upsideUp);
            AssertEqual(AnimationProgramCursor<CacatacProgram>.Start(idle), state.Program, $"Cacatac {orientation} starts idling");
            state.Function = CacatacEnemyFunction.Stopped;
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(CacatacAnimations.Programs, loop, CacatacFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(CacatacAnimations.Programs, loop)), $"Cacatac {orientation} idle loop");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function, $"Cacatac {orientation} idle restores patrol");

            (enemies, slot, state) = Create(upsideUp);
            state.Function = CacatacEnemyFunction.Stopped;
            CacatacBehavior.SetCacatacProgram(slot, state, upsideUp ? CacatacProgram.UpsideUpAttack : CacatacProgram.UpsideDownAttack);
            EnemyInstructionRunner.ShownFrames(slot, 53);
            AssertEqual(loop, state.Program.Program, $"Cacatac {orientation} attack returns to idle");
            AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function, $"Cacatac {orientation} attack resumes patrol");
            AssertEqual<SoundEffectId?>(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, CacatacNativeValues.SpikeSound), enemies.CacatacBehavior.LastCacatacSoundEffect, $"Cacatac {orientation} attack plays the spike sound");
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

    /// <summary>
    /// Drives the production main AI on a Cacatac whose patrol limits are its spawn X plus and minus one block. The
    /// typed 16.16 velocity carries its fraction into the whole X, and each turn follows the wrapped signed distance
    /// to the limit: spawned eight pixels from the room's left edge, its left limit wraps to the far right of the
    /// coordinate range, yet the walk keeps going left until X passes that limit through the wrap, and a rightward
    /// walk that lands exactly on its right limit turns back.
    /// </summary>
    internal static void VerifyCacatacPatrolLimits()
    {
        const int onePixel = 65536;
        const int halfPixel = onePixel / 2;
        var oneAndAHalfPixels = SnesSignedSixteenSixteen.FromRaw(onePixel + halfPixel);
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        // A random word that keeps the three-in-256 attack gate closed, so every update is a patrol step.
        enemies.VerificationServices().NextRandom = () => 100;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
        slot.Position = FixedPosition<RoomPixels>.FromWhole(new(8, 112));
        CacatacBehavior.InitializeCacatac(slot);
        CacatacEnemyState state = enemies.CacatacStates[0] ?? throw new InvalidOperationException("Cacatac has no state.");
        AssertEqual(new Position16<RoomPixels>(unchecked((ushort)(8 - 16)), 112), state.LeftPatrolLimit, "Cacatac left limit wraps below the room's left edge");
        AssertEqual(new Position16<RoomPixels>(24, 112), state.RightPatrolLimit, "Cacatac right limit is one block right of spawn");

        state.LeftVelocity = SnesSignedSixteenSixteen.Zero - oneAndAHalfPixels;
        slot.Position = slot.Position.WithFractionX(checked(halfPixel));
        enemies.CacatacBehavior.RunCacatacMainAi(slot, Frame());
        AssertEqual(new SnesFixedPosition(7, 0), slot.Position.X, "Cacatac leftward step borrows its fraction");
        AssertEqual(CacatacDirection.Left, state.Direction, "Cacatac keeps walking left of a limit that wrapped to the far right");
        AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function, "Cacatac keeps its leftward patrol");

        // One pixel right of the wrapped limit: the step lands half a pixel past it and turns.
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(state.LeftPatrolLimit.X + 1)));
        enemies.CacatacBehavior.RunCacatacMainAi(slot, Frame());
        AssertEqual(new SnesFixedPosition(unchecked((ushort)(state.LeftPatrolLimit.X - 1)), checked(halfPixel)), slot.Position.X,
            "Cacatac steps past its wrapped left limit");
        AssertEqual(CacatacDirection.Right, state.Direction, "Cacatac turns right once past its wrapped left limit");
        AssertEqual(CacatacEnemyFunction.MovingRight, state.Function, "Cacatac patrols right after the turn");

        state.RightVelocity = oneAndAHalfPixels;
        slot.Position = slot.Position.WithX(new SnesFixedPosition(22, checked(halfPixel)));
        enemies.CacatacBehavior.RunCacatacMainAi(slot, Frame());
        AssertEqual(new SnesFixedPosition(24, 0), slot.Position.X, "Cacatac rightward step carries its fraction into the whole X");
        AssertEqual(CacatacDirection.Left, state.Direction, "Cacatac turns left on reaching its right limit");
        AssertEqual(CacatacEnemyFunction.MovingLeft, state.Function, "Cacatac patrols left after the turn");
        Console.WriteLine("  Cacatac patrol: the 16.16 steps carry and borrow, and both turns follow the wrapped distance to the limit.");

        static ClockedEnemyAiFrame Frame() => new(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState() },
            new FrameCounters(0));
    }

    /// <summary>A Cacatac initialized upright or inverted.</summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Slot, CacatacEnemyState State) Create(bool upsideUp)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with { Bank = CacatacFrameToolingDefinitions.Bank });
        slot.SetSpawnParameter1(upsideUp ? CacatacNativeValues.UpsideUpParameter : (ushort)0);
        CacatacBehavior.InitializeCacatac(slot);
        return (enemies, slot, enemies.CacatacStates[0] ?? throw new InvalidOperationException("Cacatac has no state."));
    }

    /// <summary>Runs the Cacatac behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--cacatac-instruction-mechanics")]
    internal static int CacatacBehaviorCommand()
    {
        SuperMetroidAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCacatacBehavior), () => VerifyCacatacBehavior());
        Suite(nameof(VerifyCacatacPatrolLimits), () => VerifyCacatacPatrolLimits());
        Suite(nameof(CacatacAnimationsVerification.VerifyCacatacAnimations), () => CacatacAnimationsVerification.VerifyCacatacAnimations(rom));
        return 0;
    }
}

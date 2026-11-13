using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Enemies.Cacatac;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

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
            CacatacSegment idle = upsideUp ? CacatacSegment.UpsideUpIdle : CacatacSegment.UpsideDownIdle;
            CacatacSegment loop = upsideUp ? CacatacSegment.UpsideUpIdle : CacatacSegment.UpsideDownIdleLoop;
            (RoomEnemySystem enemies, RoomEnemySlot slot, CacatacEnemyState state) = Create(upsideUp);
            AssertEqual(new AnimationPlayback<CacatacSegment>(CacatacAnimations.StartingWith(idle), AnimationPlace.StartOfAnimation), state.Playback, $"Cacatac {orientation} starts idling");
            state.Action = CacatacAction.Stopped;
            AssertEqual(EnemyInstructionRunner.ExpectedLoop(CacatacAnimations.Segments, loop, CacatacFrameToolingDefinitions.Spritemap),
                EnemyInstructionRunner.ShownFrames(slot, EnemyInstructionRunner.LoopUpdates(CacatacAnimations.Segments, loop)), $"Cacatac {orientation} idle loop");
            AssertEqual(CacatacAction.MovingLeft, state.Action, $"Cacatac {orientation} idle restores patrol");

            (enemies, slot, state) = Create(upsideUp);
            state.Action = CacatacAction.Stopped;
            CacatacBehavior.SetCacatacAnimation(slot, state, upsideUp ? CacatacSegment.UpsideUpAttack : CacatacSegment.UpsideDownAttack);
            EnemyInstructionRunner.ShownFrames(slot, 53);
            AssertEqual(loop, state.Playback.Animation.Phases[state.Playback.Place.Phase].Segment, $"Cacatac {orientation} attack returns to idle");
            AssertEqual(CacatacAction.MovingLeft, state.Action, $"Cacatac {orientation} attack resumes patrol");
            AssertEqual<SoundEffectId?>(CacatacNativeValues.SpikeSound, enemies.Audio.LatestSound, $"Cacatac {orientation} attack plays the spike sound");
            CacatacSpikeDirection[] directions = upsideUp
                ? [CacatacSpikeDirection.LeftFacingUp, CacatacSpikeDirection.UpLeft, CacatacSpikeDirection.Up, CacatacSpikeDirection.UpRight, CacatacSpikeDirection.RightFacingUp]
                : [CacatacSpikeDirection.LeftFacingDown, CacatacSpikeDirection.DownLeft, CacatacSpikeDirection.Down, CacatacSpikeDirection.DownRight, CacatacSpikeDirection.RightFacingDown];
            AssertEqual(string.Join(",", directions.Select(direction => CacatacSpikes.Velocity(direction).ToString()).Order()),
                string.Join(",", enemies.Projectiles.EnemyProjectiles.Where(projectile => projectile.Kind == RoomEnemyProjectileKind.CacatacSpike)
                    .Select(projectile => projectile.Velocity.ToString()).Order()),
                $"Cacatac {orientation} spikes fly at their directions' velocities");
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
        var oneAndAHalfPixels = SixteenSixteenWords.ToDisplacement(onePixel + halfPixel);
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        // A random word that keeps the three-in-256 attack gate closed, so every update is a patrol step.
        enemies.VerificationServices().NextRandom = () => 100;
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
        slot.Position = FixedPosition<RoomPixels>.FromWhole(new(8, 112));
        slot.SetSpawnWords(EnemyDefinitionId.Cacatac, words => words);
        CacatacBehavior.InitializeCacatac(slot);
        CacatacEnemyState state = enemies.CacatacStates[0] ?? throw new InvalidOperationException("Cacatac has no state.");
        AssertEqual(new Position16<RoomPixels>(unchecked((ushort)(8 - 16)), 112), state.LeftPatrolLimit, "Cacatac left limit wraps below the room's left edge");
        AssertEqual(new Position16<RoomPixels>(24, 112), state.RightPatrolLimit, "Cacatac right limit is one block right of spawn");

        state.LeftVelocity = SignedSixteenSixteen.Zero - oneAndAHalfPixels;
        slot.Position = slot.Position.WithFractionX(checked(halfPixel));
        enemies.CacatacBehavior.RunCacatacMainAi(slot, Frame());
        AssertEqual(new FixedCoordinate(7, 0), slot.Position.X, "Cacatac leftward step borrows its fraction");
        AssertEqual(CacatacDirection.Left, state.Direction, "Cacatac keeps walking left of a limit that wrapped to the far right");
        AssertEqual(CacatacAction.MovingLeft, state.Action, "Cacatac keeps its leftward patrol");

        // One pixel right of the wrapped limit: the step lands half a pixel past it and turns.
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(state.LeftPatrolLimit.X + 1)));
        enemies.CacatacBehavior.RunCacatacMainAi(slot, Frame());
        AssertEqual(new FixedCoordinate(unchecked((ushort)(state.LeftPatrolLimit.X - 1)), checked(halfPixel)), slot.Position.X,
            "Cacatac steps past its wrapped left limit");
        AssertEqual(CacatacDirection.Right, state.Direction, "Cacatac turns right once past its wrapped left limit");
        AssertEqual(CacatacAction.MovingRight, state.Action, "Cacatac patrols right after the turn");

        state.RightVelocity = oneAndAHalfPixels;
        slot.Position = slot.Position.WithX(new FixedCoordinate(22, checked(halfPixel)));
        enemies.CacatacBehavior.RunCacatacMainAi(slot, Frame());
        AssertEqual(new FixedCoordinate(24, 0), slot.Position.X, "Cacatac rightward step carries its fraction into the whole X");
        AssertEqual(CacatacDirection.Left, state.Direction, "Cacatac turns left on reaching its right limit");
        AssertEqual(CacatacAction.MovingLeft, state.Action, "Cacatac patrols left after the turn");
        Console.WriteLine("  Cacatac patrol: the 16.16 steps carry and borrow, and both turns follow the wrapped distance to the limit.");

        static ClockedEnemyAiFrame Frame() => new(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) },
            new FrameCounters(0));
    }

    /// <summary>A Cacatac initialized upright or inverted.</summary>
    private static CacatacBehaviorCreateResult Create(bool upsideUp)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Cacatac, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SetSpawnParameter1(EnemyDefinitionId.Cacatac, upsideUp ? CacatacNativeValues.UpsideUpParameter : (ushort)0);
        CacatacBehavior.InitializeCacatac(slot);
        return new CacatacBehaviorCreateResult(enemies, slot, enemies.CacatacStates[0] ?? throw new InvalidOperationException("Cacatac has no state."));
    }

    /// <summary>Runs the Cacatac behavior check and its program oracle against the local retail cartridge.</summary>
    [VerificationCommand("--cacatac-instruction-mechanics")]
    internal static int CacatacBehaviorCommand()
    {
        CartridgeImportAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCacatacBehavior), () => VerifyCacatacBehavior());
        Suite(nameof(VerifyCacatacPatrolLimits), () => VerifyCacatacPatrolLimits());
        Suite(nameof(CacatacAnimationsVerification.VerifyCacatacAnimations), () => CacatacAnimationsVerification.VerifyCacatacAnimations(rom));
        return 0;
    }
}

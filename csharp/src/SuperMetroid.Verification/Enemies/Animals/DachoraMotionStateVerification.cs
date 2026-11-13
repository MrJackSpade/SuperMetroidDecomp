using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Dachora's typed speed, acceleration, echo lifetime and palette cycle keep the cartridge's word arithmetic in the
/// slot variables they view.
/// </summary>
internal static class DachoraMotionStateVerification
{
    /// <summary>Samus's air gravity fraction: 7168 sixty-five-thousand-five-hundred-thirty-sixths of a pixel per update.</summary>
    private const ushort AirGravityFraction = 7168;

    /// <summary>
    /// Runs the production speed and fall paths. At the run speed cap, a cleared palette cycle counted down borrows
    /// from its frame as one word does, becoming frame 255 with 255 ticks and loading no palette. Falling, gravity
    /// carries the fraction into the whole word, a negative whole speed included, and a whole speed reaching ten is
    /// replaced by exactly ten pixels with no fraction. Every result is checked against the slot variables, computed
    /// independently as 16-bit words.
    /// </summary>
    internal static void VerifyDachoraMotionState()
    {
        RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(16, 16));

        (RoomEnemySystem enemies, RoomEnemySlot slot, DachoraEnemyState state) = NewDachora(parameter: 1);
        state.Speed = SignedSixteenSixteen.FromParts(8, 0);
        state.SetGlowCycle(frame: 0, ticks: 0);
        enemies.DachoraBehavior.AccelerateDachora(slot, state, level);
        AssertEqual(255, state.GlowFrame, "a cleared palette cycle's tick borrows from its frame");
        AssertEqual(255, state.GlowTicks, "a cleared palette cycle's ticks wrap");
        AssertEqual(SignedSixteenSixteen.FromParts(8, 0), state.Speed, "the run speed stays at its cap");

        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, AirGravityFraction);
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(level, 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
            new FrameCounters(0));
        foreach ((short whole, ushort fraction, ushort expectedWhole, ushort expectedFraction) in new VerifyDachoraMotionStateCase[]
                 {
                     new VerifyDachoraMotionStateCase(9, 61440, 10, 0),
                     new VerifyDachoraMotionStateCase(-1, 61440, 0, 3072),
                     new VerifyDachoraMotionStateCase(3, 0, 3, AirGravityFraction),
                 })
        {
            (RoomEnemySystem fallEnemies, RoomEnemySlot fallSlot, DachoraEnemyState fall) = NewDachora(parameter: 1);
            fall.Function = DachoraAiFunction.Falling;
            fall.Speed = SignedSixteenSixteen.FromParts(whole, fraction);
            fallSlot.Position = fallSlot.Position.WithWhole(new(64, 32));

            fallEnemies.DachoraBehavior.RunDachoraMainAi(fallSlot, frame);

            string context = $"Dachora falling at {whole} and {fraction}/65536 pixels";
            AssertEqual(expectedWhole, fall.Speed.UnsignedWholePixels, $"{context}: whole speed word");
            AssertEqual(expectedFraction, fall.Speed.Fraction, $"{context}: speed fraction word");
            AssertEqual(SignedSixteenSixteen.FromParts(unchecked((short)expectedWhole), expectedFraction), fall.Speed, context);
        }

        Console.WriteLine("  Dachora motion state: the palette cycle borrows as one word; gravity carries and the fall cap replaces the speed.");
    }

    /// <summary>
    /// Runs the production echo AI. An echo with updates left counts one off and stays visible on its frame parity;
    /// an echo with none left stays at zero, never wrapping, and is hidden.
    /// </summary>
    internal static void VerifyDachoraEchoLifetime()
    {
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach ((int lifetime, ushort expected, bool invisible) in new VerifyDachoraEchoLifetimeCase[] { new VerifyDachoraEchoLifetimeCase(8, 7, false), new VerifyDachoraEchoLifetimeCase(1, 0, false), new VerifyDachoraEchoLifetimeCase(0, 0, true) })
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, DachoraEnemyState state) = NewDachora(parameter: ushort.MaxValue);
            AssertEqual(DachoraAiFunction.Echo, state.Function, "a negative parameter makes an echo");
            state.EchoLifetime = lifetime;

            enemies.DachoraBehavior.RunDachoraMainAi(slot, frame);

            AssertEqual(expected, state.EchoLifetime, $"echo lifetime {lifetime} counts down without wrapping");
            AssertEqual(invisible, slot.Presence.HasAny(EnemyPresence.Hidden), $"echo lifetime {lifetime} visibility");
        }
        Console.WriteLine("  Dachora echo lifetime: counts down to zero and stays there, hiding the echo.");
    }

    /// <summary>An initialized Dachora with spawn parameter <paramref name="parameter"/> in the first slot of a fresh room.</summary>
    private static NewDachoraResult NewDachora(ushort parameter)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.Dachora, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.Dachora, parameter);
        slot.Radius = new(8, 8);
        enemies.DachoraBehavior.InitializeDachora(slot);
        return new NewDachoraResult(enemies, slot, slot.RequireSpeciesState<DachoraEnemyState>());
    }

    /// <summary>Runs Dachora's motion-state and echo-lifetime checks.</summary>
    [VerificationCommand("--dachora-motion-state")]
    internal static int DachoraMotionStateCommand()
    {
        Suite(nameof(VerifyDachoraMotionState), () => VerifyDachoraMotionState());
        Suite(nameof(VerifyDachoraEchoLifetime), () => VerifyDachoraEchoLifetime());
        return 0;
    }
}

using SuperMetroid.Core.Enemies;
using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Draygon intro start delays and the traced path against ROM and the full Evir trajectory.</summary>
/// <remarks>
/// The cartridge keeps each Evir's delay as a negative byte distance into a path of four-byte records; the game holds
/// delays in updates and the path as typed steps. This oracle owns that translation.
/// </remarks>
internal static class DraygonIntroDanceDefinitionsVerification
{
    /// <summary>The cartridge record of path step <paramref name="step"/>: its X and Y bytes.</summary>
    private static NativeIntroRecord ReadRecord(CartridgeImportAddressSpace rom, int step)
    {
        int address = DraygonIntroDanceDefinitionsConstants.NativeMovementStreamAddress +
            step * DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes;
        return new(rom.ReadCartridgeByte(address), rom.ReadCartridgeByte(address + 1));
    }

    /// <summary>Runs delay, path, and complete production trajectory comparisons.</summary>
    internal static void VerifyDraygonIntroDanceDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyDraygonIntroLatencyDefinitions), () => VerifyDraygonIntroLatencyDefinitions(rom));
        Suite(nameof(VerifyDraygonIntroMovementDefinitions), () => VerifyDraygonIntroMovementDefinitions(rom));
        Suite(nameof(VerifyCompleteDraygonIntroTrajectory), () => VerifyCompleteDraygonIntroTrajectory(rom));

        Console.WriteLine(
            "Draygon intro dance definitions: all four start delays and all 1,007 " +
            "path steps the Evirs take match the pinned ROM; the complete 1,232-frame " +
            "production trajectory passes.");
    }

    /// <summary>
    /// The native sprite slot the Evir spawned <paramref name="dancer"/>-th takes, which indexes its native start
    /// latency: spawns fill the free slots from slot 31 down.
    /// </summary>
    private static int NativeDancerSlot(int dancer) => 31 - dancer;

    /// <summary>Compares all four Evir start delays with the native byte latencies and checks the dancer bounds.</summary>
    internal static void VerifyDraygonIntroLatencyDefinitions(CartridgeImportAddressSpace rom)
    {
        const int source = DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        for (int dancer = 0; dancer < DraygonIntroDanceDefinitions.DancerStartDelays.Count; dancer++)
        {
            int slotIndex = NativeDancerSlot(dancer);
            int address = source + (slotIndex - 28) * 2;
            short native = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            AssertEqual(native % DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes, 0,
                $"Draygon intro Evir slot {slotIndex} latency is whole records");
            AssertEqual(-native / DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes,
                DraygonIntroDanceDefinitions.DancerStartDelays[dancer],
                $"Draygon intro Evir {dancer} (native slot {slotIndex}) start delay");
        }
        Console.WriteLine("Evir start delays: all four native latencies match.");
    }

    /// <summary>
    /// Compares every move of the traced path with the cartridge's and its end with the cartridge's delete record (#2464):
    /// the path's moves are the records before the cartridge's first delete, which every Evir takes before the dance ends.
    /// </summary>
    internal static void VerifyDraygonIntroMovementDefinitions(CartridgeImportAddressSpace rom)
    {
        for (int step = 0; step <= DraygonIntroDanceDefinitions.TracedPath.Length; step++)
        {
            NativeIntroRecord native = ReadRecord(rom, step);
            Offset16<RoomPixels> actual = DraygonIntroDanceDefinitions.TracedPath[step];
            AssertEqual(false, IsDelete(native), $"Draygon intro step {step} moves");
            AssertEqual(unchecked((sbyte)native.X), actual.X, $"Draygon intro step {step} X delta");
            AssertEqual(unchecked((sbyte)native.Y), actual.Y, $"Draygon intro step {step} Y delta");
        }
        AssertEqual(true, IsDelete(ReadRecord(rom, DraygonIntroDanceDefinitions.TracedPath.Length)),
            "Draygon intro path ends at the cartridge's delete record");

        // The Evir that waits longest still takes the step after the path's last move, its delete, before the dance ends.
        int longestDelay = LongestEvirStartDelay();
        AssertEqual(true, DraygonIntroDanceDefinitions.DurationFrames - 1 - longestDelay >= DraygonIntroDanceDefinitions.TracedPath.Length,
            "Every Draygon intro Evir reaches the path's delete its dance");
    }

    /// <summary>Whether a cartridge intro path record is the delete record that ends an Evir's dance.</summary>
    private static bool IsDelete(NativeIntroRecord record) =>
        record.X == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte &&
        record.Y == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte;

    /// <summary>Compares every Evir slot's position and lifetime across the full intro, including the fight handoff.</summary>
    private static void VerifyCompleteDraygonIntroTrajectory(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        Action<DraygonEnemyState, SamusState, byte> run = (danceState, danceSamus, frameCounter) =>
            enemies.DraygonBehavior.Intro.RunDraygonIntroDance(danceState, danceSamus, frameCounter);
        // The dance starts its own stage, as the intro's handoff does.
        var state = new DraygonEnemyState(enemies.Places.InUpdateOrder[0].BoundFor(EnemyDefinitionId.DraygonBody), enemies.Places.InUpdateOrder[1], enemies.Places.InUpdateOrder[2], enemies.Places.InUpdateOrder[3])
        {
            IntroStage = UpdateCountdown.Lasting(DraygonIntroDanceDefinitions.DurationFrames),
        };

        ushort[] expectedX = new ushort[4];
        ushort[] expectedY = new ushort[4];
        bool[] expectedActive = new bool[4];
        // Each dancer is the Evir spawned in its turn; the native slot it would take indexes its expected values.
        var dancers = new RoomEffectActor[DraygonIntroDanceDefinitions.DancerStartDelays.Count];
        for (int dancer = 0; dancer < dancers.Length; dancer++)
        {
            dancers[dancer] = enemies.EffectActors.SpawnRoomEffectActor(new(0x0100, 0x0180), RoomEffectActorKind.EvirFacingLeft,
                SuperMetroid.Core.Enemies.SpriteGraphics.SharedIn(DraygonColours.Evirs));
            int actor = NativeDancerSlot(dancer) - 28;
            expectedX[actor] = 0x0100;
            expectedY[actor] = 0x0180;
            expectedActive[actor] = true;
            state.IntroDancers.Add(new DraygonIntroDancer(dancers[dancer], DraygonIntroDanceDefinitions.DancerStartDelays[dancer]));
        }

        const int latencySource = DraygonIntroDanceDefinitionsConstants.NativeMovementLatencyAddress;
        int expectedStreamOffset = 0;
        for (int frame = 0; frame < DraygonIntroDanceDefinitions.DurationFrames; frame++)
        {
            for (int slotIndex = 31; slotIndex >= 28; slotIndex--)
            {
                int actor = slotIndex - 28;
                short latency = unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(latencySource + actor * 2));
                int streamOffset = expectedStreamOffset + latency;
                if (streamOffset < 0 || !expectedActive[actor])
                    continue;

                NativeIntroRecord record = ReadRecord(rom, streamOffset / DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes);
                if (record.X == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte &&
                    record.Y == DraygonIntroDanceDefinitionsConstants.NativeDeleteByte)
                {
                    expectedActive[actor] = false;
                    continue;
                }

                expectedX[actor] = unchecked((ushort)(expectedX[actor] + unchecked((sbyte)record.X)));
                expectedY[actor] = unchecked((ushort)(expectedY[actor] + unchecked((sbyte)record.Y)));
            }

            run(state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 1);
            expectedStreamOffset += DraygonIntroDanceDefinitionsConstants.NativeMovementRecordBytes;

            AssertEqual(frame + 1, state.IntroStage.Age, $"Draygon intro frame {frame} dance step");
            for (int slotIndex = 28; slotIndex <= 31; slotIndex++)
            {
                int actor = slotIndex - 28;
                RoomEffectActor actual = dancers[31 - slotIndex];
                AssertEqual(expectedActive[actor], actual.IsActive,
                    $"Draygon intro frame {frame} slot {slotIndex} active state");
                // An Evir the dance deleted has ended; where it stood no longer shows.
                if (!expectedActive[actor])
                    continue;
                AssertEqual(expectedX[actor], actual.Position.X.Whole,
                    $"Draygon intro frame {frame} slot {slotIndex} X");
                AssertEqual(expectedY[actor], actual.Position.Y.Whole,
                    $"Draygon intro frame {frame} slot {slotIndex} Y");
            }
        }

        AssertEqual(DraygonIntroDanceDefinitions.DurationFrames, state.IntroStage.Age,
            "Draygon intro exact native duration");
        run(state, SuperMetroid.Core.Enemies.RoomEnemyFrameFixtures.StandaloneSamus(), 1);
        AssertEqual(DraygonPhase.SwoopSetup, state.Phase,
            "Draygon intro transitions to right-swoop setup after the native duration");
        AssertEqual(HorizontalDirection.Right, state.PassDirection, "Draygon's first swoop flies right (#2165)");
    }


    /// <summary>The longest start delay of the Evirs the dance spawns.</summary>
    private static int LongestEvirStartDelay() => DraygonIntroDanceDefinitions.DancerStartDelays.Max();

    /// <summary>Runs the Draygon intro dance suite through the verification command dispatcher.</summary>
    [VerificationCommand("--draygon-intro-dance-definitions")]
    internal static int DraygonIntroDanceDefinitionsCommand()
    {
        Suite(nameof(VerifyDraygonIntroDanceDefinitions), () => VerifyDraygonIntroDanceDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }

    /// <summary>The two movement bytes of one cartridge intro path record.</summary>
    /// <param name="X">The signed X movement byte.</param>
    /// <param name="Y">The signed Y movement byte.</param>
    private readonly record struct NativeIntroRecord(byte X, byte Y);
}

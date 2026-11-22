using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Enemies.Magdollite;
/// <summary>Checks Magdollite's nine rise/fall phases and overlay offsets.</summary>
internal static class MagdollitePhaseDefinitionsVerification
{
    /// <summary>Compares 27 native words and the live head, body, and overlay consumers.</summary>
    internal static void VerifyMagdollitePhaseDefinitions(CartridgeImportAddressSpace rom)
    {
        const int thresholdTable = 0xa8af55;
        const int instructionTable = 0xa8af67;
        const int overlayOffsetTable = 0xa8af79;

        for (int phaseIndex = 0; phaseIndex < 9; phaseIndex++)
        {
            MagdollitePhaseDefinition phase = MagdolliteRiseStage.At(phaseIndex).Phase;
            AssertEqual(ReadMagdolliteWord(rom, thresholdTable + phaseIndex * 2),
                phase.DistanceThreshold, $"Magdollite threshold {phaseIndex}");
            AssertEqual(ReadMagdolliteWord(rom, instructionTable + phaseIndex * 2),
                NativeEnemyAnimationOracle.EntryOf(MagdolliteNativeListAddresses.BodyEntries, phase.BodyAnimation), $"Magdollite body list {phaseIndex}");
            AssertEqual(ReadMagdolliteWord(rom, overlayOffsetTable + phaseIndex * 2),
                phase.OverlayYOffset, $"Magdollite overlay offset {phaseIndex}");
        }

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        // Each part runs its own main AI: the arm rises or falls as its action has it, and the overlay tracks the rising arm.
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();

        for (int index = 0; index < 3; index++)
        {
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[index];
            place.SetSpawnParameter1(EnemyDefinitionId.Magdollite, (ushort)index);
            place.SetSpawnParameter2(EnemyDefinitionId.Magdollite, 0);
            place.Bind(EnemyDefinitionId.Magdollite, place.KeptDefinition);
            place.Position = place.Position.WithWhole(new(0x0200, 0x0300));
        }
        // The room's population load sets up the three parts in order (#2554).
        _ = enemies.LoadAuthoredPopulation();

        RoomEnemyPlace head = enemies.Places.InUpdateOrder[0];
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[1];
        RoomEnemyPlace overlay = enemies.Places.InUpdateOrder[2];
        var headState = enemies.MagdolliteStates[0] as MagdolliteHeadState ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteStates[0].");
        var bodyState = enemies.MagdolliteStates[1] as MagdolliteBodyState ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteStates[1].");
        var overlayState = enemies.MagdolliteStates[2] as MagdolliteOverlayState ?? throw new InvalidOperationException("Verification requires enemies.MagdolliteStates[2].");
        // Speed record zero holds the arm where the test puts it (#2474).
        AssertEqual(SignedSixteenSixteen.Zero, head.HeldOccupant().SpawnSettings<MagdolliteHeadSpawnSettings>().Speed, "Magdollite speed record zero stands still");

        AssertEqual(MagdolliteRiseStage.Setup.Phase.BodyAnimation,
            bodyState.Playback.PlayingSegment, "Magdollite production initial body list");

        for (int phaseIndex = 0; phaseIndex < 9; phaseIndex++)
        {
            MagdollitePhaseDefinition phase = MagdolliteRiseStage.At(phaseIndex).Phase;
            // The overlay tracks an arm still rising.
            bodyState.Action = MagdolliteBodyAction.Rising;
            bodyState.BodyPhase = MagdolliteRiseStage.At(phaseIndex);
            body.Position = body.Position.WithWholeY(0x0300);
            overlayState.Action = MagdolliteOverlayAction.TrackingRisingBody;
            overlay.RunMainAi(samus);
            AssertEqual(unchecked((ushort)(0x0300 - phase.OverlayYOffset)), overlay.Position.Y.Whole,
                $"Magdollite production overlay Y {phaseIndex}");

            bodyState.Action = MagdolliteBodyAction.Rising;
            bodyState.BodyPhase = MagdolliteRiseStage.At(phaseIndex);
            bodyState.RiseDisplacementY = (phaseIndex < 8
                ? checked((short)-phase.DistanceThreshold)
                : (short)0);
            body.Position = body.Position.WithWholeY(0x0300);
            body.RunMainAi(samus);
            if (phase.DistanceThreshold >= MagdollitePhaseDefinitions.MaximumRise && phaseIndex < 8)
            {
                // Reaching the maximum rise compares against the slot-one overread $20AF
                // instead (see --magdollite-apex-threshold), which holds this phase.
                AssertEqual(MagdolliteRiseStage.At(phaseIndex), bodyState.BodyPhase,
                    $"Magdollite production apex phase {phaseIndex}");
            }
            else if (phaseIndex < 8)
            {
                MagdollitePhaseDefinition next = MagdolliteRiseStage.At(phaseIndex + 1).Phase;
                AssertEqual(MagdolliteRiseStage.At(phaseIndex + 1), bodyState.BodyPhase,
                    $"Magdollite production rise phase {phaseIndex}");
                AssertEqual(next.BodyAnimation, bodyState.Playback.PlayingSegment,
                    $"Magdollite production rise list {phaseIndex}");
            }
            else
            {
                AssertEqual(MagdolliteRiseStage.At(8), bodyState.BodyPhase,
                    "Magdollite terminal phase remains bounded");
            }
        }

        for (int priorPhase = 1; priorPhase < 8; priorPhase++)
        {
            bodyState.Action = MagdolliteBodyAction.Falling;
            bodyState.BodyPhase = MagdolliteRiseStage.At(priorPhase + 1);
            bodyState.RiseDisplacementY = -1;
            body.Position = body.Position.WithWholeY(0x0300);
            body.RunMainAi(samus);
            AssertEqual(MagdolliteRiseStage.At(priorPhase), bodyState.BodyPhase,
                $"Magdollite production fall phase {priorPhase}");
            AssertEqual(MagdolliteRiseStage.At(priorPhase).Phase.BodyAnimation,
                bodyState.Playback.PlayingSegment,
                $"Magdollite production fall list {priorPhase}");
        }

        AssertThrows<InvalidDataException>(() => _ = MagdolliteRiseStage.Setup.Lower,
            "Magdollite negative phase");
        AssertThrows<InvalidDataException>(() => _ = MagdolliteRiseStage.At(8).Higher,
            "Magdollite phase beyond authored table");

        Console.WriteLine(
            "Magdollite phase definitions: 27 native words and real initialization, rise, fall and overlay consumers pass with table reads forbidden.");
    }

    /// <summary>Reads one native threshold, instruction, or overlay word for the independent oracle.</summary>
    internal static ushort ReadMagdolliteWord(CartridgeImportAddressSpace bus, int address) =>
        bus.ReadCartridgeWord(address);

}

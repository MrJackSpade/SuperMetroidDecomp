using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static AssertionsVerification;

internal static class MagdollitePhaseDefinitionsVerification
{
    internal static void VerifyMagdollitePhaseDefinitions(SuperMetroidAddressSpace rom)
    {
        const int thresholdTable = 0xa8af55;
        const int instructionTable = 0xa8af67;
        const int overlayOffsetTable = 0xa8af79;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        for (int phaseIndex = 0; phaseIndex < 9; phaseIndex++)
        {
            MagdollitePhaseDefinition phase = MagdollitePhaseDefinitions.Phase(phaseIndex);
            AssertEqual(ReadMagdolliteWord(rom, thresholdTable + phaseIndex * 2),
                phase.DistanceThreshold, $"Magdollite threshold {phaseIndex}");
            AssertEqual(ReadMagdolliteWord(rom, instructionTable + phaseIndex * 2),
                phase.BodyInstructionList, $"Magdollite body list {phaseIndex}");
            AssertEqual(ReadMagdolliteWord(rom, overlayOffsetTable + phaseIndex * 2),
                phase.OverlayYOffset, $"Magdollite overlay offset {phaseIndex}");
        }

        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(NewMagdollitePhaseGuard(new TestAddressSpace()));
        Action<RoomEnemySlot, SamusState?> initialize = enemies.MagdolliteBehavior.InitializeMagdollite;
        Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?> rise = enemies.MagdolliteBehavior.RunMagdolliteBodyRising;
        Action<RoomEnemySlot, MagdolliteEnemyState> fall = enemies.MagdolliteBehavior.RunMagdolliteBodyFalling;
        Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?> trackRise = enemies.MagdolliteBehavior.RunMagdolliteOverlayTrackingRise;

        for (int index = 0; index < 3; index++)
        {
            RoomEnemySlot slot = enemies.SlotTable.Slots[index];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.Magdollite;
            slot.Parameter1 = (ushort)index;
            slot.Parameter2 = 0;
            slot.Position = slot.Position.WithWhole(new(0x0200, 0x0300));
            initialize(slot, null);
        }

        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        RoomEnemySlot body = enemies.SlotTable.Slots[1];
        RoomEnemySlot overlay = enemies.SlotTable.Slots[2];
        MagdolliteEnemyState headState = enemies.MagdolliteStates[0]!;
        MagdolliteEnemyState bodyState = enemies.MagdolliteStates[1]!;
        MagdolliteEnemyState overlayState = enemies.MagdolliteStates[2]!;
        headState.PositiveSpeedWhole = headState.PositiveSpeedFraction = 0;
        headState.NegativeSpeedWhole = headState.NegativeSpeedFraction = 0;

        AssertEqual(MagdollitePhaseDefinitions.Phase(0).BodyInstructionList,
            bodyState.DesiredInstructionList, "Magdollite production initial body list");

        for (int phaseIndex = 0; phaseIndex < 9; phaseIndex++)
        {
            MagdollitePhaseDefinition phase = MagdollitePhaseDefinitions.Phase(phaseIndex);
            bodyState.BodyReachedApex = false;
            bodyState.BodyPhaseOffset = (ushort)(phaseIndex * 2);
            body.Position = body.Position.WithWholeY(0x0300);
            trackRise(overlay, overlayState, null);
            AssertEqual(unchecked((ushort)(0x0300 - phase.OverlayYOffset)), overlay.Position.Y.Whole,
                $"Magdollite production overlay Y {phaseIndex}");

            bodyState.Function = MagdolliteEnemyFunction.BodyRising;
            bodyState.BodyReachedApex = false;
            bodyState.BodyPhaseOffset = (ushort)(phaseIndex * 2);
            bodyState.VerticalWholeDisplacement = phaseIndex < 8
                ? unchecked((ushort)-(short)phase.DistanceThreshold)
                : (ushort)0;
            body.Position = body.Position.WithWholeY(0x0300);
            rise(body, bodyState, null);
            if (phase.DistanceThreshold >= MagdollitePhaseDefinitions.MaximumRise && phaseIndex < 8)
            {
                // Reaching the maximum rise compares against the slot-one overread $20AF
                // instead (see --magdollite-apex-threshold), which holds this phase.
                AssertEqual((ushort)(phaseIndex * 2), bodyState.BodyPhaseOffset,
                    $"Magdollite production apex phase {phaseIndex}");
            }
            else if (phaseIndex < 8)
            {
                MagdollitePhaseDefinition next = MagdollitePhaseDefinitions.Phase(phaseIndex + 1);
                AssertEqual((ushort)((phaseIndex + 1) * 2), bodyState.BodyPhaseOffset,
                    $"Magdollite production rise phase {phaseIndex}");
                AssertEqual(next.BodyInstructionList, bodyState.DesiredInstructionList,
                    $"Magdollite production rise list {phaseIndex}");
            }
            else
            {
                AssertEqual((ushort)16, bodyState.BodyPhaseOffset,
                    "Magdollite terminal phase remains bounded");
            }
        }

        for (int priorPhase = 1; priorPhase < 8; priorPhase++)
        {
            bodyState.Function = MagdolliteEnemyFunction.BodyFalling;
            bodyState.BodyPhaseOffset = (ushort)((priorPhase + 1) * 2);
            bodyState.VerticalWholeDisplacement = ushort.MaxValue;
            body.Position = body.Position.WithWholeY(0x0300);
            fall(body, bodyState);
            AssertEqual((ushort)(priorPhase * 2), bodyState.BodyPhaseOffset,
                $"Magdollite production fall phase {priorPhase}");
            AssertEqual(MagdollitePhaseDefinitions.Phase(priorPhase).BodyInstructionList,
                bodyState.DesiredInstructionList,
                $"Magdollite production fall list {priorPhase}");
        }

        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(-1),
            "Magdollite negative phase");
        AssertThrows<InvalidDataException>(() => MagdollitePhaseDefinitions.Phase(9),
            "Magdollite phase beyond authored table");

        Console.WriteLine(
            "Magdollite phase definitions: 27 native words and real initialization, rise, fall and overlay consumers pass with table reads forbidden.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Magdollite words")]
    internal static ushort ReadMagdolliteWord(SuperMetroidAddressSpace bus, int address) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWord(address);

    private static CartridgeReadGuard NewMagdollitePhaseGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(0xa8af55, 0xa8af8b - 0xa8af55,
            address => $"Magdollite attempted migrated phase read ${address:X6}.");
}

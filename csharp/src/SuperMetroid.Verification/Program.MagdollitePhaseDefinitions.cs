using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares all authored Magdollite phase words with the cartridge and exercises initialization, movement, and overlay consumers with phase-table reads blocked.</summary>
    /// <param name="rom">Address space containing the cartridge's phase tables.</param>
    private static void VerifyMagdollitePhaseDefinitions(SuperMetroidAddressSpace rom)
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
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            enemies,
            new MagdollitePhaseReadGuard(new TestAddressSpace()));
        var initialize = typeof(RoomEnemySystem).GetMethod("InitializeMagdollite", flags)!
            .CreateDelegate<Action<RoomEnemySlot, SamusState?>>(enemies);
        var rise = typeof(RoomEnemySystem).GetMethod("RunMagdolliteBodyRising", flags)!
            .CreateDelegate<Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?>>(enemies);
        var fall = typeof(RoomEnemySystem).GetMethod("RunMagdolliteBodyFalling", flags)!
            .CreateDelegate<Action<RoomEnemySlot, MagdolliteEnemyState>>(enemies);
        var trackRise = typeof(RoomEnemySystem).GetMethod("RunMagdolliteOverlayTrackingRise", flags)!
            .CreateDelegate<Action<RoomEnemySlot, MagdolliteEnemyState, SamusState?>>(enemies);

        for (int index = 0; index < 3; index++)
        {
            RoomEnemySlot slot = enemies.Slots[index];
            slot.EnemyDefinitionPointer = 0xe83f;
            slot.Parameter1 = (ushort)index;
            slot.Parameter2 = 0;
            slot.XPosition = 0x0200;
            slot.YPosition = 0x0300;
            initialize(slot, null);
        }

        RoomEnemySlot head = enemies.Slots[0];
        RoomEnemySlot body = enemies.Slots[1];
        RoomEnemySlot overlay = enemies.Slots[2];
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
            body.YPosition = 0x0300;
            trackRise(overlay, overlayState, null);
            AssertEqual(unchecked((ushort)(0x0300 - phase.OverlayYOffset)), overlay.YPosition,
                $"Magdollite production overlay Y {phaseIndex}");

            bodyState.Function = MagdolliteEnemyFunction.BodyRising;
            bodyState.BodyReachedApex = false;
            bodyState.BodyPhaseOffset = (ushort)(phaseIndex * 2);
            bodyState.VerticalWholeDisplacement = phaseIndex < 8
                ? unchecked((ushort)-(short)phase.DistanceThreshold)
                : (ushort)0;
            body.YPosition = 0x0300;
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
            body.YPosition = 0x0300;
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

    /// <summary>Reads one little-endian phase-table word from the supplied address space.</summary>
    /// <param name="bus">Address space containing the phase data.</param>
    /// <param name="address">Address of the word's low byte.</param>
    /// <returns>The adjacent bytes combined into a 16-bit value.</returns>
    private static ushort ReadMagdolliteWord(SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    /// <summary>Address-space wrapper that rejects gameplay reads from Magdollite's migrated phase tables.</summary>
    /// <param name="source">Underlying address space used for permitted reads and all writes.</param>
    private sealed class MagdollitePhaseReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge reads through the phase-table guard.</summary>
        /// <param name="address">Full SNES address requested by the caller.</param>
        /// <returns>The wrapped byte when the address is outside the protected tables.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects threshold, instruction-list, and overlay-offset table reads and forwards all other byte reads.</summary>
        /// <param name="address">Full SNES address to read.</param>
        /// <returns>The wrapped byte when the address is outside the protected tables.</returns>
        public byte ReadByte(int address) => address is >= 0xa8af55 and < 0xa8af8b
            ? throw new InvalidOperationException(
                $"Magdollite attempted migrated phase read ${address:X6}.")
            : source.ReadByte(address);

        /// <summary>Forwards a byte write to the wrapped address space.</summary>
        /// <param name="address">Full SNES address to write.</param>
        /// <param name="value">Byte stored at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using System.Buffers.Binary;
using SuperMetroid.Core.Spatial;

/// <summary>Verification access to <see cref="BotwoonWallPlmProgramDefinitions"/> members production does not use.</summary>
internal static class BotwoonWallPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates native Botwoon wall program words for exact ROM ownership checks.</summary>
    extension(BotwoonWallPlmProgramDefinitions)
    {
        /// <summary>Visits crumble timing, draw, and terminal words, then the already-cleared wall list.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            yield return BotwoonWallPlmProgramDefinitions.Crumble;
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 3));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 5));
            for (int frame = 0; frame < 4; frame++)
            {
                yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 8 + frame * 4));
                yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 10 + frame * 4));
            }
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 24));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 26));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 28));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Crumble + 30));
            yield return BotwoonWallPlmProgramDefinitions.Clear;
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Clear + 2));
            yield return checked((ushort)(BotwoonWallPlmProgramDefinitions.Clear + 4));
        }
    }
}

/// <summary>Verification access to <see cref="ChozoStatuePlmDefinitions"/> members production does not use.</summary>
internal static class ChozoStatuePlmDefinitionsAccess
{
    /// <summary>Lists the authored Chozo statue PLM definitions used by room population.</summary>
    extension(ChozoStatuePlmDefinitions)
    {
        /// <summary>Enumerate the five semantic spawn cases in the original published order.</summary>
        internal static IEnumerable<ChozoStatuePlmDefinition> All
        {
            get
            {
                yield return ChozoStatuePlmDefinitions.LowerNorfairHand;
                yield return ChozoStatuePlmDefinitions.WreckedShipHand;
                yield return ChozoStatuePlmDefinitions.ClearSlopeAccess;
                yield return ChozoStatuePlmDefinitions.BlockSlopeAccess;
                yield return ChozoStatuePlmDefinitions.CrumblePlug;
            }
        }
    }
}

/// <summary>Verification access to <see cref="CrocomireArenaPlmProgramDefinitions"/> members production does not use.</summary>
internal static class CrocomireArenaPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Crocomire arena PLM instruction addresses for retail word comparison.</summary>
    extension(CrocomireArenaPlmProgramDefinitions)
    {
        /// <summary>Visits consecutive compiled words from bridge clearing through the arena program end.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            for (int address = CrocomireArenaPlmProgramDefinitions.ClearBridge; address < CrocomireArenaPlmProgramDefinitions.EndExclusive; address += 2)
                yield return (ushort)address;
        }
    }
}

/// <summary>Verification access to <see cref="DoorClosingPlmRomData"/> members production does not use.</summary>
internal static class DoorClosingPlmRomDataAccess
{
    /// <summary>Resolves a native door-closing header from its encoded orientation.</summary>
    extension(DoorClosingPlmRomData)
    {
        /// <summary>Returns the bank-$84 header selected by a raw orientation byte, or the table's zero word.</summary>
        internal static ushort GetHeader(byte orientation) =>
            (ushort)(DoorClosingPlmRomData.GetDefinition(CartridgeDoorOrientation.Decode(orientation))?.Header ?? PlmHeaderId.None);
    }
}

/// <summary>Verification access to <see cref="DoorScrollPrograms"/> members production does not use.</summary>
internal static class DoorScrollProgramsAccess
{
    /// <summary>Enumerates the door setup codes that own scroll-change programs.</summary>
    extension(DoorScrollPrograms)
    {
        /// <summary>Original callback catalog order, generated without a stored registration table.</summary>
        internal static IEnumerable<DoorSetupCode> Pointers
        {
            get
            {
                yield return DoorSetupCode.DoorCode_Scroll6_Green;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_13_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_4_Red_8_Green;
                yield return DoorSetupCode.DoorASM_Scroll_8_9_A_B_Red;
                yield return DoorSetupCode.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red;
                yield return DoorSetupCode.DoorASM_Scroll_1_4_Green;
                yield return DoorSetupCode.DoorASM_Scroll_2_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_17_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_4_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_6_Green_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_3_Green;
                yield return DoorSetupCode.DoorASM_Scroll_18_1C_Green;
                yield return DoorSetupCode.DoorASM_Scroll_5_6_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_1D_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_2_3_Green;
                yield return DoorSetupCode.DoorASM_Scroll_0_Red_1_Green;
                yield return DoorSetupCode.DoorASM_Scroll_B_Green;
                yield return DoorSetupCode.DoorASM_Scroll_Scroll_1C_Red_1D_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_4_Red;
                yield return DoorSetupCode.DoorASM_Scroll_20_24_25_Green;
                yield return DoorSetupCode.DoorASM_Scroll_2_Blue_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_Green;
                yield return DoorSetupCode.DoorASM_Scroll_6_7_Green;
                yield return DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red;
                yield return DoorSetupCode.DoorASM_Scroll_1_Blue_3_Red;
                yield return DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_2_3_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_0_1_Green;
                yield return DoorSetupCode.DoorASM_Scroll_1_Green;
                yield return DoorSetupCode.DoorASM_Scroll_F_12_Green;
                yield return DoorSetupCode.DoorASM_Scroll_6_Green_duplicate_again;
                yield return DoorSetupCode.DoorASM_Scroll_0_Green_1_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_2_Green;
                yield return DoorSetupCode.DoorASM_Scroll_3_4_Red_6_7_8_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red;
                yield return DoorSetupCode.DoorASM_Scroll_0_1_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red;
                yield return DoorSetupCode.DoorASM_Scroll_A_Green;
                yield return DoorSetupCode.DoorASM_Scroll_0_2_Green;
                yield return DoorSetupCode.DoorASM_Scroll_6_7_Blue_8_Red;
                yield return DoorSetupCode.DoorASM_Scroll_2_Red_3_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_7_Green;
                yield return DoorSetupCode.DoorASM_Scroll_1_Red_2_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red;
                yield return DoorSetupCode.DoorASM_Scroll_1_Blue_4_Red;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue_1_2_3_Red;
                yield return DoorSetupCode.DoorASM_Scroll_0_Green_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_1_Blue_4_Red;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_18_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_2_Blue_3_Red;
                yield return DoorSetupCode.DoorASM_Scroll_E_Red;
                yield return DoorSetupCode.DoorASM_Scroll_1_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_0_Green_duplicate_again;
                yield return DoorSetupCode.DoorASM_Scroll_3_Red_4_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_29_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_28_2E_Green;
                yield return DoorSetupCode.DoorASM_Scroll_6_7_8_9_A_B_Red;
                yield return DoorSetupCode.DoorASM_Scroll_A_Red_B_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_Red_1_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_9_Red_A_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_0_2_Red_1_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_1_Blue_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_6_Blue;
                yield return DoorSetupCode.DoorASM_Scroll_4_Red_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_4_7_Red;
                yield return DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_2_Green_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_0_1_Green_duplicate;
                yield return DoorSetupCode.DoorASM_Scroll_18_Blue_19_Red;
            }
        }
    }
}

/// <summary>Verification access to <see cref="DownwardGatePlmProgramDefinitions"/> members production does not use.</summary>
internal static class DownwardGatePlmProgramDefinitionsAccess
{
    /// <summary>$84:BC61: adjacent upward-gate program, outside the downward-gate decoder.</summary>
    private const ushort ResidentEnd = 0xbc61;

    /// <summary>Enumerates downward-gate instruction words and packed sound bytes at their native addresses.</summary>
    extension(DownwardGatePlmProgramDefinitions)
    {
        /// <summary>Visits downward-gate opening and trigger words while excluding adjacent upward-gate code.</summary>
        internal static IEnumerable<(ushort Address, ushort Value)> MechanicsWords
        {
            get
            {
                for (int address = DownwardGatePlmProgramDefinitions.OpenStart; address < ResidentEnd; address++)
                    if (DownwardGatePlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
                for (int address = RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft; address < DownwardGatePlmProgramDefinitions.TriggerEnd; address += 2)
                    if (DownwardGatePlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
            }
        }

        /// <summary>Exposes the opening and closing sound operand bytes in their native positions.</summary>
        internal static IEnumerable<(ushort Address, byte Value)> MechanicsBytes
        {
            get
            {
                yield return (DownwardGatePlmProgramDefinitions.ClosingSoundAddress, DownwardGatePlmRomData.MovementSound);
                yield return (DownwardGatePlmProgramDefinitions.OpeningSoundAddress, DownwardGatePlmRomData.MovementSound);
            }
        }
    }
}

/// <summary>Verification access to <see cref="KraidRoomPlmProgramDefinitions"/> members production does not use.</summary>
internal static class KraidRoomPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Kraid room PLM words for bounded cartridge comparison.</summary>
    extension(KraidRoomPlmProgramDefinitions)
    {
        /// <summary>Visits decoded Kraid ceiling-crumble words across the room PLM interval.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            for (int address = KraidRoomPlmProgramDefinitions.CrumbleCeilingBackground1; address < KraidRoomPlmProgramDefinitions.EndExclusive; address++)
                if (KraidRoomPlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out _))
                    yield return (ushort)address;
        }
    }
}

/// <summary>Verification access to <see cref="LandingSiteEntryState"/> members production does not use.</summary>
internal static class LandingSiteEntryStateAccess
{
    /// <summary>Loads landing-site entry fixtures from their door pointer and native actor state.</summary>
    extension(LandingSiteEntryState)
    {
        /// <summary>Selects the intro landing-cutscene door and its compiled command-E record.</summary>
        internal static LandingSiteEntryState LoadLandingCutscene(ISnesAddressSpace bus) =>
            LandingSiteEntryState.Load(bus, LandingSiteRomData.LandingCutsceneDoorPointer);

        /// <summary>
        /// Uses the compiled door/room definitions and selects its transfer from
        /// <c>LibBG_ScrollingSky_Tilemaps_LandingSite</c> at <c>$8F:B76A</c>.
        /// </summary>
        internal static LandingSiteEntryState Load(ISnesAddressSpace bus, ushort doorPointer)
        {
            Ensure.NotNull(bus);
            CartridgeDoorHeader door = DoorDefinitions.Get(doorPointer);

            // The destination is a bank-$8F room pointer. Rejecting any other room
            // is important: the library-background list below is specific to Landing Site and
            // silently applying it to an arbitrary door would manufacture a plausible image.
            if (door.DestinationRoomPointer != RoomHeaderPointers.LandingSite)
            {
                throw new InvalidDataException(
                    $"Door $83:{doorPointer:X4} targets room ${door.DestinationRoomPointer:X4}, " +
                    "not Landing Site $91F8.");
            }

            RoomHeaderDefinition room = RoomHeaderDefinitions.Get(RoomHeaderPointers.LandingSite);
            RoomIdentity roomIdentity = new(room.AreaIndex, room.RoomIndex);
            if (roomIdentity != RoomIdentities.LandingSite)
            {
                throw new InvalidDataException(
                    $"Landing Site room definition has logical identity {roomIdentity}, expected " +
                    $"{RoomIdentities.LandingSite}.");
            }

            ushort statePointer = RoomStateSelectionDefinitions.Select(
                RoomHeaderPointers.LandingSite, default);
            CartridgeRoomState state = RoomStateDefinitions.Get(statePointer);
            LibraryBackgroundInstruction transfer =
                LibraryBackgroundProgramDefinitions.GetDoorTransfer(
                    unchecked((ushort)LandingSiteRomData.LibraryBackgroundListAddress),
                    doorPointer);
            return new LandingSiteEntryState(
                RoomIdentity: roomIdentity,
                RoomMapPosition: room.MapPosition,
                UpScroller: room.UpScroller,
                DownScroller: room.DownScroller);
        }
    }
}

/// <summary>Verification access to <see cref="LibraryBackgroundProgramDefinitions"/> members production does not use.</summary>
internal static class LibraryBackgroundProgramDefinitionsAccess
{
    /// <summary>Reads a library-background door transfer from its compiled instruction list.</summary>
    extension(LibraryBackgroundProgramDefinitions)
    {
        /// <summary>Finds a command-E transfer for one door in a compiled program.</summary>
        internal static LibraryBackgroundInstruction GetDoorTransfer(ushort listPointer,
            ushort doorPointer)
        {
            foreach (LibraryBackgroundInstruction instruction in LibraryBackgroundProgramDefinitionsTooling.Get(listPointer).Instructions)
            {
                if (instruction.Command == LibraryBackgroundCommand.TransferForDoor &&
                    instruction.DoorPointer == doorPointer)
                    return instruction;
            }
            throw new InvalidDataException(
                $"Library background $8F:{listPointer:X4} has no command-E record " +
                $"for door $83:{doorPointer:X4}.");
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainFakeDeathPlmDrawDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmDrawDefinitionsAccess
{
    /// <summary>Resolves a Mother Brain fake-death draw list by native pointer.</summary>
    extension(MotherBrainFakeDeathPlmDrawDefinitions)
    {
        /// <summary>Searches background, regular, then boundary draw families for one native pointer.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
        {
            // Native order: background, regular, then boundary draw families.
            if (MotherBrainFakeDeathPlmDrawDefinitions.TryGetBackground(pointer, out draw)
                || MotherBrainFakeDeathPlmDrawDefinitions.TryGetRegular(pointer, out draw)
                || MotherBrainFakeDeathPlmDrawDefinitions.TryGetBoundary(pointer, out draw))
                return true;
            draw = default;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainFakeDeathPlmProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmProgramDefinitionsAccess
{
    /// <summary>Counts fake-death PLM programs and enumerates their native word addresses.</summary>
    extension(MotherBrainFakeDeathPlmProgramDefinitions)
    {
        /// <summary>Counts fixed-size fake-death PLM programs in the compiled address interval.</summary>
        internal static int ProgramCount => (MotherBrainFakeDeathPlmProgramDefinitions.EndExclusive - MotherBrainFakeDeathPlmProgramDefinitions.Start) / MotherBrainFakeDeathPlmProgramDefinitions.ProgramByteLength;

        /// <summary>Visits three native words for every fake-death PLM program.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses() =>
            Enumerable.Range(0, MotherBrainFakeDeathPlmProgramDefinitions.ProgramCount * 3)
                .Select(index => checked((ushort)(MotherBrainFakeDeathPlmProgramDefinitions.Start + index * 2)));
    }
}

/// <summary>Verification access to <see cref="NoobTubePlmProgramDefinitions"/> members production does not use.</summary>
internal static class NoobTubePlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Noob Tube program words and byte operands for cartridge comparison.</summary>
    extension(NoobTubePlmProgramDefinitions)
    {
        /// <summary>Visits main, post-sound, and already-broken Noob Tube control words.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int offset = 0; offset <= 0x30; offset += 2)
                yield return checked((ushort)(NoobTubePlmProgramDefinitions.MainStart + offset));
            for (int offset = 0; offset <= 0x10; offset += 2)
                yield return checked((ushort)(NoobTubePlmProgramDefinitions.MainAfterSound + offset));
            yield return NoobTubePlmProgramDefinitions.AlreadyBrokenStart;
            yield return checked((ushort)(NoobTubePlmProgramDefinitions.AlreadyBrokenStart + 2));
        }

        /// <summary>Locates the standalone tube-breaking sound operand byte.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            yield return NoobTubePlmProgramDefinitions.BreakSoundAddress;
        }
    }
}

/// <summary>Verification access to <see cref="ResidentDoorClosingDefinitions"/> members production does not use.</summary>
internal static class ResidentDoorClosingDefinitionsAccess
{
    /// <summary>Lists resident door-closing records across their orientation variants.</summary>
    extension(ResidentDoorClosingDefinitions)
    {
        /// <summary>Enumerates the twenty supported identities in original header order without cached records.</summary>
        internal static IEnumerable<ResidentDoorClosingDefinition> All
        {
            get
            {
                yield return new(PlmHeaderId.BombTorizoGreyDoor, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.BombTorizoGreyDoor));
                yield return new(PlmHeaderId.GreyDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingLeft));
                yield return new(PlmHeaderId.GreyDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingRight));
                yield return new(PlmHeaderId.GreyDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingUp));
                yield return new(PlmHeaderId.GreyDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreyDoorFacingDown));
                yield return new(PlmHeaderId.YellowDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingLeft));
                yield return new(PlmHeaderId.YellowDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingRight));
                yield return new(PlmHeaderId.YellowDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingUp));
                yield return new(PlmHeaderId.YellowDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.YellowDoorFacingDown));
                yield return new(PlmHeaderId.GreenDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingLeft));
                yield return new(PlmHeaderId.GreenDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingRight));
                yield return new(PlmHeaderId.GreenDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingUp));
                yield return new(PlmHeaderId.GreenDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.GreenDoorFacingDown));
                yield return new(PlmHeaderId.RedDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingLeft));
                yield return new(PlmHeaderId.RedDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingRight));
                yield return new(PlmHeaderId.RedDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingUp));
                yield return new(PlmHeaderId.RedDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.RedDoorFacingDown));
                yield return new(PlmHeaderId.MotherBrainEscapeRoomGate, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.MotherBrainEscapeRoomGate));
                yield return new(PlmHeaderId.EyeDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.EyeDoorFacingRight));
                yield return new(PlmHeaderId.EyeDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(PlmHeaderId.EyeDoorFacingLeft));
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomCallbackDefinitions"/> members production does not use.</summary>
internal static class RoomCallbackDefinitionsAccess
{
    /// <summary>Converts typed room main and setup callbacks to their native dispatcher pointers.</summary>
    extension(RoomCallbackDefinitions)
    {
        /// <summary>Returns a defined room-main callback's native pointer.</summary>
        internal static ushort PointerOf(RoomMainCallback callback) => Enum.IsDefined(callback)
            ? (ushort)callback
            : throw new ArgumentOutOfRangeException(nameof(callback));

        /// <summary>Returns a defined room-setup callback's native pointer.</summary>
        internal static ushort PointerOf(RoomSetupCallback callback) => Enum.IsDefined(callback)
            ? (ushort)callback
            : throw new ArgumentOutOfRangeException(nameof(callback));
    }
}

/// <summary>Verification access to <see cref="RoomCollisionBlock"/> members production does not use.</summary>
internal static class RoomCollisionBlockAccess
{
    /// <summary>Extracts the visual tile index from a collision block's packed room word.</summary>
    extension(RoomCollisionBlock self)
    {
        /// <summary>Low ten bits selecting the visual 16×16 block definition.</summary>
        internal ushort VisualBlockIndex => self.PackedWord.VisualBlockIndex;
    }
}

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess
{
    /// <summary>Restores mutable room level entries and behavior data from a fixture snapshot.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// Returns this level to <paramref name="source"/>'s complete state in place: every block
        /// plane, allocation tail and pending door flag. Both levels must share one layout.
        /// </summary>
        internal void RestoreFrom(RoomLevelData source)
        {
            Ensure.NotNull(source);
            if (source.Size.X != self.Size.X || source.Size.Y != self.Size.Y ||
                source.DoorListPointer != self.DoorListPointer ||
                source._blockDefinitions.Length != self._blockDefinitions.Length ||
                source._streamingForegroundAllocation.Length != self._streamingForegroundAllocation.Length ||
                source._streamingBackgroundAllocation.Length != self._streamingBackgroundAllocation.Length ||
                source._plmForegroundAllocation.Length != self._plmForegroundAllocation.Length ||
                object.ReferenceEquals(source._visualStreamingForegroundAllocation, source._streamingForegroundAllocation) !=
                    object.ReferenceEquals(self._visualStreamingForegroundAllocation, self._streamingForegroundAllocation) ||
                object.ReferenceEquals(source._visualStreamingBackgroundAllocation, source._streamingBackgroundAllocation) !=
                    object.ReferenceEquals(self._visualStreamingBackgroundAllocation, self._streamingBackgroundAllocation))
                throw new ArgumentException("A level can only be restored from one with the same layout.", nameof(source));
            source._foregroundEntries.CopyTo(self._foregroundEntries, 0);
            source._behaviorBytes.CopyTo(self._behaviorBytes, 0);
            source._backgroundEntries.CopyTo(self._backgroundEntries, 0);
            source._blockDefinitions.CopyTo(self._blockDefinitions, 0);
            source._streamingForegroundAllocation.CopyTo(self._streamingForegroundAllocation, 0);
            source._streamingBackgroundAllocation.CopyTo(self._streamingBackgroundAllocation, 0);
            source._visualStreamingForegroundAllocation.CopyTo(self._visualStreamingForegroundAllocation, 0);
            source._visualStreamingBackgroundAllocation.CopyTo(self._visualStreamingBackgroundAllocation, 0);
            source._plmForegroundAllocation.CopyTo(self._plmForegroundAllocation, 0);
            source._plmBehaviorAllocation.CopyTo(self._plmBehaviorAllocation, 0);
            self.PendingDoorTransition = source.PendingDoorTransition;
            self.ElevatorDoorContactPending = source.ElevatorDoorContactPending;
        }
    }
}

/// <summary>Verification access to <see cref="RoomLevelWord"/> members production does not use.</summary>
internal static class RoomLevelWordAccess
{
    /// <summary>Changes a level word's visual flip bits while preserving its other packed fields.</summary>
    extension(RoomLevelWord self)
    {
        /// <summary>Replaces only the parent-block visual transforms.</summary>
        internal RoomLevelWord WithVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
        {
            RoomLevelWord.ValidateVisualFlipFlags((LevelBlockFlipFlags)(visualFlipFlags));
            return new RoomLevelWord(unchecked((ushort)(
                (self.Raw & ~RoomLevelWord.VisualFlipMask) | (ushort)visualFlipFlags)));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmBlueDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBlueDoorVisualCatalogAccess
{
    /// <summary>Constructs a blue-door visual catalog fixture without requiring installed art.</summary>
    extension(RoomPlmBlueDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmBlueDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmBlueDoorVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmBombBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmBombBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates respawning bomb-block instruction words and packed operands.</summary>
    extension(RoomPlmBombBlockProgramDefinitions)
    {
        /// <summary>Visits collision, reaction, frame, terminal, and optional respawn words for each bomb block.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomPlmBombBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmBombBlockProgramDefinitions.ProgramAt((int)(index));
                yield return program.CollisionHead;
                yield return checked((ushort)(program.CollisionHead + 3));
                yield return checked((ushort)(program.CollisionHead + 5));
                yield return program.ReactionHead;
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Tail + 4 * frame));
                yield return program.Terminal;
                if (program.Respawns)
                    yield return checked((ushort)(program.Terminal +
                        (program.SingleBlock ? 2 : 4)));
            }
        }

        /// <summary>Locates bomb-block collision and reaction operands stored as bytes.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < RoomPlmBombBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmBombBlockProgramDefinitions.ProgramAt((int)(index));
                yield return checked((ushort)(program.CollisionHead + 2));
                yield return checked((ushort)(program.ReactionHead + 2));
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmBombBlockRestoreDrawDefinitions"/> members production does not use.</summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitionsAccess
{
    /// <summary>Resolves bomb-block restore draw lists by their compiled pointer.</summary>
    extension(RoomPlmBombBlockRestoreDrawDefinitions)
    {
        /// <summary>Converts a recognized bomb-block restore drawing into the shared verifier draw-list form.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (RoomPlmBombBlockRestoreDrawDefinitions.TryDescribe(pointer, out RoomPlmBombBlockRestoreDrawDefinitions.Draw draw))
            {
                list = ((RoomPlmShotBlockDrawDefinitions.DrawList)(RoomPlmBombBlockRestoreDrawDefinitions.Export((RoomPlmBombBlockRestoreDrawDefinitions.Draw)(draw))));
                return true;
            }
            list = default;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmBombTorizoHandVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBombTorizoHandVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Bomb Torizo hand-art fixture for PLM binding checks.</summary>
    extension(RoomPlmBombTorizoHandVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmBombTorizoHandVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmBombTorizoHandVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmBotwoonWallVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBotwoonWallVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Botwoon wall-art fixture for PLM binding checks.</summary>
    extension(RoomPlmBotwoonWallVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmBotwoonWallVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmBotwoonWallVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmChozoStatueVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmChozoStatueVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Chozo statue-art fixture for PLM binding checks.</summary>
    extension(RoomPlmChozoStatueVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmChozoStatueVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmChozoStatueVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmCollectibleVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmCollectibleVisualCatalogAccess
{
    /// <summary>Creates an uninitialized collectible-art fixture for PLM binding checks.</summary>
    extension(RoomPlmCollectibleVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmCollectibleVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmCollectibleVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmColoredDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmColoredDoorVisualCatalogAccess
{
    /// <summary>Creates an uninitialized colored-door art fixture for PLM binding checks.</summary>
    extension(RoomPlmColoredDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmColoredDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmColoredDoorVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmContactCrumbleProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmContactCrumbleProgramDefinitionsAccess
{
    /// <summary>Enumerates contact-crumble mechanics words and packed byte operands.</summary>
    extension(RoomPlmContactCrumbleProgramDefinitions)
    {
        /// <summary>Visits setup, frame, terminal, and optional respawn words for each contact-crumble list.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomPlmContactCrumbleProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmContactCrumbleProgramDefinitions.ProgramAt((int)(index));
                yield return program.Start;
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Start + 3 + 4 * frame));
                yield return program.Terminal;
                if (program.Respawns)
                    yield return checked((ushort)(program.Terminal +
                        (program.Dimension == 0 ? 2 : 4)));
            }
        }

        /// <summary>Locates each contact-crumble program's byte operand after its start word.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < RoomPlmContactCrumbleProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((RoomPlmContactCrumbleProgramDefinitions.ProgramAt((int)(index))).Start + 2));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmContactCrumbleRestoreDrawDefinitions"/> members production does not use.</summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitionsAccess
{
    /// <summary>Resolves contact-crumble restoration draw lists by native pointer.</summary>
    extension(RoomPlmContactCrumbleRestoreDrawDefinitions)
    {
        /// <summary>Converts a recognized contact-crumble restore drawing to a verifier draw list.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe(pointer, out RoomPlmContactCrumbleRestoreDrawDefinitions.Draw draw))
            {
                list = ((RoomPlmShotBlockDrawDefinitions.DrawList)(RoomPlmContactCrumbleRestoreDrawDefinitions.Export((RoomPlmContactCrumbleRestoreDrawDefinitions.Draw)(draw))));
                return true;
            }
            list = default;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmCrocomireVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmCrocomireVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Crocomire arena-art fixture for PLM binding checks.</summary>
    extension(RoomPlmCrocomireVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmCrocomireVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmCrocomireVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmDownwardGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmDownwardGateVisualCatalogAccess
{
    /// <summary>Creates an uninitialized downward-gate art fixture for PLM binding checks.</summary>
    extension(RoomPlmDownwardGateVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmDownwardGateVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmDownwardGateVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmDraygonCannonVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmDraygonCannonVisualCatalogAccess
{
    /// <summary>Creates an uninitialized Draygon cannon-art fixture for PLM binding checks.</summary>
    extension(RoomPlmDraygonCannonVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmDraygonCannonVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmDraygonCannonVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmDynamicCollectibleArtCatalog"/> members production does not use.</summary>
internal static class RoomPlmDynamicCollectibleArtCatalogAccess
{
    /// <summary>Creates an uninitialized dynamic collectible-art fixture for PLM binding checks.</summary>
    extension(RoomPlmDynamicCollectibleArtCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmDynamicCollectibleArtCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmDynamicCollectibleArt();
    }
}

/// <summary>Verification access to <see cref="RoomPlmElevatorPlatformVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmElevatorPlatformVisualCatalogAccess
{
    /// <summary>Creates an uninitialized elevator-platform art fixture for PLM binding checks.</summary>
    extension(RoomPlmElevatorPlatformVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmElevatorPlatformVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmElevatorPlatformVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmEscapeGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmEscapeGateVisualCatalogAccess
{
    /// <summary>Creates an uninitialized escape-gate art fixture for PLM binding checks.</summary>
    extension(RoomPlmEscapeGateVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmEscapeGateVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmEscapeGateVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmEyeDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmEyeDoorVisualCatalogAccess
{
    /// <summary>Creates an uninitialized eye-door art fixture for PLM binding checks.</summary>
    extension(RoomPlmEyeDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmEyeDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmEyeDoorVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmGrappleBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmGrappleBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates grapple-block program words and packed operands at retail addresses.</summary>
    extension(RoomPlmGrappleBlockProgramDefinitions)
    {
        /// <summary>Visits setup, frame, terminal, and optional respawn words for grapple blocks.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < RoomPlmGrappleBlockProgramDefinitions.ProgramCount; index++)
            {
                var program = RoomPlmGrappleBlockProgramDefinitions.ProgramAt((int)(index));
                yield return program.Start;
                yield return checked((ushort)(program.Start + 4));
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Start + 7 + 4 * frame));
                yield return program.TerminalAddress;
                if (program.Respawns)
                {
                    yield return checked((ushort)(program.TerminalAddress + 2));
                    yield return checked((ushort)(program.TerminalAddress + 4));
                }
            }
        }

        /// <summary>Locates the one-byte operand after each grapple-block setup pair.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < RoomPlmGrappleBlockProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((RoomPlmGrappleBlockProgramDefinitions.ProgramAt((int)(index))).Start + 6));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmGrappleBlockVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmGrappleBlockVisualCatalogAccess
{
    /// <summary>Creates an uninitialized grapple-block art fixture for PLM binding checks.</summary>
    extension(RoomPlmGrappleBlockVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmGrappleBlockVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmGrappleBlockVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmGreyDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmGreyDoorVisualCatalogAccess
{
    /// <summary>Creates an uninitialized grey-door art fixture for PLM binding checks.</summary>
    extension(RoomPlmGreyDoorVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmGreyDoorVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmGreyDoorVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmKraidVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmKraidVisualCatalogAccess
{
    /// <summary>Builds the Kraid room-art fixture from its explicit compiled visual parts.</summary>
    extension(RoomPlmKraidVisualCatalog)
    {
        /// <summary>Builds stock Kraid PLM art from the visual bits of its physical draw words.</summary>
        internal static RoomPlmKraidVisualCatalog Stock() => new(
            KraidRoomPlmDrawDefinitions.All.Select(draw =>
                new RoomPlmKraidVisualEntry(
                    KraidRoomPlmDrawDefinitions.VisualId(draw.Pointer),
                    draw.Runs.Span.ToArray().SelectMany(run =>
                        run.LevelWords.Span.ToArray().Select(word =>
                            new RoomLevelWord(word).VisualWord)).ToArray())));
    }
}

/// <summary>Verification access to <see cref="RoomPlmLinkedRestoreVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmLinkedRestoreVisualCatalogAccess
{
    /// <summary>Creates an uninitialized linked-block restoration art fixture for PLM binding checks.</summary>
    extension(RoomPlmLinkedRestoreVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmLinkedRestoreVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmLinkedRestoreVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmMaridiaElevatubeVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMaridiaElevatubeVisualCatalogAccess
{
    /// <summary>Builds the elevatube's stock visual from its single physical level word.</summary>
    extension(RoomPlmMaridiaElevatubeVisualCatalog)
    {
        /// <summary>Builds the stock elevatube appearance from its single physical level word.</summary>
        internal static RoomPlmMaridiaElevatubeVisualCatalog Stock() => new(
            [new RoomPlmMaridiaElevatubeVisualEntry(
                MaridiaElevatubePlmDefinitions.VisualId,
                [new RoomLevelWord(MaridiaElevatubePlmDefinitions.PhysicalWord).VisualWord])]);
    }
}

/// <summary>Verification access to <see cref="RoomPlmMotherBrainFakeDeathVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMotherBrainFakeDeathVisualCatalogAccess
{
    /// <summary>Uses physical fake-death cells as stock art without a second stored draw cache.</summary>
    extension(RoomPlmMotherBrainFakeDeathVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmMotherBrainFakeDeathVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmMotherBrainGlassVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMotherBrainGlassVisualCatalogAccess
{
    /// <summary>Leaves Mother Brain glass stock cells calculated while allowing custom frame bindings.</summary>
    extension(RoomPlmMotherBrainGlassVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmMotherBrainGlassVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmMotherBrainGlassVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmNoobTubeVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmNoobTubeVisualCatalogAccess
{
    /// <summary>Leaves Noob Tube stock cells calculated while allowing custom breakage frames.</summary>
    extension(RoomPlmNoobTubeVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmNoobTubeVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmNoobTubeVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmPopulationDefinitions"/> members production does not use.</summary>
internal static class RoomPlmPopulationDefinitionsAccess
{
    /// <summary>Scans bank-$84 header pointers accepted by the room PLM population loader.</summary>
    extension(RoomPlmPopulationDefinitions)
    {
        /// <summary>Scans bank-offset values for PLM headers accepted by the room population loader.</summary>
        internal static IEnumerable<ushort> Pointers
        {
            get
            {
                for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                    if (RoomPlmPopulationDefinitions.TryPlace((ushort)pointer, null))
                        yield return (ushort)pointer;
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmSamusEaterVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSamusEaterVisualCatalogAccess
{
    /// <summary>Derives Samus Eater stock appearance from physical draw words without caching art.</summary>
    extension(RoomPlmSamusEaterVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmSamusEaterVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmSamusEaterVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmScrollProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmScrollProgramDefinitionsAccess
{
    /// <summary>Finds authored scroll programs and projects their direct writes into legacy byte format.</summary>
    extension(RoomPlmScrollProgramDefinitions)
    {
        /// <summary>Scans bank-offset values for decoded scroll-state PLM programs.</summary>
        internal static IEnumerable<ushort> Pointers
        {
            get
            {
                for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                    if (RoomPlmScrollProgramDefinitions.TryApply((ushort)pointer, null)) yield return (ushort)pointer;
            }
        }

        /// <summary>Temporary native-format projection for verification/compatibility; gameplay executes writes directly.</summary>
        internal static ReadOnlyMemory<byte> Get(ushort pointer)
        {
            var bytes = new List<byte>();
            RoomPlmScrollProgramDefinitions.Apply(pointer, (index, state) => { bytes.Add((byte)index); bytes.Add((byte)state); });
            bytes.Add(0x80);
            return bytes.ToArray();
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmShotBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmShotBlockProgramDefinitionsAccess
{
    /// <summary>Enumerates shot-block setup, frame, and terminal mechanics addresses separately from draw operands.</summary>
    extension(RoomPlmShotBlockProgramDefinitions)
    {
        /// <summary>All authored control-word addresses, excluding draw-list operands.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            foreach (var program in RoomPlmShotBlockProgramDefinitions.Programs)
            {
                yield return program.Start;
                for (int frame = 0; frame < program.FrameCount; frame++)
                    yield return checked((ushort)(program.Start + 3 + 4 * frame));
                yield return program.TerminalAddress;
                if (program.RestoresLevelWord)
                    yield return checked((ushort)(program.TerminalAddress + 2));
            }
        }

        /// <summary>Locates the byte operand after each shot-block program's setup word.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            foreach (var program in RoomPlmShotBlockProgramDefinitions.Programs)
                yield return checked((ushort)(program.Start + 2));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmShotBlockVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmShotBlockVisualCatalogAccess
{
    /// <summary>Uses native shot-block draws as the stock visual selection without an installed override.</summary>
    extension(RoomPlmShotBlockVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmShotBlockVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmShotBlockVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmSpeedBoosterVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSpeedBoosterVisualCatalogAccess
{
    /// <summary>Builds bomb-reveal speed-block art from the first physical draw word's visual bits.</summary>
    extension(RoomPlmSpeedBoosterVisualCatalog)
    {
        /// <summary>Builds the bomb-reveal visual from the first physical speed-block draw word.</summary>
        internal static RoomPlmSpeedBoosterVisualCatalog Stock() => new(
            [new RoomPlmSpeedBoosterVisualEntry(
                SpeedBoosterBlockPlmDrawDefinitions.BombRevealVisualId,
                [new RoomLevelWord(SpeedBoosterBlockPlmDrawDefinitions.BombReveal
                    .Runs.Span[0].LevelWords.Span[0]).VisualWord])]);
    }
}

/// <summary>Verification access to <see cref="RoomPlmSporeSpawnCeilingVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSporeSpawnCeilingVisualCatalogAccess
{
    /// <summary>Calculates stock ceiling-crumble squares from their physical draw words.</summary>
    extension(RoomPlmSporeSpawnCeilingVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmSporeSpawnCeilingVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmSporeSpawnCeilingVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmStationVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmStationVisualCatalogAccess
{
    /// <summary>Calculates station stock appearances while retaining only customized frames.</summary>
    extension(RoomPlmStationVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmStationVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmStationVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomPlmSystem"/> members production does not use.</summary>
internal static class RoomPlmSystemAccess
{
    /// <summary>Identifies PLM headers supported by the room population loader and colored-door dispatcher.</summary>
    extension(RoomPlmSystem)
    {
        /// <summary>
        /// Returns whether a room-authored header has a setup owner in the sequential loader.
        /// This is a read-only inventory seam for private-ROM audits; production still performs
        /// dispatch and contextual failure from <see cref="PlmPopulationLoader.LoadRoomPopulation"/> itself.
        /// </summary>
        internal static bool IsSupportedRoomPopulationHeader(PlmHeaderId header)
        {
            if (header is PlmHeaderId.ScrollTrigger or
                PlmHeaderId.RightwardsScrollExtension or
                PlmHeaderId.LeftwardsScrollExtension or
                PlmHeaderId.DownwardsScrollExtension or
                PlmHeaderId.UpwardsScrollExtension or
                PlmHeaderId.MotherBrainGlass or PlmHeaderId.BombTorizoHand or
                PlmHeaderId.MapStation or PlmHeaderId.EnergyStation or
                PlmHeaderId.MissileStation or PlmHeaderId.ElevatorPlatform or
                PlmHeaderId.SaveStation or
                PlmHeaderId.SpeedBoosterEscape or
                PlmHeaderId.WreckedShipAttic or
                PlmHeaderId.NoobTube or
                PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                PlmHeaderId.MotherBrainEscapeRoomGate or
                PlmHeaderId.DownwardGate or PlmHeaderId.DownwardGateShotBlock ||
                EyeDoorPlms.IsEyeDoorHeader(header) || DraygonCannonPlms.IsDraygonCannonHeader(header))
            {
                return true;
            }
            if (RoomPlmSystem.IsColoredDoorHeader(header) ||
                GreyDoorPlms.TryIdentifyGreyDoor(header, out _))
                return true;
            return CollectiblePlms.TryIdentifyPermanentCollectible(header, out _, out _);
        }

        /// <summary>Recognizes the twelve directional yellow, green, and red door headers.</summary>
        internal static bool IsColoredDoorHeader(PlmHeaderId header) => header is
            PlmHeaderId.YellowDoorFacingLeft or PlmHeaderId.YellowDoorFacingRight or
            PlmHeaderId.YellowDoorFacingUp or PlmHeaderId.YellowDoorFacingDown or
            PlmHeaderId.GreenDoorFacingLeft or PlmHeaderId.GreenDoorFacingRight or
            PlmHeaderId.GreenDoorFacingUp or PlmHeaderId.GreenDoorFacingDown or
            PlmHeaderId.RedDoorFacingLeft or PlmHeaderId.RedDoorFacingRight or
            PlmHeaderId.RedDoorFacingUp or PlmHeaderId.RedDoorFacingDown;
    }

    /// <summary>Inspects live door, collectible, and glass PLMs and drives scoped PLM verification operations.</summary>
    extension(RoomPlmSystem self)
    {
        /// <summary>Every resident colored-door actor in the shared native PLM pool.</summary>
        internal IReadOnlyList<ColoredDoorPlmSnapshot> ColoredDoors =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: slot.ColoredDoor))
                .Where(entry => entry.door is not null)
                .Select(entry => new ColoredDoorPlmSnapshot(
                    entry.slot.HeaderPointer,
                    entry.slot.BlockIndex,
                    entry.slot.RoomArgument,
                    (entry.door ?? throw new InvalidOperationException("Verification requires entry.door.")).Color, entry.door.Orientation, entry.door.Phase, entry.door.HitCounter)),
        ];

        /// <summary>Debugger-stable views of every resident grey-door PLM.</summary>
        internal IReadOnlyList<GreyDoorPlmSnapshot> GreyDoorSnapshots =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: slot.GreyDoor))
                .Where(entry => entry.door is not null)
                .Select(entry => new GreyDoorPlmSnapshot(
                    entry.slot.HeaderPointer,
                    entry.slot.BlockIndex,
                    entry.slot.RoomArgument,
                    (entry.door ?? throw new InvalidOperationException("Verification requires entry.door.")).Orientation, entry.door.Condition, entry.door.Phase,
                    entry.door.InitialList, entry.door.FlashList, entry.door.OpeningList)),
        ];

        /// <summary>Debugger-visible projections of every live eye-door component.</summary>
        internal IReadOnlyList<EyeDoorPlmSnapshot> EyeDoorSnapshots =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: slot.EyeDoor))
                .Where(entry => entry.door is not null)
                .Select(entry => new EyeDoorPlmSnapshot(
                    entry.slot.HeaderPointer,
                    entry.slot.BlockIndex,
                    entry.slot.RoomArgument,
                    ((entry.door ?? throw new InvalidOperationException("Verification requires entry.door."))).Component,
                    (entry.door).HitCounter,
                    entry.slot.InstructionPointer,
                    entry.slot.PreInstruction)),
        ];

        /// <summary>
        /// Publishes the projectile word observed by a resident type-$C/BTS-$44 door. The
        /// resident actor, not the collision table, decides whether that family is accepted.
        /// </summary>
        internal bool TryNotifyColoredDoorHit(int blockIndex, SamusProjectileTypeWord projectileType)
        {
            if (self.TryNotifyEyeDoorHit(blockIndex, projectileType))
                return true;

            foreach (var slot in ActiveSlots(self))
            {
                if (slot.BlockIndex != blockIndex ||
                    slot.ColoredDoor is not { } door)
                    continue;

                // Once the threshold branch has selected the opening list, native clears the
                // pre-instruction pointer. Later projectiles therefore cannot enqueue another
                // family check or restart the animation while the cap is disappearing.
                if (door.Phase is ColoredDoorPhase.Opening or ColoredDoorPhase.Closing)
                    return false;

                door.PendingProjectileType = projectileType;
                door.HasPendingHit = true;
                return true;
            }
            // BTS $44 finds the resident PLM by block index; it does not distinguish the door's color.
            return self.TryNotifyGreyDoorHit(blockIndex, projectileType);
        }

        /// <summary>Publishes the native projectile word to a resident grey-door actor.</summary>
        internal bool TryNotifyGreyDoorHit(int blockIndex, SamusProjectileTypeWord projectileType)
        {
            foreach (var slot in ActiveSlots(self))
            {
                if (slot.BlockIndex != blockIndex ||
                    slot.GreyDoor is not { } door)
                    continue;

                // Once the one-hit instruction selects the opening stream, native clears the
                // shot pre-instruction. Further impacts cannot restart or duplicate the sound.
                if (door.Phase is GreyDoorPhase.Opening or GreyDoorPhase.ConvertToBlue or GreyDoorPhase.Closing)
                    return false;

                door.PendingProjectileType = projectileType;
                door.HasPendingHit = true;
                return true;
            }
            return false;
        }

        /// <summary>Whether the cartridge's room population installed PLM <c>$D6DE</c>.</summary>
        internal bool MotherBrainGlassWasLoaded => self.MotherBrainGlass._motherBrainGlassWasLoaded;

        /// <summary>Current native room argument, retained after the header deletes itself.</summary>
        internal ushort MotherBrainGlassRoomArgument => MotherBrainGlassSlot(self) is { } slot
            ? slot.RoomArgument
            : self.MotherBrainGlass._motherBrainGlassLastRoomArgument;

        /// <summary>Current bank-$84 instruction pointer, or zero after deletion.</summary>
        internal ushort MotherBrainGlassInstructionPointer => MotherBrainGlassSlot(self) is { } slot
            ? slot.InstructionPointer
            : (ushort)0;

        /// <summary>
        /// Places a constructed program in low work RAM, where the `$84` interpreter executes
        /// wrapped instruction pointers, and redirects the sole live PLM to it.
        /// </summary>
        [CpuBusBoundary("exact bus test: places a constructed PLM program in the bank-$84 low-window WRAM mirror")]
        internal void SetSoleWorkRamProgramForVerification(ISnesAddressSpace memory, ushort pointer,
            params ReadOnlySpan<ushort> instructions)
        {
            if (memory is not ISnesMutableMemory)
                throw new ArgumentException("A work-RAM program needs a live WRAM bus.", nameof(memory));
            if (pointer + instructions.Length * 2 > RoomPlmMemoryLayout.WorkRamMirrorEnd)
                throw new ArgumentOutOfRangeException(nameof(pointer), "The program must lie inside the low WRAM mirror.");
            for (int index = 0; index < instructions.Length; index++)
            {
                int address = (int)new SnesAddress(RoomPlmMemoryLayout.ProgramBank, (ushort)(pointer + index * 2));
                memory.WriteByte(address, (byte)instructions[index]);
                memory.WriteByte(address + 1, SnesLittleEndian.HighByte(instructions[index]));
            }
            self.SetSoleInstructionPointerForVerification(pointer);
        }

        /// <summary>
        /// Runs the production timed-frame draw for the sole live PLM, exactly as the `$84`
        /// interpreter dispatches a timer/draw pair, without injecting a synthetic program.
        /// </summary>
        internal void DrawSolePlmFrameForVerification(ISnesAddressSpace bus, RoomLevelData level,
            BackgroundTilemapStreamer streamer, ushort drawPointer)
        {
            PlmSlot[] active = [.. ActiveSlots(self)];
            if (active.Length != 1)
                throw new InvalidOperationException("Draw probe requires exactly one active PLM slot.");
            self.Drawing.DrawPlmInstruction(bus, level, streamer, active[0].HeaderPointer, active[0].BlockIndex, drawPointer, default(PlmView));
        }

        /// <summary>
        /// Redirects the sole live PLM to a constructed bank-$84 program for interpreter tests.
        /// This is not a gameplay operation: the real setup still owns allocation, block
        /// position, restore word and BTS, while the test controls only the next instruction.
        /// Keeping synthetic opcodes at nonretail addresses avoids rewriting an immutable
        /// compiled cartridge program in a fake address space.
        /// </summary>
        internal void SetSoleInstructionPointerForVerification(ushort instructionPointer)
        {
            PlmSlot[] active = [.. self.SlotTable._slots.Where(slot => slot.Active)];
            if (active.Length != 1)
                throw new InvalidOperationException(
                    "Instruction probe requires exactly one active PLM slot.");
            active[0].InstructionPointer = instructionPointer;
            active[0].InstructionTimer = NativeCountdown.Start(1);
        }

        /// <summary>Current PLM instruction countdown, or zero after deletion.</summary>
        internal ushort MotherBrainGlassInstructionTimer => MotherBrainGlassSlot(self) is { } slot
            ? slot.InstructionTimer.RawValue
            : (ushort)0;

        /// <summary>
        /// Publishes the projectile word to the resident eye controller at a type-$C/BTS-$44
        /// block. The following PLM pre-instruction performs the missile-family filtering.
        /// </summary>
        internal bool TryNotifyEyeDoorHit(
            int blockIndex,
            SamusProjectileTypeWord projectileType)
        {
            foreach (var slot in self.SlotTable._slots)
            {
                if (!slot.Active || slot.BlockIndex != blockIndex ||
                    slot.EyeDoor is not { } door ||
                    door.Component != EyeDoorComponent.Eye)
                {
                    continue;
                }

                // PLM_Timers is a single pending word. A later collision before the handler
                // pass replaces it rather than accumulating a host-side queue.
                slot.LoopTimer = projectileType.Raw;
                door.HasPendingHit = true;
                return true;
            }
            return false;
        }

        /// <summary>Every live elevator-platform PLM in native physical-slot order.</summary>
        internal IReadOnlyList<RoomPlmSlotSnapshot> ElevatorPlatforms => self.PopulationSlots
            .Where(snapshot => self.SlotTable._slots[snapshot.NativeSlotIndex].IsElevatorPlatform)
            .ToArray();

        /// <summary>Physical-slot views in cartridge handler order, highest ID first.</summary>
        internal IReadOnlyList<RoomPlmSlotSnapshot> PopulationSlots
        {
            get
            {
                var snapshots = new RoomPlmSlotSnapshot[self.SlotTable.ActiveCount];
                int next = 0;
                for (int index = self.SlotTable._slots.Length - 1; index >= 0; index--)
                    if (self.SlotTable._slots[index].Active) snapshots[next++] = self.Snapshot(index);
                return snapshots;
            }
        }

        /// <summary>Reads one physical PLM slot's header, block, argument, and active state.</summary>
        internal RoomPlmSlotSnapshot Snapshot(int index)
        {
            PlmSlot slot = self.SlotTable._slots[index];
            return new RoomPlmSlotSnapshot(
                NativeSlotIndex: index,
                HeaderPointer: slot.HeaderPointer,
                BlockIndex: slot.BlockIndex,
                RoomArgument: slot.RoomArgument,
                InstructionPointer: slot.InstructionPointer,
                PreInstruction: slot.PreInstruction,
                InstructionTimer: slot.InstructionTimer.RawValue,
                LinkInstruction: slot.LinkInstruction,
                LoopTimer: slot.LoopTimer);
        }

        /// <summary>Visible BG1 mutations emitted during the most recent handler pass.</summary>
        internal IReadOnlyList<PlmTilemapUpdate> TilemapUpdates => self.Drawing._tilemapUpdates;

        /// <summary>
        /// Publishes contact with BTS $47-$4D to its resident station. This is the same generic
        /// type-$B collision seam used by item and scroll PLMs; no room identity participates.
        /// </summary>
        internal bool TryNotifyStationTouch(int accessBlockIndex, byte behavior)
            => self.TryNotifyStationTouch(accessBlockIndex, new RoomBlockBehavior(behavior));

        /// <summary>Typed BTS overload used by room collision dispatch.</summary>
        internal bool TryNotifyStationTouch(int accessBlockIndex, RoomBlockBehavior behavior)
            => self.Stations.TryNotifyStationCollision(accessBlockIndex, behavior, (SamusPoseId)byte.MaxValue, true, true, 0, true);

        /// <summary>
        /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
        /// </summary>
        /// <returns>False only when all 40 native slots are occupied.</returns>
        internal bool TrySpawnBreakableGrappleBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior)
            => self.BlockReactions.TrySpawnBreakableGrappleBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior));

        /// <summary>
        /// Spawns the bank-$84 collision PLM selected by type-$F BTS zero through seven.
        /// </summary>
        /// <remarks>
        /// The caller has already satisfied setup <c>$84:CE83</c>'s speed/screw pose gate. Setup
        /// saves <c>(levelWord &amp; $F000) | $0058</c> in <c>PLM_Vars</c>, then clears only the
        /// collision nibble in level data and returns carry clear so Samus continues moving.
        /// BTS 0..3 later redraw a linked 1x1/2x1/1x2/2x2 collision shape after the exact
        /// 384-frame hold; BTS 4..7 delete after their four-frame break animation.
        /// </remarks>
        /// <returns>
        /// True when a native slot was allocated. False preserves <c>Spawn_PLM</c>'s full-pool
        /// behavior: setup never ran, so the level word remains untouched even though bank $94
        /// inherited carry clear and lets the current movement scan continue.
        /// </returns>
        internal bool TrySpawnCollisionBombBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior)
            => self.BlockReactions.TrySpawnCollisionBombBlock(level, blockIndex, new RoomBlockBehavior(behavior));

        /// <summary>
        /// Spawns the bank-$84 shot/bombed/grappled-reaction PLM selected by bombable BTS.
        /// </summary>
        /// <remarks>
        /// This is setup <c>$84:CEDA</c>, not the collision setup above. A normal bomb family
        /// (<c>$0500</c>) advances the entry instruction pointer by three bytes, deliberately
        /// skipping its leading sound-$0A opcode because the bomb explosion already owns its
        /// sound. A power bomb (<c>$0300</c>) retains that opcode. Both accepted projectile
        /// families synthesize <c>(levelWord &amp; $F000) | $0058</c> for later restoration and
        /// then apply <c>AND $8FFF</c> to live terrain. Thus a type-$F solid bomb block remains
        /// temporarily type-$8 solid until the same frame's PLM pass draws its first air frame,
        /// while a type-$7 bombable-air parent becomes ordinary air immediately.
        ///
        /// BTS 8..15 point at <c>PLMEntries_nothing</c>. Native code still allocates a slot and
        /// deletes it on the next handler pass, so this implementation retains that otherwise
        /// invisible resource/timing effect. A negative BTS is filtered by bank $94 before this
        /// method is called because it denotes an area-dependent/duplicate path.
        /// </remarks>
        /// <returns>False only when all 40 native slots are occupied.</returns>
        internal bool TrySpawnBombReactionBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior,
            SamusProjectileTypeWord projectileType)
            => self.BlockReactions.TrySpawnBombReactionBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
                projectileType);

        /// <summary>
        /// Runs the normal-bomb branch of the shootable-air/block entries selected at
        /// <c>$94:9EA6</c> and installs their exact bank-$84 instruction list.
        /// </summary>
        /// <remarks>
        /// BTS 0..3 use setup <c>$84:CE6B</c>: synthesize <c>$x052</c> for restoration and
        /// apply <c>AND $8FFF</c> to the live word. BTS 4..7 use setup <c>$84:B3C1</c>, which
        /// applies that AND directly to the original word and never restores it. BTS 8/9 and
        /// A/B normally require a power bomb or super missile; a normal bomb redirects to the
        /// tiny reveal lists at <c>$C91C/$C922</c>. BTS C..F still allocate the retail
        /// <c>PLMEntries_nothing</c> slot and delete it during the next handler pass. Negative
        /// type-$C BTS uses an eight-entry area table whose retail entries are also all no-ops;
        /// it retains that allocation even though type-$4 takes an early return in bank $94.
        /// </remarks>
        internal bool TrySpawnBombedShootableBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior,
            SamusProjectileTypeWord projectileType)
            => self.TrySpawnBombedShootableBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
                projectileType);

        /// <summary>Typed BTS overload used by bomb collision dispatch.</summary>
        internal bool TrySpawnBombedShootableBlock(
            RoomLevelData level,
            int blockIndex,
            RoomBlockBehavior bts,
            SamusProjectileTypeWord projectileType)
        {
            Ensure.NotNull(level);
            bool areaDependent = bts.UsesAreaReactionTable;
            if (areaDependent && !bts.IsAreaReactionIndex(8))
            {
                throw new ArgumentOutOfRangeException(
                    "bts",
                    "Area-dependent shootable BTS must address one of its eight native entries.");
            }
            if (!areaDependent && !bts.IsNormalReactionIndex(16))
            {
                throw new ArgumentOutOfRangeException(
                    "bts",
                    "Translated normal-bomb shootable BTS must be in range zero through fifteen.");
            }
            if (projectileType.Family != SamusProjectileFamily.Bomb)
            {
                throw new ArgumentOutOfRangeException(
                    "projectileType",
                    "This setup translation currently accepts the normal-bomb family only.");
            }

            // Spawn_PLM's descending free-slot search happens before any setup routine. A full
            // pool must therefore leave both level data and BTS completely untouched.
            for (int slotIndex = self.SlotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
            {
                PlmSlot slot = self.SlotTable._slots[slotIndex];
                if (slot.Active)
                    continue;

                RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
                PlmPopulationLoader.ClearSlot(slot);
                slot.Active = true;
                slot.BlockIndex = blockIndex;
                slot.InstructionTimer = NativeCountdown.Start(1);
                slot.RestoreLevelWord = 0;

                if (areaDependent)
                {
                    // `$94:9E8D-$9E9E` indexes the current area's eight-entry table before
                    // Spawn_PLM. Every retail entry at `$94:9F46-$9FC4` is PLMEntries_nothing.
                    slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                    return true;
                }

                if (bts.IsRespawningReaction)
                {
                    // CE6B throws away the original visual/BTS low twelve bits. The generated
                    // `$x052` word is both PLM_Vars and the source of the temporary collision
                    // word, exactly like the retail setup's two consecutive stores.
                    slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052));
                    slot.InstructionPointer = RoomPlmInstructionLists.RespawningShotBySize(bts.NormalReactionIndex);
                    level.SetForegroundEntry(
                        blockIndex,
                        unchecked((ushort)(slot.RestoreLevelWord & 0x8fff)));
                    return true;
                }

                if (bts.IsPermanentReaction)
                {
                    // Setup_DeactivatePLM does not synthesize a restore word. Clearing bits
                    // `$7000` turns type-$4 air into ordinary air and type-$C solid into type-$8
                    // solid until the same-frame list draws its first breaking frame.
                    slot.InstructionPointer = RoomPlmInstructionLists.PermanentShotBySize(bts.NormalReactionIndex - 4);
                    level.SetForegroundEntry(
                        blockIndex,
                        unchecked((ushort)(block.LevelWord & 0x8fff)));
                    return true;
                }

                if (bts.RequiresPowerBombReaction)
                {
                    // CF2E sees projectile family `$0500` and replaces the entry's normal
                    // power-bomb animation pointer with the one-frame visible `$C057` reveal.
                    slot.InstructionPointer = RoomPlmInstructionLists.BombedPowerBombBlockUnused;
                    return true;
                }

                if (bts.RequiresSuperMissileReaction)
                {
                    // CF67 performs the analogous redirect to visible super-missile word
                    // `$C09F`; it neither clears collision nor queues the shot-block sound.
                    slot.InstructionPointer = RoomPlmInstructionLists.BombedSuperMissileBlockUnused;
                    return true;
                }

                slot.InstructionPointer = RoomPlmInstructionLists.Delete;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Spawns the projectile reaction selected by a type-$4/$C shootable block's BTS byte.
        /// </summary>
        /// <remarks>
        /// `$94:9E55/$9E73` share table `$94:9EA6` for beams, missiles, bombs, and grapple.
        /// Entries zero through three select `$84:CE6B`'s respawning shot-block setup; entries
        /// four through seven select `$84:B3C1`'s permanent deactivation setup. Entries eight
        /// and nine run `$84:CF2E`'s power-bomb-family gate, while A and B run `$84:CF67`'s
        /// Super-Missile-family gate. Entries C through F are the retail no-op PLM header,
        /// and the deliberate seventeenth entry $10 selects one-frame header `$84:B974`.
        ///
        /// Negative BTS behaves differently for the two collision nibbles. Shootable air exits
        /// without spawning anything, while shootable solid indexes an area table whose retail
        /// entries are all `PLMEntries_nothing`; preserve that otherwise invisible allocation.
        /// The normal-bomb `$0500` reveal redirects are retained because bombed block reactions
        /// call the same table. All other rejected families reproduce setup's cleared PLM header:
        /// no terrain mutation and no live slot survives the synchronous Spawn_PLM call.
        /// </remarks>
        internal bool TrySpawnProjectileShotBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior,
            SamusProjectileTypeWord projectileType,
            bool solidBlock)
            => self.BlockReactions.TrySpawnProjectileShotBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
                projectileType,
                solidBlock);

        /// <summary>
        /// Spawns the special-block reveal selected by <c>$94:9D71-$9E53</c> for a bomb-family
        /// boundary visit.
        /// </summary>
        /// <remarks>
        /// Nonnegative BTS 0..7 selects a dimensioned crumble reveal, 8..D selects the native
        /// no-op entry, and E/F reveals a speed-booster block. A negative BTS selects an
        /// eight-word area table: only Brinstar entries 2..5 reveal speed blocks; every other
        /// bomb-special area entry is <c>PLMEntries_nothing</c>. Setup <c>$84:CFA0</c> accepts
        /// normal bombs without mutating terrain, so the visible type-$B word arrives on the
        /// first PLM handler pass and the object deletes on the following pass. Power Bomb
        /// family `$0300` still reaches Spawn_PLM, but `$84:CFA0` clears the new PLM header
        /// synchronously; observably no terrain changes and no active slot survives.
        /// </remarks>
        internal bool TrySpawnBombedSpecialBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior,
            AreaId areaIndex,
            SamusProjectileTypeWord projectileType)
            => self.BlockReactions.TrySpawnBombedSpecialBlock(
                level,
                blockIndex,
                new RoomBlockBehavior(behavior),
                areaIndex,
                projectileType);

        /// <summary>
        /// Executes <c>PLM_Handler</c>'s timer/instruction portion for all translated slots.
        /// </summary>
        /// <remarks>
        /// The caller supplies current layer-1 coordinates because native DrawPLM clips before
        /// queuing VRAM work. Level-data writes always occur; only the PPU-ring update is clipped.
        /// Returned updates are also exposed through the verification <c>TilemapUpdates</c> property so a debugger
        /// can inspect the exact block and destination before the runtime executes them.
        /// </remarks>
        internal IReadOnlyList<PlmTilemapUpdate> Step(
            ISnesAddressSpace bus,
            RoomLevelData level,
            BackgroundTilemapStreamer streamer,
            PlmView view,
            RoomScrollGrid? scrolls = null,
            ushort enemyDeaths = 0,
            byte enemyDeathQuota = 0)
            => self.Step(bus, level, streamer, view, new PlmFrameInputs { Scrolls = scrolls, EnemyDeaths = enemyDeaths, EnemyDeathQuota = enemyDeathQuota, ControllerNewInput = 0 });
    }

    /// <summary>Filters the physical PLM pool to slots whose active bit is set.</summary>
    private static IEnumerable<PlmSlot> ActiveSlots(RoomPlmSystem plms) =>
        plms.SlotTable._slots.Where(slot => slot.Active);

    /// <summary>The live Mother Brain glass PLM slot, as the system's own slot search finds it.</summary>
    private static PlmSlot? MotherBrainGlassSlot(RoomPlmSystem plms) =>
        plms.MotherBrainGlass.TryGetMotherBrainGlassSlot(out PlmSlot? slot) ? slot : null;
}

/// <summary>Verification access to <see cref="RoomPlmTourianAccessVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmTourianAccessVisualCatalogAccess
{
    /// <summary>Derives Tourian access-row art from physical draw words, retaining only edited frames.</summary>
    extension(RoomPlmTourianAccessVisualCatalog)
    {
        /// <summary>The stock catalog the repository installation loads, with no override selected.</summary>
        internal static RoomPlmTourianAccessVisualCatalog Stock() => RepositoryInstallation.Installation.LoadRoomPlmTourianAccessVisuals();
    }
}

/// <summary>Verification access to <see cref="RoomVisualLayoutCatalog"/> members production does not use.</summary>
internal static class RoomVisualLayoutCatalogAccess
{
    /// <summary>Constructs partial room-geometry layouts for focused visual fixtures.</summary>
    extension(RoomVisualLayoutCatalog)
    {
        /// <summary>Explicitly partial geometry fixtures; never an installed production catalog.</summary>
        internal static RoomVisualLayoutCatalog FromLayoutsForVerification(
            IReadOnlyDictionary<int, RoomVisualLayout> layouts) =>
            new RoomVisualLayoutCatalog(layouts, false);
    }
}

/// <summary>Verification access to <see cref="SpeedBoosterBlockPlmDrawDefinitions"/> members production does not use.</summary>
internal static class SpeedBoosterBlockPlmDrawDefinitionsAccess
{
    /// <summary>Resolves the speed-block bomb-reveal draw from its native pointer.</summary>
    extension(SpeedBoosterBlockPlmDrawDefinitions)
    {
        /// <summary>Recognizes the speed-block bomb-reveal draw pointer.</summary>
        internal static bool TryGet(ushort pointer,
            out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (pointer == SpeedBoosterBlockPlmProgramDefinitions.BombRevealDraw)
            {
                list = SpeedBoosterBlockPlmDrawDefinitions.BombReveal;
                return true;
            }
            list = default;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="SpeedBoosterBlockPlmProgramDefinitions"/> members production does not use.</summary>
internal static class SpeedBoosterBlockPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates speed-block reveal, timed frames, terminal commands, and packed setup bytes.</summary>
    extension(SpeedBoosterBlockPlmProgramDefinitions)
    {
        /// <summary>Visits bomb reveal, timed frame, terminal, and optional respawn words.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            yield return SpeedBoosterBlockPlmProgramDefinitions.BombReveal;
            yield return checked((ushort)(SpeedBoosterBlockPlmProgramDefinitions.BombReveal + 2));
            yield return checked((ushort)(SpeedBoosterBlockPlmProgramDefinitions.BombReveal + 4));
            for (int index = 0; index < SpeedBoosterBlockPlmProgramDefinitions.ProgramCount; index++)
            {
                var program = SpeedBoosterBlockPlmProgramDefinitions.ProgramAt((int)(index));
                yield return program.Start;
                for (int frame = 0; frame < program.FrameCount; frame++)
                {
                    yield return checked((ushort)(program.Start + 3 + frame * 4));
                    yield return checked((ushort)(program.Start + 5 + frame * 4));
                }
                yield return program.Terminal;
                if (program.Respawns)
                    yield return checked((ushort)(program.Terminal + 2));
            }
        }

        /// <summary>Locates each speed-block program's byte operand after its start word.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < SpeedBoosterBlockPlmProgramDefinitions.ProgramCount; index++)
                yield return checked((ushort)((SpeedBoosterBlockPlmProgramDefinitions.ProgramAt((int)(index))).Start + 2));
        }
    }
}

/// <summary>Verification access to <see cref="SporeSpawnCeilingPlmProgramDefinitions"/> members production does not use.</summary>
internal static class SporeSpawnCeilingPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates ceiling-crumble frame words and its final clear-state list.</summary>
    extension(SporeSpawnCeilingPlmProgramDefinitions)
    {
        /// <summary>Visits ceiling-crumble frame words followed by the clear-state list.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            yield return SporeSpawnCeilingPlmProgramDefinitions.Crumble;
            for (int frame = 0; frame < 3; frame++)
            {
                yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Crumble + 3 + frame * 4));
                yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Crumble + 5 + frame * 4));
            }
            yield return SporeSpawnCeilingPlmProgramDefinitions.Clear;
            yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Clear + 2));
            yield return checked((ushort)(SporeSpawnCeilingPlmProgramDefinitions.Clear + 4));
        }
    }
}

/// <summary>Verification access to <see cref="StationAccessPlmDefinitions"/> members production does not use.</summary>
internal static class StationAccessPlmDefinitionsAccess
{
    /// <summary>Lists map, energy, and missile station access cases in native side/BTS order.</summary>
    extension(StationAccessPlmDefinitions)
    {
        /// <summary>Enumerate the six named station/side cases in their native BTS order.</summary>
        internal static IEnumerable<StationAccessPlmDefinition> All
        {
            get
            {
                yield return StationAccessPlmDefinitions.MapRight;
                yield return StationAccessPlmDefinitions.MapLeft;
                yield return StationAccessPlmDefinitions.EnergyRight;
                yield return StationAccessPlmDefinitions.EnergyLeft;
                yield return StationAccessPlmDefinitions.MissileRight;
                yield return StationAccessPlmDefinitions.MissileLeft;
            }
        }
    }
}

/// <summary>Verification access to <see cref="StationAnimationProgramDefinitions"/> members production does not use.</summary>
internal static class StationAnimationProgramDefinitionsAccess
{
    /// <summary>Pairs station animation list identities with every frame's native duration and draw word.</summary>
    extension(StationAnimationProgramDefinitions)
    {
        /// <summary>Enumerates the station animation list identities for the native-word audit.</summary>
        internal static IEnumerable<StationAnimationList> Lists() => Enum.GetValues<StationAnimationList>();

        /// <summary>Visits each station frame's duration and draw pointer in list order.</summary>
        internal static IEnumerable<(ushort Address, ushort Value)> NativeWords()
        {
            foreach (StationAnimationList list in StationAnimationProgramDefinitions.Lists())
            for (int index = 0; index < StationAnimationProgramDefinitions.FrameCount(list); index++)
            {
                StationAnimationProgramDefinitions.Frame frame = StationAnimationProgramDefinitions.Resolve(list, index);
                yield return (checked((ushort)((ushort)list + 4 * index)), frame.Duration);
                yield return (checked((ushort)((ushort)list + 4 * index + 2)), frame.DrawPointer);
            }
        }
    }
}

/// <summary>Verification access to <see cref="TourianAccessPlmProgramDefinitions"/> members production does not use.</summary>
internal static class TourianAccessPlmProgramDefinitionsAccess
{
    /// <summary>Enumerates Tourian access crumble frames and terminal clear commands.</summary>
    extension(TourianAccessPlmProgramDefinitions)
    {
        /// <summary>Visits the Tourian access crumble frames and terminal clear commands.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses()
        {
            yield return TourianAccessPlmProgramDefinitions.Crumble;
            for (int frame = 0; frame < 4; frame++)
            {
                yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 3 + frame * 4));
                yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 5 + frame * 4));
            }
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 19));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 21));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 23));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Crumble + 25));
            yield return TourianAccessPlmProgramDefinitions.Clear;
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Clear + 2));
            yield return checked((ushort)(TourianAccessPlmProgramDefinitions.Clear + 4));
        }
    }
}

/// <summary>Verification access to <see cref="XrayOverlayVisualCatalog"/> members production does not use.</summary>
internal static class XrayOverlayVisualCatalogAccess
{
    /// <summary>Constructs partial X-Ray room overlay catalogs for focused rendering fixtures.</summary>
    extension(XrayOverlayVisualCatalog)
    {
        /// <summary>Explicitly partial room overlays for focused fixtures; never an installed catalog.</summary>
        internal static XrayOverlayVisualCatalog FromOverlaysForVerification(IEnumerable<ushort> itemMetatiles,
            IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms) =>
            new XrayOverlayVisualCatalog(itemMetatiles, rooms, false);
    }
}

/// <summary>Verification access to the cartridge encoding of <see cref="CartridgeDoorOrientation"/>.</summary>
internal static class CartridgeDoorOrientationAccess
{
    /// <summary>Encodes a typed door direction and closing flag into the cartridge's orientation byte.</summary>
    extension(CartridgeDoorOrientation orientation)
    {
        /// <summary>The cartridge byte, as <c>$82:DE2C</c> stores it in <c>DoorDirection</c>.</summary>
        internal byte Encode() => unchecked((byte)((byte)orientation.Closing << 2 | (byte)orientation.Direction));
    }
}

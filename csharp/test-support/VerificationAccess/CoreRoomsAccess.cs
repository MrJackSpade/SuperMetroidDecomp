using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using System.Buffers.Binary;

/// <summary>Verification access to <see cref="BotwoonWallPlmProgramDefinitions"/> members production does not use.</summary>
internal static class BotwoonWallPlmProgramDefinitionsAccess
{
    /// <summary>Exposes Botwoon wall PLM program metadata to cartridge verification fixtures.</summary>
    extension(BotwoonWallPlmProgramDefinitions)
    {
        /// <summary>Enumerates the native instruction words read by the Botwoon wall program.</summary>
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
    /// <summary>Provides fixture access to Chozo statue PLM definitions used by room checks.</summary>
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
    /// <summary>Exposes Crocomire arena instruction data for native-address verification.</summary>
    extension(CrocomireArenaPlmProgramDefinitions)
    {
        /// <summary>Enumerates the native instruction words read by the Crocomire arena program.</summary>
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
    /// <summary>Reads door-closing PLM ROM metadata for focused verification.</summary>
    extension(DoorClosingPlmRomData)
    {
        /// <summary>Returns the exact bank-$84 header selected by a retail door direction.</summary>
        internal static ushort GetHeader(byte direction) => DoorClosingPlmRomData.GetDefinition(direction).Header;
    }
}

/// <summary>Verification access to <see cref="DoorScrollPrograms"/> members production does not use.</summary>
internal static class DoorScrollProgramsAccess
{
    /// <summary>Provides test access to the instruction streams that drive door scrolling.</summary>
    extension(DoorScrollPrograms)
    {
        /// <summary>Original callback catalog order, generated without a stored registration table.</summary>
        internal static IEnumerable<ushort> Pointers
        {
            get
            {
                yield return DoorCodes.DoorCode_Scroll6_Green;
                yield return DoorCodes.DoorASM_Scroll_0_Blue;
                yield return DoorCodes.DoorASM_Scroll_13_Blue;
                yield return DoorCodes.DoorASM_Scroll_4_Red_8_Green;
                yield return DoorCodes.DoorASM_Scroll_8_9_A_B_Red;
                yield return DoorCodes.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red;
                yield return DoorCodes.DoorASM_Scroll_1_4_Green;
                yield return DoorCodes.DoorASM_Scroll_2_Blue;
                yield return DoorCodes.DoorASM_Scroll_17_Blue;
                yield return DoorCodes.DoorASM_Scroll_4_Blue;
                yield return DoorCodes.DoorASM_Scroll_6_Green_duplicate;
                yield return DoorCodes.DoorASM_Scroll_3_Green;
                yield return DoorCodes.DoorASM_Scroll_18_1C_Green;
                yield return DoorCodes.DoorASM_Scroll_5_6_Blue;
                yield return DoorCodes.DoorASM_Scroll_1D_Blue;
                yield return DoorCodes.DoorASM_Scroll_2_3_Green;
                yield return DoorCodes.DoorASM_Scroll_0_Red_1_Green;
                yield return DoorCodes.DoorASM_Scroll_B_Green;
                yield return DoorCodes.DoorASM_Scroll_Scroll_1C_Red_1D_Blue;
                yield return DoorCodes.DoorASM_Scroll_4_Red;
                yield return DoorCodes.DoorASM_Scroll_20_24_25_Green;
                yield return DoorCodes.DoorASM_Scroll_2_Blue_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_Green;
                yield return DoorCodes.DoorASM_Scroll_6_7_Green;
                yield return DoorCodes.DoorASM_Scroll_1_Blue_2_Red;
                yield return DoorCodes.DoorASM_Scroll_1_Blue_3_Red;
                yield return DoorCodes.DoorASM_Scroll_0_Red_4_Blue;
                yield return DoorCodes.DoorASM_Scroll_2_3_Blue;
                yield return DoorCodes.DoorASM_Scroll_0_1_Green;
                yield return DoorCodes.DoorASM_Scroll_1_Green;
                yield return DoorCodes.DoorASM_Scroll_F_12_Green;
                yield return DoorCodes.DoorASM_Scroll_6_Green_duplicate_again;
                yield return DoorCodes.DoorASM_Scroll_0_Green_1_Blue;
                yield return DoorCodes.DoorASM_Scroll_2_Green;
                yield return DoorCodes.DoorASM_Scroll_3_4_Red_6_7_8_Blue;
                yield return DoorCodes.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red;
                yield return DoorCodes.DoorASM_Scroll_0_1_Blue;
                yield return DoorCodes.DoorASM_Scroll_0_Blue_1_Red;
                yield return DoorCodes.DoorASM_Scroll_A_Green;
                yield return DoorCodes.DoorASM_Scroll_0_2_Green;
                yield return DoorCodes.DoorASM_Scroll_6_7_Blue_8_Red;
                yield return DoorCodes.DoorASM_Scroll_2_Red_3_Blue;
                yield return DoorCodes.DoorASM_Scroll_7_Green;
                yield return DoorCodes.DoorASM_Scroll_1_Red_2_Blue;
                yield return DoorCodes.DoorASM_Scroll_0_Blue_3_Red;
                yield return DoorCodes.DoorASM_Scroll_1_Blue_4_Red;
                yield return DoorCodes.DoorASM_Scroll_0_Blue_1_2_3_Red;
                yield return DoorCodes.DoorASM_Scroll_0_Green_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_1_Blue_4_Red;
                yield return DoorCodes.DoorASM_Scroll_0_Blue_3_Red_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_Blue_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_Blue_1_Red_duplicate;
                yield return DoorCodes.DoorASM_Scroll_18_Blue;
                yield return DoorCodes.DoorASM_Scroll_2_Blue_3_Red;
                yield return DoorCodes.DoorASM_Scroll_E_Red;
                yield return DoorCodes.DoorASM_Scroll_1_Blue;
                yield return DoorCodes.DoorASM_Scroll_0_Green_duplicate_again;
                yield return DoorCodes.DoorASM_Scroll_3_Red_4_Blue;
                yield return DoorCodes.DoorASM_Scroll_29_Blue;
                yield return DoorCodes.DoorASM_Scroll_28_2E_Green;
                yield return DoorCodes.DoorASM_Scroll_6_7_8_9_A_B_Red;
                yield return DoorCodes.DoorASM_Scroll_A_Red_B_Blue;
                yield return DoorCodes.DoorASM_Scroll_0_Red_4_Blue_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_Red_1_Blue;
                yield return DoorCodes.DoorASM_Scroll_9_Red_A_Blue;
                yield return DoorCodes.DoorASM_Scroll_0_2_Red_1_Blue;
                yield return DoorCodes.DoorASM_Scroll_1_Blue_duplicate;
                yield return DoorCodes.DoorASM_Scroll_6_Blue;
                yield return DoorCodes.DoorASM_Scroll_4_Red_duplicate;
                yield return DoorCodes.DoorASM_Scroll_4_7_Red;
                yield return DoorCodes.DoorASM_Scroll_1_Blue_2_Red_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_2_Green_duplicate;
                yield return DoorCodes.DoorASM_Scroll_0_1_Green_duplicate;
                yield return DoorCodes.DoorASM_Scroll_18_Blue_19_Red;
            }
        }
    }
}

/// <summary>Verification access to <see cref="DownwardGatePlmProgramDefinitions"/> members production does not use.</summary>
internal static class DownwardGatePlmProgramDefinitionsAccess
{
    /// <summary>$84:BC61: adjacent upward-gate program, outside the downward-gate decoder.</summary>
    private const ushort ResidentEnd = 0xbc61;

    /// <summary>Exposes downward-gate animation words for native-data checks.</summary>
    extension(DownwardGatePlmProgramDefinitions)
    {
        /// <summary>Lists native words that supply data to the downward-gate mechanics program.</summary>
        internal static IEnumerable<(ushort Address, ushort Value)> MechanicsWords
        {
            get
            {
                for (int address = PrivateState.StaticField<ushort>(typeof(DownwardGatePlmProgramDefinitions), "OpenStart"); address < ResidentEnd; address++)
                    if (DownwardGatePlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
                for (int address = RoomPlmInstructionLists.DownwardGateShotBlockBlueLeft; address < PrivateState.StaticField<ushort>(typeof(DownwardGatePlmProgramDefinitions), "TriggerEnd"); address += 2)
                    if (DownwardGatePlmProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value)) yield return ((ushort)address, value);
            }
        }

        /// <summary>Lists native bytes that supply control data to the downward-gate mechanics program.</summary>
        internal static IEnumerable<(ushort Address, byte Value)> MechanicsBytes
        {
            get
            {
                yield return (PrivateState.StaticField<ushort>(typeof(DownwardGatePlmProgramDefinitions), "ClosingSoundAddress"), DownwardGatePlmRomData.MovementSound);
                yield return (PrivateState.StaticField<ushort>(typeof(DownwardGatePlmProgramDefinitions), "OpeningSoundAddress"), DownwardGatePlmRomData.MovementSound);
            }
        }
    }
}

/// <summary>Verification access to <see cref="KraidRoomPlmProgramDefinitions"/> members production does not use.</summary>
internal static class KraidRoomPlmProgramDefinitionsAccess
{
    /// <summary>Provides fixture access to Kraid room PLM instruction addresses.</summary>
    extension(KraidRoomPlmProgramDefinitions)
    {
        /// <summary>Enumerates instruction words consumed by Kraid room PLM programs.</summary>
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
    /// <summary>Builds controlled Landing Site entry state for room initialization checks.</summary>
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
            ArgumentNullException.ThrowIfNull(bus);
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
                RoomMapX: room.MapX,
                RoomMapY: room.MapY,
                UpScroller: room.UpScroller,
                DownScroller: room.DownScroller);
        }
    }
}

/// <summary>Verification access to <see cref="LibraryBackgroundProgramDefinitions"/> members production does not use.</summary>
internal static class LibraryBackgroundProgramDefinitionsAccess
{
    /// <summary>Exposes the Library background animation data used by verification fixtures.</summary>
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
    /// <summary>Provides access to draw-list metadata for Mother Brain's fake-death PLMs.</summary>
    extension(MotherBrainFakeDeathPlmDrawDefinitions)
    {
        /// <summary>Finds the translated draw list for a native fake-death draw pointer.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList draw)
        {
            // Native order: background, regular, then boundary draw families.
            foreach (string family in new[] { "TryGetBackground", "TryGetRegular", "TryGetBoundary" })
            {
                object?[] lookup = [pointer, null];
                if ((bool)PrivateState.InvokeStaticWithOut(typeof(MotherBrainFakeDeathPlmDrawDefinitions), family, lookup)!)
                {
                    draw = (RoomPlmShotBlockDrawDefinitions.DrawList)lookup[1]!;
                    return true;
                }
            }
            draw = default;
            return false;
        }
    }
}

/// <summary>Verification access to <see cref="MotherBrainFakeDeathPlmProgramDefinitions"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathPlmProgramDefinitionsAccess
{
    /// <summary>Exposes fake-death PLM instruction addresses for native-data verification.</summary>
    extension(MotherBrainFakeDeathPlmProgramDefinitions)
    {
        /// <summary>Reports how many fixed-size fake-death programs are represented by the address range.</summary>
        internal static int ProgramCount => (MotherBrainFakeDeathPlmProgramDefinitions.EndExclusive - MotherBrainFakeDeathPlmProgramDefinitions.Start) / MotherBrainFakeDeathPlmProgramDefinitions.ProgramByteLength;

        /// <summary>Enumerates native instruction words read from the fake-death program table.</summary>
        internal static IEnumerable<ushort> NativeWordAddresses() =>
            Enumerable.Range(0, MotherBrainFakeDeathPlmProgramDefinitions.ProgramCount * 3)
                .Select(index => checked((ushort)(MotherBrainFakeDeathPlmProgramDefinitions.Start + index * 2)));
    }
}

/// <summary>Verification access to <see cref="NoobTubePlmProgramDefinitions"/> members production does not use.</summary>
internal static class NoobTubePlmProgramDefinitionsAccess
{
    /// <summary>Provides fixture access to the Noob Tube PLM instruction stream.</summary>
    extension(NoobTubePlmProgramDefinitions)
    {
        /// <summary>Enumerates Noob Tube words that affect collision or room mechanics.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int offset = 0; offset <= 0x30; offset += 2)
                yield return checked((ushort)(PrivateState.StaticField<ushort>(typeof(NoobTubePlmProgramDefinitions), "MainStart") + offset));
            for (int offset = 0; offset <= 0x10; offset += 2)
                yield return checked((ushort)(PrivateState.StaticField<ushort>(typeof(NoobTubePlmProgramDefinitions), "MainAfterSound") + offset));
            yield return PrivateState.StaticField<ushort>(typeof(NoobTubePlmProgramDefinitions), "AlreadyBrokenStart");
            yield return checked((ushort)(PrivateState.StaticField<ushort>(typeof(NoobTubePlmProgramDefinitions), "AlreadyBrokenStart") + 2));
        }

        /// <summary>Enumerates Noob Tube bytes used as timing or mechanics control data.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            yield return PrivateState.StaticField<ushort>(typeof(NoobTubePlmProgramDefinitions), "BreakSoundAddress");
        }
    }
}

/// <summary>Verification access to <see cref="ResidentDoorClosingDefinitions"/> members production does not use.</summary>
internal static class ResidentDoorClosingDefinitionsAccess
{
    /// <summary>Exposes resident-door close programs and their native pointer tables.</summary>
    extension(ResidentDoorClosingDefinitions)
    {
        /// <summary>Enumerates the twenty supported identities in original header order without cached records.</summary>
        internal static IEnumerable<ResidentDoorClosingDefinition> All
        {
            get
            {
                yield return new(RoomPlmHeaders.BombTorizoGreyDoor, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.BombTorizoGreyDoor));
                yield return new(RoomPlmHeaders.GreyDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreyDoorFacingLeft));
                yield return new(RoomPlmHeaders.GreyDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreyDoorFacingRight));
                yield return new(RoomPlmHeaders.GreyDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreyDoorFacingUp));
                yield return new(RoomPlmHeaders.GreyDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreyDoorFacingDown));
                yield return new(RoomPlmHeaders.YellowDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.YellowDoorFacingLeft));
                yield return new(RoomPlmHeaders.YellowDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.YellowDoorFacingRight));
                yield return new(RoomPlmHeaders.YellowDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.YellowDoorFacingUp));
                yield return new(RoomPlmHeaders.YellowDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.YellowDoorFacingDown));
                yield return new(RoomPlmHeaders.GreenDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreenDoorFacingLeft));
                yield return new(RoomPlmHeaders.GreenDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreenDoorFacingRight));
                yield return new(RoomPlmHeaders.GreenDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreenDoorFacingUp));
                yield return new(RoomPlmHeaders.GreenDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.GreenDoorFacingDown));
                yield return new(RoomPlmHeaders.RedDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.RedDoorFacingLeft));
                yield return new(RoomPlmHeaders.RedDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.RedDoorFacingRight));
                yield return new(RoomPlmHeaders.RedDoorFacingUp, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.RedDoorFacingUp));
                yield return new(RoomPlmHeaders.RedDoorFacingDown, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.RedDoorFacingDown));
                yield return new(RoomPlmHeaders.MotherBrainEscapeRoomGate, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.MotherBrainEscapeRoomGate));
                yield return new(RoomPlmHeaders.EyeDoorFacingRight, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.EyeDoorFacingRight));
                yield return new(RoomPlmHeaders.EyeDoorFacingLeft, ResidentDoorClosingDefinitions.Resolve(RoomPlmHeaders.EyeDoorFacingLeft));
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomCallbackDefinitions"/> members production does not use.</summary>
internal static class RoomCallbackDefinitionsAccess
{
    /// <summary>Maps room callback routines to their native pointers for verification.</summary>
    extension(RoomCallbackDefinitions)
    {
        /// <summary>Maps a translated main callback to its cartridge callback pointer.</summary>
        internal static ushort PointerOf(RoomMainCallback callback) => callback switch
        {
            RoomMainCallback.None => 0,
            RoomMainCallback.ScrollingSkyLand => RoomMainCodePointers.ScrollingSkyLand,
            RoomMainCallback.ScrollingSkyOcean => RoomMainCodePointers.ScrollingSkyOcean,
            RoomMainCallback.ScrollingSkyLandZebesTimebombSet => RoomMainCodePointers.ScrollingSkyLandZebesTimebombSet,
            RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions => RoomMainCodePointers.SetScreenShakingAndGenerateRandomExplosions,
            RoomMainCallback.ScrollScreenRightInDachoraRoom => RoomMainCodePointers.ScrollScreenRightInDachoraRoom,
            RoomMainCallback.MaridiaElevatube => RoomMainCodePointers.MaridiaElevatube,
            RoomMainCallback.CeresElevatorShaft => RoomMainCodePointers.CeresElevatorShaft,
            RoomMainCallback.Return => RoomMainCodePointers.Return,
            RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris => RoomMainCodePointers.SpawnCeresPreElevatorHallFallingDebris,
            RoomMainCallback.HandleCeresRidleyGetawayCutscene => RoomMainCodePointers.HandleCeresRidleyGetawayCutscene,
            RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal => RoomMainCodePointers.ShakeScreenLightHorizontalAndMediumDiagonal,
            RoomMainCallback.GenerateRandomExplosionEveryFourthFrame => RoomMainCodePointers.GenerateRandomExplosionEveryFourthFrame,
            RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal => RoomMainCodePointers.ShakeScreenMediumHorizontalAndStrongDiagonal,
            RoomMainCallback.CrocomireRoomShaking => RoomMainCodePointers.CrocomireRoomShaking,
            RoomMainCallback.RidleyRoomShaking => RoomMainCodePointers.RidleyRoomShaking,
            _ => throw new ArgumentOutOfRangeException(nameof(callback)),
        };

        /// <summary>Maps a translated setup callback to its cartridge callback pointer.</summary>
        internal static ushort PointerOf(RoomSetupCallback callback) => callback switch
        {
            RoomSetupCallback.None => 0,
            RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen => RoomSetupCodePointers.ClearBlocksAfterSavingAnimalsAndShakeScreen,
            RoomSetupCallback.AutoDestroyWallDuringEscape => RoomSetupCodePointers.AutoDestroyWallDuringEscape,
            RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape => RoomSetupCodePointers.TurnWallIntoShotBlocksDuringEscape,
            RoomSetupCallback.ReturnAfterEscapeWallSetup => RoomSetupCodePointers.ReturnAfterEscapeWallSetup,
            RoomSetupCallback.ReturnBeforeEscapeSkySetup => RoomSetupCodePointers.ReturnBeforeEscapeSkySetup,
            RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape => RoomSetupCodePointers.ShakeScreenAndCallScrollingSkyLandDuringEscape,
            RoomSetupCallback.ScrollingSkyLand => RoomSetupCodePointers.ScrollingSkyLand,
            RoomSetupCallback.ScrollingSkyOcean => RoomSetupCodePointers.ScrollingSkyOcean,
            RoomSetupCallback.Return => RoomSetupCodePointers.Return,
            RoomSetupCallback.ReturnAfterOceanSkySetup => RoomSetupCodePointers.ReturnAfterOceanSkySetup,
            RoomSetupCallback.ReturnBeforeStatueSetupA => RoomSetupCodePointers.ReturnBeforeStatueSetupA,
            RoomSetupCallback.ReturnBeforeStatueSetupB => RoomSetupCodePointers.ReturnBeforeStatueSetupB,
            RoomSetupCallback.RunStatueUnlockingAnimations => RoomSetupCodePointers.RunStatueUnlockingAnimations,
            RoomSetupCallback.SharedReturn => RoomSetupCodePointers.SharedReturn,
            RoomSetupCallback.SharedReturnB => RoomSetupCodePointers.SharedReturnB,
            RoomSetupCallback.SharedReturnC => RoomSetupCodePointers.SharedReturnC,
            RoomSetupCallback.SharedReturnD => RoomSetupCodePointers.SharedReturnD,
            RoomSetupCallback.OrdinaryReturn => RoomSetupCodePointers.OrdinaryReturn,
            RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile => RoomSetupCodePointers.SpawnPrePhantoonRoomEnemyProjectile,
            RoomSetupCallback.BossRoomReturn => RoomSetupCodePointers.BossRoomReturn,
            RoomSetupCallback.BossRoomReturnB => RoomSetupCodePointers.BossRoomReturnB,
            RoomSetupCallback.BossRoomReturnC => RoomSetupCodePointers.BossRoomReturnC,
            RoomSetupCallback.SetupShaktoolRoomPlm => RoomSetupCodePointers.SetupShaktoolRoomPlm,
            RoomSetupCallback.ReturnBeforeDraygonSetup => RoomSetupCodePointers.ReturnBeforeDraygonSetup,
            RoomSetupCallback.SetPausingCodeForDraygon => RoomSetupCodePointers.SetPausingCodeForDraygon,
            RoomSetupCallback.SetCollectedMap => RoomSetupCodePointers.SetCollectedMap,
            RoomSetupCallback.ReturnBeforeZebesTimebombSetup => RoomSetupCodePointers.ReturnBeforeZebesTimebombSetup,
            RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking => RoomSetupCodePointers.SetZebesTimebombEventAndLightHorizontalShaking,
            RoomSetupCallback.SetLightHorizontalRoomShaking => RoomSetupCodePointers.SetLightHorizontalRoomShaking,
            RoomSetupCallback.SetMediumHorizontalRoomShaking => RoomSetupCodePointers.SetMediumHorizontalRoomShaking,
            RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking => RoomSetupCodePointers.SetupEscapeRoom4PlmAndMediumHorizontalShaking,
            RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze => RoomSetupCodePointers.TurnCeresDoorToSolidBlocksAndSpawnHaze,
            RoomSetupCallback.SpawnCeresHaze => RoomSetupCodePointers.SpawnCeresHaze,
            RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze => RoomSetupCodePointers.SetCeresRidleyBgCharacterBaseAndSpawnHaze,
            _ => throw new ArgumentOutOfRangeException(nameof(callback)),
        };
    }
}

/// <summary>Verification access to <see cref="RoomCollisionBlock"/> members production does not use.</summary>
internal static class RoomCollisionBlockAccess
{
    /// <summary>Provides controlled access to collision-block behavior in room fixtures.</summary>
    extension(RoomCollisionBlock self)
    {
        /// <summary>Low ten bits selecting the visual 16×16 block definition.</summary>
        internal ushort VisualBlockIndex => self.PackedWord.VisualBlockIndex;
    }
}

/// <summary>Verification access to <see cref="RoomLevelData"/> members production does not use.</summary>
internal static class RoomLevelDataAccess
{
    /// <summary>Exposes room tile data operations used to construct and inspect verification rooms.</summary>
    extension(RoomLevelData self)
    {
        /// <summary>
        /// Returns this level to <paramref name="source"/>'s complete state in place: every block
        /// plane, allocation tail and pending door flag. Both levels must share one layout.
        /// </summary>
        internal void RestoreFrom(RoomLevelData source)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (source.WidthInBlocks != self.WidthInBlocks || source.HeightInBlocks != self.HeightInBlocks ||
                source.DoorListPointer != self.DoorListPointer ||
                PrivateState.Field<byte[]>(source, "_blockDefinitions").Length != PrivateState.Field<byte[]>(self, "_blockDefinitions").Length ||
                PrivateState.Field<ushort[]>(source, "_streamingForegroundAllocation").Length != PrivateState.Field<ushort[]>(self, "_streamingForegroundAllocation").Length ||
                PrivateState.Field<ushort[]>(source, "_streamingBackgroundAllocation").Length != PrivateState.Field<ushort[]>(self, "_streamingBackgroundAllocation").Length ||
                PrivateState.Field<ushort[]>(source, "_plmForegroundAllocation").Length != PrivateState.Field<ushort[]>(self, "_plmForegroundAllocation").Length ||
                object.ReferenceEquals(PrivateState.Field<ushort[]>(source, "_visualStreamingForegroundAllocation"), PrivateState.Field<ushort[]>(source, "_streamingForegroundAllocation")) !=
                    object.ReferenceEquals(PrivateState.Field<ushort[]>(self, "_visualStreamingForegroundAllocation"), PrivateState.Field<ushort[]>(self, "_streamingForegroundAllocation")) ||
                object.ReferenceEquals(PrivateState.Field<ushort[]>(source, "_visualStreamingBackgroundAllocation"), PrivateState.Field<ushort[]>(source, "_streamingBackgroundAllocation")) !=
                    object.ReferenceEquals(PrivateState.Field<ushort[]>(self, "_visualStreamingBackgroundAllocation"), PrivateState.Field<ushort[]>(self, "_streamingBackgroundAllocation")))
                throw new ArgumentException("A level can only be restored from one with the same layout.", nameof(source));
            PrivateState.Field<ushort[]>(source, "_foregroundEntries").CopyTo(PrivateState.Field<ushort[]>(self, "_foregroundEntries"), 0);
            PrivateState.Field<byte[]>(source, "_behaviorBytes").CopyTo(PrivateState.Field<byte[]>(self, "_behaviorBytes"), 0);
            PrivateState.Field<ushort[]>(source, "_backgroundEntries").CopyTo(PrivateState.Field<ushort[]>(self, "_backgroundEntries"), 0);
            PrivateState.Field<byte[]>(source, "_blockDefinitions").CopyTo(PrivateState.Field<byte[]>(self, "_blockDefinitions"), 0);
            PrivateState.Field<ushort[]>(source, "_streamingForegroundAllocation").CopyTo(PrivateState.Field<ushort[]>(self, "_streamingForegroundAllocation"), 0);
            PrivateState.Field<ushort[]>(source, "_streamingBackgroundAllocation").CopyTo(PrivateState.Field<ushort[]>(self, "_streamingBackgroundAllocation"), 0);
            PrivateState.Field<ushort[]>(source, "_visualStreamingForegroundAllocation").CopyTo(PrivateState.Field<ushort[]>(self, "_visualStreamingForegroundAllocation"), 0);
            PrivateState.Field<ushort[]>(source, "_visualStreamingBackgroundAllocation").CopyTo(PrivateState.Field<ushort[]>(self, "_visualStreamingBackgroundAllocation"), 0);
            PrivateState.Field<ushort[]>(source, "_plmForegroundAllocation").CopyTo(PrivateState.Field<ushort[]>(self, "_plmForegroundAllocation"), 0);
            PrivateState.Field<byte[]>(source, "_plmBehaviorAllocation").CopyTo(PrivateState.Field<byte[]>(self, "_plmBehaviorAllocation"), 0);
            PrivateState.SetProperty(self, "PendingDoorTransition", source.PendingDoorTransition);
            PrivateState.SetProperty(self, "ElevatorDoorContactPending", source.ElevatorDoorContactPending);
        }
    }
}

/// <summary>Verification access to <see cref="RoomLevelStreamDefinitions"/> members production does not use.</summary>
internal static class RoomLevelStreamDefinitionsAccess
{
    /// <summary>The lazily loaded installed stream set.</summary>
    private static object InstalledStreams =>
        PrivateState.Property<object>(PrivateState.StaticField<object>(typeof(RoomLevelStreamDefinitions), "Installed"), "Value");

    /// <summary>Provides fixture access to room-level streaming layout definitions.</summary>
    extension(RoomLevelStreamDefinitions)
    {
        /// <summary>Every distinct level-data source referenced by the compiled retail room states.</summary>
        internal static int Count => PrivateState.Property<IReadOnlyDictionary<int, ReadOnlyMemory<byte>>>(InstalledStreams, "Streams").Count;

        /// <summary>
        /// Source-cartridge provenance in the embedded corpus header (bytes 8..40), written at
        /// development-time extraction; the loader does not read it.
        /// </summary>
        internal static ReadOnlyMemory<byte> SourceSha256
        {
            get
            {
                string resource = PrivateState.StaticField<string>(typeof(RoomLevelStreamDefinitions), "ResourceName");
                using Stream source = typeof(RoomLevelStreamDefinitions).Assembly.GetManifestResourceStream(resource)
                    ?? throw new InvalidDataException($"Missing compiled resource {resource}.");
                byte[] header = new byte[40];
                source.ReadExactly(header);
                return header.AsMemory(8, 32);
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomLevelWord"/> members production does not use.</summary>
internal static class RoomLevelWordAccess
{
    /// <summary>Reads and updates the packed word representing one room block.</summary>
    extension(RoomLevelWord self)
    {
        /// <summary>Replaces only the parent-block visual transforms.</summary>
        internal RoomLevelWord WithVisualFlipFlags(LevelBlockFlipFlags visualFlipFlags)
        {
            PrivateState.InvokeStatic(typeof(RoomLevelWord), "ValidateVisualFlipFlags", (LevelBlockFlipFlags)(visualFlipFlags));
            return new RoomLevelWord(unchecked((ushort)(
                (self.Raw & ~PrivateState.StaticField<ushort>(typeof(RoomLevelWord), "VisualFlipMask")) | (ushort)visualFlipFlags)));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmBlueDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBlueDoorVisualCatalogAccess
{
    /// <summary>Creates focused blue-door visual catalogs for PLM verification.</summary>
    extension(RoomPlmBlueDoorVisualCatalog)
    {
        /// <summary>Calculate stock visuals directly; only selected custom frames need storage.</summary>
        internal static RoomPlmBlueDoorVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmBlueDoorVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmBombBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmBombBlockProgramDefinitionsAccess
{
    /// <summary>Exposes bomb-block PLM instruction data for mechanics checks.</summary>
    extension(RoomPlmBombBlockProgramDefinitions)
    {
        /// <summary>Enumerates bomb-block program words that are read as mechanics data.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmBombBlockProgramDefinitions), "ProgramCount"); index++)
            {
                object program = ((object)(PrivateState.InvokeStatic(typeof(RoomPlmBombBlockProgramDefinitions), "ProgramAt", (int)(index)))!);
                yield return PrivateState.Property<ushort>(program, "CollisionHead");
                yield return checked((ushort)(PrivateState.Property<ushort>(program, "CollisionHead") + 3));
                yield return checked((ushort)(PrivateState.Property<ushort>(program, "CollisionHead") + 5));
                yield return PrivateState.Property<ushort>(program, "ReactionHead");
                for (int frame = 0; frame < PrivateState.Property<int>(program, "FrameCount"); frame++)
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Tail") + 4 * frame));
                yield return PrivateState.Property<ushort>(program, "Terminal");
                if (PrivateState.Property<bool>(program, "Respawns"))
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Terminal") +
                        (PrivateState.Property<bool>(program, "SingleBlock") ? 2 : 4)));
            }
        }

        /// <summary>Enumerates bomb-block program bytes that are read as mechanics data.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmBombBlockProgramDefinitions), "ProgramCount"); index++)
            {
                object program = ((object)(PrivateState.InvokeStatic(typeof(RoomPlmBombBlockProgramDefinitions), "ProgramAt", (int)(index)))!);
                yield return checked((ushort)(PrivateState.Property<ushort>(program, "CollisionHead") + 2));
                yield return checked((ushort)(PrivateState.Property<ushort>(program, "ReactionHead") + 2));
            }
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmBombBlockRestoreDrawDefinitions"/> members production does not use.</summary>
internal static class RoomPlmBombBlockRestoreDrawDefinitionsAccess
{
    /// <summary>Provides test access to the draw lists that restore bombed blocks.</summary>
    extension(RoomPlmBombBlockRestoreDrawDefinitions)
    {
        /// <summary>Finds the restore draw list associated with a native PLM pointer.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (RoomPlmBombBlockRestoreDrawDefinitions.TryDescribe(pointer, out RoomPlmBombBlockRestoreDrawDefinitions.Draw draw))
            {
                list = ((RoomPlmShotBlockDrawDefinitions.DrawList)(PrivateState.InvokeStatic(typeof(RoomPlmBombBlockRestoreDrawDefinitions), "Export", (RoomPlmBombBlockRestoreDrawDefinitions.Draw)(draw)))!);
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
    /// <summary>Creates focused visual data for Bomb Torizo hand PLMs.</summary>
    extension(RoomPlmBombTorizoHandVisualCatalog)
    {
        /// <summary>Calculate original appearances directly; store only customized frames.</summary>
        internal static RoomPlmBombTorizoHandVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmBombTorizoHandVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmBotwoonWallVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmBotwoonWallVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Botwoon wall PLMs.</summary>
    extension(RoomPlmBotwoonWallVisualCatalog)
    {
        /// <summary>The native stock appearance is the visual part of the calculated
        /// nine-block air fill. Only custom artwork needs a stored payload.</summary>
        internal static RoomPlmBotwoonWallVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmBotwoonWallVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmChozoStatueVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmChozoStatueVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Chozo statue PLMs.</summary>
    extension(RoomPlmChozoStatueVisualCatalog)
    {
        /// <summary>Calculate original appearances directly; store only customized frames.</summary>
        internal static RoomPlmChozoStatueVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmChozoStatueVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmCollectibleVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmCollectibleVisualCatalogAccess
{
    /// <summary>Creates focused collectible artwork catalogs for PLM verification.</summary>
    extension(RoomPlmCollectibleVisualCatalog)
    {
        /// <summary>Calculate stock appearances directly; retain only customized frames.</summary>
        internal static RoomPlmCollectibleVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmCollectibleVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmColoredDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmColoredDoorVisualCatalogAccess
{
    /// <summary>Creates focused visual data for colored-door PLMs.</summary>
    extension(RoomPlmColoredDoorVisualCatalog)
    {
        /// <summary>Calculate stock visuals directly; only selected custom frames need storage.</summary>
        internal static RoomPlmColoredDoorVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmColoredDoorVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmContactCrumbleProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmContactCrumbleProgramDefinitionsAccess
{
    /// <summary>Exposes contact-triggered crumble instruction data for mechanics checks.</summary>
    extension(RoomPlmContactCrumbleProgramDefinitions)
    {
        /// <summary>Enumerates contact-crumble program words that encode block mechanics.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmContactCrumbleProgramDefinitions), "ProgramCount"); index++)
            {
                object program = ((object)(PrivateState.InvokeStatic(typeof(RoomPlmContactCrumbleProgramDefinitions), "ProgramAt", (int)(index)))!);
                yield return PrivateState.Property<ushort>(program, "Start");
                for (int frame = 0; frame < PrivateState.Property<int>(program, "FrameCount"); frame++)
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Start") + 3 + 4 * frame));
                yield return PrivateState.Property<ushort>(program, "Terminal");
                if (PrivateState.Property<bool>(program, "Respawns"))
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Terminal") +
                        (PrivateState.Property<int>(program, "Dimension") == 0 ? 2 : 4)));
            }
        }

        /// <summary>Enumerates contact-crumble bytes that encode timing and control data.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmContactCrumbleProgramDefinitions), "ProgramCount"); index++)
                yield return checked((ushort)(PrivateState.Property<ushort>(((object)(PrivateState.InvokeStatic(typeof(RoomPlmContactCrumbleProgramDefinitions), "ProgramAt", (int)(index)))!), "Start") + 2));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmContactCrumbleRestoreDrawDefinitions"/> members production does not use.</summary>
internal static class RoomPlmContactCrumbleRestoreDrawDefinitionsAccess
{
    /// <summary>Provides test access to draw lists used when contact crumble blocks restore.</summary>
    extension(RoomPlmContactCrumbleRestoreDrawDefinitions)
    {
        /// <summary>Finds the restore draw list associated with a native contact-crumble pointer.</summary>
        internal static bool TryGet(ushort pointer, out RoomPlmShotBlockDrawDefinitions.DrawList list)
        {
            if (RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe(pointer, out RoomPlmContactCrumbleRestoreDrawDefinitions.Draw draw))
            {
                list = ((RoomPlmShotBlockDrawDefinitions.DrawList)(PrivateState.InvokeStatic(typeof(RoomPlmContactCrumbleRestoreDrawDefinitions), "Export", (RoomPlmContactCrumbleRestoreDrawDefinitions.Draw)(draw)))!);
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
    /// <summary>Creates focused visual data for Crocomire room PLMs.</summary>
    extension(RoomPlmCrocomireVisualCatalog)
    {
        /// <summary>Calculate stock appearances from the draw geometry; retain only customized frames.</summary>
        internal static RoomPlmCrocomireVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmCrocomireVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmDownwardGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmDownwardGateVisualCatalogAccess
{
    /// <summary>Creates focused visual data for downward-gate PLMs.</summary>
    extension(RoomPlmDownwardGateVisualCatalog)
    {
        /// <summary>Calculate stock appearances from physical draws; store only customized frames.</summary>
        internal static RoomPlmDownwardGateVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmDownwardGateVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmDraygonCannonVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmDraygonCannonVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Draygon cannon PLMs.</summary>
    extension(RoomPlmDraygonCannonVisualCatalog)
    {
        /// <summary>Calculate original appearances directly; store only customized frames.</summary>
        internal static RoomPlmDraygonCannonVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmDraygonCannonVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmDynamicCollectibleArtCatalog"/> members production does not use.</summary>
internal static class RoomPlmDynamicCollectibleArtCatalogAccess
{
    /// <summary>Creates collectible-art mappings for tests that supply dynamic tile graphics.</summary>
    extension(RoomPlmDynamicCollectibleArtCatalog)
    {
        /// <summary>Resolve stock directly; retain only customized item artwork.</summary>
        internal static RoomPlmDynamicCollectibleArtCatalog Stock() => PrivateState.Uninitialized<RoomPlmDynamicCollectibleArtCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmElevatorPlatformVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmElevatorPlatformVisualCatalogAccess
{
    /// <summary>Creates focused visual data for elevator-platform PLMs.</summary>
    extension(RoomPlmElevatorPlatformVisualCatalog)
    {
        /// <summary>Calculate stock appearances directly; retain only customized frames.</summary>
        internal static RoomPlmElevatorPlatformVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmElevatorPlatformVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmEscapeGateVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmEscapeGateVisualCatalogAccess
{
    /// <summary>Creates focused visual data for escape-gate PLMs.</summary>
    extension(RoomPlmEscapeGateVisualCatalog)
    {
        /// <summary>Calculate stock visuals directly; only selected custom frames need storage.</summary>
        internal static RoomPlmEscapeGateVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmEscapeGateVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmEyeDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmEyeDoorVisualCatalogAccess
{
    /// <summary>Creates focused visual data for eye-door PLMs.</summary>
    extension(RoomPlmEyeDoorVisualCatalog)
    {
        /// <summary>Calculate stock visuals directly; retain only customized frames.</summary>
        internal static RoomPlmEyeDoorVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmEyeDoorVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmGrappleBlockProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmGrappleBlockProgramDefinitionsAccess
{
    /// <summary>Exposes grapple-block program words for focused mechanics checks.</summary>
    extension(RoomPlmGrappleBlockProgramDefinitions)
    {
        /// <summary>Enumerates grapple-block program words that encode block mechanics.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmGrappleBlockProgramDefinitions), "ProgramCount"); index++)
            {
                object program = ((object)(PrivateState.InvokeStatic(typeof(RoomPlmGrappleBlockProgramDefinitions), "ProgramAt", (int)(index)))!);
                yield return PrivateState.Property<ushort>(program, "Start");
                yield return checked((ushort)(PrivateState.Property<ushort>(program, "Start") + 4));
                for (int frame = 0; frame < PrivateState.Property<int>(program, "FrameCount"); frame++)
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Start") + 7 + 4 * frame));
                yield return PrivateState.Property<ushort>(program, "TerminalAddress");
                if (PrivateState.Property<bool>(program, "Respawns"))
                {
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "TerminalAddress") + 2));
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "TerminalAddress") + 4));
                }
            }
        }

        /// <summary>Enumerates grapple-block bytes that encode timing and control data.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmGrappleBlockProgramDefinitions), "ProgramCount"); index++)
                yield return checked((ushort)(PrivateState.Property<ushort>(((object)(PrivateState.InvokeStatic(typeof(RoomPlmGrappleBlockProgramDefinitions), "ProgramAt", (int)(index)))!), "Start") + 6));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmGrappleBlockVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmGrappleBlockVisualCatalogAccess
{
    /// <summary>Creates focused visual data for grapple-block PLMs.</summary>
    extension(RoomPlmGrappleBlockVisualCatalog)
    {
        /// <summary>Stock visuals use the calculated draw word without storing derived entries.</summary>
        internal static RoomPlmGrappleBlockVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmGrappleBlockVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmGreyDoorVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmGreyDoorVisualCatalogAccess
{
    /// <summary>Creates focused visual data for grey-door PLMs.</summary>
    extension(RoomPlmGreyDoorVisualCatalog)
    {
        /// <summary>Calculate stock visuals directly; only selected custom frames need storage.</summary>
        internal static RoomPlmGreyDoorVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmGreyDoorVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmKraidVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmKraidVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Kraid room PLMs.</summary>
    extension(RoomPlmKraidVisualCatalog)
    {
        /// <summary>Builds stock Kraid visuals from the native draw definitions.</summary>
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
    /// <summary>Creates visual data for linked blocks whose restore sequence is verified together.</summary>
    extension(RoomPlmLinkedRestoreVisualCatalog)
    {
        /// <summary>Stock words are calculated from the physical draw's visual portion.</summary>
        internal static RoomPlmLinkedRestoreVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmLinkedRestoreVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmMaridiaElevatubeVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMaridiaElevatubeVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Maridia elevatube PLMs.</summary>
    extension(RoomPlmMaridiaElevatubeVisualCatalog)
    {
        /// <summary>Builds the stock elevatube visual entry from the native draw layout.</summary>
        internal static RoomPlmMaridiaElevatubeVisualCatalog Stock() => new(
            [new RoomPlmMaridiaElevatubeVisualEntry(
                MaridiaElevatubePlmDefinitions.VisualId,
                [new RoomLevelWord(MaridiaElevatubePlmDefinitions.PhysicalWord).VisualWord])]);
    }
}

/// <summary>Verification access to <see cref="RoomPlmMotherBrainFakeDeathVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMotherBrainFakeDeathVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Mother Brain fake-death PLMs.</summary>
    extension(RoomPlmMotherBrainFakeDeathVisualCatalog)
    {
        /// <summary>Project stock appearance from physical cells without a duplicate stock cache.</summary>
        internal static RoomPlmMotherBrainFakeDeathVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmMotherBrainFakeDeathVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmMotherBrainGlassVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmMotherBrainGlassVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Mother Brain glass PLMs.</summary>
    extension(RoomPlmMotherBrainGlassVisualCatalog)
    {
        /// <summary>Calculate original appearances directly; store only customized frames.</summary>
        internal static RoomPlmMotherBrainGlassVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmMotherBrainGlassVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmNoobTubeVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmNoobTubeVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Noob Tube PLMs.</summary>
    extension(RoomPlmNoobTubeVisualCatalog)
    {
        /// <summary>Calculate original appearances directly; store only customized frames.</summary>
        internal static RoomPlmNoobTubeVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmNoobTubeVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmPopulationDefinitions"/> members production does not use.</summary>
internal static class RoomPlmPopulationDefinitionsAccess
{
    /// <summary>Exposes room PLM population pointer data for room setup verification.</summary>
    extension(RoomPlmPopulationDefinitions)
    {
        /// <summary>Enumerates candidate pointers accepted by the compiled room-population table.</summary>
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
    /// <summary>Creates focused visual data for Samus Eater PLMs.</summary>
    extension(RoomPlmSamusEaterVisualCatalog)
    {
        /// <summary>Stock appearance is calculated from the physical draw's visual bits without a cache.</summary>
        internal static RoomPlmSamusEaterVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmSamusEaterVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmScrollProgramDefinitions"/> members production does not use.</summary>
internal static class RoomPlmScrollProgramDefinitionsAccess
{
    /// <summary>Exposes scroll PLM instruction and pointer data for mechanics checks.</summary>
    extension(RoomPlmScrollProgramDefinitions)
    {
        /// <summary>Lists the translated scroll instruction lists in their native pointer order.</summary>
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
    /// <summary>Exposes projectile-reactive block instruction data for mechanics checks.</summary>
    extension(RoomPlmShotBlockProgramDefinitions)
    {
        /// <summary>All authored control-word addresses, excluding draw-list operands.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmShotBlockProgramDefinitions), "ProgramCount"); index++)
            {
                object program = ((object)(PrivateState.InvokeStatic(typeof(RoomPlmShotBlockProgramDefinitions), "ProgramAt", (int)(index)))!);
                yield return PrivateState.Property<ushort>(program, "Start");
                for (int frame = 0; frame < PrivateState.Property<int>(program, "FrameCount"); frame++)
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Start") + 3 + 4 * frame));
                yield return PrivateState.Property<ushort>(program, "TerminalAddress");
                if (PrivateState.Property<bool>(program, "RestoresLevelWord"))
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "TerminalAddress") + 2));
            }
        }

        /// <summary>Enumerates the timing and control bytes read by compiled shot-block programs.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(RoomPlmShotBlockProgramDefinitions), "ProgramCount"); index++)
                yield return checked((ushort)(PrivateState.Property<ushort>(((object)(PrivateState.InvokeStatic(typeof(RoomPlmShotBlockProgramDefinitions), "ProgramAt", (int)(index)))!), "Start") + 2));
        }
    }
}

/// <summary>Verification access to <see cref="RoomPlmShotBlockVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmShotBlockVisualCatalogAccess
{
    /// <summary>Creates focused visual data for projectile-reactive block PLMs.</summary>
    extension(RoomPlmShotBlockVisualCatalog)
    {
        /// <summary>Native visual selections, useful when no installed override is present.</summary>
        internal static RoomPlmShotBlockVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmShotBlockVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmSpeedBoosterVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmSpeedBoosterVisualCatalogAccess
{
    /// <summary>Creates focused visual data for speed-booster block PLMs.</summary>
    extension(RoomPlmSpeedBoosterVisualCatalog)
    {
        /// <summary>Builds stock speed-booster visuals from the physical bomb-reveal draw geometry.</summary>
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
    /// <summary>Creates focused visual data for Spore Spawn ceiling PLMs.</summary>
    extension(RoomPlmSporeSpawnCeilingVisualCatalog)
    {
        /// <summary>Calculate each stock square from its physical draw word; retain only custom artwork.</summary>
        internal static RoomPlmSporeSpawnCeilingVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmSporeSpawnCeilingVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmStationVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmStationVisualCatalogAccess
{
    /// <summary>Creates focused visual data for map and recharge station PLMs.</summary>
    extension(RoomPlmStationVisualCatalog)
    {
        /// <summary>Calculate stock appearances directly; retain only customized frames.</summary>
        internal static RoomPlmStationVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmStationVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomPlmSystem"/> members production does not use.</summary>
internal static class RoomPlmSystemAccess
{
    /// <summary>Provides fixture controls for PLM system state that production keeps private.</summary>
    extension(RoomPlmSystem)
    {
        /// <summary>
        /// Returns whether a room-authored header has a setup owner in the sequential loader.
        /// This is a read-only inventory seam for private-ROM audits; production still performs
        /// dispatch and contextual failure from <see cref="RoomPlmSystem.LoadRoomPopulation"/> itself.
        /// </summary>
        internal static bool IsSupportedRoomPopulationHeader(ushort header)
        {
            if (header is RoomPlmHeaders.ScrollTrigger or
                RoomPlmHeaders.RightwardsScrollExtension or
                RoomPlmHeaders.LeftwardsScrollExtension or
                RoomPlmHeaders.DownwardsScrollExtension or
                RoomPlmHeaders.UpwardsScrollExtension or
                RoomPlmHeaders.MotherBrainGlass or RoomPlmHeaders.BombTorizoHand or
                RoomPlmHeaders.MapStation or RoomPlmHeaders.EnergyStation or
                RoomPlmHeaders.MissileStation or RoomPlmHeaders.ElevatorPlatform or
                RoomPlmHeaders.SaveStation or
                RoomPlmHeaders.SpeedBoosterEscape or
                RoomPlmHeaders.WreckedShipAttic or
                RoomPlmHeaders.NoobTube or
                RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired or
                RoomPlmHeaders.MotherBrainEscapeRoomGate or
                RoomPlmHeaders.DownwardGate or RoomPlmHeaders.DownwardGateShotBlock ||
                ((bool)(PrivateState.InvokeStatic(typeof(RoomPlmSystem), "IsEyeDoorHeader", (ushort)(header)))!) || ((bool)(PrivateState.InvokeStatic(typeof(RoomPlmSystem), "IsDraygonCannonHeader", (ushort)(header)))!))
            {
                return true;
            }
            if (RoomPlmSystem.IsColoredDoorHeader(header) ||
                (bool)PrivateState.InvokeStaticWithOut(typeof(RoomPlmSystem), "TryIdentifyGreyDoor", [header, null])!)
                return true;
            return RoomPlmSystem.TryIdentifyPermanentCollectible(header, out _, out _);
        }

        /// <summary>Checks whether an instruction header identifies a translated colored door.</summary>
        internal static bool IsColoredDoorHeader(ushort header) => header is
            RoomPlmHeaders.YellowDoorFacingLeft or RoomPlmHeaders.YellowDoorFacingRight or
            RoomPlmHeaders.YellowDoorFacingUp or RoomPlmHeaders.YellowDoorFacingDown or
            RoomPlmHeaders.GreenDoorFacingLeft or RoomPlmHeaders.GreenDoorFacingRight or
            RoomPlmHeaders.GreenDoorFacingUp or RoomPlmHeaders.GreenDoorFacingDown or
            RoomPlmHeaders.RedDoorFacingLeft or RoomPlmHeaders.RedDoorFacingRight or
            RoomPlmHeaders.RedDoorFacingUp or RoomPlmHeaders.RedDoorFacingDown;
    }

    /// <summary>Exposes PLM stepping and focused spawn operations to verification harnesses.</summary>
    extension(RoomPlmSystem self)
    {
        /// <summary>Every resident colored-door actor in the shared native PLM pool.</summary>
        internal IReadOnlyList<ColoredDoorPlmSnapshot> ColoredDoors =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: PrivateState.Property<ColoredDoorPlmState?>(slot, "ColoredDoor")))
                .Where(entry => entry.door is not null)
                .Select(entry => new ColoredDoorPlmSnapshot(
                    PrivateState.Property<ushort>(entry.slot, "HeaderPointer"),
                    PrivateState.Property<int>(entry.slot, "BlockIndex"),
                    PrivateState.Property<ushort>(entry.slot, "RoomArgument"),
                    entry.door!.Color, entry.door.Orientation, entry.door.Phase, entry.door.HitCounter)),
        ];

        /// <summary>Debugger-stable views of every resident grey-door PLM.</summary>
        internal IReadOnlyList<GreyDoorPlmSnapshot> GreyDoors =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: PrivateState.Property<GreyDoorPlmState?>(slot, "GreyDoor")))
                .Where(entry => entry.door is not null)
                .Select(entry => new GreyDoorPlmSnapshot(
                    PrivateState.Property<ushort>(entry.slot, "HeaderPointer"),
                    PrivateState.Property<int>(entry.slot, "BlockIndex"),
                    PrivateState.Property<ushort>(entry.slot, "RoomArgument"),
                    entry.door!.Orientation, entry.door.Condition, entry.door.Phase,
                    entry.door.InitialList, entry.door.FlashList, entry.door.OpeningList)),
        ];

        /// <summary>Debugger-visible projections of every live eye-door component.</summary>
        internal IReadOnlyList<EyeDoorPlmSnapshot> EyeDoors =>
        [
            .. ActiveSlots(self).Select(slot => (slot, door: PrivateState.Property<object?>(slot, "EyeDoor")))
                .Where(entry => entry.door is not null)
                .Select(entry => new EyeDoorPlmSnapshot(
                    PrivateState.Property<ushort>(entry.slot, "HeaderPointer"),
                    PrivateState.Property<int>(entry.slot, "BlockIndex"),
                    PrivateState.Property<ushort>(entry.slot, "RoomArgument"),
                    PrivateState.Property<EyeDoorComponent>(entry.door!, "Component"),
                    PrivateState.Property<byte>(entry.door!, "HitCounter"),
                    PrivateState.Property<ushort>(entry.slot, "InstructionPointer"),
                    PrivateState.Property<ushort>(entry.slot, "PreInstruction"))),
        ];

        /// <summary>
        /// Publishes the projectile word observed by a resident type-$C/BTS-$44 door. The
        /// resident actor, not the collision table, decides whether that family is accepted.
        /// </summary>
        internal bool TryNotifyColoredDoorHit(int blockIndex, SamusProjectileTypeWord projectileType)
        {
            if (self.TryNotifyEyeDoorHit(blockIndex, projectileType))
                return true;

            foreach (object slot in ActiveSlots(self))
            {
                if (PrivateState.Property<int>(slot, "BlockIndex") != blockIndex ||
                    PrivateState.Property<ColoredDoorPlmState?>(slot, "ColoredDoor") is not { } door)
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
            foreach (object slot in ActiveSlots(self))
            {
                if (PrivateState.Property<int>(slot, "BlockIndex") != blockIndex ||
                    PrivateState.Property<GreyDoorPlmState?>(slot, "GreyDoor") is not { } door)
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
        internal bool MotherBrainGlassWasLoaded => PrivateState.Field<bool>(self, "_motherBrainGlassWasLoaded");

        /// <summary>Current native room argument, retained after the header deletes itself.</summary>
        internal ushort MotherBrainGlassRoomArgument => MotherBrainGlassSlot(self) is { } slot
            ? PrivateState.Property<ushort>(slot, "RoomArgument")
            : PrivateState.Field<ushort>(self, "_motherBrainGlassLastRoomArgument");

        /// <summary>Current bank-$84 instruction pointer, or zero after deletion.</summary>
        internal ushort MotherBrainGlassInstructionPointer => MotherBrainGlassSlot(self) is { } slot
            ? PrivateState.Property<ushort>(slot, "InstructionPointer")
            : (ushort)0;

        /// <summary>
        /// Redirects the sole live PLM to a constructed bank-$84 program for interpreter tests.
        /// This is not a gameplay operation: the real setup still owns allocation, block
        /// position, restore word and BTS, while the test controls only the next instruction.
        /// Keeping synthetic opcodes at nonretail addresses avoids rewriting an immutable
        /// compiled cartridge program in a fake address space.
        /// </summary>
        /// <summary>
        /// Places a constructed program in low work RAM, where the `$84` interpreter executes
        /// wrapped instruction pointers, and redirects the sole live PLM to it.
        /// </summary>
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
                memory.WriteByte(address + 1, (byte)(instructions[index] >> 8));
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
            object[] active = [.. ActiveSlots(self)];
            if (active.Length != 1)
                throw new InvalidOperationException("Draw probe requires exactly one active PLM slot.");
            PrivateState.Invoke(self, "DrawPlmInstruction", bus, level, streamer,
                PrivateState.Property<ushort>(active[0], "HeaderPointer"),
                PrivateState.Property<int>(active[0], "BlockIndex"),
                drawPointer, (ushort)0, (ushort)0, (ushort)0);
        }

        /// <summary>Sets the only active slot's instruction pointer to isolate a handler case.</summary>
        internal void SetSoleInstructionPointerForVerification(ushort instructionPointer)
        {
            object[] active = PrivateState.Field<object[]>(self, "_slots").Where(slot => PrivateState.Property<bool>(slot, "Active")).ToArray();
            if (active.Length != 1)
                throw new InvalidOperationException(
                    "Instruction probe requires exactly one active PLM slot.");
            PrivateState.SetProperty(active[0], "InstructionPointer", instructionPointer);
            PrivateState.SetProperty(active[0], "InstructionTimer", 1);
        }

        /// <summary>Current PLM instruction countdown, or zero after deletion.</summary>
        internal ushort MotherBrainGlassInstructionTimer => MotherBrainGlassSlot(self) is { } slot
            ? PrivateState.Property<ushort>(slot, "InstructionTimer")
            : (ushort)0;

        /// <summary>
        /// Publishes the projectile word to the resident eye controller at a type-$C/BTS-$44
        /// block. The following PLM pre-instruction performs the missile-family filtering.
        /// </summary>
        internal bool TryNotifyEyeDoorHit(
            int blockIndex,
            SamusProjectileTypeWord projectileType)
        {
            foreach (object slot in PrivateState.Field<object[]>(self, "_slots"))
            {
                if (!PrivateState.Property<bool>(slot, "Active") || PrivateState.Property<int>(slot, "BlockIndex") != blockIndex ||
                    PrivateState.Property<object?>(slot, "EyeDoor") is not { } door ||
                    PrivateState.Property<EyeDoorComponent>(door, "Component") != EyeDoorComponent.Eye)
                {
                    continue;
                }

                // PLM_Timers is a single pending word. A later collision before the handler
                // pass replaces it rather than accumulating a host-side queue.
                PrivateState.SetProperty(slot, "LoopTimer", projectileType.Raw);
                PrivateState.SetProperty(door, "HasPendingHit", true);
                return true;
            }
            return false;
        }

        /// <summary>Every live elevator-platform PLM in native physical-slot order.</summary>
        internal IReadOnlyList<RoomPlmSlotSnapshot> ElevatorPlatforms => self.PopulationSlots
            .Where(snapshot => PrivateState.Property<bool>(PrivateState.Field<object[]>(self, "_slots")[snapshot.NativeSlotIndex], "IsElevatorPlatform"))
            .ToArray();

        /// <summary>Physical-slot views in cartridge handler order, highest ID first.</summary>
        internal IReadOnlyList<RoomPlmSlotSnapshot> PopulationSlots
        {
            get
            {
                var snapshots = new RoomPlmSlotSnapshot[self.ActiveCount];
                int next = 0;
                for (int index = PrivateState.Field<object[]>(self, "_slots").Length - 1; index >= 0; index--)
                    if (PrivateState.Property<bool>(PrivateState.Field<object[]>(self, "_slots")[index], "Active")) snapshots[next++] = self.Snapshot(index);
                return snapshots;
            }
        }

        /// <summary>Captures one internal slot's state together with its native slot index.</summary>
        internal RoomPlmSlotSnapshot Snapshot(int index)
        {
            object slot = PrivateState.Field<object[]>(self, "_slots")[index];
            return new RoomPlmSlotSnapshot(
                NativeSlotIndex: index,
                HeaderPointer: PrivateState.Property<ushort>(slot, "HeaderPointer"),
                BlockIndex: PrivateState.Property<int>(slot, "BlockIndex"),
                RoomArgument: PrivateState.Property<ushort>(slot, "RoomArgument"),
                InstructionPointer: PrivateState.Property<ushort>(slot, "InstructionPointer"),
                PreInstruction: PrivateState.Property<ushort>(slot, "PreInstruction"),
                InstructionTimer: PrivateState.Property<ushort>(slot, "InstructionTimer"),
                LinkInstruction: PrivateState.Property<ushort>(slot, "LinkInstruction"),
                LoopTimer: PrivateState.Property<ushort>(slot, "LoopTimer"));
        }

        /// <summary>Visible BG1 mutations emitted during the most recent handler pass.</summary>
        internal IReadOnlyList<PlmTilemapUpdate> TilemapUpdates => PrivateState.Field<List<PlmTilemapUpdate>>(self, "_tilemapUpdates");

        /// <summary>
        /// Publishes contact with BTS $47-$4D to its resident station. This is the same generic
        /// type-$B collision seam used by item and scroll PLMs; no room identity participates.
        /// </summary>
        internal bool TryNotifyStationTouch(int accessBlockIndex, byte behavior)
            => self.TryNotifyStationTouch(accessBlockIndex, new RoomBlockBehavior(behavior));

        /// <summary>Typed BTS overload used by room collision dispatch.</summary>
        internal bool TryNotifyStationTouch(int accessBlockIndex, RoomBlockBehavior behavior)
            => ((bool)(PrivateState.Invoke(self, "TryNotifyStationCollision", (int)(accessBlockIndex), (RoomBlockBehavior)(behavior), (byte)(byte.MaxValue), (bool)(true), (bool)(true), (int)(0), (bool)(true)))!);

        /// <summary>
        /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
        /// </summary>
        /// <returns>False only when all 40 native slots are occupied.</returns>
        internal bool TrySpawnBreakableGrappleBlock(
            RoomLevelData level,
            int blockIndex,
            byte behavior)
            => self.TrySpawnBreakableGrappleBlock(
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
            => self.TrySpawnCollisionBombBlock(level, blockIndex, new RoomBlockBehavior(behavior));

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
            => self.TrySpawnBombReactionBlock(
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
            ArgumentNullException.ThrowIfNull(level);
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
            for (int slotIndex = PrivateState.Field<object[]>(self, "_slots").Length - 1; slotIndex >= 0; slotIndex--)
            {
                object slot = PrivateState.Field<object[]>(self, "_slots")[slotIndex];
                if (PrivateState.Property<bool>(slot, "Active"))
                    continue;

                RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
                PrivateState.InvokeStatic(typeof(RoomPlmSystem), "ClearSlot", slot);
                PrivateState.SetProperty(slot, "Active", true);
                PrivateState.SetProperty(slot, "BlockIndex", blockIndex);
                PrivateState.SetProperty(slot, "InstructionTimer", 1);
                PrivateState.SetProperty(slot, "RestoreLevelWord", 0);

                if (areaDependent)
                {
                    // `$94:9E8D-$9E9E` indexes the current area's eight-entry table before
                    // Spawn_PLM. Every retail entry at `$94:9F46-$9FC4` is PLMEntries_nothing.
                    PrivateState.SetProperty(slot, "InstructionPointer", RoomPlmInstructionLists.Delete);
                    return true;
                }

                if (bts.IsRespawningReaction)
                {
                    // CE6B throws away the original visual/BTS low twelve bits. The generated
                    // `$x052` word is both PLM_Vars and the source of the temporary collision
                    // word, exactly like the retail setup's two consecutive stores.
                    PrivateState.SetProperty(slot, "RestoreLevelWord", unchecked((ushort)((block.LevelWord & 0xf000) | 0x0052)));
                    PrivateState.SetProperty(slot, "InstructionPointer", RoomPlmInstructionLists.RespawningShotBySize(bts.NormalReactionIndex));
                    level.SetForegroundEntry(
                        blockIndex,
                        unchecked((ushort)(PrivateState.Property<ushort>(slot, "RestoreLevelWord") & 0x8fff)));
                    return true;
                }

                if (bts.IsPermanentReaction)
                {
                    // Setup_DeactivatePLM does not synthesize a restore word. Clearing bits
                    // `$7000` turns type-$4 air into ordinary air and type-$C solid into type-$8
                    // solid until the same-frame list draws its first breaking frame.
                    PrivateState.SetProperty(slot, "InstructionPointer", RoomPlmInstructionLists.PermanentShotBySize(bts.NormalReactionIndex - 4));
                    level.SetForegroundEntry(
                        blockIndex,
                        unchecked((ushort)(block.LevelWord & 0x8fff)));
                    return true;
                }

                if (bts.RequiresPowerBombReaction)
                {
                    // CF2E sees projectile family `$0500` and replaces the entry's normal
                    // power-bomb animation pointer with the one-frame visible `$C057` reveal.
                    PrivateState.SetProperty(slot, "InstructionPointer", RoomPlmInstructionLists.BombedPowerBombBlockUnused);
                    return true;
                }

                if (bts.RequiresSuperMissileReaction)
                {
                    // CF67 performs the analogous redirect to visible super-missile word
                    // `$C09F`; it neither clears collision nor queues the shot-block sound.
                    PrivateState.SetProperty(slot, "InstructionPointer", RoomPlmInstructionLists.BombedSuperMissileBlockUnused);
                    return true;
                }

                PrivateState.SetProperty(slot, "InstructionPointer", RoomPlmInstructionLists.Delete);
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
            => self.TrySpawnProjectileShotBlock(
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
            => self.TrySpawnBombedSpecialBlock(
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
            ushort layer1XPosition,
            ushort layer1YPosition,
            ushort bg1XOffset,
            RoomScrollGrid? scrolls = null,
            ushort enemyDeaths = 0,
            byte enemyDeathQuota = 0)
            => self.Step(bus, level, streamer, layer1XPosition, layer1YPosition, bg1XOffset,
                scrolls, enemyDeaths, enemyDeathQuota, controllerNewInput: 0);
    }

    /// <summary>Returns active physical PLM slots for assertions about room-owned object lifetimes.</summary>
    private static IEnumerable<object> ActiveSlots(RoomPlmSystem plms) =>
        PrivateState.Field<object[]>(plms, "_slots").Where(slot => PrivateState.Property<bool>(slot, "Active"));

    /// <summary>The live Mother Brain glass PLM slot, as the system's own slot search finds it.</summary>
    private static object? MotherBrainGlassSlot(RoomPlmSystem plms)
    {
        object?[] arguments = [null];
        return (bool)PrivateState.InvokeWithOut(plms, "TryGetMotherBrainGlassSlot", arguments)! ? arguments[0] : null;
    }
}

/// <summary>Verification access to <see cref="RoomPlmTourianAccessVisualCatalog"/> members production does not use.</summary>
internal static class RoomPlmTourianAccessVisualCatalogAccess
{
    /// <summary>Creates focused visual data for Tourian access PLMs.</summary>
    extension(RoomPlmTourianAccessVisualCatalog)
    {
        /// <summary>Calculate stock row appearance from physical draw words; retain only custom artwork.</summary>
        internal static RoomPlmTourianAccessVisualCatalog Stock() => PrivateState.Uninitialized<RoomPlmTourianAccessVisualCatalog>();
    }
}

/// <summary>Verification access to <see cref="RoomSetupCodePointers"/> members production does not use.</summary>
internal static class RoomSetupCodePointersAccess
{
    /// <summary>Provides verification predicates over room setup callback pointers.</summary>
    extension(RoomSetupCodePointers)
    {
        /// <summary>Whether a translated presentation path consumes Ceres haze from this setup.</summary>
        internal static bool SpawnsCeresHaze(ushort pointer) => pointer is
            RoomSetupCodePointers.TurnCeresDoorToSolidBlocksAndSpawnHaze or
            RoomSetupCodePointers.SpawnCeresHaze or
            RoomSetupCodePointers.SetCeresRidleyBgCharacterBaseAndSpawnHaze;
    }
}

/// <summary>Verification access to <see cref="RoomVisualLayoutCatalog"/> members production does not use.</summary>
internal static class RoomVisualLayoutCatalogAccess
{
    /// <summary>Builds explicitly partial room layouts for focused verification fixtures.</summary>
    extension(RoomVisualLayoutCatalog)
    {
        /// <summary>Explicitly partial geometry fixtures; never an installed production catalog.</summary>
        internal static RoomVisualLayoutCatalog FromLayoutsForVerification(
            IReadOnlyDictionary<int, RoomVisualLayout> layouts) =>
            ((RoomVisualLayoutCatalog)PrivateState.Construct(typeof(RoomVisualLayoutCatalog), (IReadOnlyDictionary<int, RoomVisualLayout>)(layouts), (bool)(false)));
    }
}

/// <summary>Verification access to <see cref="SpeedBoosterBlockPlmDrawDefinitions"/> members production does not use.</summary>
internal static class SpeedBoosterBlockPlmDrawDefinitionsAccess
{
    /// <summary>Exposes speed-booster block draw-list lookup for verification fixtures.</summary>
    extension(SpeedBoosterBlockPlmDrawDefinitions)
    {
        /// <summary>Resolves the compiled bomb-reveal draw list for a native draw pointer.</summary>
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
    /// <summary>Provides fixture access to speed-booster block mechanics instruction addresses.</summary>
    extension(SpeedBoosterBlockPlmProgramDefinitions)
    {
        /// <summary>Enumerates speed-booster instruction words consumed as block mechanics data.</summary>
        internal static IEnumerable<ushort> MechanicsWordAddresses()
        {
            yield return SpeedBoosterBlockPlmProgramDefinitions.BombReveal;
            yield return checked((ushort)(SpeedBoosterBlockPlmProgramDefinitions.BombReveal + 2));
            yield return checked((ushort)(SpeedBoosterBlockPlmProgramDefinitions.BombReveal + 4));
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(SpeedBoosterBlockPlmProgramDefinitions), "ProgramCount"); index++)
            {
                object program = ((object)(PrivateState.InvokeStatic(typeof(SpeedBoosterBlockPlmProgramDefinitions), "ProgramAt", (int)(index)))!);
                yield return PrivateState.Property<ushort>(program, "Start");
                for (int frame = 0; frame < PrivateState.Property<int>(program, "FrameCount"); frame++)
                {
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Start") + 3 + frame * 4));
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Start") + 5 + frame * 4));
                }
                yield return PrivateState.Property<ushort>(program, "Terminal");
                if (PrivateState.Property<bool>(program, "Respawns"))
                    yield return checked((ushort)(PrivateState.Property<ushort>(program, "Terminal") + 2));
            }
        }

        /// <summary>Enumerates speed-booster instruction bytes that control mechanics behavior.</summary>
        internal static IEnumerable<ushort> MechanicsByteAddresses()
        {
            for (int index = 0; index < PrivateState.StaticField<int>(typeof(SpeedBoosterBlockPlmProgramDefinitions), "ProgramCount"); index++)
                yield return checked((ushort)(PrivateState.Property<ushort>(((object)(PrivateState.InvokeStatic(typeof(SpeedBoosterBlockPlmProgramDefinitions), "ProgramAt", (int)(index)))!), "Start") + 2));
        }
    }
}

/// <summary>Verification access to <see cref="SporeSpawnCeilingPlmProgramDefinitions"/> members production does not use.</summary>
internal static class SporeSpawnCeilingPlmProgramDefinitionsAccess
{
    /// <summary>Exposes Spore Spawn ceiling instruction addresses for native-data checks.</summary>
    extension(SporeSpawnCeilingPlmProgramDefinitions)
    {
        /// <summary>Enumerates crumble and clear words read by the ceiling instruction programs.</summary>
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
    /// <summary>Provides fixture access to the station type and side cases encoded by BTS.</summary>
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
    /// <summary>Exposes station animation frames and native list words to verification.</summary>
    extension(StationAnimationProgramDefinitions)
    {
        /// <summary>Returns station instruction lists in native verification order.</summary>
        internal static IEnumerable<ushort> Lists()
        {
            yield return StationAnimationProgramDefinitions.MapIdle;
            yield return StationAnimationProgramDefinitions.MapAcquired;
            yield return StationAnimationProgramDefinitions.Energy;
            yield return StationAnimationProgramDefinitions.Missile;
            yield return RoomPlmInstructionLists.SaveStationIdleDraw;
            yield return RoomPlmInstructionLists.SaveStationAnimationFirstFrame;
            yield return RoomPlmInstructionLists.SaveStationAnimationSecondFrame;
        }

        /// <summary>Enumerates each station frame's duration and draw pointer at its native address.</summary>
        /// <summary>Yields each station frame duration and draw pointer at its native address.</summary>
        internal static IEnumerable<(ushort Address, ushort Value)> NativeWords()
        {
            foreach (ushort list in StationAnimationProgramDefinitions.Lists())
            for (int index = 0; index < (list is StationAnimationProgramDefinitions.MapIdle or StationAnimationProgramDefinitions.MapAcquired or StationAnimationProgramDefinitions.Energy or StationAnimationProgramDefinitions.Missile ? 3 : 1); index++)
            {
                StationAnimationProgramDefinitions.Frame frame = StationAnimationProgramDefinitions.Resolve(list, index);
                yield return (checked((ushort)(list + 4 * index)), frame.Duration);
                yield return (checked((ushort)(list + 4 * index + 2)), frame.DrawPointer);
            }
        }
    }
}

/// <summary>Verification access to <see cref="TourianAccessPlmProgramDefinitions"/> members production does not use.</summary>
internal static class TourianAccessPlmProgramDefinitionsAccess
{
    /// <summary>Exposes Tourian access instruction addresses for native-data verification.</summary>
    extension(TourianAccessPlmProgramDefinitions)
    {
        /// <summary>Enumerates Tourian crumble and clear words consumed by the translated programs.</summary>
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
    /// <summary>Creates explicitly partial X-ray overlay catalogs for focused room fixtures.</summary>
    extension(XrayOverlayVisualCatalog)
    {
        /// <summary>Explicitly partial room overlays for focused fixtures; never an installed catalog.</summary>
        internal static XrayOverlayVisualCatalog FromOverlaysForVerification(IEnumerable<ushort> itemMetatiles,
            IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)> rooms) =>
            ((XrayOverlayVisualCatalog)PrivateState.Construct(typeof(XrayOverlayVisualCatalog), (IEnumerable<ushort>)(itemMetatiles), (IEnumerable<(ushort Pointer, IReadOnlyList<XrayRoomOverlayVisual> Tiles)>)(rooms), (bool)(false)));
    }
}

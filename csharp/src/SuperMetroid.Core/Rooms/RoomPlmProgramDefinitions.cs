using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Exact compiled bank-$84 program operands shared by the interpreter and static
/// coverage audit. Draw pointers are program operands; their payloads remain in
/// the separate draw domain. A miss never supplies ROM or live-memory bytes.
/// </summary>
internal static class RoomPlmProgramDefinitions
{
    internal static bool TryReadWord(ushort address, out ushort value) =>
        RoomPlmBombBlockProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        RoomPlmContactCrumbleProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        RoomPlmGrappleBlockProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        EscapeAnimalPlmProgramDefinitions.TryReadWord(address, out value) ||
        RoomPlmBombedRevealProgramDefinitions.TryReadWord(address, out value) ||
        SamusEaterPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        TourianAccessPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        BotwoonWallPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        KraidRoomPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        CrocomireArenaPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        MaridiaElevatubePlmDefinitions.TryReadMechanicsWord(address, out value) ||
        SpeedBoosterEscapePlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        MetroidsClearedPlmRomData.TryReadInstructionWord(address, out value) ||
        WreckedShipAtticPlmRomData.TryReadInstructionWord(address, out value) ||
        ShaktoolRoomPlmRomData.TryReadInstructionWord(address, out value) ||
        SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmShotBlockProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmBombBlockProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        DownwardGatePlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        NoobTubePlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        DraygonCannonPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        BombTorizoHandPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        ChozoStatuePlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmSharedDeleteProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        ElevatorPlatformPlmDefinitions.TryReadMechanicsWord(address, out value) ||
        BlueDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        ColoredDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        GreyDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        BombTorizoGreyDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        EyeDoorPlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsWord(address, out value);

    internal static bool TryReadByte(ushort address, out byte value) =>
        EscapeAnimalPlmProgramDefinitions.TryReadByte(address, out value) ||
        SamusEaterPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        TourianAccessPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        SporeSpawnCeilingPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        BotwoonWallPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        KraidRoomPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        MaridiaElevatubePlmDefinitions.TryReadMechanicsByte(address, out value) ||
        SpeedBoosterBlockPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmShotBlockProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmBombBlockProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        DownwardGatePlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        NoobTubePlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        MotherBrainGlassPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        DraygonCannonPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        BombTorizoHandPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        ChozoStatuePlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmSharedDeleteProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        BlueDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        ColoredDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        GreyDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        BombTorizoGreyDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        EyeDoorPlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsByte(address, out value);
}

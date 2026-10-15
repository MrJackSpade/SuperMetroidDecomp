using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Exact compiled bank-$84 program operands shared by the interpreter and static
/// coverage audit. Draw pointers are program operands; their payloads remain in
/// the separate draw domain. A miss never supplies ROM or live-memory bytes.
/// </summary>
internal static class RoomPlmProgramDefinitions
{
    /// <summary>Dispatches a bank-$84 word read to compiled PLM mechanics or draw-pointer operands; misses leave no cartridge fallback.</summary>
    internal static bool TryReadWord(ushort address, out ushort value) =>
        RoomPlmBombBlockProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        RoomPlmContactCrumbleProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        RoomPlmGrappleBlockProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        EscapeAnimalPlmProgramDefinitions.TryReadWord(address, out value) ||
        OldTourianEscapeShaftWallPlmDefinitions.TryReadMechanicsWord(address, out value) ||
        CrateriaMainstreetEscapePassagePlmDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmBombedRevealProgramDefinitions.TryReadWord(address, out value) ||
        MaridiaElevatubePlmDefinitions.TryReadMechanicsWord(address, out value) ||
        ShaktoolRoomPlmRomData.TryReadInstructionWord(address, out value) ||
        RoomPlmShotBlockProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmShotBlockProgramDefinitions.TryReadDrawPointerWord(address, out value) ||
        RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmBombBlockProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        ChozoStatuePlmProgramDefinitions.TryReadMechanicsWord(address, out value) ||
        ElevatorPlatformPlmDefinitions.TryReadMechanicsWord(address, out value) ||
        MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsWord(address, out value);

    /// <summary>Dispatches byte reads for compiled PLM instruction streams, including unaligned operands.</summary>
    internal static bool TryReadByte(ushort address, out byte value) =>
        EscapeAnimalPlmProgramDefinitions.TryReadByte(address, out value) ||
        MaridiaElevatubePlmDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmShotBlockProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmBombBlockProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        ChozoStatuePlmProgramDefinitions.TryReadMechanicsByte(address, out value) ||
        MotherBrainEscapeGatePlmProgramDefinitions.TryReadMechanicsByte(address, out value);
}

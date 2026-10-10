// Value shapes returned by verification access members. Production state carries no
// test-only view records; each one here mirrors the private state its access member reads.

namespace SuperMetroid.Core.Game
{
    /// <summary>One native <c>(signed Y offset, timer)</c> Mother Brain corpse-rotting table record.</summary>
    internal readonly record struct MotherBrainCorpseRotEntry(short YOffset, ushort Timer);

    /// <summary>One shared native <c>(signed Y offset, timer)</c> corpse-rotting record.</summary>
    internal readonly record struct CorpseRottingTableEntry(short YOffset, ushort Timer);

    /// <summary>One Crocomire BG2 frame and the vertical correction its spritemap applies.</summary>
    internal readonly record struct CrocomireBg2VerticalCorrection(ushort SpritemapPointer, ushort Offset);

    /// <summary>One bank-$B2 Space Pirate collision frame and its components.</summary>
    internal readonly record struct SpacePirateCollisionFrame(ushort Pointer, SpacePirateCollisionComponent[] Components);

    /// <summary>One bank-$B2 Space Pirate hitbox list and its rectangles.</summary>
    internal readonly record struct SpacePirateCollisionList(ushort Pointer, SpacePirateCollisionHitbox[] Rectangles);

    /// <summary>One Dachora instruction program: its entry, timed-frame count, and whether it loops.</summary>
    internal readonly record struct DachoraInstructionProgram(ushort Entry, int FrameCount, bool Loops);
}

namespace SuperMetroid.Core.Rooms
{
    /// <summary>Verification view of one occupied physical PLM slot.</summary>
    internal readonly record struct RoomPlmSlotSnapshot(
        int NativeSlotIndex,
        PlmHeaderId HeaderPointer,
        int BlockIndex,
        ushort RoomArgument,
        ushort InstructionPointer,
        ushort PreInstruction,
        ushort InstructionTimer,
        ushort LinkInstruction,
        ushort LoopTimer);

    /// <summary>Verification view of a resident colored-door PLM slot.</summary>
    internal readonly record struct ColoredDoorPlmSnapshot(
        PlmHeaderId Header,
        int BlockIndex,
        ushort RoomArgument,
        ColoredDoorColor Color,
        ColoredDoorOrientation Orientation,
        ColoredDoorPhase Phase,
        byte HitCounter);

    /// <summary>Verification view of a resident grey-door PLM slot.</summary>
    internal readonly record struct GreyDoorPlmSnapshot(
        PlmHeaderId Header,
        int BlockIndex,
        ushort RoomArgument,
        ColoredDoorOrientation Orientation,
        GreyDoorCondition Condition,
        GreyDoorPhase Phase,
        ushort InitialList,
        ushort FlashList,
        ushort OpeningList);

    /// <summary>Verification view of one of the three physical eye-door PLMs.</summary>
    internal readonly record struct EyeDoorPlmSnapshot(
        PlmHeaderId Header,
        int BlockIndex,
        ushort RoomArgument,
        EyeDoorComponent Component,
        byte HitCounter,
        ushort InstructionPointer,
        ushort PreInstruction);

    /// <summary>One resident door header and the closing instruction list it selects.</summary>
    internal readonly record struct ResidentDoorClosingDefinition(PlmHeaderId Header, ushort ClosingInstructionList);
}

namespace SuperMetroid.Core.Runtime
{
    /// <summary>Auditable host placement chosen by the verification grounded-Samus initializer.</summary>
    internal readonly record struct DebugGroundedSamusPlacement(
        ushort XPosition,
        ushort YPosition,
        ushort DesiredScreenY,
        int BlockX,
        int BlockY,
        SuperMetroid.Core.Rooms.RoomCollisionBlock FloorBlock,
        byte FloorHeight);
}

// Value shapes returned by verification access members. Production state carries no
// test-only view records; each one here mirrors the private state its access member reads.

namespace SuperMetroid.Core.Game
{
    /// <summary>One native <c>(signed Y offset, timer)</c> Mother Brain corpse-rotting table record.</summary>
    /// <param name="YOffset">Signed vertical displacement applied to the corpse sprite.</param>
    /// <param name="Timer">Native countdown associated with this displacement.</param>
    internal readonly record struct MotherBrainCorpseRotEntry(short YOffset, ushort Timer);

    /// <summary>One shared native <c>(signed Y offset, timer)</c> corpse-rotting record.</summary>
    /// <param name="YOffset">Signed vertical displacement applied during corpse decay.</param>
    /// <param name="Timer">Native countdown paired with this displacement.</param>
    internal readonly record struct CorpseRottingTableEntry(short YOffset, ushort Timer);

    /// <summary>One Crocomire BG2 frame and the vertical correction its spritemap applies.</summary>
    /// <param name="SpritemapPointer">Bank-relative pointer identifying the BG2 composition.</param>
    /// <param name="Offset">Vertical adjustment associated with the selected composition.</param>
    internal readonly record struct CrocomireBg2VerticalCorrection(ushort SpritemapPointer, ushort Offset);

    /// <summary>One bank-$B2 Space Pirate collision frame and its components.</summary>
    /// <param name="Pointer">Bank-relative address of the native collision frame.</param>
    /// <param name="Components">Collision parts extracted from that frame, in native order.</param>
    internal readonly record struct SpacePirateCollisionFrame(ushort Pointer, SpacePirateCollisionComponent[] Components);

    /// <summary>One bank-$B2 Space Pirate hitbox list and its rectangles.</summary>
    /// <param name="Pointer">Bank-relative address identifying this hitbox list.</param>
    /// <param name="Rectangles">Rectangles comprising the list, preserving cartridge order.</param>
    internal readonly record struct SpacePirateCollisionList(ushort Pointer, SpacePirateCollisionHitbox[] Rectangles);

    /// <summary>One Dachora instruction program: its entry, timed-frame count, and whether it loops.</summary>
    /// <param name="Entry">Bank-relative instruction-list entry point.</param>
    /// <param name="FrameCount">Number of timed visual frames represented by the program.</param>
    /// <param name="Loops">Whether execution returns to repeat the animation sequence.</param>
    internal readonly record struct DachoraInstructionProgram(ushort Entry, int FrameCount, bool Loops);
}

namespace SuperMetroid.Core.Rooms
{
    /// <summary>Verification view of one occupied physical PLM slot.</summary>
    /// <param name="NativeSlotIndex">Zero-based index of the occupied slot in the room's fixed PLM array.</param>
    /// <param name="HeaderPointer">Pointer to the PLM definition that created this resident entry.</param>
    /// <param name="BlockIndex">Room block currently associated with the PLM.</param>
    /// <param name="RoomArgument">Per-instance argument copied from the room's PLM record.</param>
    /// <param name="InstructionPointer">Current instruction-list position.</param>
    /// <param name="PreInstruction">Pre-instruction routine selector applied before list dispatch.</param>
    /// <param name="InstructionTimer">Remaining delay before the current instruction advances.</param>
    /// <param name="LinkInstruction">Saved link target used by linked PLM instruction flow.</param>
    /// <param name="LoopTimer">Remaining loop delay maintained for this resident slot.</param>
    internal readonly record struct RoomPlmSlotSnapshot(
        int NativeSlotIndex,
        ushort HeaderPointer,
        int BlockIndex,
        ushort RoomArgument,
        ushort InstructionPointer,
        ushort PreInstruction,
        ushort InstructionTimer,
        ushort LinkInstruction,
        ushort LoopTimer);

    /// <summary>Verification view of a resident colored-door PLM slot.</summary>
    /// <param name="Header">Native PLM definition identifying the door.</param>
    /// <param name="BlockIndex">Room block occupied by the door.</param>
    /// <param name="RoomArgument">Instance argument stored in the room's PLM record.</param>
    /// <param name="Color">Door color controlling its projectile response.</param>
    /// <param name="Orientation">Axis along which the door opens.</param>
    /// <param name="Phase">Current closed, hit, or opening phase.</param>
    /// <param name="HitCounter">Accumulated qualifying hits used by the door logic.</param>
    internal readonly record struct ColoredDoorPlmSnapshot(
        ushort Header,
        int BlockIndex,
        ushort RoomArgument,
        ColoredDoorColor Color,
        ColoredDoorOrientation Orientation,
        ColoredDoorPhase Phase,
        byte HitCounter);

    /// <summary>Verification view of a resident grey-door PLM slot.</summary>
    /// <param name="Header">Native PLM definition identifying the grey door.</param>
    /// <param name="BlockIndex">Room block occupied by the door.</param>
    /// <param name="RoomArgument">Instance argument stored in the room's PLM record.</param>
    /// <param name="Orientation">Axis along which the door opens.</param>
    /// <param name="Condition">Gameplay condition that governs whether the door may open.</param>
    /// <param name="Phase">Current instruction-driven grey-door phase.</param>
    /// <param name="InitialList">Instruction list run when the door initializes.</param>
    /// <param name="FlashList">Instruction list used for the door's flash response.</param>
    /// <param name="OpeningList">Instruction list that performs the opening sequence.</param>
    internal readonly record struct GreyDoorPlmSnapshot(
        ushort Header,
        int BlockIndex,
        ushort RoomArgument,
        ColoredDoorOrientation Orientation,
        GreyDoorCondition Condition,
        GreyDoorPhase Phase,
        ushort InitialList,
        ushort FlashList,
        ushort OpeningList);

    /// <summary>Verification view of one of the three physical eye-door PLMs.</summary>
    /// <param name="Header">Native PLM definition identifying the eye door.</param>
    /// <param name="BlockIndex">Room block occupied by this eye-door component.</param>
    /// <param name="RoomArgument">Instance argument stored in the room's PLM record.</param>
    /// <param name="Component">Which physical eye-door component this slot represents.</param>
    /// <param name="HitCounter">Accumulated hits tracked by this component.</param>
    /// <param name="InstructionPointer">Current instruction-list position.</param>
    /// <param name="PreInstruction">Pre-instruction routine selector applied before list dispatch.</param>
    internal readonly record struct EyeDoorPlmSnapshot(
        ushort Header,
        int BlockIndex,
        ushort RoomArgument,
        EyeDoorComponent Component,
        byte HitCounter,
        ushort InstructionPointer,
        ushort PreInstruction);

    /// <summary>One resident door header and the closing instruction list it selects.</summary>
    /// <param name="Header">Native PLM header for the resident door.</param>
    /// <param name="ClosingInstructionList">Bank-relative instruction-list pointer used to close it.</param>
    internal readonly record struct ResidentDoorClosingDefinition(ushort Header, ushort ClosingInstructionList);
}

namespace SuperMetroid.Core.Runtime
{
    /// <summary>Auditable host placement chosen by the verification grounded-Samus initializer.</summary>
    /// <param name="XPosition">Player world X coordinate selected for the fixture.</param>
    /// <param name="YPosition">Player world Y coordinate selected for the fixture.</param>
    /// <param name="DesiredScreenY">Screen-space vertical target used to derive the placement.</param>
    /// <param name="BlockX">Room block column containing the support surface.</param>
    /// <param name="BlockY">Room block row containing the support surface.</param>
    /// <param name="FloorBlock">Collision data for the block supporting Samus.</param>
    /// <param name="FloorHeight">Surface height within the supporting block.</param>
    internal readonly record struct DebugGroundedSamusPlacement(
        ushort XPosition,
        ushort YPosition,
        ushort DesiredScreenY,
        int BlockX,
        int BlockY,
        SuperMetroid.Core.Rooms.RoomCollisionBlock FloorBlock,
        byte FloorHeight);
}

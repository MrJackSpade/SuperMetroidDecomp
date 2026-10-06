namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$AA address.</summary>
internal readonly record struct ChozoStatueInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for the Lower Norfair and Wrecked Ship Chozo-statue
/// sequences. Interleaved spritemap operands resolve to installed presentation
/// identities when artwork is bound; diagnostic buses may still supply them.
/// </summary>
internal static class ChozoStatueInstructionProgramDefinitions
{
    /// <summary><c>InstList_Chozo_LowerNorfair_Initial</c> at $AA:E39D.</summary>
    internal const ushort LowerNorfairInitial = 0xe39d;
    /// <summary><c>InstList_Chozo_LowerNorfair_Activated_0</c> at $AA:E3A7.</summary>
    internal const ushort LowerNorfairActivated = 0xe3a7;
    /// <summary><c>InstList_Chozo_WreckedShip_Initial</c> at $AA:E457.</summary>
    internal const ushort WreckedShipInitial = 0xe457;
    /// <summary><c>InstList_Chozo_WreckedShip_Activated_0</c> at $AA:E461.</summary>
    internal const ushort WreckedShipActivated = 0xe461;

    /// <summary>$AA:E445: Lower Norfair activation pre-instruction.</summary>
    private const ushort LowerNorfairActivation = 0xe445;
    /// <summary>$AA:E7AE: Wrecked Ship activation pre-instruction.</summary>
    private const ushort WreckedShipActivation = 0xe7ae;
    /// <summary>$AA:E7DA: Wrecked Ship carrying/walking pre-instruction.</summary>
    private const ushort WreckedShipWalking = 0xe7da;

    // Independent pose holds and footstep offsets remain unresolved under issue1165.
    // The layout conversion does not exempt these payloads or the per-scene holds.
    private static readonly ushort[] AcquisitionHolds = [32, 8, 80];
    private static readonly ushort[] StrideHolds = [8, 11, 8, 6];
    private static readonly short[] FootstepOffsets = [-8, -20, -16, 0];

    internal static int MechanicsWordCount => 166;
    internal static int PresentationWordCount => 52;

    internal static ChozoStatueInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, false).Selected;
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, true).Selected.Address;
    }

    /// <summary>
    /// $AA:E39D-E428/E457-E57E: shared acquisition, rise, breathing and release
    /// sequences, with the Wrecked Ship's two-stride carrying loop and final half
    /// stride. Commands occupy one word plus their declared operand; each pose
    /// occupies a duration and separate visual word. No instruction table is built.
    /// </summary>
    private static Layout BuildLayout(int index, bool presentation)
    {
        var layout = new Layout(index, presentation);
        BuildScene(ref layout, false);
        BuildScene(ref layout, true);
        return layout;
    }

    private static void BuildScene(ref Layout layout, bool wreckedShip)
    {
        layout.Address = wreckedShip ? WreckedShipInitial : LowerNorfairInitial;
        layout.Command(ChozoStatueInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY,
            wreckedShip ? WreckedShipActivation : LowerNorfairActivation);
        layout.Pose(1);
        layout.Word(CommonEnemyInstructionCodes.Sleep);
        layout.Word(ChozoStatueInstructionCodes.Instruction_CommonAA3_SetEnemy0FB2ToRTS);
        int movementBase = wreckedShip ? 0 : 32;
        for (int pose = 0; pose < 3; pose++)
        {
            layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_Movement_IndexInY, (ushort)(movementBase + pose * 2));
            layout.Pose(!wreckedShip && pose == 2 ? (ushort)48 : AcquisitionHolds[pose]);
        }
        layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoGrabsSamusSFX);
        layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_Movement_IndexInY, (ushort)(movementBase + 6));
        layout.Pose(wreckedShip ? (ushort)128 : (ushort)64);
        for (int pose = 0; pose < 4; pose++) layout.Pose((ushort)(6 + pose * 2));
        layout.Pose(wreckedShip ? (ushort)128 : (ushort)96);
        if (!wreckedShip) layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_StartLoweringAcid);
        layout.Command(CommonEnemyInstructionCodes.SetTimer, wreckedShip ? (ushort)4 : (ushort)5);
        ushort breathing = layout.Address;
        for (int pose = 0; pose < 6; pose++)
            layout.Pose(pose == 0 ? StrideHolds[1] : pose % 2 == 0 ? StrideHolds[3] : StrideHolds[0]);
        layout.Command(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, breathing);
        if (wreckedShip)
        {
            layout.Command(ChozoStatueInstructionCodes.Instruction_CommonAA_Enemy0FB2_InY, WreckedShipWalking);
            layout.Command(CommonEnemyInstructionCodes.SetTimer, 16);
            ushort walking = layout.Address;
            for (int pose = 0; pose < 8; pose++) BuildStridePose(ref layout, pose, true);
            layout.Command(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate, walking);
            for (int pose = 0; pose < 4; pose++) BuildStridePose(ref layout, pose, false);
        }
        layout.Word(ChozoStatueInstructionCodes.Instruction_CommonAA3_SetEnemy0FB2ToRTS);
        for (int pose = 0; pose < 4; pose++)
        {
            layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_Movement_IndexInY, (ushort)(movementBase + (3 - pose) * 2));
            layout.Pose(pose == 0 ? (ushort)128 : AcquisitionHolds[3 - pose]);
        }
        if (wreckedShip) layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_ReleaseSamus_BlockSlopeAccess);
        else
        {
            layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_UnlockSamus);
            layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_SetLoweredAcidPosition);
        }
        layout.Word(CommonEnemyInstructionCodes.Sleep);
    }

    private static void BuildStridePose(ref Layout layout, int pose, bool footsteps)
    {
        layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_Movement_IndexInY, (ushort)(pose == 0 ? 22 : 6 + pose * 2));
        layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_SpawnChozoSpikeClearingFootstepProjectile,
            unchecked((ushort)FootstepOffsets[pose % 4]));
        layout.Pose(StrideHolds[pose % 4]);
        if (footsteps && pose % 4 == 0) layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoFootstepsSFX);
    }

    private struct Layout(int target, bool presentation)
    {
        private int remaining = target;
        internal ushort Address;
        internal ChozoStatueInstructionMechanicsWord Selected;

        internal void Word(ushort value)
        {
            if (!presentation && remaining-- == 0) Selected = new(Address, value);
            Address += 2;
        }
        internal void Command(ushort instruction, ushort operand) { Word(instruction); Word(operand); }
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (presentation && remaining-- == 0) Selected = new(Address, 0);
            Address += 2;
        }
    }

    /// <summary>Returns fixed Chozo-statue control or rejects pointers outside its lists.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Chozo statue instruction mechanics pointer $AA:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xaa0000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}

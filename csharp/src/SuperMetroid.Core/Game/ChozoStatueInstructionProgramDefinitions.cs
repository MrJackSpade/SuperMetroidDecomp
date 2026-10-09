namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the Lower Norfair and Wrecked Ship Chozo-statue
/// sequences. Interleaved spritemap operands resolve to installed presentation
/// identities when artwork is bound; diagnostic buses may still supply them.
/// </summary>
internal abstract class ChozoStatueInstructionProgramDefinitions
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

    // Pose holds are authored animation cadence (reviewed under #1165). Footstep offsets are the
    // chosen spike-clearing spawn points within each drawn stride pose: compared with the support-foot
    // origins -31/-28/-14/-7 in ChozoStrideGeometryDefinitions they differ by +23/+8/-2/+7, so no
    // stride geometry derives them; they are placement choices attached to the artwork.
    /// <summary>Authored holds for the acquisition poses shared by the statue sequences.</summary>
    private static readonly ushort[] AcquisitionHolds = [32, 8, 80];
    /// <summary>Authored duration pattern used by the breathing and stride animations.</summary>
    private static readonly ushort[] StrideHolds = [8, 11, 8, 6];
    /// <summary>Signed vertical spawn offsets selected for the four repeating footstep poses.</summary>
    private static readonly short[] FootstepOffsets = [-8, -20, -16, 0];

    /// <summary>Number of instruction words available to mechanics-oriented consumers.</summary>
    public static int MechanicsWordCount => 166;
    /// <summary>Number of pose words whose following addresses identify installed presentation data.</summary>
    public static int PresentationWordCount => 52;

    /// <summary>Returns the mechanics operand at a position in the compiled Chozo statue instruction lists.</summary>
    /// <param name="index">Zero-based position among mechanics words.</param>
    /// <returns>The bank-$AA address and value of the selected word.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside <see cref="MechanicsWordCount"/>.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return BuildLayout(index, false).Selected;
    }

    /// <summary>Returns the address following a pose-duration word, where its presentation operand is stored.</summary>
    /// <param name="index">Zero-based position among pose words.</param>
    /// <returns>The bank-$AA address associated with the selected pose.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
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

    /// <summary>Appends one statue's acquisition, breathing, release, and any scene-specific control sequence.</summary>
    /// <param name="layout">Cursor and selection state shared with the compiled-list walk.</param>
    /// <param name="wreckedShip"><see langword="true"/> to append the Wrecked Ship scene; otherwise append Lower Norfair.</param>
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

    /// <summary>Appends one Wrecked Ship stride pose with its movement and footstep-projectile operands.</summary>
    /// <param name="layout">Cursor and selection state for the Wrecked Ship instruction list.</param>
    /// <param name="pose">Pose index used to choose movement data, footstep offset, and hold duration.</param>
    /// <param name="footsteps">Whether this pose is in the repeating carrying loop that emits periodic footstep sounds.</param>
    private static void BuildStridePose(ref Layout layout, int pose, bool footsteps)
    {
        layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_Movement_IndexInY, (ushort)(pose == 0 ? 22 : 6 + pose * 2));
        layout.Command(ChozoStatueInstructionCodes.Instruction_Chozo_SpawnChozoSpikeClearingFootstepProjectile,
            unchecked((ushort)FootstepOffsets[pose % 4]));
        layout.Pose(StrideHolds[pose % 4]);
        if (footsteps && pose % 4 == 0) layout.Word(ChozoStatueInstructionCodes.Instruction_Chozo_PlayChozoFootstepsSFX);
    }

    /// <summary>Tracks an instruction-list address and captures a requested mechanics word or presentation location.</summary>
    /// <param name="target">Zero-based word position to capture.</param>
    /// <param name="presentation">Whether selection targets pose presentation locations rather than mechanics operands.</param>
    private struct Layout(int target, bool presentation)
    {
        /// <summary>Number of eligible words still to skip before the target is selected.</summary>
        private int remaining = target;
        /// <summary>Current bank-$AA address while words are appended.</summary>
        internal ushort Address;
        /// <summary>Captured address and value for the requested mechanics word or presentation location.</summary>
        internal InstructionMechanicsWord Selected;

        /// <summary>Appends one word and captures it when it is the requested mechanics operand.</summary>
        /// <param name="value">Word value to place at the current address.</param>
        internal void Word(ushort value)
        {
            if (!presentation && remaining-- == 0) Selected = new(Address, value);
            Address += 2;
        }
        /// <summary>Appends a two-word instruction consisting of its opcode and operand.</summary>
        /// <param name="instruction">Native instruction opcode.</param>
        /// <param name="operand">Word operand consumed by that instruction.</param>
        internal void Command(ushort instruction, ushort operand) { Word(instruction); Word(operand); }
        /// <summary>Appends a timed pose and captures its following presentation location when requested.</summary>
        /// <param name="duration">Interpreter ticks for which the pose remains active.</param>
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
}

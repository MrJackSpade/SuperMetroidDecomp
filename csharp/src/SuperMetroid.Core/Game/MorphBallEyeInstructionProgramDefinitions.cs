namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled frame timing and terminal control for the Morph Ball eye body and mount.
/// Interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class MorphBallEyeInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Eye_Active</c> at $A8:8FAC.</summary>
    internal const ushort Active = 0x8fac;
    /// <summary><c>InstList_Eye_FacingRight_Deactivating</c> at $A8:8FF0.</summary>
    internal const ushort FacingRightDeactivating = 0x8ff0;
    /// <summary><c>InstList_Eye_FacingRight_Closed</c> at $A8:8FFC.</summary>
    internal const ushort FacingRightClosed = 0x8ffc;
    /// <summary><c>InstList_Eye_FacingLeft_Deactivating</c> at $A8:9002.</summary>
    internal const ushort FacingLeftDeactivating = 0x9002;
    /// <summary><c>InstList_Eye_FacingLeft_Closed</c> at $A8:900E.</summary>
    internal const ushort FacingLeftClosed = 0x900e;
    /// <summary><c>InstList_Eye_FacingRight_Activating</c> at $A8:9014.</summary>
    internal const ushort FacingRightActivating = 0x9014;
    /// <summary><c>InstList_Eye_FacingLeft_Activating</c> at $A8:9026.</summary>
    internal const ushort FacingLeftActivating = 0x9026;
    /// <summary><c>InstList_Eye_Mount_FacingRight</c> at $A8:9038.</summary>
    internal const ushort MountFacingRight = 0x9038;
    /// <summary><c>InstList_Eye_Mount_FacingDown</c> at $A8:903E.</summary>
    internal const ushort MountFacingDown = 0x903e;
    /// <summary><c>InstList_Eye_Mount_FacingLeft</c> at $A8:9044.</summary>
    internal const ushort MountFacingLeft = 0x9044;
    /// <summary><c>InstList_Eye_Mount_FacingUp</c> at $A8:904A.</summary>
    internal const ushort MountFacingUp = 0x904a;
    /// <summary><c>EyeConstants</c>, adjacent non-instruction data at $A8:9050.</summary>
    internal const ushort AdjacentProximityDefinitions = 0x9050;

    internal const int ActiveFrameCount = 16;

    // Authored eyelid cadence (reviewed under #1165): closing uses these holds forward,
    // opening reverses them after its activation delay; the reversal is calculated.
    private static readonly ushort[] EyelidDurations = [8, 48, 5];

    public static int MechanicsWordCount => 46;
    public static int PresentationWordCount => 36;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 18)
            return index < ActiveFrameCount ? new((ushort)(Active + index * 4), 10)
                : new((ushort)(Active + 64 + (index - 16) * 2),
                    index == 16 ? CommonEnemyInstructionCodes.Goto : Active);
        if (index < 28)
        {
            int local = (index - 18) % 5;
            ushort start = (ushort)(FacingRightDeactivating + (index - 18) / 5 * 18);
            return new((ushort)(start + local * 4),
                local == 4 ? CommonEnemyInstructionCodes.Sleep : EyelidDurations[local == 3 ? 1 : local]);
        }
        if (index < 38)
        {
            int local = (index - 28) % 5;
            ushort start = (ushort)(FacingRightActivating + (index - 28) / 5 * 18);
            return new((ushort)(start + local * 4), local switch
            {
                0 => 32,
                4 => CommonEnemyInstructionCodes.Sleep,
                _ => EyelidDurations[3 - local],
            });
        }
        int mountWord = index - 38;
        return new((ushort)(MountFacingRight + mountWord / 2 * 6 + (mountWord % 2) * 4),
            (mountWord & 1) == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
    }

    /// <summary>
    /// Native $A8:8FAC-904F contains sixteen tracking frames, two four-frame closing
    /// sequences, two four-frame opening sequences and four one-frame mount programs.
    /// Sleep/goto control widths determine the gaps between their visual operands.
    /// </summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 16) return (ushort)(Active + index * 4 + 2);
        if (index < 24)
            return (ushort)(FacingRightDeactivating + (index - 16) / 4 * 18 + (index % 4) * 4 + 2);
        if (index < 32)
            return (ushort)(FacingRightActivating + (index - 24) / 4 * 18 + (index % 4) * 4 + 2);
        return (ushort)(MountFacingRight + (index - 32) * 6 + 2);
    }

    /// <summary>True only for an eye-body or mount visual operand.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
            if (PresentationWordAddress(index) == address) return true;
        return false;
    }

    /// <summary>Returns fixed eye control or rejects pointers outside its eleven lists.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Morph Ball eye instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }

}

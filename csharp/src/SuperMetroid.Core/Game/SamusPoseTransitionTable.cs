using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Read-only port of <c>DetermineProspectivePoseFromTransitionTable</c> at <c>$91:81A9</c>.
/// </summary>
/// <remarks>
/// This matcher deliberately stops before applying a prospective pose. The runtime owns the
/// much larger transition contract and currently applies only the independently verified
/// standing/right-running route. The returned record remains valuable debugger state: it
/// proves which cartridge entry won for a precise held/newly-pressed input chord.
/// </remarks>
public static class SamusPoseTransitionTable
{
    private const int PosePointerTable = 0x919ee2;
    private const ushort StartAndSelectMask = 0x3000;

    /// <summary>
    /// Finds the first matching transition for canonical Super Metroid input bits.
    /// </summary>
    /// <param name="bus">CPU address space containing bank-$91 transition data.</param>
    /// <param name="currentPose">Current pose whose pointer-table entry is selected.</param>
    /// <param name="canonicalHeldInput">
    /// Held input after custom bindings have been translated to the table's canonical bits.
    /// Directional bits already equal their SNES hardware values; canonical action bits are
    /// jump=$0080, shoot=$0040, aim-down=$0020, aim-up=$0010.
    /// </param>
    /// <param name="canonicalNewInput">Rising-edge form of the same canonical input.</param>
    public static SamusPoseTransition? Find(
        ISnesAddressSpace bus,
        byte currentPose,
        ushort canonicalHeldInput,
        ushort canonicalNewInput)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // $91:81A9 branches directly to lookup-failure handling when the raw controller
        // word is zero, before it reads this table at all.
        if (canonicalHeldInput == 0)
            return null;

        // Start and Select never participate in pose chords. $91:81F4 initializes its two
        // complement masks from only bits $0F00, then adds canonical action bits explicitly.
        ushort held = (ushort)(canonicalHeldInput & ~StartAndSelectMask);
        ushort newlyPressed = (ushort)(canonicalNewInput & ~StartAndSelectMask);

        ushort tablePointer = ReadWord(bus, AddWithinBank(PosePointerTable, currentPose * 2));
        int entryAddress = 0x910000 | tablePointer;

        // A valid table ends with a single $FFFF word. The retail bank cannot contain more
        // than 10,923 six-byte records, so this guard diagnoses corrupt/synthetic data while
        // retaining fixed-bank wrapping in every actual address calculation.
        for (int entryIndex = 0; entryIndex < 10_923; entryIndex++)
        {
            ushort requiredNew = ReadWord(bus, entryAddress);
            if (requiredNew == 0xffff)
                return null;

            ushort requiredHeld = ReadWord(bus, AddWithinBank(entryAddress, 2));
            ushort prospectivePose = ReadWord(bus, AddWithinBank(entryAddress, 4));

            // Assembly complements the actual inputs and rejects an entry if any required
            // bit is absent. Expressing that as (required & actual)==required is equivalent
            // and makes clear that extra held buttons are allowed. ROM order supplies
            // priority when several entries match the same larger chord.
            bool newMatches = requiredNew == 0 || (requiredNew & newlyPressed) == requiredNew;
            bool heldMatches = requiredHeld == 0 || (requiredHeld & held) == requiredHeld;
            if (newMatches && heldMatches)
            {
                // $91:81E7 treats a transition back to the current pose as not found.
                if (prospectivePose == currentPose)
                    return null;

                return new SamusPoseTransition(
                    CurrentPose: currentPose,
                    ProspectivePose: prospectivePose,
                    RequiredNewInput: requiredNew,
                    RequiredHeldInput: requiredHeld,
                    EntryAddress: entryAddress);
            }

            entryAddress = AddWithinBank(entryAddress, 6);
        }

        throw new InvalidDataException(
            $"Samus pose ${currentPose:X2} transition table did not terminate within bank $91.");
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

}

/// <summary>Debugger-readable winning six-byte transition-table record.</summary>
public readonly record struct SamusPoseTransition(
    byte CurrentPose,
    ushort ProspectivePose,
    ushort RequiredNewInput,
    ushort RequiredHeldInput,
    int EntryAddress);

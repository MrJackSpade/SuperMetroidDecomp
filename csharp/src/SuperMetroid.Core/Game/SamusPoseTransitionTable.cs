using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Read-only port of <c>DetermineProspectivePoseFromTransitionTable</c> at <c>$91:81A9</c>.
/// </summary>
/// <remarks>
/// This matcher deliberately stops before applying a prospective pose. The runtime owns the
/// transition side effects. Authored conditions are compiled; non-authored pose indexes fail
/// explicitly instead of parsing adjacent bank-$91 code as a transition list. The returned
/// record remains valuable debugger state: it proves which cartridge entry won for a precise
/// held/newly-pressed input chord.
/// </remarks>
public static class SamusPoseTransitionTable
{
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
        ushort canonicalNewInput) =>
        Lookup(bus, currentPose, canonicalHeldInput, canonicalNewInput).Transition;

    /// <summary>
    /// Runs the complete native lookup and preserves whether failure enters
    /// <c>Samus_Pose_Func2</c>'s pose-definition fallback.
    /// </summary>
    /// <remarks>
    /// There are two observably different ways to return without a prospective pose.
    /// Zero input or exhausting a nonempty table calls <c>$91:82D9</c>. A table
    /// beginning with <c>$FFFF</c> returns directly while any input is held;
    /// matching a record whose target is the current pose returns directly. A nullable
    /// transition alone cannot represent that distinction. In particular, running-gun
    /// pose <c>$0B</c> matches itself while Right+Shoot is held, but Shoot alone reaches
    /// the terminator and must fall back to standing pose <c>$01</c>.
    /// </remarks>
    public static SamusPoseTransitionLookup Lookup(
        ISnesAddressSpace bus,
        byte currentPose,
        ushort canonicalHeldInput,
        ushort canonicalNewInput)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // $91:81A9 branches directly to lookup-failure handling when the raw controller
        // word is zero, before it reads this table at all.
        if (canonicalHeldInput == 0)
            return new SamusPoseTransitionLookup(null, UsesPoseDefinitionFallback: true);

        // Start and Select never participate in pose chords. $91:81F4 initializes its two
        // complement masks from only bits $0F00, then adds canonical action bits explicitly.
        ushort held = (ushort)(canonicalHeldInput & ~StartAndSelectMask);
        ushort newlyPressed = (ushort)(canonicalNewInput & ~StartAndSelectMask);

        if (!SamusPoseInputDefinitions.TryGet(currentPose, out ushort tablePointer, out var rules))
        {
            throw new InvalidDataException(
                $"Pose ${currentPose:X2} has no authored input-transition graph; " +
                "adjacent bank-$91 code is not mechanics metadata.");
        }
        int entryAddress = SamusMovementRomData.Banks.Pose | tablePointer;

        for (int entryIndex = 0; ; entryIndex++)
        {
            if (entryIndex == rules.Length)
            {
                return new SamusPoseTransitionLookup(
                    null,
                    UsesPoseDefinitionFallback: entryIndex != 0);
            }

            SamusPoseInputRule rule = rules[entryIndex];
            ushort requiredNew = rule.RequiredNewInput;
            ushort requiredHeld = rule.RequiredHeldInput;
            ushort prospectivePose = rule.TargetPose;

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
                {
                    return new SamusPoseTransitionLookup(
                        null,
                        UsesPoseDefinitionFallback: false);
                }

                return new SamusPoseTransitionLookup(
                    new SamusPoseTransition(
                        CurrentPose: currentPose,
                        ProspectivePose: prospectivePose,
                        RequiredNewInput: requiredNew,
                        RequiredHeldInput: requiredHeld,
                        EntryAddress: entryAddress),
                    UsesPoseDefinitionFallback: false);
            }

            entryAddress = AddWithinBank(entryAddress, 6);
        }
    }

}

/// <summary>Debugger-readable winning six-byte transition-table record.</summary>
public readonly record struct SamusPoseTransition(
    byte CurrentPose,
    ushort ProspectivePose,
    ushort RequiredNewInput,
    ushort RequiredHeldInput,
    int EntryAddress);

/// <summary>
/// Full control-flow result from <c>$91:81A9</c>, including its otherwise invisible
/// branch into pose-definition fallback at <c>$91:82D9</c>.
/// </summary>
public readonly record struct SamusPoseTransitionLookup(
    SamusPoseTransition? Transition,
    bool UsesPoseDefinitionFallback);

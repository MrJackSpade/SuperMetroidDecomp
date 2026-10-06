namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A8 address.</summary>
internal readonly record struct SparkInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>Compiled mechanics words from Wrecked Ship Spark's four animation programs.</summary>
/// <remarks>
/// Callback identities, durations, terminal control, and branch targets are immutable
/// simulation data. Spritemap selections resolve compiled identities to installed artwork.
/// </remarks>
internal static class SparkInstructionProgramDefinitions
{
    /// <summary><c>$A8:E5A7</c>, make tangible and flicker into the active loop.</summary>
    internal const ushort FlickerOn = 0xe5a7;

    /// <summary><c>$A8:E5D1</c>, continuously active four-frame loop.</summary>
    internal const ushort Active = 0xe5d1;

    /// <summary><c>$A8:E5E5</c>, flicker out, become intangible, and sleep.</summary>
    internal const ushort FlickerOut = 0xe5e5;

    /// <summary><c>$A8:E609</c>, stationary falling-spark emitter loop.</summary>
    internal const ushort Emitter = 0xe609;

    /// <summary>$A8:E62A, Instruction_Spark_SetAsTangible clears IgnoreSamusCollision before activation.</summary>
    private const ushort SetTangible = 0xe62a;
    /// <summary>$A8:E61D, Instruction_Spark_SetAsIntangible sets IgnoreSamusCollision after deactivation.</summary>
    private const ushort SetIntangible = 0xe61d;
    /// <summary>$A8:E5A9/B1/B9/C1: four visible flashes, two each of flickering poses zero and one. Independently reviewed visual choreography: tangible before the sequence; lifetime uses a separate timer.</summary>
    private const ushort ActivationFlashOnTicks = 1;
    /// <summary>$A8:E5AD/B5/BD: empty spritemap between the first three flashes. Independently reviewed visual blank cadence; the nonzero empty-map pointer preserves the fixed collision box.</summary>
    private const ushort ActivationFlashOffTicks = 2;
    /// <summary>$A8:E5C5: shorter final blank before sustained activation. Independently reviewed final visual gap, without a gameplay callback.</summary>
    private const ushort ActivationFinalGapTicks = 1;
    /// <summary>$A8:E5C9/CD: successive poses two and three without an intervening blank. Independently reviewed sustained visual cadence before the continuous loop.</summary>
    private const ushort ActivationSustainedTicks = 2;
    /// <summary>$A8:E5D1-E5DD and E609-E615: reviewed three-tick continuous visual cadence. Both four-pose loops have no callbacks; separate function timers own lifetime and emission, and nonzero maps preserve fixed contact radii.</summary>
    private const ushort ContinuousVisualCadence = 3;
    /// <summary>$A8:E5E5-E601: every deactivation pose/blank interval advances after one tick; this choice and its later tangibility callback remain outside the activation exception.</summary>
    private const ushort UnresolvedFlickerOutCadence = 1;

    internal static int MechanicsWordCount => 33;
    internal static int PresentationWordCount => 26;
    internal static SparkInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index == 0) return new(FlickerOn, SetTangible);
        if (index <= 10) return new((ushort)(FlickerOn + 2 + (index - 1) * 4), ActivationDuration(index - 1));
        if (index < 17) return LoopWord(Active, index - 11);
        if (index < 25) return new((ushort)(FlickerOut + (index - 17) * 4), UnresolvedFlickerOutCadence);
        if (index < 27) return new((ushort)(FlickerOut + 32 + (index - 25) * 2),
            index == 25 ? SetIntangible : CommonEnemyInstructionCodes.Sleep);
        return LoopWord(Emitter, index - 27);
    }
    private static ushort ActivationDuration(int frame)
    {
        if (frame >= 8) return ActivationSustainedTicks;
        if ((frame & 1) == 0) return ActivationFlashOnTicks;
        return frame == 7 ? ActivationFinalGapTicks : ActivationFlashOffTicks;
    }
    private static SparkInstructionMechanicsWord LoopWord(ushort start, int index) => index < 4
        ? new((ushort)(start + index * 4), ContinuousVisualCadence)
        : new((ushort)(start + 16 + (index - 4) * 2), index == 4 ? CommonEnemyInstructionCodes.Goto : start);

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 10) return (ushort)(FlickerOn + 4 + index * 4);
        if (index < 14) return (ushort)(Active + 2 + (index - 10) * 4);
        if (index < 22) return (ushort)(FlickerOut + 2 + (index - 14) * 4);
        return (ushort)(Emitter + 2 + (index - 22) * 4);
    }
    /// <summary>Returns one fixed control word or rejects pointers outside the four programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SparkInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Wrecked Ship Spark instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
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

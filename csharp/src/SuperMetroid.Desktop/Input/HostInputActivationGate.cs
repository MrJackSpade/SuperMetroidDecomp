namespace SuperMetroid.Desktop.Input;

/// <summary>
/// Prevents a controller state sampled before desktop deactivation from becoming a
/// permanent held input when Windows or an RDP session drops the corresponding release.
/// </summary>
/// <remarks>
/// The gate does not guess that a long press is invalid. After deactivation it requires
/// one completely neutral merged keyboard/gamepad sample before accepting input again.
/// This preserves arbitrary legitimate holds while making focus loss an explicit release
/// boundary for both host producers.
/// </remarks>
internal sealed class HostInputActivationGate
{
    /// <summary>Set on deactivation and cleared only by a neutral merged device sample.</summary>
    private bool suppressedUntilNeutral;

    /// <summary>Suppresses subsequent input until every host control has been released.</summary>
    public void SuppressUntilNeutral() => suppressedUntilNeutral = true;

    /// <summary>Returns a safe controller word for the current activation state.</summary>
    public ushort Filter(ushort input)
    {
        if (!suppressedUntilNeutral)
            return input;

        if (input == 0)
            suppressedUntilNeutral = false;
        return 0;
    }
}

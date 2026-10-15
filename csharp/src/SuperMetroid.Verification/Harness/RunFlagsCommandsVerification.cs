namespace SuperMetroid.Verification.Harness;

/// <summary>Accepts verification command flags whose current handlers return success without work.</summary>
internal static class RunFlagsCommandsVerification
{

    /// <summary>Recognizes the gameplay-message definition flag and exits successfully without running a suite.</summary>
    [VerificationCommand("--gameplay-message-definitions")]
    internal static int GameplayMessageDefinitionsCommand()
    {
        return 0;
    }

    /// <summary>Recognizes the Android-host flag and exits successfully without running a suite.</summary>
    [VerificationCommand("--android-host")]
    internal static int AndroidHostCommand()
    {
        return 0;
    }
}

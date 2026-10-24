namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>Native Boyon values the behavior check expects. Only verification reads them.</summary>
internal static class BoyonNativeValues
{
    /// <summary>Library-two sound $0E, which <c>Instruction_Boyon_88C6</c> at $A2:88C6 queues when a bounce starts.</summary>
    internal static readonly SoundEffectId BounceSound = Library2Sound.SplashOut;
}

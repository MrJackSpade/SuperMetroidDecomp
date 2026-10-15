namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>Native Oum values the Oum checks read or expect. Only verification reads them.</summary>
internal static class MaridiaLargeSnailNativeValues
{
    /// <summary>$A2:CB77 <c>InstListPointers_Oum</c>, the eight-entry animation selector table.</summary>
    internal const int SelectorTable = 0xa2cb77;

    /// <summary>Library-two sound $0E, which <c>Instruction_Oum_PlaySplashedOutOfWaterSFX</c> at $A2:CB6B queues.</summary>
    internal const ushort SplashSound = 0x000e;
}

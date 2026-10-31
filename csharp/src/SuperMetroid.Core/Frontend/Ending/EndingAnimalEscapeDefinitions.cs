namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Bank-$8B definitions for the rescued animals' escape pod ($EF21).</summary>
internal static class EndingAnimalEscapeDefinitions
{
    /// <summary>$8B:DD98 tests event $0F, then $DDA7 forces native byte slot $04.</summary>
    internal const int ForcedSlot = 2;
    /// <summary>$8B:EF99-EFA2 initializes both pod coordinates to $0080.</summary>
    internal const ushort Origin = 128;
    /// <summary>$8B:EFAB initializes the actor's general-purpose timer (not a spawn delay).</summary>
    internal const ushort GeneralTimer = 260;
    /// <summary>$8B:EFB6 adds $0080 to the 16-bit Y fraction each actor call.</summary>
    internal const ushort YFractionVelocity = 128;
    /// <summary>$8B:EFD2 advances one horizontal pixel per actor call.</summary>
    internal const ushort XVelocity = 1;
    /// <summary>$8B:EFD8 deletes the actor when its center reaches $0110.</summary>
    internal const ushort DeleteX = 272;
}

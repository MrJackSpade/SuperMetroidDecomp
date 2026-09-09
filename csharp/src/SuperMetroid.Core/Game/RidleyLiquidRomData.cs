namespace SuperMetroid.Core.Game;

/// <summary>Native Lower Norfair Ridley writes to the shared room liquid motion words.</summary>
internal static class RidleyLiquidRomData
{
    /// <summary>$A6:A478 reveal completion sets the acid target to 440 world pixels.</summary>
    public const ushort BattleHeight = 440;
    /// <summary>$A6:A478 rising velocity: signed -96 in the cartridge's 8.8 format.</summary>
    public const ushort RiseVelocity = unchecked((ushort)-96);
    /// <summary>$A6:A478 delay before the acid starts rising.</summary>
    public const ushort RiseDelay = 32;
    /// <summary>$A6:C551 death-roar completion lowers acid to 528 world pixels.</summary>
    public const ushort DrainedHeight = 528;
    /// <summary>$A6:C551 downward velocity: 64 in 8.8 format.</summary>
    public const ushort DrainVelocity = 64;
    /// <summary>$A6:C551 delay before the acid starts draining.</summary>
    public const ushort DrainDelay = 1;
}

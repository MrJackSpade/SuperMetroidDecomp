namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Commands in a charge-flare delay stream; every other byte is a delay. The values are arbitrary:
/// <c>ChargeFlareAnimationDefinitions</c> encodes each as its stream byte.
/// </summary>
internal enum ChargeFlareDelayCommand
{
    /// <summary>Stream byte $FE: subtracts the following byte from the selected frame.</summary>
    Rewind,
    /// <summary>Stream byte $FF: resets the frame to zero.</summary>
    Restart,
}

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Commands in a charge-flare delay stream; every other entry is a delay. The values are arbitrary:
/// <c>ChargeFlareAnimationDefinitions</c> encodes each as its stream entry.
/// </summary>
internal enum ChargeFlareDelayCommand
{
    /// <summary>Subtracts the following delay from the selected frame.</summary>
    Rewind,
    /// <summary>Resets the frame to zero.</summary>
    Restart,
}

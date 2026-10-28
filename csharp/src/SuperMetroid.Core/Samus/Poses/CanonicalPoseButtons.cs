namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The canonical action and direction buttons the bank-$91 transition tables name; combinations are required chords.
/// The values are arbitrary bits: <c>SamusPoseInputDefinitions</c> maps each to the controller button standing for it.
/// </summary>
[Flags]
internal enum CanonicalPoseButtons
{
    /// <summary>Canonical aim diagonally up.</summary>
    AimUp = 1 << 0,
    /// <summary>Canonical aim diagonally down.</summary>
    AimDown = 1 << 1,
    /// <summary>Canonical Shoot.</summary>
    Shoot = 1 << 2,
    /// <summary>Canonical Jump.</summary>
    Jump = 1 << 3,
    /// <summary>Right direction.</summary>
    Right = 1 << 4,
    /// <summary>Left direction.</summary>
    Left = 1 << 5,
    /// <summary>Down direction.</summary>
    Down = 1 << 6,
    /// <summary>Up direction.</summary>
    Up = 1 << 7,
}

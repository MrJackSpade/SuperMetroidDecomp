namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Norfair Ridley's facing, which selects his arena targets, claw offsets and turning pose. The cartridge's facing index
/// for each is verification's <c>RidleyNativeWords.Facings</c> (#1452).
/// </summary>
public enum RidleyFacing
{
    /// <summary>Ridley faces left.</summary>
    Left,
    /// <summary>Ridley is in the intermediate turning pose.</summary>
    Turning,
    /// <summary>Ridley faces right.</summary>
    Right,
}

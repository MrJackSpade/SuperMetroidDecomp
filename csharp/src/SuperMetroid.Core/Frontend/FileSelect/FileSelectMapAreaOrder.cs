using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The file-select map's area order used by label composition.</summary>
public static class FileSelectMapAreaOrder
{
    /// <summary>Translates a file-select area identity0..5 to the corresponding game area.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the game's order and its
    /// label/selection consumers. The menu and game use different categorical area
    /// numbering: area selection performs the inverse search, while label
    /// composition resolves this direction. Named result cases
    /// express that identity translation without a stored permutation or numeric fit.
    /// Unsupported int identities preserve ArgumentOutOfRangeException.
    /// </remarks>
    public static AreaId Get(int displayIndex) => displayIndex switch
    {
        0 => AreaId.Crateria,
        1 => AreaId.WreckedShip,
        2 => AreaId.Tourian,
        3 => AreaId.Brinstar,
        4 => AreaId.Maridia,
        5 => AreaId.Norfair,
        _ => throw new ArgumentOutOfRangeException(nameof(displayIndex)),
    };
}

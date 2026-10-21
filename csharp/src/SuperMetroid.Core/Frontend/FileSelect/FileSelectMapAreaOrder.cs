using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Compiled $81:AAA0 FileSelectMapArea_IndexTable order used by label composition.</summary>
public static class FileSelectMapAreaOrder
{
    /// <summary>Translates a native file-select area identity0..5 to the corresponding game area.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against all six words at $81:AAA0 and native
    /// label/selection consumers. The menu and game use different categorical area
    /// numbering: native $81:A398/$81:ADC3 perform the inverse search, while label
    /// composition at $81:A9EC/$81:A9FE resolves this direction. Named result cases
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

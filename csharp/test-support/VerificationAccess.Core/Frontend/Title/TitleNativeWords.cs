using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// Verification's native view of the title identities (#1416): the bank-$8B routine words naming each card command.
/// Core's values are arbitrary; a ROM oracle translates through this table.
/// </summary>
internal static class TitleNativeWords
{
    /// <summary>The routine word naming each <see cref="TitleCardCue"/>.</summary>
    internal static readonly NativeWordIdentities<TitleCardCue> Instructions = new(new Dictionary<TitleCardCue, ushort>
    {
        [TitleCardCue.RemoveCard] = 0x9438,
        [TitleCardCue.StartLowerPan] = 0x9ce1,
        [TitleCardCue.StartUpperPan] = 0x9d5d,
        [TitleCardCue.StartDownwardPan] = 0x9dd6,
        [TitleCardCue.StartFinalZoom] = 0x9e58,
    });

    /// <summary>$8C:879D, TitleSequenceSpritemaps_SuperMetroidTitleLogo: the logo composition's bank-$8C word.</summary>
    internal const ushort SuperMetroidLogoSpritemap = 0x879d;

    /// <summary>The bytes of a command word in a title card list.</summary>
    internal const int InstructionWordByteCount = 2;
}

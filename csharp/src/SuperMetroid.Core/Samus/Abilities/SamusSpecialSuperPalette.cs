using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Ports the Metroid-attachment branch of <c>HandleMiscSamusPalette</c>. A draining Metroid is the
/// retail producer: Samus alternates between the Speed Booster palette and the normal suit, one per update, until the
/// Metroid releases her (<see cref="SamusState.MetroidAttachmentPalette"/>).
/// </summary>
public static class SamusSpecialSuperPalette
{
    /// <returns>True when the alternation owned—and wrote—the visible Samus palette.</returns>
    public static bool Update(
        SamusColours colours,
        SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        switch (samus.MetroidAttachmentPalette)
        {
            case SamusMetroidAttachmentPalette.Inactive:
                return false;
            case SamusMetroidAttachmentPalette.SpeedBoostNext:
                colours.ShowSuit(SamusShades.FullBodyCycle(SamusFullBodyCyclePalettes.SpeedBoostPalette(
                    samus.EquippedItems.ShownSuit())));
                samus.MetroidAttachmentPalette = SamusMetroidAttachmentPalette.NormalSuitNext;
                return true;
            case SamusMetroidAttachmentPalette.NormalSuitNext:
                SamusNormalSuitPalette.Load(colours, samus.EquippedItems);
                samus.MetroidAttachmentPalette = SamusMetroidAttachmentPalette.SpeedBoostNext;
                return true;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(SamusMetroidAttachmentPalette)} {samus.MetroidAttachmentPalette}.");
        }
    }
}

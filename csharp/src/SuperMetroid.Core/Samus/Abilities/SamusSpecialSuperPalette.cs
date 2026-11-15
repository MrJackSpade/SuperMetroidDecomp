using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Runs the Metroid-attachment branch of Samus's miscellaneous colours. A draining Metroid is the
/// retail producer: Samus alternates between the Speed Booster colouring and the normal suit, one per update, until the
/// Metroid releases her (<see cref="SamusState.MetroidAttachmentColouring"/>).
/// </summary>
public static class SamusSpecialSuperPalette
{
    /// <returns>True when the alternation owned—and wrote—Samus's visible colours.</returns>
    public static bool Update(
        SamusColours colours,
        SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        switch (samus.MetroidAttachmentColouring)
        {
            case SamusMetroidAttachmentPalette.Inactive:
                return false;
            case SamusMetroidAttachmentPalette.SpeedBoostNext:
                colours.ShowSuit(SamusShades.FullBodyCycle(SamusFullBodyCyclePalettes.SpeedBoostPalette(
                    samus.EquippedItems.ShownSuit())));
                samus.MetroidAttachmentColouring = SamusMetroidAttachmentPalette.NormalSuitNext;
                return true;
            case SamusMetroidAttachmentPalette.NormalSuitNext:
                SamusNormalSuitPalette.Load(colours, samus.EquippedItems);
                samus.MetroidAttachmentColouring = SamusMetroidAttachmentPalette.SpeedBoostNext;
                return true;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(SamusMetroidAttachmentPalette)} {samus.MetroidAttachmentColouring}.");
        }
    }
}

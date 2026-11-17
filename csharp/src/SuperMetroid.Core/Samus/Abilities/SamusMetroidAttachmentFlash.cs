using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Runs the Metroid attachment's flashing among Samus's miscellaneous colours. A draining Metroid is the
/// only cause: Samus alternates between the Speed Booster glow and the normal suit, one per update, until the
/// Metroid releases her (<see cref="SamusState.MetroidAttachmentShade"/>).
/// </summary>
public static class SamusMetroidAttachmentFlash
{
    /// <returns>True when the alternation owned—and wrote—the visible Samus shade.</returns>
    public static bool Update(
        SamusColours colours,
        SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        switch (samus.MetroidAttachmentShade)
        {
            case SamusMetroidAttachmentShade.Inactive:
                return false;
            case SamusMetroidAttachmentShade.SpeedBoostNext:
                colours.ShowSuit(SamusShades.FullBodyCycle(SamusFullBodyCycleShades.SpeedBoostShade(
                    samus.EquippedItems.ShownSuit())));
                samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.NormalSuitNext;
                return true;
            case SamusMetroidAttachmentShade.NormalSuitNext:
                SamusSuitShade.Show(colours, samus.EquippedItems);
                samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.SpeedBoostNext;
                return true;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(SamusMetroidAttachmentShade)} {samus.MetroidAttachmentShade}.");
        }
    }
}

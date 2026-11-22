using SuperMetroid.Core.Samus.Colours;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// Runs the Metroid attachment's flashing among Samus's miscellaneous colours. A draining Metroid is the
/// only cause: Samus alternates between the Speed Booster glow and the normal suit, one per update, until the
/// Metroid releases her (<see cref="SamusState.MetroidAttachmentShade"/>).
/// </summary>
public static partial class SamusMetroidAttachmentFlash
{
    /// <summary>What each step of the alternation shows, and whether it owned and wrote the visible Samus shade.</summary>
    private static IdentityTable<SamusMetroidAttachmentShade, Func<SamusColours, SamusState, bool>> Alternations => field ??=
        new(new Dictionary<SamusMetroidAttachmentShade, Func<SamusColours, SamusState, bool>>
        {
            [SamusMetroidAttachmentShade.Inactive] = static (colours, samus) => false,
            [SamusMetroidAttachmentShade.SpeedBoostNext] = static (colours, samus) =>
            {
                colours.ShowSuit(SamusShades.FullBodyCycle(SamusFullBodyCycleShades.SpeedBoostShade(
                    samus.EquippedItems.ShownSuit())));
                samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.NormalSuitNext;
                return true;
            },
            [SamusMetroidAttachmentShade.NormalSuitNext] = static (colours, samus) =>
            {
                SamusSuitShade.Show(colours, samus.EquippedItems);
                samus.MetroidAttachmentShade = SamusMetroidAttachmentShade.SpeedBoostNext;
                return true;
            },
        });

    /// <returns>True when the alternation owned—and wrote—the visible Samus shade.</returns>
    public static bool Update(
        SamusColours colours,
        SamusState samus)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        return Alternations[samus.MetroidAttachmentShade](colours, samus);
    }
}

using SuperMetroid.Core;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// The installed editable gameplay messages (#1466): the one-row titles, the large item panels and the station and save
/// notices, from which the import lays out the tile words of each form a message box shows. The game names the form it
/// shows (<see cref="ShownMessage"/>) and the backend draws the image the import composed of these words.
/// </summary>
/// <param name="titles">The one-row titles, or null when none are installed.</param>
/// <param name="panels">The large item panels, or null when none are installed.</param>
/// <param name="notices">The station and save notices, or null when none are installed.</param>
public sealed class GameplayMessageArtwork(
    GameplayMessageTitlePresentation? titles,
    GameplayMessagePanelPresentation? panels = null,
    GameplayMessageNoticePresentation? notices = null)
{
    /// <summary>The messages the ordinary message coroutine shows, in the order the message sheets hold them (#2567).</summary>
    private static readonly IReadOnlyList<GameplayMessageId> MessageOrder = Ensure.EveryMemberOnce<GameplayMessageId>(
    [
        GameplayMessageId.EnergyTank,
        GameplayMessageId.MissileTank,
        GameplayMessageId.SuperMissileTank,
        GameplayMessageId.PowerBombTank,
        GameplayMessageId.GrappleBeam,
        GameplayMessageId.XrayScope,
        GameplayMessageId.VariaSuit,
        GameplayMessageId.SpringBall,
        GameplayMessageId.MorphBall,
        GameplayMessageId.ScrewAttack,
        GameplayMessageId.HiJumpBoots,
        GameplayMessageId.SpaceJump,
        GameplayMessageId.SpeedBooster,
        GameplayMessageId.ChargeBeam,
        GameplayMessageId.IceBeam,
        GameplayMessageId.WaveBeam,
        GameplayMessageId.SpazerBeam,
        GameplayMessageId.PlasmaBeam,
        GameplayMessageId.Bombs,
        GameplayMessageId.MapDataAccessCompleted,
        GameplayMessageId.EnergyRechargeCompleted,
        GameplayMessageId.MissileRechargeCompleted,
        GameplayMessageId.SaveConfirmation,
        GameplayMessageId.SaveCompleted,
        GameplayMessageId.ReserveTank,
        GameplayMessageId.GravitySuit,
        GameplayMessageId.GunshipSaveConfirmation,
    ]);

    /// <summary>The one-row titles, or null.</summary>
    public GameplayMessageTitlePresentation? Titles { get; } = titles;

    /// <summary>The large item panels, or null.</summary>
    public GameplayMessagePanelPresentation? Panels { get; } = panels;

    /// <summary>The station and save notices, or null.</summary>
    public GameplayMessageNoticePresentation? Notices { get; } = notices;

    /// <summary>
    /// The tile words of <paramref name="shown"/>, 32 per row: a notice with its save choice's selection row, a panel with
    /// the glyph of the button it names, or a title.
    /// </summary>
    /// <exception cref="InvalidOperationException">No installed presentation shows the message.</exception>
    /// <exception cref="InvalidDataException">A panel is shown with no button, or its glyph falls outside its words.</exception>
    public BackgroundTile[] Tiles(ShownMessage shown)
    {
        GameplayMessageId message = shown.Message;
        if (Notices is { } noticeArtwork && noticeArtwork.Contains(message))
        {
            BackgroundTile[] notice = noticeArtwork.Build(message);
            if (shown.Choice is { } choice)
                noticeArtwork.ApplySelection(message, notice, yesSelected: choice == SaveChoice.Yes);
            return notice;
        }
        if (Panels is { } panelArtwork && panelArtwork.Contains(message))
        {
            BackgroundTile[] panel = panelArtwork.Build(message);
            ControllerButton button = shown.Button
                ?? throw new InvalidDataException($"Gameplay-message panel {message} is shown with no button.");
            int byteOffset = GameplayMessageButtonGlyphs.SpecialGlyphByteOffset(message);
            if ((byteOffset & 1) != 0 || byteOffset + 1 >= panel.Length * sizeof(ushort))
                throw new InvalidDataException($"Message {message} configurable-button offset ${byteOffset:X4} is outside its tilemap.");
            panel[byteOffset / sizeof(ushort)] = GameplayMessageButtonGlyphs.ResolveGlyph(button);
            return panel;
        }
        if (Titles is { } titleArtwork && titleArtwork.Contains(message))
            return titleArtwork.Build(message);
        throw new InvalidOperationException($"Gameplay message {message} requires installed presentation assets.");
    }

    /// <summary>
    /// Every form the message box can show: each ordinary message, a panel once for each button its glyph can show and
    /// once for none, a save confirmation once for each choice.
    /// </summary>
    public static IEnumerable<ShownMessage> Forms()
    {
        foreach (GameplayMessageId message in OrdinaryMessages)
        {
            if (GameplayMessageButtons.PanelBinding(message) is not null)
            {
                foreach (ControllerButton button in GameplayMessageBoxDefinitions.Buttons.GlyphOrder)
                    yield return new ShownMessage(message, button, null);
                yield return new ShownMessage(message, ControllerButton.None, null);
            }
            else if (GameplayMessageNotices.IsSaveConfirmation(message))
            {
                yield return new ShownMessage(message, null, SaveChoice.Yes);
                yield return new ShownMessage(message, null, SaveChoice.No);
            }
            else
                yield return new ShownMessage(message, null, null);
        }
    }

    /// <summary>The messages the ordinary message coroutine shows.</summary>
    private static IEnumerable<GameplayMessageId> OrdinaryMessages =>
        MessageOrder;
}

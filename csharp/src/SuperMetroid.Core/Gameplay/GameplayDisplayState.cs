using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What the gameplay display currently shows: Samus's Mode 7 transforms, the displayed PPU, layer-3 FX and Morph
/// Ball eye beam snapshots, the HUD character base, and the default layer blending.
/// </summary>
internal sealed class GameplayDisplayState
{

    /// <summary>
    /// Native WRAM <c>$1982</c>. Room/HDMA setup owns this word; the current Landing Site
    /// shell starts at bank-$88's cleared-FX default two.
    /// </summary>
    public LayerBlendingConfiguration LayerBlendingDefaultConfig { get; set; } =
        LayerBlendingConfiguration.NormalGameplay;

    /// <summary>
    /// Ceres's current shadow Mode 7 matrix. The shaft room main publishes it after the
    /// drawing pass, and sprite producers use it only while calculating their temporary
    /// rendered origins. The translated consumer never moves physical coordinates.
    /// </summary>
    public SamusMode7Transform? ActiveSamusMode7Transform { get; set; }

    /// <summary>
    /// Everything the most recent accepted (or forced-blank) NMI made PPU-visible. A lag NMI
    /// leaves it unchanged. Door setup that writes registers before a room becomes visible
    /// updates it through <see cref="PublishDoorMode7"/>.
    /// </summary>
    public GameplayNmiDisplayLatch Latched { get; internal set; } = InitialLatch;

    /// <summary>
    /// BG3 character base selected by gameplay initialization or a room's library-
    /// background command list, as a VRAM word address. Kraid temporarily selects word $2000; ordinary rooms use
    /// word $4000. Setting a value outside VRAM throws.
    /// </summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    public int GameplayHudCharacterBaseWord
    {
        get;
        internal set => field = Hardware.SnesPpuLayout.RequireVramWord(value);
    } = GameplayVramLayout.HudCharacterWord;

    /// <summary>Moves the HUD's BG3 characters to Kraid's base, as Kraid's library-background transfer does.</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    internal void SelectKraidHudCharacterBase() =>
        GameplayHudCharacterBaseWord = Rooms.RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord;

    /// <summary>
    /// Door setup's direct write of the Mode 7 registers and BGMODE before the room is exposed
    /// (<c>$8F:E4E0</c>, or <c>$8F:E513</c> clearing them). Later room-main changes reach the
    /// display only through the NMI latch.
    /// </summary>
    internal void PublishDoorMode7(SamusMode7Transform? transform) =>
        Latched = Latched with
        {
            SamusMode7Transform = transform,
            Scene = Latched.Scene with { CeresElevatorMode7 = transform is not null },
        };

    /// <summary>The latch in effect before any NMI: power-on registers and no active effects.</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    private static GameplayNmiDisplayLatch InitialLatch => new(
        default, default, null, null, null, null, 0,
        new GameplayLatchedSceneRegisters(false, GameplayVramLayout.HudCharacterWord, default,
            LayerBlendingConfiguration.Unconfigured, null, null, 0),
        default, default, null, null);

    /// <summary>The BG3 character region at the base BG34NBA currently selects (Kraid's room moves it to word $2000).</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    internal VramRegion HudCharactersAtCurrentBase(SnesVram vram)
    {
        VramRegion ordinary = new GameplayVram(vram).HudCharacters;
        return new VramStore(vram).All.Slice(GameplayHudCharacterBaseWord, ordinary.WordCount);
    }
}

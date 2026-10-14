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
    /// Ceres Mode 7 matrix made PPU-visible by the most recent accepted NMI. This is the
    /// register-phase companion to <see cref="SuperMetroidRuntime.DisplayedOam"/>: software rendering must use
    /// both displayed values together rather than mixing current main-loop shadow words
    /// with the preceding NMI's sprites.
    /// </summary>
    public SamusMode7Transform? DisplayedSamusMode7Transform { get; internal set; }

    /// <summary>
    /// Ordinary BG scroll registers and room-shake deltas published by the most recent
    /// accepted NMI. These belong to the same PPU-visible frame as <see cref="SuperMetroidRuntime.DisplayedOam"/>.
    /// </summary>
    public GameplayPpuRenderSnapshot DisplayedGameplayPpu { get; internal set; }

    /// <summary>
    /// BG3 character base selected by gameplay initialization or a room's library-
    /// background command list. Kraid temporarily selects word $2000; ordinary rooms use
    /// word $4000.
    /// </summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    public ushort GameplayHudCharacterBaseWord { get; internal set; } =
        GameplayVramLayout.HudCharacterWord;

    /// <summary>Room-FX BG3 state published by the most recent accepted NMI.</summary>
    public RoomLayer3FxRenderSnapshot? DisplayedRoomLayer3Fx { get; internal set; }

    /// <summary>
    /// Morph-ball eye beam geometry published with <see cref="SuperMetroidRuntime.DisplayedOam"/>. A nullable
    /// record distinguishes a genuinely inactive beam from a malformed active slot.
    /// </summary>
    public MorphBallEyeBeamRenderSnapshot? DisplayedMorphBallEyeBeam { get; internal set; }

    /// <summary>The BG3 character region at the base BG34NBA currently selects (Kraid's room moves it to word $2000).</summary>
    [PpuLayoutBoundary("rendering view: the BG34NBA BG3 character-base register shadow")]
    internal VramRegion HudCharactersAtCurrentBase(SnesVram vram)
    {
        VramRegion ordinary = new GameplayVram(vram).HudCharacters;
        return new VramStore(vram).All.Slice(GameplayHudCharacterBaseWord, ordinary.WordCount);
    }
}

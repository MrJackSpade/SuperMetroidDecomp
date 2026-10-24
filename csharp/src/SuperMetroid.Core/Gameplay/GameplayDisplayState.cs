using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;

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
    /// The characters BG3 reads, selected by gameplay initialization or a room's library-background command list (#1406):
    /// Kraid's room selects the copy his list makes while his body covers the standard ones.
    /// </summary>
    public BgCharacterSet Layer3Characters { get; internal set; } = BgCharacterSet.Layer3;

    /// <summary>Selects the copy of the HUD's characters Kraid's library background makes, as his list's transfer does.</summary>
    internal void SelectKraidHudCharacterBase() => Layer3Characters = BgCharacterSet.KraidLayer3;

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
    private static GameplayNmiDisplayLatch InitialLatch => new(
        default, default, null, null, null, null, 0,
        new GameplayLatchedSceneRegisters(false, BgCharacterSet.Layer3, default,
            LayerBlendingConfiguration.Unconfigured, null, null, 0),
        default, default, null, null);
}

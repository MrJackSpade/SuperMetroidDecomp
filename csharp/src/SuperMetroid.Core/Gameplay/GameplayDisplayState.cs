using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What the gameplay display currently shows: the elevator shaft's transform, the latched scene, the HUD's characters
/// and the default layer blending.
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
    /// The scene the most recent accepted (or forced-blank) frame shows. A lag frame leaves it unchanged. Door setup
    /// that shows the elevator shaft's floor before the room becomes visible updates it through
    /// <see cref="PublishDoorElevatorFloor"/>.
    /// </summary>
    public GameplayDisplayLatch Latched { get; internal set; } = InitialLatch;

    /// <summary>
    /// The characters BG3 reads, selected by gameplay initialization or a room's library-background command list (#1406):
    /// Kraid's room selects the copy his list makes while his body covers the standard ones.
    /// </summary>
    public BgCharacterSet Layer3Characters { get; internal set; } = BgCharacterSet.Layer3;

    /// <summary>Selects the copy of the HUD's characters Kraid's library background makes, as his list's transfer does.</summary>
    internal void SelectKraidHudCharacterBase() => Layer3Characters = BgCharacterSet.KraidLayer3;

    /// <summary>
    /// Door setup shows the elevator shaft's floor before the room is exposed (<c>$8F:E4E0</c>), or removes it
    /// (<c>$8F:E513</c>), scrolled with the latched layer-1 scroll. A getaway floor already latched stays, as it takes
    /// precedence over the shaft's. Later room-main changes reach the display only through the frame latch.
    /// </summary>
    /// <param name="transform">The shaft's transform, or null to remove the floor.</param>
    internal void PublishDoorElevatorFloor(SamusMode7Transform? transform) =>
        Latched = Latched with
        {
            PerspectiveFloor = Latched.PerspectiveFloor is { ShowsLayer2Below: true } getaway ? getaway
                : transform is { } shaft ? CeresPerspectiveFloors.ElevatorShaft(shaft, Latched.Ppu.Bg1Scroll)
                : null,
        };

    /// <summary>The latch in effect before any accepted frame: nothing scrolled, no floor and no active effects.</summary>
    private static GameplayDisplayLatch InitialLatch => new(
        default, default, null, null, null, null, 0,
        new GameplayLatchedScene(BgCharacterSet.Layer3, default,
            LayerBlendingConfiguration.Unconfigured, null, null, 0),
        default, default, null, null);
}

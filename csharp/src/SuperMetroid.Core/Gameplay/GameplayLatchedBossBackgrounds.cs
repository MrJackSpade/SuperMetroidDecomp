using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The body a boss shows on its body layer in an accepted frame (#1352-#1356, #1466): the layer, where Kraid's, Draygon's,
/// Crocomire's and Mother Brain's are viewed from, and the pictures it shows. Enemy AI chooses each during the main loop.
/// </summary>
/// <param name="Body">The body layer a boss shows, or null without one.</param>
/// <param name="KraidBodyScroll">Where Kraid's body layer is viewed from, or null when Kraid does not own it.</param>
/// <param name="DraygonBody">Draygon's body position and where his body layer is viewed from, or null without Draygon (#1354).</param>
/// <param name="CrocomireBg2Scroll">Where Crocomire's body layer is viewed from, or null when no Crocomire owns it.</param>
/// <param name="MotherBrainBody">Where Mother Brain's body layer is viewed from and her death-flicker hiding, or null without either (#1356).</param>
/// <param name="ShownBody">
/// The body layer's parts as the frame shows them, each picture for the frame its animation had selected, resolved once the
/// frame's animations have bound; null without a body.
/// </param>
public readonly record struct GameplayLatchedBossBackgrounds(
    BossBodyLayer? Body,
    Position16<BackgroundPixels>? KraidBodyScroll,
    DraygonBodyPlacement? DraygonBody,
    Position16<BackgroundPixels>? CrocomireBg2Scroll,
    MotherBrainBodyPlacement? MotherBrainBody,
    ShownBossBody? ShownBody = null);

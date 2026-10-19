using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// Boss-owned BG register shadows an accepted NMI uploaded: Ceres Ridley's Mode 7 matrix
/// (M7A..M7Y, <c>$80:91EE</c>), Kraid's body on BG2 (#1352), Crocomire's BG2SC/BG2HOFS/BG2VOFS, and Mother
/// Brain's BG2 map size and TM BG2 bit. Each is written by enemy AI during the main loop.
/// </summary>
/// <param name="CeresRidleyMode7">Ridley getaway Mode 7 registers before room shake, or null when BGMODE is not Mode 7.</param>
/// <param name="KraidBody">Kraid's body frame on BG2 at his BG2 scroll, or null when Kraid does not own BG2.</param>
/// <param name="EnemyBg2">The enemy BG2 image a boss draws its body into, or null before a boss attaches it (#1353).</param>
/// <param name="DraygonBody">Draygon's body position and the BG2 scroll placing his body image, or null without Draygon (#1354).</param>
/// <param name="CrocomireBg2Scroll">Crocomire's BG2 scroll, or null when no Crocomire owns BG2.</param>
/// <param name="MotherBrainBg2ScrollOverride">Whether Mother Brain selected the 32x32 BG2 map.</param>
/// <param name="MotherBrainDeathBg2Hidden">Whether Mother Brain's death flicker removed BG2 from TM.</param>
public readonly record struct GameplayLatchedBossBackgrounds(
    CeresRidleyMode7Snapshot? CeresRidleyMode7,
    BossBg2Layer? KraidBody,
    EnemyBg2ImageSnapshot? EnemyBg2,
    DraygonBodyPlacement? DraygonBody,
    Position16<Bg2Pixels>? CrocomireBg2Scroll,
    bool MotherBrainBg2ScrollOverride,
    bool MotherBrainDeathBg2Hidden);

using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The bodies bosses draw as layer 2 in an accepted frame: Kraid's body (#1352), the enemy layer-2 image, Draygon's
/// and Mother Brain's placements, and Crocomire's scroll. Enemy AI chooses each during the main loop.
/// </summary>
/// <param name="KraidBody">Kraid's body frame on BG2 at his BG2 scroll, or null when Kraid does not own BG2.</param>
/// <param name="EnemyBg2">The enemy BG2 image a boss draws its body into, or null before a boss attaches it (#1353).</param>
/// <param name="DraygonBody">Draygon's body position and the BG2 scroll placing his body image, or null without Draygon (#1354).</param>
/// <param name="CrocomireBg2Scroll">Crocomire's BG2 scroll, or null when no Crocomire owns BG2.</param>
/// <param name="MotherBrainBody">Mother Brain's BG2 placement and death-flicker hiding, or null without either (#1356).</param>
/// <param name="BodyPictures">
/// The picture of each word of the body Kraid or the enemy BG2 image shows, resolved once the NMI's uploads have bound;
/// default without a body (#1392).
/// </param>
public readonly record struct GameplayLatchedBossBackgrounds(
    BossBg2Layer? KraidBody,
    EnemyBg2ImageSnapshot? EnemyBg2,
    DraygonBodyPlacement? DraygonBody,
    Position16<BackgroundPixels>? CrocomireBg2Scroll,
    MotherBrainBodyPlacement? MotherBrainBody,
    ImmutableArray<BgTilePicture?> BodyPictures = default);

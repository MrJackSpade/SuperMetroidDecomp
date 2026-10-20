using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>The Ceres encounter's status and the operations the runtime runs at its fixed phase points.</summary>
internal interface ICeresEncounter
{
    /// <summary>How far Ceres's escape has progressed.</summary>
    CeresEscapeStatus CeresStatus { get; set; }

    /// <summary>Whether a Ridley encounter is loaded.</summary>
    bool RidleyPresent { get; }

    /// <summary>Resolves Samus's projectiles against Ceres Ridley before EnemyMain; returns the hits.</summary>
    int ResolveCeresRidleyProjectileHits(SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles);

    /// <summary>Draws the Baby and door OBJ during EnemyMain, before the post-enemy hook is installed.</summary>
    void DrawCeresRidleyImmediateBabyAndDoor(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera);

    /// <summary>Runs Ceres Ridley's post-enemy draw hook.</summary>
    void DrawCeresRidleyPostEnemyHook(SpriteFrameBuffer sprites, SpriteArtwork objects, Position16<RoomPixels> camera);

    /// <summary>Runs the getaway room's main routine.</summary>
    void RunCeresRidleyGetawayRoomMain(SamusState? samus, int nmiFrameCounter);
}

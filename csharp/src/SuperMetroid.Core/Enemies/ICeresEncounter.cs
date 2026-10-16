using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies;

/// <summary>The Ceres encounter's status and the operations the runtime runs at its fixed phase points.</summary>
internal interface ICeresEncounter
{
    /// <summary>Native ceres_status ($093F).</summary>
    ushort CeresStatus { get; set; }

    /// <summary>Whether a Ridley encounter is loaded.</summary>
    bool RidleyPresent { get; }

    /// <summary>Resolves Samus's projectiles against Ceres Ridley before EnemyMain; returns the hits.</summary>
    int ResolveCeresRidleyProjectileHits(ISnesAddressSpace bus, SamusProjectileSystem projectiles, SamusBombProjectileSystem sharedProjectiles);

    /// <summary>Draws the Baby and door OBJ during EnemyMain, before the post-enemy hook is installed.</summary>
    void DrawCeresRidleyImmediateBabyAndDoor(OamBuffer oam, Position16<RoomPixels> camera);

    /// <summary>Runs Ceres Ridley's post-enemy draw hook.</summary>
    void DrawCeresRidleyPostEnemyHook(OamBuffer oam, Position16<RoomPixels> camera);

    /// <summary>Runs the getaway room's main routine.</summary>
    void RunCeresRidleyGetawayRoomMain(SamusState? samus, ushort nmiFrameCounter);
}

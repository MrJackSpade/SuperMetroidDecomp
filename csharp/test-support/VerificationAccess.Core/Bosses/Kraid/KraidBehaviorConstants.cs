using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.SpriteObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Cartridge values and helpers of <see cref="KraidBehavior"/> that only verification reads.</summary>
internal static class KraidBehaviorConstants
{
    /// <summary>Converts a recorded native mouth-shape address into wrapped world-space collision bounds.</summary>
    internal static KraidBehavior.KraidCollisionScratch LoadKraidCollisionScratch(RoomEnemySlot body, ushort pointer) =>
        KraidBehavior.LoadKraidCollisionScratch(body, KraidMouthHitboxDefinitionsConstants.ResolveCollision(pointer));
}

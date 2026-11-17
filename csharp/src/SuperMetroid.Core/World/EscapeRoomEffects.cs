using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.World;

/// <summary>
/// The escape rooms' mains' shared work (#2287): their explosions and the diagonal turns of their shaking. Each room's
/// setup starts its own quake (<see cref="RoomStates.RoomSetupBehavior"/>).
/// </summary>
internal sealed class EscapeRoomEffects
{
    /// <summary>Suppresses explosion generation while gameplay time is frozen.</summary>
    private readonly GameplayTimeFreeze _freeze;
    /// <summary>Paces the explosions on the escape's two- or four-update cadence.</summary>
    private readonly FrameClock _clock;
    /// <summary>Supplies the game's random sequence and Zebes timebomb event.</summary>
    private readonly SharedGameState _system;

    /// <summary>Binds the freeze, clock and system owners the escape rooms' mains read.</summary>
    internal EscapeRoomEffects(GameplayTimeFreeze freeze, FrameClock clock, SharedGameState system)
    {
        _freeze = freeze;
        _clock = clock;
        _system = system;
    }
    /// <summary>
    /// Runs one escape room's main at the post-draw boundary (#2287): its diagonal shaking where it has one, then its
    /// explosions at its site's cadence.
    /// </summary>
    /// <param name="room">The loaded room.</param>
    /// <param name="site">Where the room's explosions go off.</param>
    /// <param name="diagonal">The room's diagonal shaking and its countdown, or null for a room whose shaking never turns diagonal.</param>
    internal void StepEscapeRoom(RoomScope room, EscapeExplosionSite site, EscapeDiagonalQuake? diagonal)
    {
        RoomEnemySystem enemies = room.Enemies;
        bool outdoors = site == EscapeExplosionSite.Outdoors;
        if (diagonal is { Shaking: var shaking, Countdown: var countdown })
        {
            if (!countdown.Countdown.IsZero)
            {
                countdown.Countdown = countdown.Countdown.Decrement().Remaining;
                if (countdown.Countdown.IsZero)
                    enemies.Earthquake.ChangeShake(shaking.Horizontal);
            }
            else if (shaking.TurnsDiagonal.IsTakenBy(_system.NextRandom()))
            {
                countdown.Countdown = WrappingCountdown.Start(ZebesEscapeDefinitions.DiagonalFrames);
                enemies.Earthquake.ChangeShake(shaking.Diagonal);
            }
        }
        UpdateCadence explosionCadence = outdoors
            ? ZebesEscapeDefinitions.OutdoorExplosionEverySecondUpdate
            : ZebesEscapeDefinitions.IndoorExplosionEveryFourthUpdate;
        if (!_freeze.TimeIsFrozen && explosionCadence.IsDue(_clock.Counter))
        {
            int random = _system.NextRandom();
            Position16<RoomPixels> explosion = new(
                room.Camera.PixelPosition.X + ZebesEscapeDefinitions.ExplosionScreenX.Of(random),
                room.Camera.PixelPosition.Y + ZebesEscapeDefinitions.ExplosionScreenY.Of(random));
            Position16<RoomBlocks> block = explosion.ToRoomBlock();
            // Outdoors an explosion goes off only over scenery, and one whose random draw leaves its kind open takes the kind
            // its block's column phase gives it; indoors such an explosion is a small explosion.
            if (!outdoors || !room.LevelData.GetObjectCollisionBlock(block).Terrain.Appearance.ShowsNothing)
                enemies.RoomObjectSpawns.SpawnEscapeExplosion(explosion, _system.NextRandom(), outdoors
                    ? ZebesEscapeExplosions.OutdoorKindLeftOpen(block)
                    : ZebesEscapeExplosionDefinitions.IndoorKindLeftOpen);
        }
        if (outdoors) enemies.Earthquake.KeepShaking();
    }
}

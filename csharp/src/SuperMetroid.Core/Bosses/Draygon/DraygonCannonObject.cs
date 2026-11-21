using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One of Draygon's wall cannons (#1554). A whole cannon arms its missile wake and shows its shield on its first
/// update, then rests. From its second update on, a Missile or Super Missile that struck its block wakes it on the
/// next update, before its timing runs: the hit is counted at once, and a hit short of the third flashes the shield and
/// returns to rest, while the destroying hit (the third, or any Super Missile) stops the cannon's firing, writes its
/// destroyed terrain and starts its smoke. A cannon set up destroyed does that on its first update.
/// Other projectiles' strikes stay unread until a missile's replaces them.
/// </summary>
[RoomLifetime]
internal sealed class DraygonCannonObject : RoomObjectBehaviour, IShootableRoomObject
{
    /// <summary>Sets up a cannon facing <paramref name="orientation"/> that controls <paramref name="cannon"/>.</summary>
    /// <param name="cannons">The room's cannons, through which a destroyed cannon stops Draygon's firing of its wall cannon.</param>
    /// <param name="orientation">Which wall the cannon faces out from.</param>
    /// <param name="cannon">The firing cannon Draygon stops when this one is destroyed.</param>
    /// <param name="start">Whether the cannon starts whole or already destroyed.</param>
    internal DraygonCannonObject(DraygonCannonObjects cannons, DraygonCannonOrientation orientation, DraygonWallCannon cannon, DraygonCannonStart start)
    {
        _cannons = Ensure.NotNull(cannons);
        Cannon = cannon;
        _look = DraygonCannonAnimationDefinitions.Looks.TryGetValue(orientation, out DraygonCannonLook? look)
            ? look
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonOrientation)} {orientation}.");
        player = AnimationPlayer<DraygonCannonSegment, DraygonCannonDraw, NoAnimationCue>.Starting(
            DraygonCannonAnimationDefinitions.Segments, _look.Shield);
        Phase = start == DraygonCannonStart.Destroyed ? DraygonCannonPhase.StartingDestroyed : DraygonCannonPhase.StartingWhole;
    }

    /// <summary>The room's cannons, through which a destroyed cannon stops Draygon's firing of its wall cannon (#2400).</summary>
    private readonly DraygonCannonObjects _cannons;

    /// <summary>The firing cannon Draygon stops when this one is destroyed.</summary>
    private DraygonWallCannon Cannon { get; }

    /// <summary>What the cannon shows, by its facing.</summary>
    private readonly DraygonCannonLook _look;

    /// <summary>Where the cannon is in its course.</summary>
    private DraygonCannonPhase Phase { get; set; }

    /// <summary>
    /// The hits the cannon has survived, 0 to 2; a cannon set up destroyed is never hit. The hit that destroys a cannon, a
    /// third missile or any Super Missile, is not added.
    /// </summary>
    private int Hits { get; set; }

    /// <summary>Whether a missile strike wakes the cannon: while it shows its shield or its hit flash, from its first update until a hit destroys it (#2560).</summary>
    internal bool WakeArmed => Phase is DraygonCannonPhase.Shielded or DraygonCannonPhase.Flashing;

    /// <summary>The strike on the cannon's block it has yet to take (#2562).</summary>
    private readonly StrikeAwaitingRoomObject<ResidentObjectProjectileHit> _strike = new();

    /// <summary>
    /// The play of what the cannon shows (#2401): its shield, its hit flash or its sparks, each picture with the updates it
    /// has left. Nothing plays until the cannon's first update shows its shield or destroys it.
    /// </summary>
    private readonly AnimationPlayer<DraygonCannonSegment, DraygonCannonDraw, NoAnimationCue> player;

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => _strike.Land(hit);

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && _strike.Awaiting is { } hit && hit.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            // The struck cannon takes the hit on the update it wakes, before its timing runs; any other strike waits on.
            _ = _strike.Take();
            CountHit(place, context, destroysAtOnce: hit.Family == SamusProjectileFamily.SuperMissile);
            return;
        }
        switch (Phase)
        {
            case DraygonCannonPhase.StartingWhole:
                Show(place, context, _look.Shield, DraygonCannonPhase.Shielded);
                return;
            case DraygonCannonPhase.StartingDestroyed:
                Destroy(place, context);
                return;
            case DraygonCannonPhase.Shielded or DraygonCannonPhase.Smoking:
                // The shield shows its time and rests until a missile wakes the cannon; the sparks loop for good.
                DrawNewPicture(place, context);
                return;
            case DraygonCannonPhase.Flashing:
                // Once the flash has played, the shield shows again and the cannon rests on it.
                if (!DrawNewPicture(place, context) && player.HasStopped)
                    Show(place, context, _look.Shield, DraygonCannonPhase.Shielded);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DraygonCannonPhase)} {Phase}.");
        }
    }

    /// <summary>Plays the cannon's pictures one update on and draws the picture the update shows anew, if any.</summary>
    /// <returns>Whether the update showed a new picture.</returns>
    private bool DrawNewPicture(RoomObjectPlace place, RoomObjectStepContext context)
    {
        if (player.StepToNewFrame() is not { } shown)
            return false;
        context.Draw(place, DraygonCannonDraws.Family, shown);
        return true;
    }

    /// <summary>
    /// Counts a missile hit: short of the destroying count the shield flashes; the third hit, or one that
    /// <paramref name="destroysAtOnce"/>, destroys the cannon.
    /// </summary>
    private void CountHit(RoomObjectPlace place, RoomObjectStepContext context, bool destroysAtOnce)
    {
        if (!destroysAtOnce && Hits + 1 < DraygonCannonDefinitions.HitsToDestroy)
        {
            Hits++;
            Show(place, context, _look.HitFlash, DraygonCannonPhase.Flashing);
            return;
        }
        Destroy(place, context);
    }

    /// <summary>Destroys the cannon: Draygon stops its firing, its terrain is destroyed, and its sparks start.</summary>
    private void Destroy(RoomObjectPlace place, RoomObjectStepContext context)
    {
        _cannons.DisableCannon(Cannon);
        DraygonCannonObjects.WriteDestroyedTerrain(context.Level, place.Block);
        Show(place, context, _look.Sparks, DraygonCannonPhase.Smoking);
    }

    /// <summary>Starts <paramref name="animation"/> in <paramref name="phase"/> and draws its first picture at once.</summary>
    private void Show(RoomObjectPlace place, RoomObjectStepContext context, Animation<DraygonCannonSegment> animation, DraygonCannonPhase phase)
    {
        player.Show(animation);
        context.Draw(place, DraygonCannonDraws.Family, player.Showing);
        Phase = phase;
    }
}

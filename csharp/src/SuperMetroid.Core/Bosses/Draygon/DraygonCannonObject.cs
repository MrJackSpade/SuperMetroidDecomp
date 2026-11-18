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
        bool destroyed = start == DraygonCannonStart.Destroyed;
        Phase = destroyed ? DraygonCannonPhase.StartingDestroyed : DraygonCannonPhase.StartingWhole;
        // A cannon set up destroyed counts as having taken its destroying hits.
        Hits = destroyed ? DraygonCannonDefinitions.HitsToDestroy : 0;
    }

    /// <summary>The room's cannons, through which a destroyed cannon stops Draygon's firing of its wall cannon (#2400).</summary>
    private readonly DraygonCannonObjects _cannons;

    /// <summary>The firing cannon Draygon stops when this one is destroyed.</summary>
    internal DraygonWallCannon Cannon { get; }

    /// <summary>What the cannon shows, by its facing.</summary>
    private readonly DraygonCannonLook _look;

    /// <summary>Where the cannon is in its course.</summary>
    internal DraygonCannonPhase Phase { get; private set; }

    /// <summary>
    /// The hits the cannon has survived, 0 to 2; a cannon set up destroyed holds the destroying count. The hit that
    /// destroys a cannon, a third missile or any Super Missile, is not added.
    /// </summary>
    internal int Hits { get; private set; }

    /// <summary>Whether a missile strike wakes the cannon: from its first update until a hit destroys it.</summary>
    internal bool WakeArmed { get; private set; }

    /// <summary>The strike on the cannon's block its wake has not yet read, or null for none.</summary>
    internal ResidentObjectProjectileHit? PendingHit { get; private set; }

    /// <summary>Which image of its flash or smoke the cannon shows.</summary>
    private int _frame;

    /// <summary>The updates left before the cannon's next step.</summary>
    private int _updatesBeforeNextStep = 1;

    /// <inheritdoc/>
    public void Strike(ResidentObjectProjectileHit hit) => PendingHit = hit;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && PendingHit is { } hit && hit.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            // The struck cannon takes the hit on the update it wakes, before its timing runs.
            PendingHit = null;
            CountHit(place, context, destroysAtOnce: hit.Family == SamusProjectileFamily.SuperMissile);
            return;
        }
        if (--_updatesBeforeNextStep != 0)
            return;
        switch (Phase)
        {
            case DraygonCannonPhase.StartingWhole:
                WakeArmed = true;
                Show(place, context, _look.Shield, DraygonCannonPhase.Shielded);
                return;
            case DraygonCannonPhase.StartingDestroyed:
                Destroy(place, context);
                return;
            case DraygonCannonPhase.Shielded:
                // The cannon rests on its shield until a missile wakes it.
                _updatesBeforeNextStep = 1;
                return;
            case DraygonCannonPhase.Flashing:
                _frame++;
                if (_frame < _look.HitFlash.Count)
                    Show(place, context, _look.HitFlash[_frame], DraygonCannonPhase.Flashing);
                else
                    Show(place, context, _look.Shield, DraygonCannonPhase.Shielded);
                return;
            case DraygonCannonPhase.Smoking:
                _frame = (_frame + 1) % _look.Smoke.Count;
                Show(place, context, _look.Smoke[_frame], DraygonCannonPhase.Smoking);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DraygonCannonPhase)} {Phase}.");
        }
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
            _frame = 0;
            Show(place, context, _look.HitFlash[0], DraygonCannonPhase.Flashing);
            return;
        }
        WakeArmed = false;
        Destroy(place, context);
    }

    /// <summary>Destroys the cannon: Draygon stops its firing, its terrain is destroyed, and its smoke starts.</summary>
    private void Destroy(RoomObjectPlace place, RoomObjectStepContext context)
    {
        _cannons.DisableCannon(Cannon);
        DraygonCannonObjects.WriteDestroyedTerrain(context.Level, place.Block);
        _frame = 0;
        Show(place, context, _look.Smoke[0], DraygonCannonPhase.Smoking);
    }

    /// <summary>Draws <paramref name="frame"/> and holds it for its updates in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectPlace place, RoomObjectStepContext context, TerrainChangeFrame frame, DraygonCannonPhase phase)
    {
        context.Draw(place, frame.Draw);
        _updatesBeforeNextStep = frame.Updates;
        Phase = phase;
    }
}

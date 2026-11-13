using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// One of Draygon's wall cannons (#1554). A whole cannon arms its missile wake and shows its shield on its first
/// update, then rests. From its second update on, a Missile or Super Missile that struck its block wakes it on the
/// next update, before its timing runs: the hit is counted at once (a Super Missile counts as enough to destroy it),
/// and a hit short of the third flashes the shield and returns to rest, while the destroying hit stops the cannon's
/// firing, writes its destroyed terrain and starts its smoke. A cannon set up destroyed does that on its first update.
/// Other projectiles' strikes stay unread until a missile's replaces them.
/// </summary>
internal sealed class DraygonCannonObject : RoomObjectBehaviour, IShootableRoomObject
{
    /// <summary>Sets up a cannon facing <paramref name="orientation"/> that controls <paramref name="cannon"/>.</summary>
    /// <param name="orientation">Which wall the cannon faces out from.</param>
    /// <param name="cannon">The firing cannon Draygon stops when this one is destroyed.</param>
    /// <param name="start">Whether the cannon starts whole or already destroyed.</param>
    internal DraygonCannonObject(DraygonCannonOrientation orientation, DraygonWallCannon cannon, DraygonCannonStart start)
    {
        Cannon = cannon;
        _look = DraygonCannonAnimationDefinitions.Looks.TryGetValue(orientation, out DraygonCannonLook? look)
            ? look
            : throw new InvalidOperationException($"Undefined {nameof(DraygonCannonOrientation)} {orientation}.");
        bool destroyed = start == DraygonCannonStart.Destroyed;
        Phase = destroyed ? DraygonCannonPhase.StartingDestroyed : DraygonCannonPhase.StartingWhole;
        // A cannon set up destroyed counts as having taken its destroying hits.
        Hits = destroyed ? DraygonCannonDefinitions.HitsToDestroy : 0;
    }

    /// <summary>The firing cannon Draygon stops when this one is destroyed.</summary>
    internal DraygonWallCannon Cannon { get; }

    /// <summary>What the cannon shows, by its facing.</summary>
    private readonly DraygonCannonLook _look;

    /// <summary>Where the cannon is in its course.</summary>
    internal DraygonCannonPhase Phase { get; private set; }

    /// <summary>
    /// The hits counted so far: 0 to 2 while whole, the Super Missile's seed once one struck, or the destroying count for
    /// a cannon set up destroyed.
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
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (WakeArmed && PendingHit is { } hit && hit.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
        {
            // A Super Missile seeds the count far enough that the hit it wakes destroys the cannon.
            if (hit.Family == SamusProjectileFamily.SuperMissile)
                Hits = DraygonCannonDefinitions.SuperMissileHitCounterSeed;
            PendingHit = null;
            Phase = DraygonCannonPhase.Struck;
            _updatesBeforeNextStep = 1;
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
            case DraygonCannonPhase.Struck when Hits + 1 < DraygonCannonDefinitions.HitsToDestroy:
                Hits++;
                _frame = 0;
                Show(place, context, _look.HitFlash[0], DraygonCannonPhase.Flashing);
                return;
            case DraygonCannonPhase.Struck:
                WakeArmed = false;
                Destroy(place, context);
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

    /// <summary>Destroys the cannon: Draygon stops its firing, its terrain is destroyed, and its smoke starts.</summary>
    private void Destroy(RoomObjectSlot place, RoomObjectStepContext context)
    {
        context.RoomObjects.Bosses().DraygonCannons.DisableCannon(Cannon);
        DraygonCannonObjects.WriteDestroyedTerrain(context.Level, place.Block);
        _frame = 0;
        Show(place, context, _look.Smoke[0], DraygonCannonPhase.Smoking);
    }

    /// <summary>Draws <paramref name="frame"/> and holds it for its updates in <paramref name="phase"/>.</summary>
    private void Show(RoomObjectSlot place, RoomObjectStepContext context, TerrainChangeFrame frame, DraygonCannonPhase phase)
    {
        context.Draw(place, frame.Draw);
        _updatesBeforeNextStep = frame.Updates;
        Phase = phase;
    }
}

using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// A shot from one of the cannons in Draygon's walls. It gathers at the muzzle until the cue "fire" of its timeline,
/// and then flies the way it was aimed when it appeared.
/// </summary>
public partial class DraygonCannonShot : Projectile
{
    private PixelVelocity _aimed;
    private bool _flying;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        _aimed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        if (cue != "fire" || _flying)
            return;
        _flying = true;
        Velocity = _aimed;
    }
}

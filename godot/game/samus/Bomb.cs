using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of Samus's bombs: it lies where it was laid until its time runs out, and then bursts. Its settings do all of
/// that. What is its own is the bomb jump: a burst that catches the morph ball throws it.
/// </summary>
public partial class Bomb : Projectile
{
    /// <inheritdoc/>
    protected override void OnEnded(Room room)
    {
        if (Launcher is not Samus samus || Settings.EndingBurst.Length == 0)
            return;
        ProjectileSettings burst = ShotType.Read(room.Content.ReadData(Settings.EndingBurst)).Settings;
        var blast = PixelRect.Around(Position.X.Whole, Position.Y.Whole, burst.Width / 2, burst.Height / 2);
        if (blast.Overlaps(samus.BodyRect))
            samus.BombJump(Position);
    }
}

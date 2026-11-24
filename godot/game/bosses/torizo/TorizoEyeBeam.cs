using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// A beam from Golden Torizo's eyes: it flies straight and bursts on what it meets. Where one bursts on the floor,
/// flames follow if the Torizo is letting them burn when the cue "floor_flames" of its timeline comes.
/// </summary>
/// <remarks>Its timelines are "wall_impact", "floor_impact" and "floor_flames".</remarks>
public partial class TorizoEyeBeam : CuedProjectile
{
    private Torizo? _torizo;

    /// <summary>Tells it which Torizo fired it.</summary>
    public void FiredBy(Torizo torizo) => _torizo = torizo;

    /// <inheritdoc/>
    protected override void OnOwnCue(Room room, string[] cue)
    {
        if (cue[0] != "floor_flames")
        {
            base.OnOwnCue(room, cue);
            return;
        }

        if (_torizo is { } torizo && IsInstanceValid(torizo) && torizo.EyeBeamFlames)
            ShowTimeline("floor_flames");
    }

    /// <inheritdoc/>
    protected override void Move(Room room)
    {
        if (MoveAcrossMeetingTerrain(room, Speed.X))
        {
            Moving = false;
            ShowTimeline("wall_impact");
        }
        else if (MoveDownMeetingTerrain(room, Speed.Y))
        {
            Moving = false;
            Position = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole / 16 * 16 + 6);
            ShowTimeline("floor_impact");
        }
    }
}

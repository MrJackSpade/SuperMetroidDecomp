using Engine.Actors;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// The glow at Mother Brain's hand as she fires its beam, and each piece of the beam. Each "piece" cue of its
/// timeline has her lay down the next piece of the beam; "end" takes it away.
/// </summary>
public partial class MotherBrainHandBeam : Projectile
{
    private MotherBrain? _owner;

    /// <summary>Says whose hand it glows at.</summary>
    public void FiredBy(MotherBrain owner) => _owner = owner;

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "piece":
                if (_owner is { } owner && IsInstanceValid(owner) && !owner.IsDead)
                    owner.LayHandBeamPiece();
                break;
            case "end":
                Leave(room);
                break;
            default:
                throw new InvalidOperationException($"The glow at Mother Brain's hand does not know the cue \"{cue}\".");
        }
    }
}

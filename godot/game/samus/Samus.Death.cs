using Engine.Actors;
using Game.Data;

namespace Game;

/// <content>
/// Samus's part in her death, which <see cref="DeathMode"/> plays: the look she keeps until her death takes up its
/// own pose, what is settled of her after its last pass of play, and her death's colours.
/// </content>
public partial class Samus
{
    /// <summary>Whether she was in a spin jump as her energy ran out.</summary>
    private bool _diedSpinning;

    /// <summary>
    /// The look she shows while dying: the one she had as her energy ran out, whose pictures go on while play goes
    /// on beneath her death, until her death takes up its own pose.
    /// </summary>
    public string DeathLook { get; private set; } = "standing";

    /// <summary>She begins to die out of the state <paramref name="from"/>, whose look she keeps.</summary>
    protected override void OnDying(PlayerState from)
    {
        DeathLook = SamusLooks.Of(this, from);
        _diedSpinning = from is SpinJumpingState;
    }

    /// <summary>
    /// Her death's last pass of play is done: she holds her beam alone, a hurt's flicker ends, and a knockback that
    /// carried her through that pass carries her no further.
    /// </summary>
    public void SettleForDeath()
    {
        SelectBeam();
        BeInvincibleFor(0);
        EndThrow();
        (State as DyingState)?.EndMovement();
    }

    /// <summary>Her death takes up its pose, which she shows from now. A spin she died in has its sound stopped.</summary>
    public void BeginDeathPose()
    {
        DeathLook = "dying";
        if (_diedSpinning)
            Room.Session.Sounds.Request(Sounds.SpinJumpStop, channel: "samus");
    }

    /// <summary>Shows a step of her death's colours: her suit's, in the suit she wears, and those of her body without it.</summary>
    public void ShowDeathColours(int step)
    {
        Colours.ShowSuit(Colours.DeathSuitOf(SuitWorn, step));
        Colours.ShowSuitless(Colours.DeathSuitlessAt(step));
    }

    /// <summary>Shows a step of her death's whiteout in her beams' colours, which whiten with everything about her.</summary>
    public void ShowDeathWhiteout(int step) => Colours.ShowBeams(Colours.DeathWhiteoutAt(step));
}

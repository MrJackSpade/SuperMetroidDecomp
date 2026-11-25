using Engine.Actors;
using Engine.Content;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// What play needs to know of how far a spin's or a wall jump's pictures have got: the cartridge times her spin's
/// sound and her screw attack's glow by them. She counts it herself, once an update as her update ends, where her
/// visual steps the same pictures; the visual is never asked.
/// </summary>
public partial class Samus
{
    /// <summary>How many updates a wall jump's launch shows before its spin begins.</summary>
    [Export, DataValue("wall_jump_launch_updates")]
    public int WallJumpLaunchUpdates { get; set; } = 10;

    /// <summary>How many updates a spin jump shows its first picture for, before it turns.</summary>
    [Export, DataValue("spin_start_updates")]
    public int SpinStartUpdates { get; set; } = 4;

    /// <summary>Which spin a wall jump goes into once its launch has been shown.</summary>
    private enum WallJumpSpin
    {
        /// <summary>None yet: it is still launching.</summary>
        None,

        /// <summary>The plain spin.</summary>
        Plain,

        /// <summary>The space jump's.</summary>
        Space,

        /// <summary>The screw attack's.</summary>
        Screw,
    }

    /// <summary>The spin or wall jump state last counted, to tell a new one from the one going on.</summary>
    private PlayerState? _spinSeen;

    /// <summary>How many updates she has been in that state: none on the update it began.</summary>
    private int _spinUpdates;

    /// <summary>How many updates since her wall jump began, through the spin it goes into; below nothing with none under way.</summary>
    private int _wallJumpUpdates = -1;

    private WallJumpSpin _wallJumpSpin;

    /// <summary>
    /// The look of the wall jump she is in, the spin it has gone into once its launch has been shown; null with no
    /// wall jump under way.
    /// </summary>
    public string? WallJumpLook => _wallJumpUpdates < 0 ? null : _wallJumpSpin switch
    {
        WallJumpSpin.Screw => "wall_jump_screw",
        WallJumpSpin.Space => "wall_jump_space",
        WallJumpSpin.Plain => "wall_jump_spinning",
        _ => "wall_jumping",
    };

    /// <summary>
    /// The sound of the spin she is in, as the cartridge picks it to ask for again; null when she is in none. A spin
    /// jump's is its look's. A wall jump's is the plain spin's through its launch, and from then on that of the spin
    /// it went into.
    /// </summary>
    public string? SpinSound => State switch
    {
        WallJumpingState or SpinJumpingState { IsOfWallJump: true } when _wallJumpUpdates >= 0 => _wallJumpSpin switch
        {
            WallJumpSpin.Screw => Sounds.ScrewAttack,
            WallJumpSpin.Space => Sounds.SpaceJump,
            _ => Sounds.SpinJump,
        },
        SpinJumpingState spin => spin.ShownSound,
        _ => null,
    };

    /// <summary>
    /// Counts her spin on by one update, as her update ends. A wall jump's count runs on through the spin it lets go
    /// into. As its launch runs out the spin it goes into is chosen, once: the plain one with her head under a liquid
    /// and no gravity suit, else the screw attack's, else the space jump's, by what is switched on.
    /// </summary>
    private void CountSpin()
    {
        if (State is not (SpinJumpingState or WallJumpingState))
        {
            (_spinSeen, _spinUpdates, _wallJumpUpdates, _wallJumpSpin) = (null, 0, -1, WallJumpSpin.None);
            return;
        }

        if (ReferenceEquals(State, _spinSeen))
        {
            _spinUpdates++;
            if (_wallJumpUpdates >= 0)
                _wallJumpUpdates++;
        }
        else
        {
            bool goesOn = State is SpinJumpingState { IsOfWallJump: true } && _wallJumpUpdates >= 0;
            (_spinSeen, _spinUpdates) = (State, 0);
            if (State is WallJumpingState)
                (_wallJumpUpdates, _wallJumpSpin) = (0, WallJumpSpin.None);
            else if (goesOn)
                _wallJumpUpdates++;
            else
                (_wallJumpUpdates, _wallJumpSpin) = (-1, WallJumpSpin.None);
        }

        if (_wallJumpUpdates >= WallJumpLaunchUpdates && _wallJumpSpin == WallJumpSpin.None)
        {
            _wallJumpSpin = !Inventory.IsOn(Equipment.GravitySuit) && IsWhollyInLiquid ? WallJumpSpin.Plain
                : Inventory.IsOn(Equipment.ScrewAttack) ? WallJumpSpin.Screw
                : Inventory.IsOn(Equipment.SpaceJump) ? WallJumpSpin.Space
                : WallJumpSpin.Plain;
        }
    }
}

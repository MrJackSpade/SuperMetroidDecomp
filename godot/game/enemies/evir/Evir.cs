using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// An Evir: a seahorse that bobs up and down in the water, facing Samus, and throws the spine ball it holds at her
/// when she comes near. The ball is an enemy of its own, which grows back in the Evir's grip once it has flown far
/// out of view. The Evir's arms are a second visual.
/// </summary>
/// <remarks>
/// Its timelines are "body_left" and "body_right", and "arms_left" and "arms_right" for the visual named "Arms".
/// </remarks>
public partial class Evir : Enemy
{
    /// <summary>How far it bobs each update, as a number of pixels.</summary>
    [Export]
    public double BobSpeed { get; set; } = 0.5;

    /// <summary>How many updates it bobs one way before turning back.</summary>
    [Export]
    public int BobUpdates { get; set; } = 64;

    /// <summary>
    /// How many updates its first bob lasts. Below nothing, the plain half of <see cref="BobUpdates"/>; the cartridge
    /// reads a number that belongs to another of the room's enemies, which a room gives here.
    /// </summary>
    [Export]
    public int FirstBobUpdates { get; set; } = -1;

    /// <summary>The scene of its spine ball.</summary>
    [Export]
    public PackedScene? SpitScene { get; set; }

    /// <summary>Where its arms are when it faces right, across from its middle, in pixels.</summary>
    [Export, DataValue("arms_across")]
    public int ArmsAcross { get; set; } = 4;

    /// <summary>How far below its middle its arms are, in pixels.</summary>
    [Export, DataValue("arms_down")]
    public int ArmsDown { get; set; } = 10;

    /// <summary>Where it holds its spine ball when it faces right, across from its middle, in pixels.</summary>
    [Export, DataValue("grip_across")]
    public int GripAcross { get; set; } = 4;

    /// <summary>How far below its middle it holds its spine ball, in pixels.</summary>
    [Export, DataValue("grip_down")]
    public int GripDown { get; set; } = 18;

    private EvirSpit? _spit;
    private TimelinePlayer? _arms;
    private Countdown _bob;
    private bool _bobsDown;

    /// <summary>Where it holds its spine ball.</summary>
    public PixelPosition Grip =>
        PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole) + PixelOffset.FromWhole(Facing == Facing.Left ? -GripAcross : GripAcross, GripDown);

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        // Its arms stand just behind Samus, in front of its body.
        if (GetNodeOrNull<ActorVisual>("Arms") is { } arms)
            RoomDepth.Stand(arms, Standing.JustBehind);
        FaceSamus(restart: true);
        _bob = Countdown.Of((FirstBobUpdates >= 0 ? FirstBobUpdates : BobUpdates / 2) + 1);
        PackedScene scene = SpitScene ?? throw new InvalidOperationException($"The Evir \"{Name}\" names no scene for its spine ball.");
        _spit = scene.Instantiate<EvirSpit>();
        _spit.Holder = this;
        _spit.Position = Grip;
        if (!Room.Spawn(_spit))
            _spit = null;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // While its spine ball is growing back it keeps facing the way it was.
        if (_spit is not { IsGrowingBack: true })
            FaceSamus(restart: false);
        Pixels speed = Pixels.FromNumber(BobSpeed);
        Position += new PixelOffset(Pixels.Zero, _bobsDown ? speed : -speed);
        _bob = _bob.Stepped();
        if (_bob.IsOver)
        {
            _bob = Countdown.Of(BobUpdates + 1);
            _bobsDown = !_bobsDown;
        }

        StepArms();
    }

    /// <inheritdoc/>
    /// <remarks>Its arms move on while a hurt holds its body still: they are not hurt with it.</remarks>
    protected override void WhileHurt() => StepArms();

    /// <summary>Moves its arms on by one update, where they are for the way it faces.</summary>
    private void StepArms()
    {
        if (_arms is null || GetNodeOrNull<ActorVisual>("Arms") is not { } arms)
            return;
        _arms.Step();
        arms.Position = new Vector2(Facing == Facing.Left ? -ArmsAcross : ArmsAcross, ArmsDown);
        arms.Show(_arms.Shown);
    }

    /// <inheritdoc/>
    protected override void OnStatusBegan(string status)
    {
        // Frozen, its spine ball freezes with it, unless the ball is already on its way.
        if (status == Frozen && _spit is { IsDead: false, IsFlying: false })
            _spit.BeginStatus(Frozen, StatusUpdatesLeft);
    }

    /// <inheritdoc/>
    protected override void OnDied() => _spit?.GoWithHolder();

    /// <summary>Turns to face Samus, body and arms.</summary>
    private void FaceSamus(bool restart)
    {
        // By whole pixels: in its own pixel column Samus counts as on its right.
        Facing = Target.X.Whole < Position.X.Whole ? Facing.Left : Facing.Right;
        Play(Facing == Facing.Left ? "body_left" : "body_right", restart);
        FrameTimeline arms = Timelines.Get(Facing == Facing.Left ? "arms_left" : "arms_right");
        if (_arms is null || _arms.Timeline != arms)
            _arms = new TimelinePlayer(arms);
    }
}

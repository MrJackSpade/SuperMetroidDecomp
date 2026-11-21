using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// What Samus's collision with one door block does (#2545): she enters the door the block leads through, she stands at the
/// elevator's door and so arms the elevator, or she only brushes the elevator's door. The elevator's door leads to no room,
/// so it stops her as a solid block does whatever her pose; a door she enters lets her into the doorway.
/// </summary>
internal abstract partial record DoorContact
{
    /// <summary>Only this type's own cases exist.</summary>
    private DoorContact()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this door contact, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(DoorContactReactions<TContext> reactions, TContext context);


    /// <summary>Whether the block stops Samus as a solid block does: the elevator's door, which leads to no room.</summary>
    public abstract bool Blocks { get; }

    /// <summary>
    /// The contact Samus makes with a door block leading to <paramref name="exit"/> in <paramref name="pose"/>: a door she
    /// enters, or the elevator's door, which only a standing pose arms; running, aerial, morphed and damaged poses that
    /// meet it only brush it.
    /// </summary>
    /// <param name="exit">Where the block leads.</param>
    /// <param name="pose">Samus's pose as room collision reads it.</param>
    public static DoorContact Of(DoorBlockExit exit, SamusCollisionPose pose)
    {
        if (exit is DoorBlockExit.ThroughDoor { Door: var door })
            return new EntersDoor(door);
        return pose is SamusCollisionPose.StandingNormal or SamusCollisionPose.StandingOther
            ? StandsAtElevator.Instance
            : BrushesElevator.Instance;
    }

    /// <summary>Samus enters <paramref name="Door"/>, which the door transition then goes through.</summary>
    /// <param name="Door">The door the block leads through.</param>
    internal sealed partial record EntersDoor(DoorDefinition Door) : DoorContact
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DoorContactReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).EntersDoor(this, context);

        /// <inheritdoc/>
        public override bool Blocks => false;
    }

    /// <summary>Samus stands at the elevator's door, which arms the elevator.</summary>
    internal sealed partial record StandsAtElevator : DoorContact
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DoorContactReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).StandsAtElevator(this, context);

        /// <summary>The one stand at the elevator's door.</summary>
        internal static StandsAtElevator Instance { get; } = new();

        /// <inheritdoc/>
        public override bool Blocks => true;
    }

    /// <summary>Samus meets the elevator's door in a pose other than standing, which stops her and arms nothing.</summary>
    internal sealed partial record BrushesElevator : DoorContact
    {
        /// <inheritdoc/>
        internal override void React<TContext>(DoorContactReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).BrushesElevator(this, context);

        /// <summary>The one brush of the elevator's door.</summary>
        internal static BrushesElevator Instance { get; } = new();

        /// <inheritdoc/>
        public override bool Blocks => true;
    }
}

/// <summary>What the owner of a <see cref="DoorContact"/> does on each of its cases (#2553).</summary>
/// <param name="EntersDoor">Reacts to a <see cref="DoorContact.EntersDoor"/>.</param>
/// <param name="StandsAtElevator">Reacts to a <see cref="DoorContact.StandsAtElevator"/>.</param>
/// <param name="BrushesElevator">Reacts to a <see cref="DoorContact.BrushesElevator"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record DoorContactReactions<TContext>(Action<DoorContact.EntersDoor, TContext> EntersDoor, Action<DoorContact.StandsAtElevator, TContext> StandsAtElevator, Action<DoorContact.BrushesElevator, TContext> BrushesElevator);

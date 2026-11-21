namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// One reaction of one of the reaction families (#1722, #2571): what a reacting collision kind reads from its block's
/// behaviour, typed by its family. A reaction carries its family, its installed key and the behaviour that holds it, so
/// nothing recovers a family from an untyped enum. Two reactions are equal exactly when they are the same family's same
/// reaction. A reader of one family matches that family's reaction (<see cref="RoomCollisionBlock.Kind"/>), which a
/// block of the family always carries.
/// </summary>
public abstract record BlockReaction
{
    /// <summary>Only the families' reactions below derive from it.</summary>
    private protected BlockReaction()
    {
    }

    /// <summary>The family whose kinds read this reaction.</summary>
    public abstract BlockReactionFamily Family { get; }

    /// <summary>The reaction's installed key (#2520), as its family's written-out keys spell it.</summary>
    [InstalledKey]
    public abstract string Key { get; }

    /// <summary>The behaviour of a block whose family's reaction is this one.</summary>
    public abstract RoomBlockBehavior Behavior { get; }

    /// <summary>Names the reaction by its family and installed key.</summary>
    public sealed override string ToString() => $"{Family} reaction {Key}";

    /// <summary>A shootable block's reaction.</summary>
    /// <param name="reaction">The reaction.</param>
    public static BlockReaction Of(ShootableReaction reaction) => new ShootableBlockReaction(reaction);

    /// <summary>A bombable block's reaction.</summary>
    /// <param name="reaction">The reaction.</param>
    public static BlockReaction Of(BombableReaction reaction) => new BombableBlockReaction(reaction);

    /// <summary>A special block's reaction.</summary>
    /// <param name="reaction">The reaction.</param>
    public static BlockReaction Of(SpecialBlockReaction reaction) => new SpecialBlockBlockReaction(reaction);

    /// <summary>A special-air block's reaction.</summary>
    /// <param name="reaction">The reaction.</param>
    public static BlockReaction Of(SpecialAirReaction reaction) => new SpecialAirBlockReaction(reaction);

    /// <summary>A spike's reaction.</summary>
    /// <param name="reaction">The reaction.</param>
    public static BlockReaction Of(SpikeReaction reaction) => new SpikeBlockReaction(reaction);

    /// <summary>A grapple block's reaction.</summary>
    /// <param name="reaction">The reaction.</param>
    public static BlockReaction Of(GrappleReaction reaction) => new GrappleBlockReaction(reaction);

    /// <summary>A shootable block's or shootable air's reaction.</summary>
    /// <param name="Reaction">The reaction.</param>
    public sealed record ShootableBlockReaction(ShootableReaction Reaction) : BlockReaction
    {
        /// <inheritdoc/>
        public override BlockReactionFamily Family => BlockReactionFamily.Shootable;

        /// <inheritdoc/>
        [InstalledKey]
        public override string Key => ShootableReactionKeys.Installed.Of(Reaction);

        /// <inheritdoc/>
        public override RoomBlockBehavior Behavior => RoomBlockBehavior.Reacting(Reaction);
    }

    /// <summary>A bombable block's or bombable air's reaction.</summary>
    /// <param name="Reaction">The reaction.</param>
    public sealed record BombableBlockReaction(BombableReaction Reaction) : BlockReaction
    {
        /// <inheritdoc/>
        public override BlockReactionFamily Family => BlockReactionFamily.Bombable;

        /// <inheritdoc/>
        [InstalledKey]
        public override string Key => BombableReactionKeys.Installed.Of(Reaction);

        /// <inheritdoc/>
        public override RoomBlockBehavior Behavior => RoomBlockBehavior.Reacting(Reaction);
    }

    /// <summary>A special block's reaction.</summary>
    /// <param name="Reaction">The reaction.</param>
    public sealed record SpecialBlockBlockReaction(SpecialBlockReaction Reaction) : BlockReaction
    {
        /// <inheritdoc/>
        public override BlockReactionFamily Family => BlockReactionFamily.Special;

        /// <inheritdoc/>
        [InstalledKey]
        public override string Key => SpecialBlockReactionKeys.Installed.Of(Reaction);

        /// <inheritdoc/>
        public override RoomBlockBehavior Behavior => RoomBlockBehavior.Reacting(Reaction);
    }

    /// <summary>A special-air block's reaction.</summary>
    /// <param name="Reaction">The reaction.</param>
    public sealed record SpecialAirBlockReaction(SpecialAirReaction Reaction) : BlockReaction
    {
        /// <inheritdoc/>
        public override BlockReactionFamily Family => BlockReactionFamily.SpecialAir;

        /// <inheritdoc/>
        [InstalledKey]
        public override string Key => SpecialAirReactionKeys.Installed.Of(Reaction);

        /// <inheritdoc/>
        public override RoomBlockBehavior Behavior => RoomBlockBehavior.Reacting(Reaction);
    }

    /// <summary>A spike block's or spike air's reaction.</summary>
    /// <param name="Reaction">The reaction.</param>
    public sealed record SpikeBlockReaction(SpikeReaction Reaction) : BlockReaction
    {
        /// <inheritdoc/>
        public override BlockReactionFamily Family => BlockReactionFamily.Spike;

        /// <inheritdoc/>
        [InstalledKey]
        public override string Key => SpikeReactionKeys.Installed.Of(Reaction);

        /// <inheritdoc/>
        public override RoomBlockBehavior Behavior => RoomBlockBehavior.Reacting(Reaction);
    }

    /// <summary>A grapple block's reaction.</summary>
    /// <param name="Reaction">The reaction.</param>
    public sealed record GrappleBlockReaction(GrappleReaction Reaction) : BlockReaction
    {
        /// <inheritdoc/>
        public override BlockReactionFamily Family => BlockReactionFamily.Grapple;

        /// <inheritdoc/>
        [InstalledKey]
        public override string Key => GrappleReactionKeys.Installed.Of(Reaction);

        /// <inheritdoc/>
        public override RoomBlockBehavior Behavior => RoomBlockBehavior.Reacting(Reaction);
    }
}

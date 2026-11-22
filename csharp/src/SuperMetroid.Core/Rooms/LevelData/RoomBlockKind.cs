using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// How a block collides, with the reaction a reacting kind carries (#2554): each collision kind is one case, and the case of
/// a kind that reads a reaction holds the one its block's behaviour gives it. A reader of a block's kind and reaction
/// matches this closed set of cases exhaustively, so no collision kind is read apart from the reaction it carries and no
/// block of a reacting kind can be met without its reaction.
/// </summary>
public abstract partial record RoomBlockKind
{
    /// <summary>Only the kinds below derive from it.</summary>
    private protected RoomBlockKind()
    {
    }

    /// <summary>The collision kind this case is.</summary>
    public abstract RoomCollisionType Collision { get; }

    /// <summary>What <paramref name="reader"/> makes of a block of this kind: the reader's member for this case.</summary>
    /// <typeparam name="TAnswer">What the reader makes of a block.</typeparam>
    /// <param name="reader">The reader, which answers every kind a room's cell can be.</param>
    public abstract TAnswer Read<TAnswer>(IRoomBlockReader<TAnswer> reader);

    /// <summary>
    /// This kind as a whole block's, one that stands for itself; none for a cell continuing another block, which stands
    /// for the block its link leads to (<see cref="Compatibility.ContinuedBlocks"/>).
    /// </summary>
    internal abstract Whole? AsWholeBlock { get; }

    /// <summary>
    /// The kind of a whole block, one that stands for itself rather than continuing another (#2554): every kind but the two
    /// extensions. A read that has reached the block a cell stands for meets only these.
    /// </summary>
    public abstract partial record Whole : RoomBlockKind
    {
        /// <summary>Only the whole kinds below derive from it.</summary>
        private protected Whole()
        {
        }

        /// <summary>What <paramref name="reader"/> makes of a whole block of this kind: the reader's member for this case.</summary>
        /// <typeparam name="TAnswer">What the reader makes of a block.</typeparam>
        /// <param name="reader">The reader, which answers every kind a whole block can be.</param>
        public abstract TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader);

        /// <inheritdoc/>
        public sealed override TAnswer Read<TAnswer>(IRoomBlockReader<TAnswer> reader) => Read(reader);

        /// <inheritdoc/>
        internal sealed override Whole AsWholeBlock => this;
    }

    /// <summary>
    /// Whether <paramref name="collision"/> with <paramref name="behavior"/> makes a block, and then its case in
    /// <paramref name="kind"/>: a kind that reads a reaction makes one only when the behaviour gives it its reaction.
    /// </summary>
    /// <param name="collision">How the block collides.</param>
    /// <param name="behavior">The block's behaviour.</param>
    /// <param name="kind">The block's case, when it makes one.</param>
    /// <exception cref="InvalidOperationException"><paramref name="collision"/> is undefined.</exception>
    public static bool TryOf(RoomCollisionType collision, RoomBlockBehavior behavior, [NotNullWhen(true)] out RoomBlockKind? kind)
    {
        kind = collision switch
        {
            RoomCollisionType.Air => Air.Instance,
            RoomCollisionType.Slope => Slope.Instance,
            RoomCollisionType.HorizontalExtension => HorizontalExtension.Instance,
            RoomCollisionType.VerticalExtension => VerticalExtension.Instance,
            RoomCollisionType.UnrevealedAir => UnrevealedAir.Instance,
            RoomCollisionType.SolidBlock => SolidBlock.Instance,
            RoomCollisionType.DoorBlock => DoorBlock.Instance,
            RoomCollisionType.SpikeAir => behavior.Spike is { } spikeAir ? new SpikeAir(spikeAir) : null,
            RoomCollisionType.SpikeBlock => behavior.Spike is { } spikeBlock ? new SpikeBlock(spikeBlock) : null,
            RoomCollisionType.SpecialAir => behavior.SpecialAir is { } specialAir ? new SpecialAir(specialAir) : null,
            RoomCollisionType.SpecialBlock => behavior.Special is { } special ? new SpecialBlock(special) : null,
            RoomCollisionType.ShootableAir => behavior.Shootable is { } shootableAir ? new ShootableAir(shootableAir) : null,
            RoomCollisionType.ShootableBlock => behavior.Shootable is { } shootableBlock ? new ShootableBlock(shootableBlock) : null,
            RoomCollisionType.BombableAir => behavior.Bombable is { } bombableAir ? new BombableAir(bombableAir) : null,
            RoomCollisionType.BombableBlock => behavior.Bombable is { } bombableBlock ? new BombableBlock(bombableBlock) : null,
            RoomCollisionType.GrappleBlock => behavior.Grapple is { } grapple ? new GrappleBlock(grapple) : null,
            _ => throw new InvalidOperationException($"Undefined collision kind {collision}."),
        };
        return kind is not null;
    }

    /// <summary>Empty air.</summary>
    public sealed partial record Air : Whole
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private Air()
        {
        }

        /// <summary>The one empty-air case.</summary>
        public static Air Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.Air;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).Air();
    }

    /// <summary>A slope, whose geometry the block's behaviour selects.</summary>
    public sealed partial record Slope : Whole
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private Slope()
        {
        }

        /// <summary>The one slope case.</summary>
        public static Slope Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.Slope;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).Slope();
    }

    /// <summary>A horizontal extension, colliding as the block its behaviour's displacement names.</summary>
    public sealed partial record HorizontalExtension : RoomBlockKind
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private HorizontalExtension()
        {
        }

        /// <summary>The one horizontal-extension case.</summary>
        public static HorizontalExtension Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.HorizontalExtension;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IRoomBlockReader<TAnswer> reader) => Ensure.NotNull(reader).HorizontalExtension();

        /// <inheritdoc/>
        internal override Whole? AsWholeBlock => null;
    }

    /// <summary>A vertical extension, colliding as the block its behaviour's displacement names.</summary>
    public sealed partial record VerticalExtension : RoomBlockKind
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private VerticalExtension()
        {
        }

        /// <summary>The one vertical-extension case.</summary>
        public static VerticalExtension Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.VerticalExtension;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IRoomBlockReader<TAnswer> reader) => Ensure.NotNull(reader).VerticalExtension();

        /// <inheritdoc/>
        internal override Whole? AsWholeBlock => null;
    }

    /// <summary>Air that reacts to nothing and that the X-ray scope does not reveal.</summary>
    public sealed partial record UnrevealedAir : Whole
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private UnrevealedAir()
        {
        }

        /// <summary>The one unrevealed-air case.</summary>
        public static UnrevealedAir Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.UnrevealedAir;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).UnrevealedAir();
    }

    /// <summary>An ordinary solid block.</summary>
    public sealed partial record SolidBlock : Whole
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private SolidBlock()
        {
        }

        /// <summary>The one solid-block case.</summary>
        public static SolidBlock Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SolidBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).SolidBlock();
    }

    /// <summary>A door block.</summary>
    public sealed partial record DoorBlock : Whole
    {
        /// <summary>Only <see cref="Instance"/> is made.</summary>
        private DoorBlock()
        {
        }

        /// <summary>The one door-block case.</summary>
        public static DoorBlock Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.DoorBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).DoorBlock();
    }

    /// <summary>Spike air, with its spike reaction.</summary>
    /// <param name="Reaction">The spike reaction the block's behaviour gives it.</param>
    public sealed partial record SpikeAir(SpikeReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpikeAir;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).SpikeAir(Reaction);
    }

    /// <summary>A solid spike block, with its spike reaction.</summary>
    /// <param name="Reaction">The spike reaction the block's behaviour gives it.</param>
    public sealed partial record SpikeBlock(SpikeReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpikeBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).SpikeBlock(Reaction);
    }

    /// <summary>Special air, with its special-air reaction.</summary>
    /// <param name="Reaction">The special-air reaction the block's behaviour gives it.</param>
    public sealed partial record SpecialAir(SpecialAirReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpecialAir;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).SpecialAir(Reaction);
    }

    /// <summary>A solid special block, with its special-block reaction.</summary>
    /// <param name="Reaction">The special-block reaction the block's behaviour gives it.</param>
    public sealed partial record SpecialBlock(SpecialBlockReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpecialBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).SpecialBlock(Reaction);
    }

    /// <summary>Shootable air, with its shootable reaction.</summary>
    /// <param name="Reaction">The shootable reaction the block's behaviour gives it.</param>
    public sealed partial record ShootableAir(ShootableReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.ShootableAir;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).ShootableAir(Reaction);
    }

    /// <summary>A solid shootable block, with its shootable reaction.</summary>
    /// <param name="Reaction">The shootable reaction the block's behaviour gives it.</param>
    public sealed partial record ShootableBlock(ShootableReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.ShootableBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).ShootableBlock(Reaction);
    }

    /// <summary>Bombable air, with its bombable reaction.</summary>
    /// <param name="Reaction">The bombable reaction the block's behaviour gives it.</param>
    public sealed partial record BombableAir(BombableReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.BombableAir;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).BombableAir(Reaction);
    }

    /// <summary>A solid bombable block, with its bombable reaction.</summary>
    /// <param name="Reaction">The bombable reaction the block's behaviour gives it.</param>
    public sealed partial record BombableBlock(BombableReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.BombableBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).BombableBlock(Reaction);
    }

    /// <summary>A grapple block, with its grapple reaction.</summary>
    /// <param name="Reaction">The grapple reaction the block's behaviour gives it.</param>
    public sealed partial record GrappleBlock(GrappleReaction Reaction) : Whole
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.GrappleBlock;

        /// <inheritdoc/>
        public override TAnswer Read<TAnswer>(IWholeBlockReader<TAnswer> reader) => Ensure.NotNull(reader).GrappleBlock(Reaction);
    }
}

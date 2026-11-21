using System.Diagnostics.CodeAnalysis;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// How a block collides, with the reaction a reacting kind carries (#2554): each collision kind is one case, and the case of
/// a kind that reads a reaction holds the one its block's behaviour gives it. A reader of a block's kind and reaction
/// matches this closed set of cases exhaustively, so no collision kind is read apart from the reaction it carries and no
/// block of a reacting kind can be met without its reaction.
/// </summary>
public abstract record RoomBlockKind
{
    /// <summary>Only the kinds below derive from it.</summary>
    private protected RoomBlockKind()
    {
    }

    /// <summary>The collision kind this case is.</summary>
    public abstract RoomCollisionType Collision { get; }

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
    public sealed record Air : RoomBlockKind
    {
        private Air()
        {
        }

        /// <summary>The one empty-air case.</summary>
        public static Air Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.Air;
    }

    /// <summary>A slope, whose geometry the block's behaviour selects.</summary>
    public sealed record Slope : RoomBlockKind
    {
        private Slope()
        {
        }

        /// <summary>The one slope case.</summary>
        public static Slope Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.Slope;
    }

    /// <summary>A horizontal extension, colliding as the block its behaviour's displacement names.</summary>
    public sealed record HorizontalExtension : RoomBlockKind
    {
        private HorizontalExtension()
        {
        }

        /// <summary>The one horizontal-extension case.</summary>
        public static HorizontalExtension Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.HorizontalExtension;
    }

    /// <summary>A vertical extension, colliding as the block its behaviour's displacement names.</summary>
    public sealed record VerticalExtension : RoomBlockKind
    {
        private VerticalExtension()
        {
        }

        /// <summary>The one vertical-extension case.</summary>
        public static VerticalExtension Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.VerticalExtension;
    }

    /// <summary>Air that reacts to nothing and that the X-ray scope does not reveal.</summary>
    public sealed record UnrevealedAir : RoomBlockKind
    {
        private UnrevealedAir()
        {
        }

        /// <summary>The one unrevealed-air case.</summary>
        public static UnrevealedAir Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.UnrevealedAir;
    }

    /// <summary>An ordinary solid block.</summary>
    public sealed record SolidBlock : RoomBlockKind
    {
        private SolidBlock()
        {
        }

        /// <summary>The one solid-block case.</summary>
        public static SolidBlock Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SolidBlock;
    }

    /// <summary>A door block.</summary>
    public sealed record DoorBlock : RoomBlockKind
    {
        private DoorBlock()
        {
        }

        /// <summary>The one door-block case.</summary>
        public static DoorBlock Instance { get; } = new();

        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.DoorBlock;
    }

    /// <summary>Spike air, with its spike reaction.</summary>
    /// <param name="Reaction">The spike reaction the block's behaviour gives it.</param>
    public sealed record SpikeAir(SpikeReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpikeAir;
    }

    /// <summary>A solid spike block, with its spike reaction.</summary>
    /// <param name="Reaction">The spike reaction the block's behaviour gives it.</param>
    public sealed record SpikeBlock(SpikeReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpikeBlock;
    }

    /// <summary>Special air, with its special-air reaction.</summary>
    /// <param name="Reaction">The special-air reaction the block's behaviour gives it.</param>
    public sealed record SpecialAir(SpecialAirReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpecialAir;
    }

    /// <summary>A solid special block, with its special-block reaction.</summary>
    /// <param name="Reaction">The special-block reaction the block's behaviour gives it.</param>
    public sealed record SpecialBlock(SpecialBlockReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.SpecialBlock;
    }

    /// <summary>Shootable air, with its shootable reaction.</summary>
    /// <param name="Reaction">The shootable reaction the block's behaviour gives it.</param>
    public sealed record ShootableAir(ShootableReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.ShootableAir;
    }

    /// <summary>A solid shootable block, with its shootable reaction.</summary>
    /// <param name="Reaction">The shootable reaction the block's behaviour gives it.</param>
    public sealed record ShootableBlock(ShootableReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.ShootableBlock;
    }

    /// <summary>Bombable air, with its bombable reaction.</summary>
    /// <param name="Reaction">The bombable reaction the block's behaviour gives it.</param>
    public sealed record BombableAir(BombableReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.BombableAir;
    }

    /// <summary>A solid bombable block, with its bombable reaction.</summary>
    /// <param name="Reaction">The bombable reaction the block's behaviour gives it.</param>
    public sealed record BombableBlock(BombableReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.BombableBlock;
    }

    /// <summary>A grapple block, with its grapple reaction.</summary>
    /// <param name="Reaction">The grapple reaction the block's behaviour gives it.</param>
    public sealed record GrappleBlock(GrappleReaction Reaction) : RoomBlockKind
    {
        /// <inheritdoc/>
        public override RoomCollisionType Collision => RoomCollisionType.GrappleBlock;
    }
}

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's arm (<see cref="KraidArmEntity"/>) or foot (<see cref="KraidFootEntity"/>): both animate as his parts do, meet
/// Samus and her shots by their frames' hitboxes, and run their own shot and bomb reactions behind their ignoring
/// definitions.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the limb occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the limb, which it joins as it initializes.</param>
internal abstract partial class KraidLimbEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference)
    : KraidEntity(species, occupant, place, common, reference), IEnemyTypedAnimation, IPlacedEnemyPicture
{
    /// <summary>Where the limb's pose places its picture: the foot's step along its walk (#2210).</summary>
    public Offset16<RoomPixels> PicturePlacement => Pose?.Placement ?? Offset16<RoomPixels>.Zero;

    /// <summary>The depth the limb's pose draws it at: behind the room's front tiles while Kraid rises (#2210).</summary>
    public ActorDepth? PictureDepth => Pose?.Depth;

    /// <summary>The pose the limb's animation shows, or null before it shows one.</summary>
    private KraidPartPose? Pose => Part.Pose;

    /// <summary>The limb's own part of the encounter, its animation included (#2298).</summary>
    protected abstract KraidAnimatedPartState Part { get; }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Limbs.AdvanceKraidPartAnimation(Part);

    /// <summary>The limb's own shot reaction: an arm's or a foot's hitboxes decide it behind its definition's ignoring one.</summary>
    public override EnemyShotReaction ShotReaction => LimbShotReaction;

    /// <summary>The limb's own shot reaction, which its hitboxes decide behind its definition's ignoring one (#2301).</summary>
    protected abstract EnemyShotReaction LimbShotReaction { get; }

    /// <summary>Runs the limb's own reaction to every bomb that overlaps it, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        ReactToLimbBomb(context);
        return true;
    }

    /// <summary>The limb's own reaction to a bomb its hitboxes select (#2301).</summary>
    protected abstract void ReactToLimbBomb(EnemyBombContext context);

    /// <summary>The limb's contact and shots meet its frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;
}

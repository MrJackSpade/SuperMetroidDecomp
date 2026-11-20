using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>
/// How a title demonstration stages Samus once its room has loaded (#2500): the pose she starts in, which also places her
/// on the floor she stands on, the energy the low-health demonstration starts her with, or the shinespark she is already
/// launched into. <see cref="AttractDemoSamusSetups"/> holds the demonstrations' setups.
/// </summary>
public abstract record AttractDemoSamusSetup
{
    /// <summary>Closes the hierarchy to the setups declared here.</summary>
    private AttractDemoSamusSetup()
    {
    }

    /// <summary>Samus staged by this setup at the room position <paramref name="position"/>.</summary>
    public AttractDemoSamusStaging At(Position16<RoomPixels> position) => new(this, position);

    /// <summary>Stages <paramref name="samus"/>, whose inventory, energy and position the scene has already given her.</summary>
    internal abstract void Apply(SamusState samus);

    /// <summary>Samus faces forward, as she stands on arriving at the Landing Site.</summary>
    public sealed record ForwardFacing : AttractDemoSamusSetup
    {
        /// <inheritdoc />
        internal override void Apply(SamusState samus) => SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);
    }

    /// <summary>Samus starts in a pose, with less energy than her capacity when the setup names it.</summary>
    /// <param name="Pose">The pose Samus starts in, whose vertical radius also places her on a floor (<see cref="OnFloor"/>).</param>
    /// <param name="Energy">The energy Samus starts with in place of her capacity, or null for her full energy.</param>
    public sealed record Posed(SamusPoseId Pose, int? Energy = null) : AttractDemoSamusSetup
    {
        /// <summary>
        /// Samus staged in <see cref="Pose"/> at room X <paramref name="x"/>, resting on a flat floor <paramref name="floorRow"/>
        /// blocks down the room: the floor's top in pixels, less the pose's vertical radius.
        /// </summary>
        public AttractDemoSamusStaging OnFloor(int x, int floorRow) =>
            At(new(x, floorRow * RoomGridConversions.BlockPixels - SamusPoseMetadata.ReadPoseYRadius(Pose)));

        /// <inheritdoc />
        internal override void Apply(SamusState samus)
        {
            if (Energy is { } energy)
                samus.Health = energy;
            samus.Pose = Pose;
        }
    }

    /// <summary>Samus starts already launched into a shinespark.</summary>
    /// <param name="Launch">The shinespark launch she starts in.</param>
    public sealed record Shinespark(ShinesparkLaunch Launch) : AttractDemoSamusSetup
    {
        /// <inheritdoc />
        internal override void Apply(SamusState samus) => samus.Shinespark.BeginDemoLaunch(samus, Launch);
    }
}

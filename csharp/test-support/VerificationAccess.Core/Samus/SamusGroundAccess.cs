using System.Runtime.CompilerServices;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The ground a check stands Samus on outside a room (#2554): production hands the room's ground to her animation step and
/// landings, and a check that drives them on a lone Samus names the ground it gives her, dry with no dust until it names one.
/// </summary>
internal static class SamusGroundAccess
{
    /// <summary>The ground each check's Samus stands on, by Samus.</summary>
    private static readonly ConditionalWeakTable<SamusState, SamusGround> grounds = new();

    /// <summary>Dry ground whose landings raise no dust, which a Samus stands on until a check names another.</summary>
    private static SamusGround Unnamed { get; } = new(SamusGroundWetness.Dry, DryLandingsRaiseDust: false);

    /// <summary>The ground a check stands Samus on.</summary>
    /// <param name="samus">Samus.</param>
    extension(SamusState samus)
    {
        /// <summary>The ground the check stands her on.</summary>
        internal SamusGround Ground
        {
            get => grounds.TryGetValue(samus, out SamusGround? ground) ? ground : Unnamed;
            set => grounds.AddOrUpdate(samus, Ensure.NotNull(value));
        }
    }

    /// <summary>The liquid physics' steps on the ground the check stands Samus on.</summary>
    /// <param name="physics">Samus's liquid physics.</param>
    extension(SamusLiquidPhysicsState physics)
    {
        /// <summary>Prepares her animation frame on the ground the check stands her on.</summary>
        /// <param name="samus">Samus.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="frameCounter">The frame counter.</param>
        /// <param name="bossPresent">Whether a boss is present.</param>
        internal void PrepareAnimationFrame(SamusState samus, SharedRandom random, int frameCounter, bool bossPresent) =>
            physics.PrepareAnimationFrame(samus, samus.Ground, random, frameCounter, bossPresent);

        /// <summary>Handles her landing's sounds and graphics on the ground the check stands her on.</summary>
        /// <param name="samus">Samus.</param>
        /// <param name="previousMovementType">Her movement type before the landing.</param>
        /// <param name="previousPose">Her pose before the landing.</param>
        /// <param name="impactYSpeed">Her landing speed.</param>
        internal void HandleLandingSoundEffectsAndGraphics(SamusState samus, SamusMovementType previousMovementType, SamusPoseId previousPose,
            UnsignedSixteenSixteen impactYSpeed) =>
            physics.HandleLandingSoundEffectsAndGraphics(samus, samus.Ground, previousMovementType, previousPose, impactYSpeed);
    }
}

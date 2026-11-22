using F = SuperMetroid.Core.Frontend.Ending.EndingSpriteFamily;
using L = SuperMetroid.Core.Frontend.Ending.EndingSpriteLayer;
using R = SuperMetroid.Core.Frontend.Ending.EndingSpriteRole;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Each ending actor role's pictures, sprite layer and motion (#2489): the starfields lie behind every other sprite
/// (#1514), and each role names the motion its actors start with.
/// </summary>
internal static partial class EndingSpriteRoleDefinitions
{
    /// <summary>Every ending actor role's definition.</summary>
    internal static IdentityTable<EndingSpriteRole, EndingSpriteRoleDefinition> Roles { get; } = new(new Dictionary<EndingSpriteRole, EndingSpriteRoleDefinition>
        {
            [R.SideCloudRightLeading] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.SideCloud(HorizontalDirection.Left)),
            [R.SideCloudLeftLeading] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.SideCloud(HorizontalDirection.Right)),
            [R.SideCloudRightTrailing] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.SideCloud(HorizontalDirection.Left)),
            [R.SideCloudLeftTrailing] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.SideCloud(HorizontalDirection.Right)),
            [R.TopCloudPattern] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.TopOrBottomCloud(VerticalDirection.Down)),
            [R.TopCloudEdge] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.TopOrBottomCloud(VerticalDirection.Down)),
            [R.BottomCloudEdge] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.TopOrBottomCloud(VerticalDirection.Up)),
            [R.BottomCloudPattern] = new(F.EndingCloud, L.Actors, () => EndingCloudDrift.TopOrBottomCloud(VerticalDirection.Up)),
            [R.ExplodingZebes] = new(F.EndingExplosion, L.Actors, () => new EndingStillMotion()),
            [R.ExplosionLava] = new(F.EndingExplosion, L.Actors, () => new EndingExplosionLayerMotion()),
            [R.ExplosionGlow] = new(F.EndingExplosion, L.Actors, () => new EndingExplosionLayerMotion()),
            [R.ExplosionStars] = new(F.EndingExplosion, L.Stars, () => new EndingExplosionLayerMotion()),
            [R.ExplosionSilhouette] = new(F.EndingExplosion, L.Actors, () => new EndingStillMotion()),
            [R.ExplosionStarsRight] = new(F.EndingExplosion, L.Stars, EndingFlyawayStarsMotion.WaitingWhenCued),
            [R.ExplosionStarsLeft] = new(F.EndingExplosion, L.Stars, EndingFlyawayStarsMotion.WaitingFromSpawn),
            [R.ExplosionAfterglow] = new(F.EndingExplosion, L.Actors, () => new EndingFinaleExitMotion()),
            [R.OperationWasText] = new(F.EndingCompletionText, L.Actors, () => new EndingStillMotion()),
            [R.CompletedSuccessfullyText] = new(F.EndingCompletionText, L.Actors, () => new EndingStillMotion()),
            [R.ClearTimeText] = new(F.EndingCompletionText, L.Actors, () => new EndingStillMotion()),
            [R.ClearTimeDigit] = new(F.EndingCompletionText, L.Actors, () => new EndingStillMotion()),
            [R.RewardSamus] = new(F.EndingReward, L.Actors, () => new EndingStillMotion()),
            [R.AnimalEscape] = new(F.EndingAnimalEscape, L.Actors, () => new EndingAnimalPodMotion()),
        });
}

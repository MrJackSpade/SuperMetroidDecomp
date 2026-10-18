using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Enemies.Gunship.GunshipDustProgram;

namespace SuperMetroid.Core.Enemies.Gunship.Artwork;

/// <summary>Frame lists and counts of <see cref="GunshipDustVisualDefinitions"/> that only verification read.</summary>
internal static class GunshipDustVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="GunshipDustVisualDefinitions"/>.</summary>
    extension(GunshipDustVisualDefinitions)
    {
        /// <summary>The frames in installed order.</summary>
        internal static IReadOnlyList<GunshipDustFrame> Frames => [new(P.RightInnerLoop, 0), new(P.RightInnerLoop, 1), new(P.RightInnerLoop, 2), new(P.RightInnerLoop, 3), new(P.RightInnerLoop, 4), new(P.RightInnerLoop, 5), new(P.RightInnerLoop, 6), new(P.RightInnerLoop, 7), new(P.RightMiddleLoop, 0), new(P.RightMiddleLoop, 1), new(P.RightMiddleLoop, 2), new(P.RightMiddleLoop, 3), new(P.RightMiddleLoop, 4), new(P.RightMiddleLoop, 5), new(P.RightMiddleLoop, 6), new(P.RightMiddleLoop, 7), new(P.RightOuterLoop, 0), new(P.RightOuterLoop, 1), new(P.RightOuterLoop, 2), new(P.RightOuterLoop, 3), new(P.RightOuterLoop, 4), new(P.RightOuterLoop, 5), new(P.RightOuterLoop, 6), new(P.LeftInnerLoop, 0), new(P.LeftInnerLoop, 1), new(P.LeftInnerLoop, 2), new(P.LeftInnerLoop, 3), new(P.LeftInnerLoop, 4), new(P.LeftInnerLoop, 5), new(P.LeftInnerLoop, 6), new(P.LeftInnerLoop, 7), new(P.LeftMiddleLoop, 0), new(P.LeftMiddleLoop, 1), new(P.LeftMiddleLoop, 2), new(P.LeftMiddleLoop, 3), new(P.LeftMiddleLoop, 4), new(P.LeftMiddleLoop, 5), new(P.LeftMiddleLoop, 6), new(P.LeftMiddleLoop, 7), new(P.LeftOuterLoop, 0), new(P.LeftOuterLoop, 1), new(P.LeftOuterLoop, 2), new(P.LeftOuterLoop, 3), new(P.LeftOuterLoop, 4), new(P.LeftOuterLoop, 5), new(P.LeftOuterLoop, 6)];
    }
}

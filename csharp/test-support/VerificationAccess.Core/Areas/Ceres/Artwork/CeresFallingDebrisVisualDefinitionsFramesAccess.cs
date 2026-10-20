using P = SuperMetroid.Core.Areas.Ceres.CeresFallingDebrisProgram;

namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>Frame lists and counts of <see cref="CeresFallingDebrisVisualDefinitions"/> that only verification read.</summary>
internal static class CeresFallingDebrisVisualDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="CeresFallingDebrisVisualDefinitions"/>.</summary>
    extension(CeresFallingDebrisVisualDefinitions)
    {
        /// <summary>The frames in installed order.</summary>
        internal static IReadOnlyList<CeresFallingDebrisProgram> Frames => [P.Light, P.Dark];
    }
}

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>What each pose does with Samus's arm cannon.</summary>
internal static class SamusPoseAims
{
    /// <summary>The aim of <paramref name="pose"/>.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    internal static SamusPoseAim Read(SamusPoseId pose) => SamusPoseTraits.Of(pose).Aim;
}

using SuperMetroid.Core.Frontend.Intro;
using static SuperMetroid.Core.Frontend.Ending.EndingLogoDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Selects the ending logo's actors (<see cref="EndingLogoDefinitions"/>): the S halves and circle halves in spawn order, and
/// where each starts.
/// </summary>
internal static class EndingLogoActors
{
    /// <summary>The logo actor allocated at <paramref name="index"/>.</summary>
    private static EndingLogoActorKind KindAt(int index, string parameter) =>
        (uint)index < (uint)ActorKinds.Length ? ActorKinds[index] : throw new ArgumentOutOfRangeException(parameter);

    /// <summary>Named native actor cases; allocation index is not a sampled numeric curve.</summary>
    public static CinematicActorDefinition Actor(int index) => KindAt(index, nameof(index)) switch
    {
        EndingLogoActorKind.UpperS => UpperS,
        EndingLogoActorKind.LowerS => LowerS,
        EndingLogoActorKind.UpperCircle => UpperCircle,
        EndingLogoActorKind.LowerCircle => LowerCircle,
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
    /// <summary>F18F–F1D4 initialize these world-space centers before the $100 scroll subtraction.</summary>
    public static Position16<ScreenPixels> Origin(int actor) => KindAt(actor, nameof(actor)) switch
    {
        EndingLogoActorKind.UpperS => new(530, 231),
        EndingLogoActorKind.LowerS => new(246, 519),
        EndingLogoActorKind.UpperCircle => new(385, 366),
        EndingLogoActorKind.LowerCircle => new(391, 384),
        _ => throw new ArgumentOutOfRangeException(nameof(actor)),
    };
}

using System.Collections.Frozen;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The pose each ground turn turns from (#2553): every pose whose input list can turn Samus on the ground, bound once, as the
/// pose definitions load, to what it holds for the turn. A front view holds nothing; every other such pose aims, which the
/// binding checks, so the turn receives its aiming direction by type.
/// </summary>
internal static class SamusGroundTurnSources
{
    /// <summary>What each pose Samus can turn from on the ground holds for the turn.</summary>
    private static FrozenDictionary<SamusPoseId, SamusGroundTurnSource> Sources { get; } = Bind();

    /// <summary>What <paramref name="pose"/>, whose input list turned Samus on the ground, holds for the turn.</summary>
    internal static SamusGroundTurnSource Of(SamusPoseId pose) => Sources[pose];

    /// <summary>Binds every pose whose input list can turn Samus on the ground to its source.</summary>
    /// <exception cref="InvalidDataException">A pose whose list can turn her is neither a front view nor aiming.</exception>
    private static FrozenDictionary<SamusPoseId, SamusGroundTurnSource> Bind()
    {
        var sources = new Dictionary<SamusPoseId, SamusGroundTurnSource>();
        foreach ((SamusPoseId pose, SamusPoseDefinition definition) in SamusPoseDefinitions.Poses)
        {
            if (!definition.Inputs.TurnsOnGround)
                continue;
            sources[pose] = SamusPoseMetadata.IsForwardFacingPose(pose)
                ? SamusGroundTurnSource.FacingForward.Instance
                : definition.Aim is SamusPoseAim.Aiming { Direction: var direction }
                    ? new SamusGroundTurnSource.Aimed(direction)
                    : throw new InvalidDataException($"Pose {pose} can turn on the ground through {definition.Inputs} but aims nowhere ({definition.Aim}).");
        }
        return sources.ToFrozenDictionary();
    }
}

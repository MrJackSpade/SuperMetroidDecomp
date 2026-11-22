namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// What a pose falls back to when no input transition applies: another pose, or keeping the pose Samus is in.
/// </summary>
public readonly partial record struct SamusNoInputPose
{
    /// <summary>The pose fallen back to, or null where the pose is kept.</summary>
    private readonly SamusPoseId? _pose;

    /// <summary>A fallback to <paramref name="pose"/>, or keeping the current pose where it is null.</summary>
    private SamusNoInputPose(SamusPoseId? pose) => _pose = pose;

    /// <summary>The fallback that keeps the pose Samus is in.</summary>
    public static SamusNoInputPose KeepCurrentPose => default;

    /// <summary>The fallback to <paramref name="pose"/>.</summary>
    public static SamusNoInputPose ToPose(SamusPoseId pose) => new(pose);

    /// <summary>Whether the fallback keeps the pose Samus is in.</summary>
    public bool KeepsCurrentPose => _pose is null;

    /// <summary>The pose Samus falls back to from <paramref name="currentPose"/>.</summary>
    public SamusPoseId From(SamusPoseId currentPose) => _pose ?? currentPose;

    /// <summary>The pose fallen back to, when the fallback names one rather than keeping the current pose.</summary>
    public bool TryGetPose(out SamusPoseId pose)
    {
        pose = _pose.GetValueOrDefault();
        return _pose is not null;
    }
}

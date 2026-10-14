using SuperMetroid.Core.Areas.Tourian;

/// <summary>Tourian statue progress as a PLM fixture supplies it (#633).</summary>
internal sealed class FixtureTourianStatue(Func<bool> finished) : ITourianStatueProgress
{
    /// <summary>Reads fixture-owned Tourian statue completion on demand.</summary>
    public bool IsFinished => finished();
}

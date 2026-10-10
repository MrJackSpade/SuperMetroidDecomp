namespace SuperMetroid.Core.Game;

public sealed partial class SamusState
{
    /// <summary>Caches the health-warning state created on first access through <see cref="HealthWarning"/>.</summary>
    private SamusHealthWarningState? _healthWarning;

    /// <summary>Persistent $0A6A warning state, shared by native beta and reserve recovery.</summary>
    public SamusHealthWarningState HealthWarning => _healthWarning ??= new();
}

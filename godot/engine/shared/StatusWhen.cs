namespace Engine.Shared;

/// <summary>When a reaction puts its target into its status.</summary>
public enum StatusWhen
{
    /// <summary>Never: the reaction has no status.</summary>
    Never,

    /// <summary>On every such hit.</summary>
    Always,

    /// <summary>Only when the hit would otherwise kill: the target takes the status and lives.</summary>
    InsteadOfDying,
}

namespace SuperMetroid.Core.Frontend;

/// <summary>Reports whether a credits update copied a row and whether the source has ended.</summary>
internal readonly record struct CreditsObjectStepResult(
    bool CopiedRow,
    bool Finished);

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Reports whether a credits update copied a row and whether the source has ended.</summary>
[IndependentFlag("independent outcomes of one credits step: a row was copied, and the object finished")]
internal readonly record struct CreditsObjectStepResult(
    bool CopiedRow,
    bool Finished);

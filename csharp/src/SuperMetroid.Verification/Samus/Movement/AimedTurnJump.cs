using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The source, spin target and neutral target of one aimed turn jump.</summary>
/// <param name="Source">The source.</param>
/// <param name="SpinTarget">The spin target.</param>
/// <param name="NeutralTarget">The neutral target.</param>
internal readonly record struct AimedTurnJump(SamusPoseId Source, SamusPoseId SpinTarget, SamusPoseId NeutralTarget);

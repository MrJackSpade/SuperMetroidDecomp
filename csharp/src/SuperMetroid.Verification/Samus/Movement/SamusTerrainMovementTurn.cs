using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The source, target, definition and spin target of one samus terrain movement turn.</summary>
/// <param name="Source">The source.</param>
/// <param name="Target">The target.</param>
/// <param name="Definition">The definition.</param>
/// <param name="SpinTarget">The spin target.</param>
internal readonly record struct SamusTerrainMovementTurn(SamusPoseId Source, SamusPoseId Target, byte[] Definition, SamusPoseId SpinTarget);

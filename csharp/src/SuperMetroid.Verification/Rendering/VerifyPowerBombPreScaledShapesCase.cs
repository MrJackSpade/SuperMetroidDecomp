using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Rendering;

/// <summary>The stream and table of one verify power bomb pre scaled shapes case.</summary>
/// <param name="Stream">The stream.</param>
/// <param name="Table">The table.</param>
internal readonly record struct VerifyPowerBombPreScaledShapesCase(PowerBombShapeStream Stream, int Table);

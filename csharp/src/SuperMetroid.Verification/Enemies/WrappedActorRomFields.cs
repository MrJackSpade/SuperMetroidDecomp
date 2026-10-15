namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The retail ROM addresses of one wrapping Ceres actor's X, Y, attribute, horizontal-delta and
/// wrap-mask words (#633).
/// </summary>
[SuperMetroid.Core.Spatial.SpatialComponents("ROM addresses: the actor's X and Y words are separate retail table entries, not one stored position.")]
internal readonly record struct WrappedActorRomFields(int XWord, int YWord, int AttributeWord, int DeltaWord, int MaskWord);

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// The retail ROM addresses of one wrapping Ceres actor's X, Y, attribute, horizontal-delta and
/// wrap-mask words (#633).
/// </summary>
internal readonly record struct WrappedActorRomFields(int XWord, int YWord, int AttributeWord, int DeltaWord, int MaskWord);

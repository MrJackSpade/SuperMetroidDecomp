namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The native id and name of one map sprite role oracle result.</summary>
/// <param name="NativeId">The native id.</param>
/// <param name="Name">The name.</param>
internal readonly record struct MapSpriteRoleOracleResult(ushort NativeId, string Name);

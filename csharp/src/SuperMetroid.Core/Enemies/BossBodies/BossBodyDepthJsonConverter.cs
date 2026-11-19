namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>Reads and writes a <see cref="BossBodyDepth"/> in an installed document by its installed key (#2520).</summary>
internal sealed class BossBodyDepthJsonConverter() : InstalledKeyJsonConverter<BossBodyDepth>(BossBodyDepthKeys.Installed);

namespace SuperMetroid.Core.Assets;

/// <summary>Reads and writes a <see cref="ActorDepth"/> in an installed document by its installed key (#2520).</summary>
internal sealed partial class ActorDepthJsonConverter() : InstalledKeyJsonConverter<ActorDepth>(ActorDepthKeys.Installed);

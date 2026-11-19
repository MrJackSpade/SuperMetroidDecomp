namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Reads and writes a <see cref="CeresDestructionActorRole"/> in an installed document by its installed key (#2520).</summary>
internal sealed class CeresDestructionActorRoleJsonConverter() : InstalledKeyJsonConverter<CeresDestructionActorRole>(CeresDestructionActorRoleKeys.Installed);

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Reads and writes a <see cref="CeresRearViewActorRole"/> in an installed document by its installed key (#2520).</summary>
internal sealed class CeresRearViewActorRoleJsonConverter() : InstalledKeyJsonConverter<CeresRearViewActorRole>(CeresRearViewActorRoleKeys.Installed);

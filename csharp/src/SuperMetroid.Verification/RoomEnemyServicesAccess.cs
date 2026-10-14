using SuperMetroid.Core.Enemies;

/// <summary>Verification access to the services of a <see cref="RoomEnemySystem"/> (#633).</summary>
internal static class RoomEnemyServicesAccess
{
    /// <summary>Installs a verification service layer around the room's current enemy services while preserving the existing delegate.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>The case's own services, installed over whatever <c>Load</c> installed on first use.</summary>
        internal VerificationRoomEnemyServices VerificationServices()
        {
            // The installed services, if any; the case layers its own over them.
            IRoomEnemyServices? current = self.Room.installedServices;
            if (current is VerificationRoomEnemyServices installed)
                return installed;
            var services = new VerificationRoomEnemyServices(current);
            self.Room.Services = services;
            return services;
        }
    }
}

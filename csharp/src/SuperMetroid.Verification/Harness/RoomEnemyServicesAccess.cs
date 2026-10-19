using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Harness;

/// <summary>Verification access to the services of a <see cref="RoomEnemySystem"/> (#633).</summary>
internal static class RoomEnemyServicesAccess
{
    /// <summary>The verification service layer a case built its enemy system with (#633).</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>The case's own services, which the enemy system was built with.</summary>
        internal VerificationRoomEnemyServices VerificationServices() =>
            self.Room.InstalledServices as VerificationRoomEnemyServices ?? throw new InvalidOperationException(
                "A case changes its enemy services only when it builds the enemy system with VerificationRoomEnemyServices.");
    }
}

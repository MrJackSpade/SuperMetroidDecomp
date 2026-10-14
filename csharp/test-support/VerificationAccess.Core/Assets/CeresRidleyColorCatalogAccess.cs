using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

/// <summary>Verification access to <see cref="CeresRidleyColorCatalog"/> members production does not use.</summary>
internal static class CeresRidleyColorCatalogAccess
{
    /// <summary>Exposes phase-specific color definitions held privately by the installed catalog.</summary>
    extension(CeresRidleyColorCatalog self)
    {
        /// <summary>Reads an initial Ceres Ridley color before the fade phases.</summary>
        internal Bgr555 ResolveStart(int color) => self.start.Resolve(color);

        /// <summary>Reads one eye-fade row and color independently of body fade.</summary>
        internal Bgr555 ResolveEyeFade(int row, int color) => self.eyeFade.Resolve(row, color);

        /// <summary>Reads one body-fade row and color independently of eye fade.</summary>
        internal Bgr555 ResolveBodyFade(int row, int color) => self.bodyFade.Resolve(row, color);

        /// <summary>Reads a health-dependent paint row from the installed catalog.</summary>
        internal Bgr555 ResolveHealth(int row, int color) => self.health.Resolve(row, color);

        /// <summary>Reads the Ceres alarm-paint sequence at a selected row.</summary>
        internal Bgr555 ResolveAlarm(int row, int color) => self.alarm.Resolve(row, color);

        /// <summary>Reads the baby-Metroid paint sequence at a selected row.</summary>
        internal Bgr555 ResolveBaby(int row, int color) => self.baby.Resolve(row, color);
    }
}

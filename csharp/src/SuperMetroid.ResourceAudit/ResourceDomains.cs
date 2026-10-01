namespace SuperMetroid.ResourceAudit;

/// <summary>Resource identity spaces; bank-$86 operands are not bank-$8D OAM pointers.</summary>
internal static class ResourceDomains
{
    public const string EnemyDisplay = "enemy-display";
    public const string EnemySimple = "enemy-simple-oam";
    public const string EnemyExtended = "enemy-extended-display";
    public const string EnemyProjectileProgram = "enemy-projectile-program-frame";
    public const string EnemyProjectileSprite = "enemy-projectile-oam";
    public const string PaletteFx = "palette-fx-color";
    public const string SamusProjectile = "samus-projectile-oam";
    public const string Consumer = "resource-consumer";
    public const string CompiledSelector = "compiled-visual-selector";
}

/// <summary>Bank ownership of resource identities, derived from production catalogs.</summary>
internal static class ResourceBanks
{
    /// <summary>$86: instruction operands selecting enemy-projectile presentation frames.</summary>
    public const byte EnemyProjectilePrograms = (byte)(SuperMetroid.Core.Game.EnemyProjectileCodePointers.BankBase >> 16);
    /// <summary>$8D: enemy-projectile OAM compositions and palette-FX colors.</summary>
    public const byte ProjectileOamAndPaletteFx = (byte)(SuperMetroid.Core.Game.RoomFxRomData.Banks.PaletteFx >> 16);
    /// <summary>$93: Samus projectile animation compositions.</summary>
    public const byte SamusProjectiles = SuperMetroid.Core.Game.SamusProjectileRomData.Banks.ProjectileNumber;
}

/// <summary>Win32 SetErrorMode bits matching the repository console process policy.</summary>
internal static class ConsoleErrorPolicy
{
    /// <summary>SEM_FAILCRITICALERRORS: report critical errors through the console boundary.</summary>
    public const uint FailCriticalErrors = 0x0001;
    /// <summary>SEM_NOGPFAULTERRORBOX: no Windows fault-reporting dialog.</summary>
    public const uint NoFaultDialog = 0x0002;
    /// <summary>SEM_NOOPENFILEERRORBOX: file errors never prompt through legacy Windows UI.</summary>
    public const uint NoOpenFileDialog = 0x8000;
}

namespace SuperMetroid.ResourceAudit;

/// <summary>Resource identity spaces; bank-$86 operands are not bank-$8D OAM pointers.</summary>
internal static class ResourceDomains
{
    /// <summary>Identity space for ordinary enemy display compositions.</summary>
    public const string EnemyDisplay = "enemy-display";
    /// <summary>Identity space for simple enemy OAM spritemaps.</summary>
    public const string EnemySimple = "enemy-simple-oam";
    /// <summary>Identity space for extended enemy display compositions.</summary>
    public const string EnemyExtended = "enemy-extended-display";
    /// <summary>Identity space for enemy-projectile program frame operands in bank $86.</summary>
    public const string EnemyProjectileProgram = "enemy-projectile-program-frame";
    /// <summary>Identity space for enemy-projectile OAM spritemaps.</summary>
    public const string EnemyProjectileSprite = "enemy-projectile-oam";
    /// <summary>Identity space for palette-FX color operands.</summary>
    public const string PaletteFx = "palette-fx-color";
    /// <summary>Identity space for Samus projectile OAM spritemaps.</summary>
    public const string SamusProjectile = "samus-projectile-oam";
    /// <summary>Identity space for production readers of audited resources.</summary>
    public const string Consumer = "resource-consumer";
    /// <summary>Identity space for selectors whose visual choice is compiled into code.</summary>
    public const string CompiledSelector = "compiled-visual-selector";
    /// <summary>Identity space for Mother Brain room flash palette rows.</summary>
    public const string MotherBrainRoomFlash = "mother-brain-room-flash-row";
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

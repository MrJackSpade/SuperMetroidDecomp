namespace SuperMetroid.ResourceAudit.Reports;

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

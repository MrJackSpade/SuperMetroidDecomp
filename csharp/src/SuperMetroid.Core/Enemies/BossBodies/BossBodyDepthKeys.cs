namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The installed key of each <see cref="BossBodyDepth"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class BossBodyDepthKeys
{
    /// <summary>Each BossBodyDepth member's installed key.</summary>
    internal static InstalledKeys<BossBodyDepth> Installed { get; } = new(new Dictionary<BossBodyDepth, string>
    {
        [BossBodyDepth.BehindSprites] = "BehindSprites",
        [BossBodyDepth.InFrontOfSprites] = "InFrontOfSprites",
    });
}

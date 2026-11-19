namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The installed key of each <see cref="ShootableReaction"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ShootableReactionKeys
{
    /// <summary>Each ShootableReaction member's installed key.</summary>
    internal static InstalledKeys<ShootableReaction> Installed { get; } = new(new Dictionary<ShootableReaction, string>
    {
        [ShootableReaction.RespawningBreak1x1] = "RespawningBreak1x1",
        [ShootableReaction.RespawningBreak2x1] = "RespawningBreak2x1",
        [ShootableReaction.RespawningBreak1x2] = "RespawningBreak1x2",
        [ShootableReaction.RespawningBreak2x2] = "RespawningBreak2x2",
        [ShootableReaction.PermanentBreak1x1] = "PermanentBreak1x1",
        [ShootableReaction.PermanentBreak2x1] = "PermanentBreak2x1",
        [ShootableReaction.PermanentBreak1x2] = "PermanentBreak1x2",
        [ShootableReaction.PermanentBreak2x2] = "PermanentBreak2x2",
        [ShootableReaction.RespawningPowerBombBlock] = "RespawningPowerBombBlock",
        [ShootableReaction.PermanentPowerBombBlock] = "PermanentPowerBombBlock",
        [ShootableReaction.RespawningSuperMissileBlock] = "RespawningSuperMissileBlock",
        [ShootableReaction.PermanentSuperMissileBlock] = "PermanentSuperMissileBlock",
        [ShootableReaction.Nothing] = "Nothing",
        [ShootableReaction.CollisionProbe] = "CollisionProbe",
        [ShootableReaction.IgnoresShots] = "IgnoresShots",
        [ShootableReaction.BlueDoorCapLeft] = "BlueDoorCapLeft",
        [ShootableReaction.BlueDoorCapRight] = "BlueDoorCapRight",
        [ShootableReaction.BlueDoorCapUp] = "BlueDoorCapUp",
        [ShootableReaction.BlueDoorCapDown] = "BlueDoorCapDown",
        [ShootableReaction.ResidentObjectTrigger] = "ResidentObjectTrigger",
        [ShootableReaction.CollectibleTrigger] = "CollectibleTrigger",
        [ShootableReaction.GateTriggerBlueLeft] = "GateTriggerBlueLeft",
        [ShootableReaction.GateTriggerBlueRight] = "GateTriggerBlueRight",
        [ShootableReaction.GateTriggerRedLeft] = "GateTriggerRedLeft",
        [ShootableReaction.GateTriggerRedRight] = "GateTriggerRedRight",
        [ShootableReaction.GateTriggerGreenLeft] = "GateTriggerGreenLeft",
        [ShootableReaction.GateTriggerGreenRight] = "GateTriggerGreenRight",
        [ShootableReaction.GateTriggerYellowLeft] = "GateTriggerYellowLeft",
        [ShootableReaction.GateTriggerYellowRight] = "GateTriggerYellowRight",
        [ShootableReaction.AnimalEscape] = "AnimalEscape",
        [ShootableReaction.NothingWhenSolid] = "NothingWhenSolid",
    });
}

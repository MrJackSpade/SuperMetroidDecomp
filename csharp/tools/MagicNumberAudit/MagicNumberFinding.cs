namespace SuperMetroid.SourceAudit;

/// <summary>One classified source occurrence.</summary>
internal sealed record MagicNumberFinding(
    string RelativePath,
    int LineNumber,
    string Literal,
    MagicNumberCategory Category,
    string Source)
{

    /// <summary>Actionable source location and expected named owner for this raw value.</summary>
    public string Diagnostic =>
        $"{RelativePath}:{LineNumber}: raw {Literal} [{Category}]. " +
        $"Expected {ExpectedContainer(Category)}. {Remediation(Category)} Source: {Source}";

    /// <summary>Names the catalog or type expected to own values in a classified domain.</summary>
    private static string ExpectedContainer(MagicNumberCategory category) => category switch
    {
        MagicNumberCategory.RomAddress => "a named ROM address/range catalog",
        MagicNumberCategory.CallbackPointer => "a named callback/code-pointer catalog",
        MagicNumberCategory.DomainDiscriminator => "a domain enum or named value",
        MagicNumberCategory.CollisionType => "RoomCollisionType",
        MagicNumberCategory.BlockBehavior => "RoomBlockBehavior/RoomBlockBehaviorValues",
        MagicNumberCategory.PoseOrMovement => "a Samus pose or movement enum/catalog",
        MagicNumberCategory.ProjectileFamily => "a typed projectile family/kind",
        MagicNumberCategory.AudioId => "SoundEffectId or a named music command",
        MagicNumberCategory.ControllerBits => "ControllerButtons/typed controller input",
        MagicNumberCategory.PackedPpuWord => "a packed SNES word wrapper",
        MagicNumberCategory.SramOffset => "SaveRamLayout or another SRAM layout container",
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Undefined magic-number category."),
    };

    /// <summary>Explains the named-owner and reviewed intrinsic-data paths for one category.</summary>
    private static string Remediation(MagicNumberCategory category) =>
        $"Move the value to the {ExpectedContainer(category)} and reference that symbol; " +
        $"if it is intrinsic algorithm data, add a reviewed same-line allow({category}) reason.";
}

namespace SuperMetroid.SourceAudit;

/// <summary>Domains whose raw hexadecimal values require a named owner or reviewed exemption.</summary>
internal enum MagicNumberCategory
{
    /// <summary>A cartridge bus address that belongs in an address or range catalog.</summary>
    RomAddress,
    /// <summary>A native callback or pre-instruction pointer embedded in functional code.</summary>
    CallbackPointer,
    /// <summary>A switch-arm value selecting a domain state without a named member.</summary>
    DomainDiscriminator,
    /// <summary>A room collision type that should use the collision domain.</summary>
    CollisionType,
    /// <summary>A block BTS behavior byte that should use a named room behavior value.</summary>
    BlockBehavior,
    /// <summary>A Samus pose or movement dispatch value outside its typed catalog.</summary>
    PoseOrMovement,
    /// <summary>A projectile family or kind selected by a raw value.</summary>
    ProjectileFamily,
    /// <summary>A sound or music command identifier used without its audio catalog.</summary>
    AudioId,
    /// <summary>A controller bit mask embedded in an input expression.</summary>
    ControllerBits,
    /// <summary>A bit-packed PPU or level word manipulated without a named wrapper.</summary>
    PackedPpuWord,
    /// <summary>An SRAM offset used outside its saved-memory layout owner.</summary>
    SramOffset,
}

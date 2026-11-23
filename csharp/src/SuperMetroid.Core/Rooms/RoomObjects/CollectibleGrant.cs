using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What collecting a permanent item gives Samus (#2481): an energy or reserve tank's energy, an ammunition tank's capacity
/// and ammunition, an item of equipment, a suit, or a beam with the beam it switches off.
/// </summary>
internal abstract partial record CollectibleGrant
{
    /// <summary>Only the grants below derive from this.</summary>
    private protected CollectibleGrant()
    {
    }

    /// <summary>An Energy Tank: <paramref name="energy"/> more maximum health, and her health filled to it.</summary>
    internal static CollectibleGrant EnergyTank(int energy) => new EnergyTankGrant(energy);

    /// <summary>
    /// A Reserve Tank: <paramref name="energy"/> more reserve capacity, and the automatic reserve mode where she had no
    /// mode yet.
    /// </summary>
    internal static CollectibleGrant ReserveTank(int energy) => new ReserveTankGrant(energy);

    /// <summary>A Missile tank: <paramref name="missiles"/> more missiles, both her maximum and what she carries.</summary>
    internal static CollectibleGrant MissileTank(int missiles) => new MissileTankGrant(missiles);

    /// <summary>A Super Missile tank: <paramref name="superMissiles"/> more Super Missiles, both her maximum and what she carries.</summary>
    internal static CollectibleGrant SuperMissileTank(int superMissiles) => new SuperMissileTankGrant(superMissiles);

    /// <summary>A Power Bomb tank: <paramref name="powerBombs"/> more Power Bombs, both her maximum and what she carries.</summary>
    internal static CollectibleGrant PowerBombTank(int powerBombs) => new PowerBombTankGrant(powerBombs);

    /// <summary>An item of equipment, <paramref name="item"/>, collected and equipped.</summary>
    internal static CollectibleGrant Equipment(SamusEquipmentFlags item) => new EquipmentGrant(item);

    /// <summary>
    /// A suit, <paramref name="suit"/>, collected and equipped; the beam's charge is cleared as it is equipped, before its
    /// message, so no charge is held through the fanfare or the suit's reveal that follows it.
    /// </summary>
    internal static CollectibleGrant Suit(SamusEquipmentFlags suit) => new SuitGrant(suit);

    /// <summary>
    /// A beam, <paramref name="beam"/>, collected and switched on, and <paramref name="switchesOff"/> switched off: Spazer and
    /// Plasma are the one pair that cannot be on together.
    /// </summary>
    internal static CollectibleGrant Beam(SamusBeamFlags beam, SamusBeamFlags switchesOff) => new BeamGrant(beam, switchesOff);

    /// <summary>Gives the grant to <paramref name="samus"/>.</summary>
    internal abstract void Apply(IRoomSamus samus);

    /// <summary>An Energy Tank's energy.</summary>
    /// <param name="Energy">The maximum health it adds.</param>
    private sealed partial record EnergyTankGrant(int Energy) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.MaxHealth += Energy;
            samus.Health = samus.MaxHealth;
        }
    }

    /// <summary>A Reserve Tank's energy.</summary>
    /// <param name="Energy">The reserve capacity it adds.</param>
    private sealed partial record ReserveTankGrant(int Energy) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.MaxReserveEnergy += Energy;
            if (samus.ReserveTankMode == ReserveTankMode.None)
                samus.ReserveTankMode = ReserveTankMode.Auto;
        }
    }

    /// <summary>A Missile tank's missiles.</summary>
    /// <param name="Missiles">The missiles it adds.</param>
    private sealed partial record MissileTankGrant(int Missiles) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.MaxMissiles += Missiles;
            samus.Missiles += Missiles;
        }
    }

    /// <summary>A Super Missile tank's Super Missiles.</summary>
    /// <param name="SuperMissiles">The Super Missiles it adds.</param>
    private sealed partial record SuperMissileTankGrant(int SuperMissiles) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.MaxSuperMissiles += SuperMissiles;
            samus.SuperMissiles += SuperMissiles;
        }
    }

    /// <summary>A Power Bomb tank's Power Bombs.</summary>
    /// <param name="PowerBombs">The Power Bombs it adds.</param>
    private sealed partial record PowerBombTankGrant(int PowerBombs) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.MaxPowerBombs += PowerBombs;
            samus.PowerBombs += PowerBombs;
        }
    }

    /// <summary>An item of equipment.</summary>
    /// <param name="Item">The item collected and equipped.</param>
    private sealed partial record EquipmentGrant(SamusEquipmentFlags Item) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.EquippedItems |= Item;
            samus.CollectedItems |= Item;
        }
    }

    /// <summary>A suit, which clears the beam's charge as it is equipped.</summary>
    /// <param name="Equipped">The suit collected and equipped.</param>
    private sealed partial record SuitGrant(SamusEquipmentFlags Equipped) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.EquippedItems |= Equipped;
            samus.CollectedItems |= Equipped;
            samus.ClearBeamCharge();
        }
    }

    /// <summary>A beam, and the beam it switches off.</summary>
    /// <param name="Collected">The beam collected and switched on.</param>
    /// <param name="SwitchesOff">The beam switched off as it switches on: Plasma for Spazer, Spazer for Plasma, none for any other.</param>
    private sealed partial record BeamGrant(SamusBeamFlags Collected, SamusBeamFlags SwitchesOff) : CollectibleGrant
    {
        /// <inheritdoc/>
        internal override void Apply(IRoomSamus samus)
        {
            Ensure.NotNull(samus);
            samus.CollectedBeams |= Collected;
            samus.EquippedBeams |= Collected;
            samus.EquippedBeams &= ~SwitchesOff;
        }
    }
}

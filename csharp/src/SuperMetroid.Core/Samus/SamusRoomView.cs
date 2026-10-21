using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>Samus as room objects read and change her (#1395); every member reads or writes <paramref name="samus"/>.</summary>
[ArchitectureReview("back-reference", "A stateless view of its owner: Samus holds the view she presents to room objects, and every member reads or writes her.")]
internal sealed class SamusRoomView(SamusState samus) : IRoomSamus
{
    /// <inheritdoc/>
    public FixedPosition<RoomPixels> Position { get => samus.Position; set => samus.Position = value; }

    /// <inheritdoc/>
    public Extent16<RoomPixels> Radius => samus.Kinematics.Radius;

    /// <inheritdoc/>
    public int Health { get => samus.Health; set => samus.Health = value; }

    /// <inheritdoc/>
    public int MaxHealth { get => samus.MaxHealth; set => samus.MaxHealth = value; }

    /// <inheritdoc/>
    public int MaxReserveEnergy { get => samus.MaxReserveEnergy; set => samus.MaxReserveEnergy = value; }

    /// <inheritdoc/>
    public ReserveTankMode ReserveTankMode { get => samus.ReserveTankMode; set => samus.ReserveTankMode = value; }

    /// <inheritdoc/>
    public int Missiles { get => samus.Missiles; set => samus.Missiles = value; }

    /// <inheritdoc/>
    public int MaxMissiles { get => samus.MaxMissiles; set => samus.MaxMissiles = value; }

    /// <inheritdoc/>
    public int SuperMissiles { get => samus.SuperMissiles; set => samus.SuperMissiles = value; }

    /// <inheritdoc/>
    public int MaxSuperMissiles { get => samus.MaxSuperMissiles; set => samus.MaxSuperMissiles = value; }

    /// <inheritdoc/>
    public int PowerBombs { get => samus.PowerBombs; set => samus.PowerBombs = value; }

    /// <inheritdoc/>
    public int MaxPowerBombs { get => samus.MaxPowerBombs; set => samus.MaxPowerBombs = value; }

    /// <inheritdoc/>
    public SamusBeamFlags EquippedBeams { get => samus.EquippedBeams; set => samus.EquippedBeams = value; }

    /// <inheritdoc/>
    public SamusBeamFlags CollectedBeams { get => samus.CollectedBeams; set => samus.CollectedBeams = value; }

    /// <inheritdoc/>
    public SamusEquipmentFlags EquippedItems { get => samus.EquippedItems; set => samus.EquippedItems = value; }

    /// <inheritdoc/>
    public SamusEquipmentFlags CollectedItems { get => samus.CollectedItems; set => samus.CollectedItems = value; }

    /// <inheritdoc/>
    public int ProjectileFlareCounter { get => samus.ProjectileFlareCounter; set => samus.ProjectileFlareCounter = value; }

    /// <inheritdoc/>
    public NativeCountdown InvincibilityTimer { get => samus.InvincibilityTimer; set => samus.InvincibilityTimer = value; }

    /// <inheritdoc/>
    public IRoomSamusControl Control => samus.Control;

    /// <inheritdoc/>
    public bool IsShinesparking =>
        samus.Pose is >= SamusPoseId.ShinesparkHorizontalRightPose and <= SamusPoseId.ShinesparkDiagonalLeftPose;

    /// <inheritdoc/>
    public bool IsActivelySpeedBoosting => samus.HorizontalSpeed.IsActivelySpeedBoosting;

    /// <inheritdoc/>
    public void FaceForward() => SamusPoseMetadata.ApplyForwardFacingPoseSetup(samus);

    /// <inheritdoc/>
    public void TakePeriodicDamage(ushort damage) => samus.LiquidPhysics.AccumulatePeriodicDamage(0, damage);
}

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// One picture of Crocomire's body animations (#1747): a fight-body or melting pose, or a pose of his skeleton, named for
/// the installed frame <see cref="Artwork.CrocomireBodyFrames"/> shows for it. A fight-body pose also shows how far his
/// mouth is open, which picks his Power Bomb reaction; a skeleton pose shows none.
/// </summary>
public readonly record struct CrocomireBodyFrame
{
    /// <summary>Names the frame and its mouth.</summary>
    /// <param name="name">The frame's name.</param>
    /// <param name="mouth">How far the frame's mouth is open; null for a skeleton frame.</param>
    private CrocomireBodyFrame(string name, CrocomireMouth? mouth)
    {
        Name = name;
        Mouth = mouth;
    }

    /// <summary>The frame's name.</summary>
    internal string Name { get; }

    /// <summary>
    /// How far the fight body's mouth is open in this frame, which the game read from the parts of the frame's picture;
    /// editable sprite art cannot change it. Null for a skeleton frame, which never takes a Power Bomb reaction.
    /// </summary>
    internal CrocomireMouth? Mouth { get; }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>Charging first.</summary>
    internal static CrocomireBodyFrame ChargingFirst { get; } = new(nameof(ChargingFirst), CrocomireMouth.Closed);

    /// <summary>Charging second.</summary>
    internal static CrocomireBodyFrame ChargingSecond { get; } = new(nameof(ChargingSecond), CrocomireMouth.Closed);

    /// <summary>Charging third.</summary>
    internal static CrocomireBodyFrame ChargingThird { get; } = new(nameof(ChargingThird), CrocomireMouth.Closed);

    /// <summary>Charging fourth.</summary>
    internal static CrocomireBodyFrame ChargingFourth { get; } = new(nameof(ChargingFourth), CrocomireMouth.Closed);

    /// <summary>Charging fifth.</summary>
    internal static CrocomireBodyFrame ChargingFifth { get; } = new(nameof(ChargingFifth), CrocomireMouth.Closed);

    /// <summary>Charging sixth.</summary>
    internal static CrocomireBodyFrame ChargingSixth { get; } = new(nameof(ChargingSixth), CrocomireMouth.Closed);

    /// <summary>Charging seventh.</summary>
    internal static CrocomireBodyFrame ChargingSeventh { get; } = new(nameof(ChargingSeventh), CrocomireMouth.Closed);

    /// <summary>Charging eighth.</summary>
    internal static CrocomireBodyFrame ChargingEighth { get; } = new(nameof(ChargingEighth), CrocomireMouth.Closed);

    /// <summary>Charging ninth.</summary>
    internal static CrocomireBodyFrame ChargingNinth { get; } = new(nameof(ChargingNinth), CrocomireMouth.Closed);

    /// <summary>Charging tenth.</summary>
    internal static CrocomireBodyFrame ChargingTenth { get; } = new(nameof(ChargingTenth), CrocomireMouth.Closed);

    /// <summary>Charging eleventh.</summary>
    internal static CrocomireBodyFrame ChargingEleventh { get; } = new(nameof(ChargingEleventh), CrocomireMouth.Closed);

    /// <summary>Charging twelfth.</summary>
    internal static CrocomireBodyFrame ChargingTwelfth { get; } = new(nameof(ChargingTwelfth), CrocomireMouth.Closed);

    /// <summary>Initial.</summary>
    internal static CrocomireBodyFrame Initial { get; } = new(nameof(Initial), CrocomireMouth.Closed);

    /// <summary>Wait for first second damage sixth.</summary>
    internal static CrocomireBodyFrame WaitForFirstSecondDamageSixth { get; } = new(nameof(WaitForFirstSecondDamageSixth), CrocomireMouth.Closed);

    /// <summary>Wait for first second damage fifth.</summary>
    internal static CrocomireBodyFrame WaitForFirstSecondDamageFifth { get; } = new(nameof(WaitForFirstSecondDamageFifth), CrocomireMouth.Closed);

    /// <summary>Wait for first second damage fourth.</summary>
    internal static CrocomireBodyFrame WaitForFirstSecondDamageFourth { get; } = new(nameof(WaitForFirstSecondDamageFourth), CrocomireMouth.Closed);

    /// <summary>Wait for first second damage third.</summary>
    internal static CrocomireBodyFrame WaitForFirstSecondDamageThird { get; } = new(nameof(WaitForFirstSecondDamageThird), CrocomireMouth.Closed);

    /// <summary>Wait for first second damage second.</summary>
    internal static CrocomireBodyFrame WaitForFirstSecondDamageSecond { get; } = new(nameof(WaitForFirstSecondDamageSecond), CrocomireMouth.Closed);

    /// <summary>Wait for first second damage first.</summary>
    internal static CrocomireBodyFrame WaitForFirstSecondDamageFirst { get; } = new(nameof(WaitForFirstSecondDamageFirst), CrocomireMouth.Closed);

    /// <summary>Projectile attack first.</summary>
    internal static CrocomireBodyFrame ProjectileAttackFirst { get; } = new(nameof(ProjectileAttackFirst), CrocomireMouth.Closed);

    /// <summary>Projectile attack second.</summary>
    internal static CrocomireBodyFrame ProjectileAttackSecond { get; } = new(nameof(ProjectileAttackSecond), CrocomireMouth.Closed);

    /// <summary>Projectile attack third.</summary>
    internal static CrocomireBodyFrame ProjectileAttackThird { get; } = new(nameof(ProjectileAttackThird), CrocomireMouth.Closed);

    /// <summary>Projectile attack fourth.</summary>
    internal static CrocomireBodyFrame ProjectileAttackFourth { get; } = new(nameof(ProjectileAttackFourth), CrocomireMouth.Closed);

    /// <summary>Projectile attack fifth.</summary>
    internal static CrocomireBodyFrame ProjectileAttackFifth { get; } = new(nameof(ProjectileAttackFifth), CrocomireMouth.Closed);

    /// <summary>Projectile attack sixth.</summary>
    internal static CrocomireBodyFrame ProjectileAttackSixth { get; } = new(nameof(ProjectileAttackSixth), CrocomireMouth.Closed);

    /// <summary>Projectile attack seventh.</summary>
    internal static CrocomireBodyFrame ProjectileAttackSeventh { get; } = new(nameof(ProjectileAttackSeventh), CrocomireMouth.PartlyOpen);

    /// <summary>Projectile attack eighth.</summary>
    internal static CrocomireBodyFrame ProjectileAttackEighth { get; } = new(nameof(ProjectileAttackEighth), CrocomireMouth.FullyOpen);

    /// <summary>Step forward after delay.</summary>
    internal static CrocomireBodyFrame StepForwardAfterDelay { get; } = new(nameof(StepForwardAfterDelay), CrocomireMouth.Closed);

    /// <summary>Stepping forward first.</summary>
    internal static CrocomireBodyFrame SteppingForwardFirst { get; } = new(nameof(SteppingForwardFirst), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward second.</summary>
    internal static CrocomireBodyFrame SteppingForwardSecond { get; } = new(nameof(SteppingForwardSecond), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward third.</summary>
    internal static CrocomireBodyFrame SteppingForwardThird { get; } = new(nameof(SteppingForwardThird), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward fourth.</summary>
    internal static CrocomireBodyFrame SteppingForwardFourth { get; } = new(nameof(SteppingForwardFourth), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward fifth.</summary>
    internal static CrocomireBodyFrame SteppingForwardFifth { get; } = new(nameof(SteppingForwardFifth), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward sixth.</summary>
    internal static CrocomireBodyFrame SteppingForwardSixth { get; } = new(nameof(SteppingForwardSixth), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward seventh.</summary>
    internal static CrocomireBodyFrame SteppingForwardSeventh { get; } = new(nameof(SteppingForwardSeventh), CrocomireMouth.FullyOpen);

    /// <summary>Stepping forward eighth.</summary>
    internal static CrocomireBodyFrame SteppingForwardEighth { get; } = new(nameof(SteppingForwardEighth), CrocomireMouth.PartlyOpen);

    /// <summary>Stepping forward ninth.</summary>
    internal static CrocomireBodyFrame SteppingForwardNinth { get; } = new(nameof(SteppingForwardNinth), CrocomireMouth.Closed);

    /// <summary>Projectile attack loop first.</summary>
    internal static CrocomireBodyFrame ProjectileAttackLoopFirst { get; } = new(nameof(ProjectileAttackLoopFirst), CrocomireMouth.FullyOpen);

    /// <summary>Projectile attack loop second.</summary>
    internal static CrocomireBodyFrame ProjectileAttackLoopSecond { get; } = new(nameof(ProjectileAttackLoopSecond), CrocomireMouth.FullyOpen);

    /// <summary>Projectile attack loop third.</summary>
    internal static CrocomireBodyFrame ProjectileAttackLoopThird { get; } = new(nameof(ProjectileAttackLoopThird), CrocomireMouth.FullyOpen);

    /// <summary>Projectile attack loop fourth.</summary>
    internal static CrocomireBodyFrame ProjectileAttackLoopFourth { get; } = new(nameof(ProjectileAttackLoopFourth), CrocomireMouth.FullyOpen);

    /// <summary>Projectile attack loop fifth.</summary>
    internal static CrocomireBodyFrame ProjectileAttackLoopFifth { get; } = new(nameof(ProjectileAttackLoopFifth), CrocomireMouth.FullyOpen);

    /// <summary>Melting one top row.</summary>
    internal static CrocomireBodyFrame MeltingOneTopRow { get; } = new(nameof(MeltingOneTopRow), CrocomireMouth.Closed);

    /// <summary>Melting one top two rows.</summary>
    internal static CrocomireBodyFrame MeltingOneTopTwoRows { get; } = new(nameof(MeltingOneTopTwoRows), CrocomireMouth.Closed);

    /// <summary>Melting one top three rows.</summary>
    internal static CrocomireBodyFrame MeltingOneTopThreeRows { get; } = new(nameof(MeltingOneTopThreeRows), CrocomireMouth.Closed);

    /// <summary>Melting one top four rows.</summary>
    internal static CrocomireBodyFrame MeltingOneTopFourRows { get; } = new(nameof(MeltingOneTopFourRows), CrocomireMouth.Closed);

    /// <summary>Melting two top row.</summary>
    internal static CrocomireBodyFrame MeltingTwoTopRow { get; } = new(nameof(MeltingTwoTopRow), CrocomireMouth.Closed);

    /// <summary>Melting two top two rows.</summary>
    internal static CrocomireBodyFrame MeltingTwoTopTwoRows { get; } = new(nameof(MeltingTwoTopTwoRows), CrocomireMouth.Closed);

    /// <summary>Melting two top three rows.</summary>
    internal static CrocomireBodyFrame MeltingTwoTopThreeRows { get; } = new(nameof(MeltingTwoTopThreeRows), CrocomireMouth.Closed);

    /// <summary>Melting two top four rows.</summary>
    internal static CrocomireBodyFrame MeltingTwoTopFourRows { get; } = new(nameof(MeltingTwoTopFourRows), CrocomireMouth.Closed);

    /// <summary>Skeleton falling first.</summary>
    internal static CrocomireBodyFrame SkeletonFallingFirst { get; } = new(nameof(SkeletonFallingFirst), null);

    /// <summary>Skeleton falling second.</summary>
    internal static CrocomireBodyFrame SkeletonFallingSecond { get; } = new(nameof(SkeletonFallingSecond), null);

    /// <summary>Skeleton falling third.</summary>
    internal static CrocomireBodyFrame SkeletonFallingThird { get; } = new(nameof(SkeletonFallingThird), null);

    /// <summary>Skeleton falls apart first.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartFirst { get; } = new(nameof(SkeletonFallsApartFirst), null);

    /// <summary>Skeleton falls apart second.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartSecond { get; } = new(nameof(SkeletonFallsApartSecond), null);

    /// <summary>Skeleton falls apart third.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartThird { get; } = new(nameof(SkeletonFallsApartThird), null);

    /// <summary>Skeleton falls apart fourth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartFourth { get; } = new(nameof(SkeletonFallsApartFourth), null);

    /// <summary>Skeleton falls apart fifth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartFifth { get; } = new(nameof(SkeletonFallsApartFifth), null);

    /// <summary>Skeleton falls apart sixth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartSixth { get; } = new(nameof(SkeletonFallsApartSixth), null);

    /// <summary>Skeleton falls apart seventh.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartSeventh { get; } = new(nameof(SkeletonFallsApartSeventh), null);

    /// <summary>Skeleton falls apart eighth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartEighth { get; } = new(nameof(SkeletonFallsApartEighth), null);

    /// <summary>Skeleton falls apart ninth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartNinth { get; } = new(nameof(SkeletonFallsApartNinth), null);

    /// <summary>Skeleton falls apart tenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartTenth { get; } = new(nameof(SkeletonFallsApartTenth), null);

    /// <summary>Skeleton falls apart eleventh.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartEleventh { get; } = new(nameof(SkeletonFallsApartEleventh), null);

    /// <summary>Skeleton falls apart twelfth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartTwelfth { get; } = new(nameof(SkeletonFallsApartTwelfth), null);

    /// <summary>Skeleton falls apart thirteenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartThirteenth { get; } = new(nameof(SkeletonFallsApartThirteenth), null);

    /// <summary>Skeleton falls apart fourteenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartFourteenth { get; } = new(nameof(SkeletonFallsApartFourteenth), null);

    /// <summary>Skeleton falls apart fifteenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartFifteenth { get; } = new(nameof(SkeletonFallsApartFifteenth), null);

    /// <summary>Skeleton falls apart sixteenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartSixteenth { get; } = new(nameof(SkeletonFallsApartSixteenth), null);

    /// <summary>Skeleton falls apart seventeenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartSeventeenth { get; } = new(nameof(SkeletonFallsApartSeventeenth), null);

    /// <summary>Skeleton falls apart eighteenth.</summary>
    internal static CrocomireBodyFrame SkeletonFallsApartEighteenth { get; } = new(nameof(SkeletonFallsApartEighteenth), null);

    /// <summary>Skeleton stable.</summary>
    internal static CrocomireBodyFrame SkeletonStable { get; } = new(nameof(SkeletonStable), null);

    /// <summary>Dead.</summary>
    internal static CrocomireBodyFrame Dead { get; } = new(nameof(Dead), null);

    /// <summary>Skeleton flowing down river first.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverFirst { get; } = new(nameof(SkeletonFlowingDownRiverFirst), null);

    /// <summary>Skeleton flowing down river second.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverSecond { get; } = new(nameof(SkeletonFlowingDownRiverSecond), null);

    /// <summary>Skeleton flowing down river third.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverThird { get; } = new(nameof(SkeletonFlowingDownRiverThird), null);

    /// <summary>Skeleton flowing down river fourth.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverFourth { get; } = new(nameof(SkeletonFlowingDownRiverFourth), null);

    /// <summary>Skeleton flowing down river fifth.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverFifth { get; } = new(nameof(SkeletonFlowingDownRiverFifth), null);

    /// <summary>Skeleton flowing down river sixth.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverSixth { get; } = new(nameof(SkeletonFlowingDownRiverSixth), null);

    /// <summary>Skeleton flowing down river seventh.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverSeventh { get; } = new(nameof(SkeletonFlowingDownRiverSeventh), null);

    /// <summary>Skeleton flowing down river eighth.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverEighth { get; } = new(nameof(SkeletonFlowingDownRiverEighth), null);

    /// <summary>Skeleton flowing down river ninth.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverNinth { get; } = new(nameof(SkeletonFlowingDownRiverNinth), null);

    /// <summary>Skeleton flowing down river tenth.</summary>
    internal static CrocomireBodyFrame SkeletonFlowingDownRiverTenth { get; } = new(nameof(SkeletonFlowingDownRiverTenth), null);
}

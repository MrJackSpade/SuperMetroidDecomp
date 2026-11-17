namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>
/// The Sidehoppers' and Dessgeegas' floor and ceiling hop and landing animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="HopperAnimations"/>.
/// </summary>
internal enum HopperSegment
{
    /// <summary>
    /// A Sidehopper leaping off the floor on straight legs with the hop sound; it moves off screen too.
    /// </summary>
    SidehopperFloorJumping,
    /// <summary>
    /// A Sidehopper landing on the floor with its sound, its legs bending and crouching twice before it is ready to hop
    /// again.
    /// </summary>
    SidehopperFloorLanded,
    /// <summary>
    /// A Sidehopper leaping off the ceiling on straight legs with the hop sound; it moves off screen too.
    /// </summary>
    SidehopperCeilingJumping,
    /// <summary>
    /// A Sidehopper landing on the ceiling with its sound, its legs bending and crouching twice before it is ready to
    /// hop again.
    /// </summary>
    SidehopperCeilingLanded,
    /// <summary>A Dessgeega leaping off the floor on straight legs; it moves off screen too.</summary>
    DessgeegaFloorJumping,
    /// <summary>
    /// A Dessgeega landing on the floor, its legs bending and crouching twice before it is ready to hop again.
    /// </summary>
    DessgeegaFloorLanded,
    /// <summary>A Dessgeega leaping off the ceiling on straight legs; it moves off screen too.</summary>
    DessgeegaCeilingJumping,
    /// <summary>
    /// A Dessgeega landing on the ceiling, its legs bending and crouching twice before it is ready to hop again.
    /// </summary>
    DessgeegaCeilingLanded,
    /// <summary>
    /// A large Sidehopper leaping off the floor on straight legs with the hop sound; it moves off screen too.
    /// </summary>
    LargeSidehopperFloorJumping,
    /// <summary>
    /// A large Sidehopper landing on the floor with its sound, its legs bending and crouching twice before it is ready
    /// to hop again.
    /// </summary>
    LargeSidehopperFloorLanded,
    /// <summary>
    /// A large Sidehopper leaping off the ceiling on straight legs with the hop sound; it moves off screen too.
    /// </summary>
    LargeSidehopperCeilingJumping,
    /// <summary>
    /// A large Sidehopper landing on the ceiling with its sound, its legs bending and crouching twice before it is
    /// ready to hop again.
    /// </summary>
    LargeSidehopperCeilingLanded,
    /// <summary>A large Dessgeega leaping off the floor on straight legs; it moves off screen too.</summary>
    LargeDessgeegaFloorJumping,
    /// <summary>
    /// A large Dessgeega landing on the floor, its legs bending and crouching twice before it is ready to hop again.
    /// </summary>
    LargeDessgeegaFloorLanded,
    /// <summary>A large Dessgeega leaping off the ceiling on straight legs; it moves off screen too.</summary>
    LargeDessgeegaCeilingJumping,
    /// <summary>
    /// A large Dessgeega landing on the ceiling, its legs bending and crouching twice before it is ready to hop again.
    /// </summary>
    LargeDessgeegaCeilingLanded,
}

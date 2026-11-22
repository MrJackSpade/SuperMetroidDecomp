namespace SuperMetroid.Core;

/// <summary>
/// Learns how a game snapshot holds each type of the game (#2602). Each marked project's registration is generated: every
/// type's own members, the collections, arrays and enums its fields are declared with, the methods its delegates are made
/// from, and the definition catalogs its static members hold, so a snapshot names every type, field and method it holds in
/// code rather than looking them up while the game runs.
/// </summary>
public interface IStateGraphRegistry
{
}

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>Whether Samus's touch and shots reach a Powamp half, as its placement says (#2474). Its values are arbitrary.</summary>
internal enum PowampContact
{
    /// <summary>The half takes Samus's touch and shots.</summary>
    Accepts,

    /// <summary>Samus's touch and shots pass the half by, as a grappled balloon's placement has them.</summary>
    Refuses,
}

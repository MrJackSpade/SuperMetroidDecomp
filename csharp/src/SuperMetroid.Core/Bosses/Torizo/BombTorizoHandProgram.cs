namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo hand's PLM programs (#1296), in place of its list.</summary>
internal enum BombTorizoHandProgram
{
    /// <summary>Shows the intact hand and sleeps until Samus has Bombs (<c>InstList_PLM_BombTorizosCrumblingChozo</c>).</summary>
    Sleeping,
    /// <summary>The list after the sleep: the hand crumbles into eight fragments and the boss music starts.</summary>
    Crumble,
}

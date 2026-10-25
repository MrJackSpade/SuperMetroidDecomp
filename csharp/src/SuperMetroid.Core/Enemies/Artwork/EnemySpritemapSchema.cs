namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Enemy composition schema revisions accepted by the catalog, oldest first.</summary>
internal static class EnemySpritemapSchema
{
    /// <summary>Schema revision 4.</summary>
    internal const int Legacy = 4;
    /// <summary>Schema revision 5.</summary>
    internal const int Intermediate = 5;
    /// <summary>Schema revision 6.</summary>
    internal const int Earlier = 6;
    /// <summary>Schema revision 7.</summary>
    internal const int Prior = 7;
    /// <summary>Schema revision 8.</summary>
    internal const int Previous = 8;
    /// <summary>Schema revision 9.</summary>
    internal const int PreOwtchStoke = 9;
    /// <summary>Schema revision 10.</summary>
    internal const int PreRipper = 10;
    /// <summary>Schema revision 11.</summary>
    internal const int PreFireflea = 11;
    /// <summary>Schema revision 12.</summary>
    internal const int PreMagdollite = 12;
    /// <summary>Last art-only composition schema; later accepted schemas own editable display bindings.</summary>
    internal const int PreDisplayBindings = 13;
    /// <summary>Schema revision 14.</summary>
    internal const int PreCeresDoor = 14;
    /// <summary>Schema revision 15.</summary>
    internal const int PreCeresBaby = 15;
    /// <summary>Schema revision 16.</summary>
    internal const int PreRio = 16;
    /// <summary>Schema revision 17.</summary>
    internal const int PreLowerNorfairRio = 17;
    /// <summary>Schema revision 18.</summary>
    internal const int PreNorfairRio = 18;
    /// <summary>Schema revision 19.</summary>
    internal const int PrePuyo = 19;
    /// <summary>Schema revision 20.</summary>
    internal const int PreBull = 20;
    /// <summary>Schema revision 21.</summary>
    internal const int PreAlcoon = 21;
    /// <summary>Schema revision 22.</summary>
    internal const int PreBeetom = 22;
    /// <summary>Schema revision 23.</summary>
    internal const int PreHopper = 23;
    /// <summary>Schema revision 24.</summary>
    internal const int PreChoot = 24;
    /// <summary>Schema revision 25.</summary>
    internal const int PreHZoomer = 25;
    /// <summary>Schema revision 26.</summary>
    internal const int PreSbug = 26;
    /// <summary>Schema revision 27.</summary>
    internal const int PreFuneNamihe = 27;
    /// <summary>Schema revision 28.</summary>
    internal const int PreKamer = 28;
    /// <summary>Schema revision 29.</summary>
    internal const int PreElevator = 29;
    /// <summary>Schema revision 30.</summary>
    internal const int PreDraygonIntro = 30;
    /// <summary>Schema revision 31.</summary>
    internal const int PreDraygonBreath = 31;
    /// <summary>Schema revision 32.</summary>
    internal const int PreRoomSpriteObject = 32;
    /// <summary>Schema revision 33.</summary>
    internal const int PreYappingMaw = 33;
    /// <summary>Schema revision 34.</summary>
    internal const int PreKiHunter = 34;
    /// <summary>Schema revision 35.</summary>
    internal const int PreMotherBrain = 35;
    /// <summary>Schema revision 36.</summary>
    internal const int PreDeadTorizo = 36;
    /// <summary>Schema revision 37.</summary>
    internal const int PreRidleySupplement = 37;
    /// <summary>Schema revision 38.</summary>
    internal const int PreSciser = 38;
    /// <summary>Schema revision 39.</summary>
    internal const int PreFly = 39;
    /// <summary>Schema revision 40.</summary>
    internal const int PreKago = 40;
    /// <summary>Schema revision 41.</summary>
    internal const int PreFaceBlock = 41;
    /// <summary>Schema revision 42.</summary>
    internal const int PreMorphBallEye = 42;
    /// <summary>Schema revision 43.</summary>
    internal const int PreShutter = 43;
    /// <summary>Schema revision 44.</summary>
    internal const int PreMetroid = 44;
    /// <summary>Schema revision 45.</summary>
    internal const int PreShaktool = 45;
    /// <summary>Schema revision 46.</summary>
    internal const int PreTripperKamer = 46;
    /// <summary>Schema revision 47.</summary>
    internal const int PreDragon = 47;
    /// <summary>Schema revision 48.</summary>
    internal const int PreMultiviola = 48;
    /// <summary>Schema revision 49.</summary>
    internal const int PreNorfairLavaJumper = 49;
    /// <summary>Schema revision 50.</summary>
    internal const int PreChozoStatue = 50;
    /// <summary>Schema revision 51.</summary>
    internal const int PreViola = 51;
    /// <summary>Schema revision 52.</summary>
    internal const int PreRinka = 52;
    /// <summary>Schema revision 53.</summary>
    internal const int PreDeadTorizoStationary = 53;
    /// <summary>Schema revision 54.</summary>
    internal const int PreDeadTourianCorpse = 54;
    /// <summary>Schema revision 55.</summary>
    internal const int PreMochtroid = 55;
    /// <summary>Schema revision 56.</summary>
    internal const int PreEvir = 56;
    /// <summary>Schema revision 57.</summary>
    internal const int PreWorkRobot = 57;
    /// <summary>Schema revision 58.</summary>
    internal const int PreYard = 58;
    /// <summary>Schema revision 59.</summary>
    internal const int PreBotwoon = 59;
    /// <summary>Schema revision 60.</summary>
    internal const int PreGunship = 60;
    /// <summary>Schema before the tatori family; previous compositions and bindings remain editable.</summary>
    internal const int PreMamaTurtle = 61;
    /// <summary>Schema before the Zero crawler; existing edits and bindings remain valid.</summary>
    internal const int PreZero = 62;
    /// <summary>Schema before the friendly-animal compositions; existing edits remain valid.</summary>
    internal const int PreFriendlyAnimal = 63;
    /// <summary>Schema before audit-identified ordinary environmental/Tourian artwork additions.</summary>
    internal const int PreAuditOrdinary = 64;
    /// <summary>Schema before Kraid's two ordinary belly-lint frames were installed.</summary>
    internal const int PreKraidLint = 65;
    /// <summary>Schema before Puromi/Nuclear Waffle's eight head frames were installed.</summary>
    internal const int PreNuclearWaffle = 66;
    /// <summary>Schema before the single-frame Kzan and Polyp compositions were installed.</summary>
    internal const int PreSingleFrame = 67;
    /// <summary>Schema before Ridley's eight ordinary breakup body compositions.</summary>
    internal const int PreRidleyBreakup = 68;
    /// <summary>The current schema, holding every named frame.</summary>
    internal const int Current = 69;

    /// <summary>Whether <paramref name="version"/> is a revision this catalog accepts.</summary>
    internal static bool IsSupported(int version) => version is >= Legacy and <= Current;
}

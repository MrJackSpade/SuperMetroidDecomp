namespace SuperMetroid.Core.Assets;

/// <summary>Enemy composition schema revisions accepted by the catalog, oldest first.</summary>
internal enum EnemySpritemapSchema
{
    /// <summary>Schema revision 4.</summary>
    Legacy = 4,
    /// <summary>Schema revision 5.</summary>
    Intermediate = 5,
    /// <summary>Schema revision 6.</summary>
    Earlier = 6,
    /// <summary>Schema revision 7.</summary>
    Prior = 7,
    /// <summary>Schema revision 8.</summary>
    Previous = 8,
    /// <summary>Schema revision 9.</summary>
    PreOwtchStoke = 9,
    /// <summary>Schema revision 10.</summary>
    PreRipper = 10,
    /// <summary>Schema revision 11.</summary>
    PreFireflea = 11,
    /// <summary>Schema revision 12.</summary>
    PreMagdollite = 12,
    /// <summary>Last art-only composition schema; later accepted schemas own editable display bindings.</summary>
    PreDisplayBindings = 13,
    /// <summary>Schema revision 14.</summary>
    PreCeresDoor = 14,
    /// <summary>Schema revision 15.</summary>
    PreCeresBaby = 15,
    /// <summary>Schema revision 16.</summary>
    PreRio = 16,
    /// <summary>Schema revision 17.</summary>
    PreLowerNorfairRio = 17,
    /// <summary>Schema revision 18.</summary>
    PreNorfairRio = 18,
    /// <summary>Schema revision 19.</summary>
    PrePuyo = 19,
    /// <summary>Schema revision 20.</summary>
    PreBull = 20,
    /// <summary>Schema revision 21.</summary>
    PreAlcoon = 21,
    /// <summary>Schema revision 22.</summary>
    PreBeetom = 22,
    /// <summary>Schema revision 23.</summary>
    PreHopper = 23,
    /// <summary>Schema revision 24.</summary>
    PreChoot = 24,
    /// <summary>Schema revision 25.</summary>
    PreHZoomer = 25,
    /// <summary>Schema revision 26.</summary>
    PreSbug = 26,
    /// <summary>Schema revision 27.</summary>
    PreFuneNamihe = 27,
    /// <summary>Schema revision 28.</summary>
    PreKamer = 28,
    /// <summary>Schema revision 29.</summary>
    PreElevator = 29,
    /// <summary>Schema revision 30.</summary>
    PreDraygonIntro = 30,
    /// <summary>Schema revision 31.</summary>
    PreDraygonBreath = 31,
    /// <summary>Schema revision 32.</summary>
    PreRoomSpriteObject = 32,
    /// <summary>Schema revision 33.</summary>
    PreYappingMaw = 33,
    /// <summary>Schema revision 34.</summary>
    PreKiHunter = 34,
    /// <summary>Schema revision 35.</summary>
    PreMotherBrain = 35,
    /// <summary>Schema revision 36.</summary>
    PreDeadTorizo = 36,
    /// <summary>Schema revision 37.</summary>
    PreRidleySupplement = 37,
    /// <summary>Schema revision 38.</summary>
    PreSciser = 38,
    /// <summary>Schema revision 39.</summary>
    PreFly = 39,
    /// <summary>Schema revision 40.</summary>
    PreKago = 40,
    /// <summary>Schema revision 41.</summary>
    PreFaceBlock = 41,
    /// <summary>Schema revision 42.</summary>
    PreMorphBallEye = 42,
    /// <summary>Schema revision 43.</summary>
    PreShutter = 43,
    /// <summary>Schema revision 44.</summary>
    PreMetroid = 44,
    /// <summary>Schema revision 45.</summary>
    PreShaktool = 45,
    /// <summary>Schema revision 46.</summary>
    PreTripperKamer = 46,
    /// <summary>Schema revision 47.</summary>
    PreDragon = 47,
    /// <summary>Schema revision 48.</summary>
    PreMultiviola = 48,
    /// <summary>Schema revision 49.</summary>
    PreNorfairLavaJumper = 49,
    /// <summary>Schema revision 50.</summary>
    PreChozoStatue = 50,
    /// <summary>Schema revision 51.</summary>
    PreViola = 51,
    /// <summary>Schema revision 52.</summary>
    PreRinka = 52,
    /// <summary>Schema revision 53.</summary>
    PreDeadTorizoStationary = 53,
    /// <summary>Schema revision 54.</summary>
    PreDeadTourianCorpse = 54,
    /// <summary>Schema revision 55.</summary>
    PreMochtroid = 55,
    /// <summary>Schema revision 56.</summary>
    PreEvir = 56,
    /// <summary>Schema revision 57.</summary>
    PreWorkRobot = 57,
    /// <summary>Schema revision 58.</summary>
    PreYard = 58,
    /// <summary>Schema revision 59.</summary>
    PreBotwoon = 59,
    /// <summary>Schema revision 60.</summary>
    PreGunship = 60,
    /// <summary>Schema before the tatori family; previous compositions and bindings remain editable.</summary>
    PreMamaTurtle = 61,
    /// <summary>Schema before the Zero crawler; existing edits and bindings remain valid.</summary>
    PreZero = 62,
    /// <summary>Schema before the friendly-animal compositions; existing edits remain valid.</summary>
    PreFriendlyAnimal = 63,
    /// <summary>Schema before audit-identified ordinary environmental/Tourian artwork additions.</summary>
    PreAuditOrdinary = 64,
    /// <summary>Schema before Kraid's two ordinary belly-lint frames were installed.</summary>
    PreKraidLint = 65,
    /// <summary>Schema before Puromi/Nuclear Waffle's eight head frames were installed.</summary>
    PreNuclearWaffle = 66,
    /// <summary>Schema before the single-frame Kzan and Polyp compositions were installed.</summary>
    PreSingleFrame = 67,
    /// <summary>Schema before Ridley's eight ordinary breakup body compositions.</summary>
    PreRidleyBreakup = 68,
    /// <summary>The current schema, holding every named frame.</summary>
    Current = 69,
}

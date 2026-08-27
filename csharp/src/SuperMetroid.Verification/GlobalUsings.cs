// Assertions are verifier infrastructure, not members of the executable's partial Program
// type. A global static import keeps call sites compact across every domain-focused file.
global using static VerificationAssert;

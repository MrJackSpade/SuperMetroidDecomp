/* #376: WRAM and native-entry definitions for an accepted airborne attachment. */
enum GrappleAttachmentFixture {
  /* C742 executes HandleConnectingGrapple AND its firing-caller continuation. */
  NativeAcceptedGrappleConnection = 0x9bc742,
  GrappleLength = 0xcfe, GrappleLengthDelta = 0xd00,
  GrappleAnchorX = 0xd08, GrappleAnchorY = 0xd0c,
  GrappleFireDirection = 0xd34,
  RightwardGrappleDirection = 2,
  AutomaticGrappleRetraction = 0xfff8
};

static int verify_grapple_attachment(void) {
  memset(ram, 0, sizeof(ram));
  word(SamusX, 177); word(SamusY, 552);
  word(SamusYSpeed, 1);
  word(GrappleAnchorX, 199); word(GrappleAnchorY, 536);
  word(GrappleLength, 12); word(GrappleFireDirection, RightwardGrappleDirection);
  run(NativeAcceptedGrappleConnection);
  printf("Native accepted connection: length delta=%04X\n", readword(GrappleLengthDelta));
  if (readword(GrappleLengthDelta) != AutomaticGrappleRetraction)
    Die("Native automatic attachment retraction differs\n");
  return 0;
}

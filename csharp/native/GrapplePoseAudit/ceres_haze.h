/* Execute the cartridge HDMA pre-instructions to establish the room-owned
   gradient lifecycle independently of either managed rendering implementation. */
enum {
  CeresHazeAlive = 0x88de10, CeresHazeDead = 0x88de15,
  CeresHazeFadeIn = 0x88de2d, CeresHazeHold = 0x88de74,
  CeresHazeFadeOut = 0x88de96,
  CeresHazeDoorFunction = 0x99c, CeresHazePreInstruction = 0x18f0,
  CeresHazeCounter = 0x1914, CeresHazeChannel = 0x1920,
  CeresHazeTable = 0x9d00, CeresHazeDoorFadeIn = 0xe737,
  CeresHazeDoorFadeOut = 0xe2db
};
static int verify_ceres_haze(void) {
  /* The HDMA dispatcher invokes these pre-instructions with eight-bit indexes. */
  for (unsigned red = 0; red < 2; red++) {
    memset(ram, 0, sizeof(ram));
    unsigned channel = red ? 0x20 : 0x80;
    run_with_index_width(red ? CeresHazeDead : CeresHazeAlive, true);
    if (readword(CeresHazeCounter) != 0 || readword(CeresHazeChannel) != channel)
      Die("Ceres haze must wait for room fade-in\n");
    word(CeresHazeDoorFunction, CeresHazeDoorFadeIn);
    run_with_index_width(red ? CeresHazeDead : CeresHazeAlive, true);
    if (readword(CeresHazeCounter) != 1)
      Die("Ceres haze fade-in must start with the zero-intensity table\n");
    for (unsigned frame = 1; frame < 16; frame++) run_with_index_width(CeresHazeFadeIn, true);
    for (unsigned band = 0; band < 16; band++) {
      if (ram[CeresHazeTable + band] != (channel | band))
        Die("Ceres haze native full-gradient band mismatch\n");
    }
    run_with_index_width(CeresHazeFadeIn, true);
    if (readword(CeresHazePreInstruction) != (CeresHazeHold & 0xffff))
      Die("Ceres haze must hold the room-selected channel after fading in\n");
    word(CeresHazeDoorFunction, CeresHazeDoorFadeOut);
    run_with_index_width(CeresHazeHold, true);
    if (readword(CeresHazePreInstruction) != (CeresHazeFadeOut & 0xffff))
      Die("Ceres haze did not enter source-room fade-out\n");
    printf("Ceres haze %s: waits for room fade, 16 ramps, bands 0..15, source fade-out verified.\n", red ? "red" : "blue");
  }
  return 0;
}

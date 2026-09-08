/* $91:FA76 wall-jump dust producer. Addresses name the WRAM contract, not
   an alternate implementation of the routine: run() executes cartridge bytes. */
enum {
  WallDustNative = 0x91fa76, WallDustTimer = 0xada, WallDustX = 0xae2,
  WallDustY = 0xaea, WallDustType = 0xaf2, WallDustFacing = 0xa1e,
  WallDustWater = 0x195e, WallDustLava = 0x1962, WallDustOptions = 0x197e
};
static int verify_wall_jump_dust(void) {
  unsigned count = 0;
  for (unsigned direction = 4; direction <= 8; direction += 4) {
    for (unsigned medium = 0; medium < 5; medium++) {
      memset(ram, 0, sizeof(ram));
      word(SamusX, 128); word(SamusY, 96); word(SamusRadiusY, 19);
      word(Pose, direction == 8 ? 0x83 : 0x84);
      ram[WallDustFacing] = direction;
      word(WallDustWater, medium == 1 || medium == 3 ? 100 : medium == 4 ? 114 : 0xffff);
      word(WallDustLava, medium == 2 ? 100 : 0xffff);
      word(WallDustOptions, medium == 3 ? 4 : 0);
      word(WallDustType, 0x0102); word(WallDustTimer, 7);
      word(WallDustX, 33); word(WallDustY, 44);
      run(WallDustNative);
      bool suppressed = medium == 1 || medium == 2;
      unsigned expected_x = direction == 8 ? 122 : 134;
      printf("direction=%u medium=%u type=%04X timer=%u x=%u y=%u\n", direction, medium,
        readword(WallDustType), readword(WallDustTimer), readword(WallDustX), readword(WallDustY));
      if (readword(WallDustType) != (suppressed ? 0x0102 : 0x0600) ||
          readword(WallDustTimer) != (suppressed ? 7 : 3) ||
          readword(WallDustX) != (suppressed ? 33 : expected_x) ||
          readword(WallDustY) != (suppressed ? 44 : 114))
        Die("Native wall-jump dust contract mismatch\n");
      count++;
    }
  }
  printf("Native wall-jump dust: %u direction/liquid/slot-preservation cases passed.\n", count);
  return 0;
}

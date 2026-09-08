/* #51: execute the ROM's actual slot-one eye HDMA builder at the reported
   01/10 camera position. Output is diagnostic CSV, not a translated oracle. */
enum EyeWindowFixture {
  EyeWindowNative = 0x88e987,
  EyeWindowBodyX = 0xfba, EyeWindowBodyY = 0xfbe, EyeWindowAngle = 0xfee,
  EyeWindowCameraX = 0x911, EyeWindowCameraY = 0x915,
  EyeWindowWidth = 0x192c, EyeWindowTable = 0x9100
};
static int dump_eye_windows(void) {
  const unsigned angles[] = {160, 176, 192, 208, 224};
  puts("angle,width,y,left,right");
  for (unsigned a = 0; a < 5; a++) for (unsigned width = 0; width <= 4; width += 4) {
    memset(ram, 0, sizeof(ram));
    word(EyeWindowBodyX, 552); word(EyeWindowBodyY, 616);
    word(EyeWindowCameraX, 384); word(EyeWindowCameraY, 512);
    word(EyeWindowAngle, angles[a]); word(EyeWindowWidth, width);
    for (unsigned y = 0; y < 256; y++) word(EyeWindowTable + y * 2, 0x00ff);
    run_with_index_width(EyeWindowNative, true);
    if (angles[a] == 192 && width == 0 && readword(EyeWindowTable + 103 * 2) != 0xa800)
      Die("Native eye horizontal apex differs from expected slot-one origin\n");
    for (unsigned y = 0; y < 224; y++) {
      unsigned window = readword(EyeWindowTable + y * 2);
      printf("%u,%u,%u,%u,%u\n", angles[a], width, y, window & 255, window >> 8);
    }
  }
  return 0;
}

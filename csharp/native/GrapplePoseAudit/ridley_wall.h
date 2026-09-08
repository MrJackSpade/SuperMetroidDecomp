/* $A6:D86B integration calls $D914 only at the left boundary, before clearing X velocity. */
enum { RidleyIntegrate = 0xa6d86b, RidleyVx = 0xfaa, RidleyVy = 0xfac,
  RidleyMinY = 0x8000, RidleyMaxY = 0x8002, RidleyMinX = 0x8004, RidleyMaxX = 0x8006,
  RidleyArea = 0x79f, RidleyQuakeType = 0x183e, RidleyQuakeTimer = 0x1840 };
static int verify_ridley_wall(void) {
  const int cases[][5] = {
    /* area, X, VX, VY, expected quake */
    {6, 41, -640, 0, 1}, {6, 41, -639, 0, 0},
    {6, 40, -1, 640, 1}, {6, 222, 640, 0, 0},
    {6, 43, -640, 0, 0}, {2, 41, -640, 0, 0}
  };
  for (unsigned i = 0; i < sizeof(cases) / sizeof(cases[0]); i++) {
    memset(ram, 0, sizeof(ram));
    word(RidleyArea, cases[i][0]); word(EnemyX, cases[i][1]); word(EnemyY, 100);
    word(RidleyVx, cases[i][2]); word(RidleyVy, cases[i][3]);
    word(RidleyMinX, 40); word(RidleyMaxX, 224); word(RidleyMinY, 0); word(RidleyMaxY, 200);
    run(RidleyIntegrate);
    printf("Ridley wall case %u: X=%u quake=%u timer=%u\n", i,
      readword(EnemyX), readword(RidleyQuakeType), readword(RidleyQuakeTimer));
    if (readword(RidleyQuakeType) != (cases[i][4] ? 33 : 0) ||
        readword(RidleyQuakeTimer) != (cases[i][4] ? 12 : 0))
      Die("Native Ridley wall impact mismatch\n");
  }
  return 0;
}

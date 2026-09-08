#include <stdio.h>
#include <stdint.h>
#include <stdbool.h>
#include <stdlib.h>
#include <string.h>
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"
#include "fixture.h"

/* Execute ROM bytes, not translated gameplay. Unknown accesses fail on stderr. */
static uint8_t rom[0x300000], ram[0x20000];
static bool returned;
static unsigned multiplier, product, dividend, quotient, remainder;
void Die(const char *message) { fprintf(stderr, "%s", message); exit(2); }
int CpuOpcodeHook(uint32_t address) { fprintf(stderr, "Unexpected CPU opcode hook at %06X\n", address); exit(2); return 0; }
bool HookedFunctionRts(int is_long) { (void)is_long; returned = true; return true; }
uint8_t snes_cpuRead(Snes *snes, uint32_t address) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if ((bank & 0x7f) < 0x40) {
    if (offset == 0x4214) return quotient;
    if (offset == 0x4215) return quotient >> 8;
    if (offset == 0x4216) return remainder;
    if (offset == 0x4217) return remainder >> 8;
  }
  if (bank == 0x7e || bank == 0x7f) return ram[address - 0x7e0000];
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) return ram[offset];
  if (offset >= 0x8000) {
    unsigned index = ((bank & 0x7f) << 15) | (offset & 0x7fff);
    if (index < sizeof(rom)) return rom[index];
  }
  fprintf(stderr, "Unmapped read %06X\n", address); exit(2);
}
void snes_cpuWrite(Snes *snes, uint32_t address, uint8_t value) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  /* The CPU waits the required cycles in the retail routines; these fixtures only
     observe completed arithmetic results, not intermediate hardware latency. */
  if ((bank & 0x7f) < 0x40) {
    if (offset == 0x4202) { multiplier = value; return; }
    if (offset == 0x4203) { product = multiplier * value; remainder = product; return; }
    if (offset == 0x4204) { dividend = (dividend & 0xff00) | value; return; }
    if (offset == 0x4205) { dividend = (dividend & 0xff) | (value << 8); return; }
    if (offset == 0x4206) {
      quotient = value ? dividend / value : 0xffff;
      remainder = value ? dividend % value : dividend;
      return;
    }
  }
  if (bank == 0x7e || bank == 0x7f) { ram[address - 0x7e0000] = value; return; }
  if ((bank & 0x7f) < 0x40 && offset < 0x2000) { ram[offset] = value; return; }
  fprintf(stderr, "Unmapped write %06X\n", address); exit(2);
}
static void word(unsigned address, unsigned value) { ram[address] = value; ram[address + 1] = value >> 8; }
static unsigned readword(unsigned address) { return ram[address] | ram[address + 1] << 8; }
static void run_with_index_width(unsigned address, bool short_indexes) {
  Cpu *cpu = cpu_init(NULL, 0);
  cpu->pc = address & 0xffff; cpu->k = cpu->db = address >> 16;
  cpu->sp = cpu->spBreakpoint = 0x1ff;
  cpu->xf = short_indexes;
  returned = false;
  for (int i = 0; i < 100000 && !returned; i++) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (!returned) Die("Native fixture exceeded instruction limit\n");
}
static void run(unsigned address) { run_with_index_width(address, false); }
#include "wall_jump_dust.h"
#include "ceres_haze.h"
#include "ridley_wall.h"
#include "eye_window.h"
int main(int argc, char **argv) {
  if (argc != 2 && argc != 3) { fprintf(stderr, "Usage: audit <unheadered-rom> [shutter-ceiling|shutter-carry|bomb-wall|shutter-bomb-arc]\n"); return 2; }
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom)) return 2;
  fclose(file);
  if (argc == 3 && !strcmp(argv[2], "wall-jump-dust")) return verify_wall_jump_dust();
  if (argc == 3 && !strcmp(argv[2], "ceres-haze")) return verify_ceres_haze();
  if (argc == 3 && !strcmp(argv[2], "ridley-wall")) return verify_ridley_wall();
  if (argc == 3 && !strcmp(argv[2], "eye-window")) return dump_eye_windows();
  if (argc == 3 && !strcmp(argv[2], "shutter-bomb-arc")) {
    FILE *seed = fopen("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.wram", "rb");
    FILE *trace = fopen("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.csv", "r");
    if (!seed || !trace || fread(ram, 1, sizeof(ram), seed) != sizeof(ram))
      Die("Missing/truncated shutter bomb-arc fixture; run --shutter-native-arc first\n");
    fclose(seed);
    FILE *bombs = fopen("csharp/test-fixtures/issue-347-repeated-bombs/bomb-arc.projectiles", "rb");
    if (!bombs) Die("Missing bomb contact input snapshots\n");
    const unsigned bomb_fields[] = { ProjectileX, ProjectileY, ProjectileRadiusX, ProjectileRadiusY,
      ProjectileDirection, ProjectileType, ProjectileDamage, ProjectileTimer };
    unsigned frame, x, y, speed, direction, platform, fraction, count = 0;
    int fields;
    while ((fields = fscanf(trace, "%u,%u,%u,%u,%u,%u,%u", &frame, &x, &y, &speed, &direction, &platform, &fraction)) == 7) {
      word(NmiFrameWord, readword(NmiFrameWord) + 1);
      if (readword(BombJumpDirection) && !(readword(BombJumpDirection) & 0xff00)) {
        word(SpecialPose, 0xffff); word(SuperSpecialPose, 0xffff); word(ProspectivePose, 0xffff);
        run(NativeBombJumpSetup);
        run(NativeUpdatePose);
      }
      word(ExtraYWhole, 0);
      run(NativeEnemySamusInteraction);
      run(0xa20000 | readword(ShutterFunction));
      /* These are observed projectile inputs, not observed collision/direction
         outputs. Native $A0:9785 independently computes overlap using native
         Samus position before beta movement. Projectile lifecycle is not under
         test in this comparison. */
      for (unsigned slot = 5; slot < 10; slot++)
      for (unsigned field = 0; field < 8; field++) {
        int low = fgetc(bombs), high = fgetc(bombs);
        if (low == EOF || high == EOF) Die("Truncated bomb contact input snapshots\n");
        word(bomb_fields[field] + slot * 2, low | high << 8);
      }
      word(BombCount, 5);
      run(NativeProjectileInteraction);
      run(NativeEnemyBombInteraction);
      if (readword(SamusMovementHandler) == (NativeBombJumpStart & 0xffff)) run(NativeBombJumpStart);
      else if (readword(SamusMovementHandler) == (NativeBombJumpMain & 0xffff)) run(NativeBombJumpMain);
      else if (ram[MovementType] == 4) run(NativeGroundedMorphMovement);
      else if (ram[MovementType] == 8) run(NativeFallingMorphMovement);
      else Die("Unexpected posture in native bomb-arc continuation\n");
      /* The port's public movement seam includes ceiling pose-command side
         effects. Compare after the cartridge's own selection/pose dispatch,
         rather than mistaking intermediate nonzero speed for a divergence. */
      if (readword(CollisionPoseInput)) {
        word(PreviousPose, readword(Pose));
        word(PreviousDirection, readword(Pose + 2));
        word(SpecialPose, 0xffff); word(SuperSpecialPose, 0xffff); word(ProspectivePose, 0xffff);
        run(NativeCollisionPose);
        run(NativeUpdatePose);
      }
      unsigned actual_x = readword(SamusX) * 65536u + readword(SamusXFraction);
      unsigned actual_y = readword(SamusY) * 65536u + readword(SamusYFraction);
      unsigned actual_speed = readword(SamusYSpeed) * 65536u + readword(SamusYSubspeed);
      printf("frame %u: native X=%08X Y=%08X VY=%08X dir=%04X; port X=%08X Y=%08X VY=%08X dir=%04X\n",
        frame, actual_x, actual_y, actual_speed, readword(BombJumpDirection), x, y, speed, direction);
      if (actual_x != x || actual_y != y || actual_speed != speed || readword(BombJumpDirection) != direction ||
          readword(EnemyY) != platform || readword(EnemyYFraction) != fraction)
        Die("Native/port bomb-ascent divergence\n");
      count++;
    }
    if (fields != EOF || !count) Die("Invalid/empty shutter bomb-arc trace\n");
    fclose(trace);
    if (fgetc(bombs) != EOF) Die("Trailing bomb contact inputs\n");
    fclose(bombs);
    printf("%u actual-room ascent/landing/carry frames match native movement, pose transitions and platform AI.\n", count);
    return 0;
  }
  if (argc == 3 && !strcmp(argv[2], "bomb-wall")) {
    memset(ram, 0, sizeof(ram));
    word(RoomWidth, 16); word(SamusX, 59); word(SamusY, 80);
    word(SamusXFraction, 0xf000);
    word(SamusRadiusX, 5); word(SamusRadiusY, 7);
    word(Pose, 0x1d); ram[MovementType] = 4;
    for (int row = 0; row < 10; row++) word(LevelWords + (row * 16 + 4) * 2, 0x8000);
    word(BombJumpDirection, 0x0803); word(SamusYSpeed, 2); word(SamusYSubspeed, 0xc000);
    word(SamusYSubacceleration, 0x4000); word(SamusYDirection, 1);
    run(NativeBombJumpMain);
    printf("Native diagonal bomb/wall: X=%u Y=%u.%04X direction=%04X speed=%u.%04X\n",
      readword(SamusX), readword(SamusY), readword(SamusYFraction), readword(BombJumpDirection),
      readword(SamusYSpeed), readword(SamusYSubspeed));
    if (readword(SamusX) != 59 || readword(SamusY) != 77 || readword(SamusYFraction) != 0x4000 ||
        readword(BombJumpDirection) != 0x0803 || readword(SamusYSpeed) != 2 || readword(SamusYSubspeed) != 0x8000)
      Die("Native bomb-wall baseline changed\n");
    return 0;
  }
  if (argc == 3 && !strcmp(argv[2], "shutter-carry")) {
    /* Continue the critical contact through the real platform AI, not a scripted
       upward nudge. No bombs or horizontal movement are simulated here: this
       isolates the post-ceiling carry/grounding feedback loop. */
    memset(ram, 0, sizeof(ram));
    word(RoomWidth, 32); word(SamusX, 371); word(SamusY, 71);
    word(SamusRadiusX, 5); word(SamusRadiusY, 7);
    word(LevelWords + (3 * 32 + 23) * 2, 0x8000);
    word(InteractiveEnemyBytes, 2); word(InteractiveEnemyList, 0); word(InteractiveEnemyList + 2, 0xffff);
    word(EnemyX, 360); word(EnemyY, 110); word(EnemyYFraction, 0x8000);
    word(EnemyRadiusX, 8); word(EnemyRadiusY, 32); word(EnemyPropertiesWord, 0x8000);
    word(ShutterUpWhole, 0xffff); word(ShutterUpFraction, 0x8000);
    word(ShutterMinimumY, 0); /* No stop occurs within this bounded slice. */
    int minimum_gap = 0;
    puts("frame,platformY,platformFraction,carrying,extraY,samusY,gap");
    for (int frame = 0; frame < 64; frame++) {
      word(ExtraYWhole, 0);
      run(NativeShutterMovingUp);
      unsigned extra = readword(ExtraYWhole), carrying = readword(ShutterMovingSamus);
      run(NativeGroundedY);
      int gap = (int)readword(EnemyY) - 32 - (int)readword(SamusY) - 7;
      if (gap < minimum_gap) minimum_gap = gap;
      printf("%d,%u,%u,%u,%d,%u,%d\n", frame, readword(EnemyY), readword(EnemyYFraction),
        carrying, (int16_t)extra, readword(SamusY), gap);
      unsigned expected_y = frame == 0 || (frame & 1) ? 71 : 72;
      if (readword(SamusY) != expected_y || readword(EnemyY) != 110 - (frame + 1) / 2 ||
          readword(EnemyYFraction) != ((frame & 1) ? 0x8000 : 0) || carrying != 1 ||
          (int16_t)extra != ((frame & 1) ? -1 : 0))
        Die("Native coupled carry/grounding baseline changed\n");
    }
    fprintf(stderr, "Native coupled carry/grounding: minimum gap %d; not a complete bomb/input replay.\n", minimum_gap);
    return 0;
  }
  if (argc == 3) {
    if (strcmp(argv[2], "shutter-ceiling")) Die("Unknown native audit case\n");
    memset(ram, 0, sizeof(ram));
    word(RoomWidth, 32); word(SamusX, 371); word(SamusY, 71);
    word(SamusRadiusX, 5); word(SamusRadiusY, 7);
    /* The ball's right edge meets the solid ceiling beside the open shaft. */
    word(LevelWords + (3 * 32 + 23) * 2, 0x8000);
    word(InteractiveEnemyBytes, 2); word(InteractiveEnemyList, 0); word(InteractiveEnemyList + 2, 0xffff);
    word(EnemyX, 360); word(EnemyY, 109); word(EnemyRadiusX, 8); word(EnemyRadiusY, 32);
    word(EnemyPropertiesWord, 0x8000);
    word(ExtraYWhole, 0xffff);
    run(NativeGroundedY);
    unsigned after_up = readword(SamusY);
    word(ExtraYWhole, 0);
    run(NativeGroundedY);
    unsigned after_down = readword(SamusY);
    printf("Native shutter ceiling: upward carry Y=71 -> %u; following zero-carry grounding -> %u\n", after_up, after_down);
    if (after_up != 71 || after_down != 72) Die("Unexpected native ceiling/contact result\n");
    return 0;
  }
  for (int left = 0; left < 2; left++) {
    memset(ram, 0, sizeof(ram));
    word(Pose, left ? 0x14 : 0x13); ram[MovementType] = NormalJumping;
    word(Shot, 0x40); word(PreviousDrawNewInput, 0x40);
    word(GrappleFunction, GrappleInactive);
    /* Current NMI new keys remain zero: only the post-draw snapshot can fire. */
    run(NativeGrappleInactive);
    printf("Native retained Fire edge, facing %s: function=%04X\n", left ? "left" : "right", readword(GrappleFunction));
    if (readword(GrappleFunction) != GrappleFiring) return 1;
  }
  /* State 0's ledge geometry: the body's center is over air, but its left
     boundary intersects a solid column. Execute the original ROM routine with
     the swing radius, not a fabricated upward nudge or a center-only probe. */
  memset(ram, 0, sizeof(ram));
  word(RoomWidth, 96); word(SamusX, 0x542); word(SamusY, 0x4a1);
  word(SamusRadiusX, 5); word(SamusRadiusY, 17);
  word(LevelWords + 2 * (74 * 96 + 82), 0x1311);
  ram[BlockBts + 74 * 96 + 82] = 0x53;
  word(LevelWords + 2 * (74 * 96 + 83), 0x8311);
  for (int row = 75; row <= 77; row++) {
    word(LevelWords + 2 * (row * 96 + 82), row == 75 ? 0x82f3 : 0x0313);
    if (row == 75) word(LevelWords + 2 * (row * 96 + 83), 0x8311);
  }
  Cpu *collision = cpu_init(NULL, 0);
  collision->pc = NativePostGrappleCollision & 0xffff;
  collision->k = NativePostGrappleCollision >> 16;
  collision->db = 0x90;
  collision->sp = collision->spBreakpoint = 0x1ff;
  returned = false;
  for (int i = 0; i < 100000 && !returned; i++) cpu_runOpcode(collision);
  unsigned ejectedY = ram[SamusY] | ram[SamusY + 1] << 8;
  printf("Native post-grapple ledge ejection: Y=%04X (start 04A1, radius 17)\n", ejectedY);
  cpu_free(collision);
  if (!returned || ejectedY != 0x48f) return 1;
  word(Pose, 0xb2); word(PreviousPose, 0xb2); word(PreviousDirection, 0x1608);
  word(SuperSpecialPose, ReleasedLeft); word(SuperSpecialCommand, 7);
  run(NativeUpdatePose);
  printf("Native queued release pose: Y=%04X pose=%04X radius=%u\n", readword(SamusY), readword(Pose), readword(SamusRadiusY));
  run(NativeSetRadius);
  word(DisplacementFraction, 0x1c00); word(DisplacementWhole, 0);
  run(NativeMoveVertical);
  word(SuperSpecialPose, 0xffff); word(SpecialPose, 0xffff);
  word(ProspectivePose, 0xa5); word(PoseCommand, 5);
  run(NativeUpdatePose);
  run(NativeSetRadius);
  printf("Native landing pose: Y=%04X pose=%04X radius=%u\n", readword(SamusY), readword(Pose), readword(SamusRadiusY));
  if (readword(SamusY) != 0x48d || readword(Pose) != 0xa5 || readword(SamusRadiusY) != 21) return 1;
  FILE *vectors = fopen("csharp/test-fixtures/issue-350-grounded-grapple-floor-clip/post-grapple-vectors.csv", "wb");
  if (!vectors) Die("Cannot create post-grapple reference vectors\n");
  fprintf(vectors, "type,bts,x,y,radius,ceiling,resultY\n");
  const unsigned low[] = { 0, 7, 8, 15 };
  for (unsigned type = 0; type < 16; type++)
  for (unsigned bts = 0; bts < (type == 1 ? 256 : 1); bts++) {
    if (bts & 0x20) continue; /* Unused bit does not select a new slope. */
    for (unsigned xi = 0; xi < 4; xi++)
    for (unsigned yi = 0; yi < 4; yi++)
    for (unsigned ceiling = 0; ceiling < 2; ceiling++) {
      memset(ram, 0, sizeof(ram));
      unsigned x = 80 + low[xi], y = 64 + low[yi];
      word(RoomWidth, 12); word(SamusX, x); word(SamusY, y);
      word(SamusRadiusX, 5); word(SamusRadiusY, 17);
      for (unsigned row = 2; row <= 5; row++)
      for (unsigned col = 3; col <= 7; col++) {
        if (row < 4 && !ceiling) continue;
        unsigned index = row * 12 + col;
        word(LevelWords + 2 * index, type << 12); ram[BlockBts + index] = bts;
      }
      run(NativePostGrappleCollision);
      fprintf(vectors, "%u,%u,%u,%u,17,%u,%u\n", type, bts, x, y, ceiling, readword(SamusY));
    }
  }
  fclose(vectors);
  for (int left = 0; left < 2; left++) {
    memset(ram, 0, sizeof(ram));
    word(Shot, 0x40); word(Jump, 0x80); word(Dash, 0x8000); word(Cancel, 0x4000);
    word(Select, 0x2000); word(AimDown, 0x20); word(AimUp, 0x10);
    word(Pose, left ? ReleasedLeft : ReleasedRight);
    ram[MovementType] = NormalJumping;
    word(HeldInput, left ? 0x210 : 0x110);
    word(ProspectivePose, 0xffff);
    Cpu *cpu = cpu_init(NULL, 0);
    cpu->pc = NativePoseInput & 0xffff; cpu->k = cpu->db = NativePoseInput >> 16;
    cpu->sp = cpu->spBreakpoint = 0x1ff;
    returned = false;
    for (int i = 0; i < 10000 && !returned; i++) cpu_runOpcode(cpu);
    unsigned result = ram[ProspectivePose] | ram[ProspectivePose + 1] << 8;
    printf("Native released-%s held aim: prospective pose=%04X\n", left ? "left" : "right", result);
    if (!returned || result != (left ? AimLeft : AimRight)) return 1;
    cpu_free(cpu);
  }
  return 0;
}

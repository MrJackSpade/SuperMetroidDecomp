/* #425: original, unpatched 65816 movement on a constructed descending slope.
 * No SDL, window, save file, or translated gameplay is used by this runner. */
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <stdint.h>
#include <string.h>
#include "../../../upstream-sm/src/snes/cpu.h"
#include "../../../upstream-sm/src/snes/snes.h"
#include "../../../upstream-sm/src/variables.h"
#include "fixture.h"

uint8 g_ram[0x20000];
static uint8 rom[0x300000];
static bool returned;
static unsigned multiplicand, dividend, quotient, remainder;
void Die(const char *message) { fprintf(stderr, "%s\n", message); exit(2); }
int CpuOpcodeHook(uint32 address) { fprintf(stderr, "Unexpected hook %06X\n", address); exit(2); }
bool HookedFunctionRts(int is_long) { (void)is_long; returned = true; return true; }
uint8 snes_cpuRead(Snes *snes, uint32 address) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) return g_ram[address - 0x7e0000];
  if ((bank & 0x7f) < 0x40) {
    if (offset < 0x2000) return g_ram[offset];
    if (offset == 0x4214) return quotient;
    if (offset == 0x4215) return quotient >> 8;
    if (offset == 0x4216) return remainder;
    if (offset == 0x4217) return remainder >> 8;
  }
  unsigned index = ((bank & 0x7f) << 15) | (offset & 0x7fff);
  if (offset >= 0x8000 && index < sizeof(rom)) return rom[index];
  fprintf(stderr, "Unmapped read %06X\n", address); exit(2);
}
void snes_cpuWrite(Snes *snes, uint32 address, uint8 value) {
  (void)snes;
  unsigned bank = address >> 16, offset = address & 0xffff;
  if (bank == 0x7e || bank == 0x7f) { g_ram[address - 0x7e0000] = value; return; }
  if ((bank & 0x7f) < 0x40) {
    if (offset < 0x2000) { g_ram[offset] = value; return; }
    if (offset == 0x4202) { multiplicand = value; return; }
    if (offset == 0x4203) { remainder = multiplicand * value; return; }
    if (offset == 0x4204) { dividend = (dividend & 0xff00) | value; return; }
    if (offset == 0x4205) { dividend = (dividend & 0xff) | value << 8; return; }
    if (offset == 0x4206) {
      quotient = value ? dividend / value : 0xffff;
      remainder = value ? dividend % value : dividend; return;
    }
  }
  fprintf(stderr, "Unmapped write %06X\n", address); exit(2);
}
static void run(unsigned address) {
  Cpu *cpu = cpu_init(NULL, 0);
  cpu->pc = address; cpu->k = cpu->db = address >> 16;
  cpu->sp = cpu->spBreakpoint = 0x1ff0;
  returned = false;
  for (int n = 0; n < 1000000 && !returned; n++) cpu_runOpcode(cpu);
  cpu_free(cpu);
  if (!returned) Die("Movement exceeded instruction budget");
}
int main(int argc, char **argv) {
  if (argc != 2) { fprintf(stderr, "Usage: audit unheadered-retail-ROM > trace.csv\n"); return 2; }
  FILE *file = fopen(argv[1], "rb");
  if (!file || fread(rom, 1, sizeof(rom), file) != sizeof(rom) || fgetc(file) != EOF)
    Die("Expected unheadered 3 MiB ROM");
  fclose(file);
  printf("left,height,up,phase,frame,input,x,y,pose,movement,anim,timer,base,extra,accel,yspeed,ydir,incomingYSpeed\n");
  for (int left = 0; left < 2; left++)
  for (int height = 201; height <= 203; height++)
  for (int up = 19; up <= 21; up++)
  for (int phase = 0; phase <= 1; phase++) {
    memset(g_ram, 0, sizeof(g_ram));
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144 * 80 * 2;
    interactive_enemy_indexes[0] = 0xffff;
    /* Flat landing followed by continuous 45-degree descent. Both direction
       cases include adjacent X pixels; do not assume native edge symmetry. */
    for (int x = 0; x < 144; x++) {
      int distance = left ? 59 - x : x - 68;
      int floor = 16 + (distance >= 0 ? distance : 0);
      for (int y = floor; y < 80; y++) {
        level_data[y * 144 + x] = distance >= 0 && y == floor ? 0x1000 : 0x8000;
        BTS[y * 144 + x] = distance >= 0 && y == floor ? (left ? 0x12 : 0x52) : 0;
      }
    }
    fx_y_pos = lava_acid_y_pos = 0xffff;
    equipped_items = collected_items = 4; game_state = 8;
    samus_health = samus_max_health = 99; enable_horiz_slope_coll = 3;
    samus_x_pos = samus_prev_x_pos = 1024 + (left ? -phase : phase);
    samus_y_pos = samus_prev_y_pos = height;
    samus_pose = samus_prev_pose = left ? 0x32 : 0x31;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = 8; samus_y_dir = 2;
    run(RefreshRadius);
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    uint16 previous = 0;
    for (int frame = 0; frame < 150; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2;
      uint16 input = frame == up ? 0x800 : 0;
      if (frame >= 60) input |= left ? 0x200 : 0x100;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      run(InputPhase); run(InteractionPhase); samus_contact_damage_index = 0;
      uint32 incoming = (uint32)samus_y_speed << 16 | samus_y_subspeed;
      run(0x900000 | samus_movement_handler);
      unsigned stages[] = {AnimationPhase,TransitionPhase,CollisionPosePhase,ApplyPosePhase,
        PoseHistoryPhase,HurtPhase,CollisionPhase};
      for (int i = 0; i < 7; i++) run(stages[i]);
      printf("%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%02X,%02X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X%04X,%04X,%08X\n",
        left,height,up,phase,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_anim_frame,samus_anim_frame_timer,
        samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,
        samus_x_accel_mode,samus_y_speed,samus_y_subspeed,samus_y_dir,incoming);
    }
  }
  return 0;
}

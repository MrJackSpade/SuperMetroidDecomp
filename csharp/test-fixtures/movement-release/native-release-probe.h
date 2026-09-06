// Headless cartridge-CPU experiment for #312/#313. This constructs an empty room
// with a flat floor; it contains no player recording, SRAM, or cartridge bytes.
// Include from sm_rtl.c and dispatch DiagnosticMovementRelease before SDL starts.
#include "snes/cart.h"
#ifdef _WIN32
// Avoid windows.h's ScreenOff/IDA macro collisions in the translated game unit.
__declspec(dllimport) unsigned int __stdcall SetErrorMode(unsigned int mode);
enum { ProbeNoCriticalErrorDialog = 1, ProbeNoFaultDialog = 2 };
#endif
int DiagnosticMovementRelease(const char *rom) {
#ifdef _WIN32
  SetErrorMode(ProbeNoCriticalErrorDialog | ProbeNoFaultDialog);
#endif
  if (!SnesInit(rom)) return 2;
  // SnesInit patches carry instructions for the C/CPU comparison harness. Those
  // patches are inappropriate for a cartridge golden trace: restore the bytes.
  size_t rom_length = 0;
  uint8 *retail = ReadWholeFile(rom, &rom_length);
  size_t header = rom_length & 0x7fff;
  if (!retail || (header != 0 && header != 512) ||
      rom_length - header > g_snes->cart->romSize) {
    free(retail);
    fprintf(stderr, "Cartridge size mismatch in movement probe.\n");
    return 3;
  }
  memcpy(g_snes->cart->rom, retail + header, rom_length - header);
  free(retail);
  for (int water = 0; water < 2; water++) {
    cpu_reset(g_snes->cpu);
    memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false;
    g_snes->cpu->sp = 0x1ff0;
    g_snes->cpu->dp = 0;
    room_width_in_blocks = 16;
    room_height_in_blocks = 32;
    interactive_enemy_indexes[0] = 0xffff;
    for (int i = 256; i < 272; i++) level_data[i] = 0x8000;
    fx_y_pos = water ? 8 : 0xffff;
    lava_acid_y_pos = 0xffff;
    fx_liquid_options = 0x80;
    fx_type = water ? 6 : 0;
    liquid_physics_type = water;
    samus_x_pos = 200;
    samus_y_pos = 235;
    samus_x_radius = 5;
    samus_y_radius = 21;
    samus_pose = samus_prev_pose = 10; // Running left.
    samus_pose_x_dir = samus_prev_pose_x_dir = 4;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 1;
    samus_x_base_speed = 2;
    samus_x_base_subspeed = 0xc000; // Full running base speed, no extra run speed.
    samus_x_speed_table_pointer = 0x9f55; // Retail normal table, not preceding grapple entry.
    samus_input_handler = 0xe913;
    samus_health = 99;
    samus_anim_frame_timer = 5;
    for (int frame = 0; frame < 16; frame++) {
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = joypad1_newkeys = 0;
      // Retail alpha input, beta movement/animation, then collision and pose
      // transitions. Execute the actual 65816 instructions, not translated C.
      RunAsmCode(0x918000, 0, 0, 0, 0);
      RunAsmCode(0x90a337, 0, 0, 0, 0);
      printf("MOVE water=%d x=%04X.%04X base=%04X.%04X extra=%04X.%04X external=%04X.%04X table=%04X collision=%d\n", water,
        samus_x_pos, samus_x_subpos, samus_x_base_speed, samus_x_base_subspeed,
        samus_x_extra_run_speed, samus_x_extra_run_subspeed, extra_samus_x_displacement,
        extra_samus_x_subdisplacement, samus_x_speed_table_pointer, samus_collision_flag);
      RunAsmCode(0x908000, 0, 0, 0, 0);
      RunAsmCode(0x91e8b6, 0, 0, 0, 0);
      RunAsmCode(0x91eb88, 0, 0, 0, 0);
      printf("RELEASE water=%d frame=%d x=%04X.%04X speed=%04X.%04X mode=%d pose=%04X\n",
        water, frame, samus_x_pos, samus_x_subpos, samus_x_base_speed,
        samus_x_base_subspeed, samus_x_accel_mode, samus_pose);
    }
  }
  return 0;
}

#include "native-bounded-cpu.h"

// Fixed recorded inputs, not the managed search policy. No ROM or state export.
int DiagnosticBombTraversal(const char *rom, const char *inputsPath, const char *output) {
  uint16 inputs[600]; char line[128];
  FILE *in = fopen(inputsPath, "r"); if (!in) return 3;
  if (!fgets(line, sizeof(line), in)) { fclose(in); return 3; }
  for (int frame = 0; frame < 600; frame++) {
    unsigned index, input;
    if (!fgets(line, sizeof(line), in) || sscanf(line, "%u,%x", &index, &input) != 2 || index != frame || input > 0xffff) {
      fclose(in); return 3;
    }
    inputs[frame] = input;
  }
  if (fgets(line, sizeof(line), in)) { fclose(in); return 3; }
  fclose(in);
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "left,frame,input,x,y,pose,bombDirection,ySpeed,yDirection,baseSpeed,bombCount,inputLocked,bombStarting,bombActive");
  for (int b = 0; b < 5; b++) fprintf(f, ",b%dType,b%dFuse,b%dX,b%dY,b%dList,b%dTimer,b%dSprite",b,b,b,b,b,b,b);
  fprintf(f, "\n");
  for (int left = 0; left < 2; left++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks = 144; room_height_in_blocks = 80;
    room_width_in_scrolls = 9; room_height_in_scrolls = 5; room_size_in_blocks = 144*80*2;
    interactive_enemy_indexes[0] = 0xffff;
    for (int x = 0; x < 144; x++) level_data[12*144+x] = level_data[16*144+x] = 0x8000;
    fx_y_pos = lava_acid_y_pos = 0xffff; equipped_items = 0x1004; samus_health = 99;
    samus_x_pos = samus_prev_x_pos = 128; samus_y_pos = samus_prev_y_pos = 249;
    samus_pose = samus_prev_pose = left ? 0x41 : 0x1d;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_movement_type = samus_prev_movement_type = samus_prev_movement_type2 = 4;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    uint16 previous = 0;
    for (int frame = 0; frame < 600; frame++) {
      uint16 input = inputs[frame];
      if (left) input = (input & ~0x300) | ((input & 0x100) << 1) | ((input & 0x200) >> 1);
      samus_new_pose = samus_new_pose_interrupted = samus_new_pose_transitional = 0xffff;
      samus_momentum_routine_index = samus_special_transgfx_index = samus_hurt_switch_index = 0;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      ProbeRunBounded(0x90ec22); ProbeRunBounded(0x90e90f); ProbeRunBounded(0x909c5b);
      ProbeRunBounded(0x90ac1c); ProbeRunBounded(0x90bf9d);
      ProbeRunBounded(0x90aece); ProbeRunBounded(0xa09785);
      samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      ProbeRunBounded(0x908000); ProbeRunBounded(0x90dde9);
      ProbeRunBounded(0x91e8b6); ProbeRunBounded(0x91eb88);
      ProbeRunBounded(0x90eab3); ProbeRunBounded(0x90e9ce); ProbeRunBounded(0xa09169);
      fprintf(f, "%d,%d,%04X,%04X%04X,%04X%04X,%02X,%04X,%04X%04X,%04X,%04X%04X,%04X,%d,%d,%d",
        left,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,samus_pose,bomb_jump_dir,
        samus_y_speed,samus_y_subspeed,samus_y_dir,samus_x_base_speed,samus_x_base_subspeed,bomb_counter,
        samus_input_handler == 0xe90e,samus_movement_handler == 0xe025,samus_movement_handler == 0xe032);
      for(int b=5;b<10;b++) fprintf(f,",%04X,%04X,%04X,%04X,%04X,%04X,%04X",projectile_type[b],projectile_variables[b],projectile_x_pos[b],projectile_y_pos[b],projectile_bomb_instruction_ptr[b],projectile_bomb_instruction_timers[b],projectile_spritemap_pointers[b]);
      fprintf(f,"\n");
    }
  }
  fclose(f); return 0;
}

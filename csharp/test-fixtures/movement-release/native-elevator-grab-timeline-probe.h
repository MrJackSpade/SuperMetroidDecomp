#include "native-bounded-cpu.h"

// #454: actual input/actor/beta sequence through the first activation or 24 frames.
int DiagnosticElevatorGrabTimeline(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f, "up,parity,left,scenario,start,delay,frame,input,x,y,pose,movement,base,extra,status,contact\n");
  for(int up=0;up<2;up++) for(int parity=0;parity<2;parity++)
  for(int left=0;left<2;left++) for(int scenario=0;scenario<3;scenario++)
  for(int start=120;start<152;start++) for(int delay=7;delay<=13;delay++) {
    cpu_reset(g_snes->cpu); memset(g_ram, 0, sizeof(g_ram));
    g_snes->cpu->e = false; g_snes->cpu->sp = 0x1ff0; g_snes->cpu->dp = 0;
    room_width_in_blocks=64; room_height_in_blocks=192; room_size_in_blocks=64*192*2;
    room_width_in_scrolls=4; room_height_in_scrolls=12; door_list_pointer=0x9b00;
    interactive_enemy_indexes[0]=0xffff;
    for(int column=0;column<64;column++) level_data[16*64+column]=0x8000;
    level_data[16*64+8]=0x9000; BTS[16*64+8]=9;
    fx_y_pos=lava_acid_y_pos=0xffff; fx_liquid_options=0x80;
    equipped_items=4; enable_horiz_slope_coll=3;
    samus_health = samus_max_health = 99;
    samus_x_pos = samus_prev_x_pos = start; samus_y_pos = samus_prev_y_pos = scenario==1?219:235;
    samus_pose = samus_prev_pose = left ? 2 : 1;
    samus_pose_x_dir = samus_prev_pose_x_dir = left ? 4 : 8;
    samus_anim_frame_timer = 1; samus_x_speed_table_pointer = 0x9f55;
    samus_input_handler = 0xe913; samus_movement_handler = 0xa337; grapple_beam_function = 0xc4f0;
    button_config_run_b = 0x8000; button_config_jump_a = 0x80; button_config_shoot_x = 0x40;
    button_config_aim_up_R = 0x10; button_config_aim_down_L = 0x20;
    button_config_itemcancel_y = 0x4000; button_config_itemswitch = 0x2000;
    Enemy_Elevator *e=Get_Elevator(0); e->base.x_pos=136; e->base.y_pos=256;
    e->elevat_parameter_1=up; ProbeRunBounded(0xa394e6);
    uint16 previous = 0;
    for (int frame = 0; frame < 24; frame++) {
      nmi_frame_counter_word = nmi_frame_counter_byte = frame + 2 + parity;
      uint16 input = 0x10;
      if(scenario==2 && frame<8) input|=left?0x200:0x100;
      if(frame>=delay && (frame-delay)%4==0) input|=up?0x800:0x400;
      joypad1_lastkeys = input; joypad1_newkeys = input & ~previous; previous = input;
      ProbeRunBounded(0x90e695); ProbeRunBounded(0xa09785);
      ProbeRunBounded(0xa3952a); samus_contact_damage_index = 0;
      ProbeRunBounded(0x900000 | samus_movement_handler);
      uint32 stages[] = {0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
      for (int stage=0;stage<7;stage++) {
        // Command seven replaces beta with E8EC: movement/minimap/animation,
        // without the ordinary pose/projectile transition stages.
        if(elevator_status && stage!=0) continue;
        ProbeRunBounded(stages[stage]);
      }
      fprintf(f,"%d,%d,%d,%d,%d,%d,%d,%04X,%04X%04X,%04X%04X,%04X,%04X,%04X%04X,%04X%04X,%04X,%04X\n",
        up,parity,left,scenario,start,delay,frame,input,samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,
        samus_pose,samus_movement_type,samus_x_base_speed,samus_x_base_subspeed,samus_x_extra_run_speed,samus_x_extra_run_subspeed,elevator_status,elevator_flags);
      if(elevator_status) break;
    }
  }
  fclose(f); return 0;
}

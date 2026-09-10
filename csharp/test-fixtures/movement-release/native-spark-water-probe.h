#include "native-bounded-cpu.h"

// Fully submerged water travel in all three directions, with/without Gravity Suit.
// Health starts at 99; capture includes the terminating room collision, cheats off.
int DiagnosticSparkWater(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"direction,left,gravity,frame,input,pose,shine,windup,x,y,health,crash\n");
  for(int medium=0;medium<3;medium++) for(int left=0;left<2;left++) for(int offset=0;offset<2;offset++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    room_width_in_blocks=16; room_height_in_blocks=32;
    room_width_in_scrolls=1; room_height_in_scrolls=2; room_size_in_blocks=1024;
    interactive_enemy_indexes[0]=0xffff;
    for(int x=0;x<16;x++) level_data[x]=level_data[16*16+x]=0x8000;
    for(int y=0;y<16;y++) level_data[y*16]=level_data[y*16+15]=0x8000;
    fx_y_pos=8; lava_acid_y_pos=0xffff;
    fx_liquid_options=0x80; fx_type=6; liquid_physics_type=1;
    equipped_items=0x2000|(offset?0x20:0); enable_horiz_slope_coll=3;
    samus_health=samus_max_health=99;
    samus_x_pos=samus_prev_x_pos=128; samus_y_pos=samus_prev_y_pos=235;
    samus_pose=samus_prev_pose=left?2:1; samus_pose_x_dir=samus_prev_pose_x_dir=left?4:8;
    samus_x_radius=5; samus_y_radius=21;
    samus_anim_frame_timer=1; samus_x_speed_table_pointer=0x9f55;
    samus_input_handler=0xe913; samus_movement_handler=0xa337; grapple_beam_function=0xc4f0;
    button_config_run_b=0x8000; button_config_jump_a=0x80; button_config_shoot_x=0x40;
    button_config_aim_up_R=0x10; button_config_aim_down_L=0x20;
    button_config_itemcancel_y=0x4000; button_config_itemswitch=0x2000;
    uint16 previous=0;
    for(int frame=0;frame<160;frame++) {
      if(frame==20) { speed_boost_counter=0x400; ProbeRunBounded(0x91f7b0); }
      nmi_frame_counter_word=nmi_frame_counter_byte=frame+2;
      uint16 input=frame>=24?0x80:0;
      if(frame>=28) { if(medium==0) input|=left?0x200:0x100; else if(medium==2) input|=0x10; }
      joypad1_lastkeys=input; joypad1_newkeys=input&~previous; previous=input;
      ProbeRunBounded(0x90e695);
      ProbeRunBounded(0xa09785); samus_contact_damage_index=0;
      ProbeRunBounded(0x900000|samus_movement_handler);
      uint32 stages[]={0x908000,0x90dde9,0x91e8b6,0x91eb88,0x90eab3,0x90e9ce,0xa09169};
      for(int stage=0;stage<7;stage++) ProbeRunBounded(stages[stage]);
      if(timer_for_shine_timer==1) ProbeRunBounded(0x91dac7);
      else if(timer_for_shine_timer==6) ProbeRunBounded(0x91db3a);
      fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X%04X,%04X%04X,%04X,%d\n",
        medium,left,offset,frame,input,samus_pose,samus_shine_timer,timer_for_shinesparks_startstop,
        samus_x_pos,samus_x_subpos,samus_y_pos,samus_y_subpos,samus_health,samus_movement_handler==0xd346);
      if(samus_movement_handler==0xd346) break;
    }
  }
  fclose(f); return 0;
}

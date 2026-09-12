#include "native-bounded-cpu.h"

// #409: controller-fired diagonal beams; observe linear-room shot-block writes.
int DiagnosticWrapShot(const char *rom, const char *output) {
  int status = ProbeLoadRetailMovementRom(rom); if (status) return status;
  FILE *f = fopen(output, "wx"); if (!f) return 4;
  fprintf(f,"left,beam,frame,x,subx,y,suby,type,list,changed\n");
  for (int left=0;left<2;left++) for(int beam=0;beam<3;beam++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false;g_snes->cpu->sp=0x1ff0;g_snes->cpu->dp=0;
    room_width_in_blocks=64;room_height_in_blocks=128;
    room_width_in_scrolls=4;room_height_in_scrolls=8;room_size_in_blocks=64*128*2;
    // Remote target column is reached only by linear index aliasing, not by
    // the projectile's world-space hitbox. A solid local edge blocks non-Wave.
    for(int row=0;row<128;row++) {
      level_data[row*64+(left?63:0)]=0x4000;
      level_data[row*64+(left?0:63)]=0x8000;
    }
    samus_pose=left?8:7;samus_pose_x_dir=left?4:8;
    samus_x_pos=left?32:992;samus_y_pos=128;
    equipped_beams=beam==0?0:beam==1?1:5;
    button_config_shoot_x=0x40;layer1_x_pos=left?0:768;
    for(int frame=0;frame<40;frame++) {
      joypad1_lastkeys=joypad1_newkeys=frame==0?0x40:0;
      ProbeRunBounded(0x90b80d);
      projectile_index=0;
      if(projectile_bomb_instruction_ptr[0]) ProbeRunBounded(0x90aece);
      fprintf(f,"%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,",left,beam,frame,
        projectile_x_pos[0],projectile_bomb_x_subpos[0],projectile_y_pos[0],projectile_bomb_y_subpos[0],projectile_type[0],projectile_bomb_instruction_ptr[0]);
      for(int row=0;row<128;row++) {
        int index=row*64+(left?63:0);
        if(level_data[index]!=0x4000) fprintf(f,"%04X:%04X;",index,level_data[index]);
      }
      fprintf(f,"\n");
    }
  }
  fclose(f);return 0;
}

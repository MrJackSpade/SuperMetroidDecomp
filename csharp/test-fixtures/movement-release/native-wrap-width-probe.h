#include "native-bounded-cpu.h"

// #409: geometry-equivalent door caps at the three established retail launch sites.
int DiagnosticWrapWidth(const char *rom,const char *output) {
  int status=ProbeLoadRetailMovementRom(rom);if(status)return status;
  FILE *f=fopen(output,"wx");if(!f)return 4;
  fprintf(f,"room,beam,offset,frame,x,subx,y,suby,type,list,rx,ry,target\n");
  const int widths[]={144,128,64}, heights[]={80,16,192};
  const int originsX[]={2260,2004,38}, originsY[]={546,34,1606}, targets[]={0x1561,0x301,0x2981};
  for(int room=0;room<3;room++)for(int beam=0;beam<3;beam++)for(int offset=-1;offset<=1;offset++) {
    cpu_reset(g_snes->cpu);memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false;g_snes->cpu->sp=0x1ff0;g_snes->cpu->dp=0;
    int width=widths[room],height=heights[room],target=targets[room];bool left=room==2;
    room_width_in_blocks=width;room_height_in_blocks=height;
    room_width_in_scrolls=width/16;room_height_in_scrolls=height/16;room_size_in_blocks=width*height*2;
    for(int row=0;row<height;row++)level_data[row*width+(left?0:width-1)]=0x8000;
    level_data[target]=0xc40c;BTS[target]=0x41;
    for(int row=1;row<4;row++){level_data[target+row*width]=0xd40c;BTS[target+row*width]=0xff;}
    samus_pose=left?8:7;samus_pose_x_dir=left?4:8;
    samus_x_pos=originsX[room]+offset;samus_y_pos=originsY[room];
    equipped_beams=beam==0?0:beam==1?1:left?5:9;
    button_config_shoot_x=0x40;layer1_x_pos=left?0:width*16-256;layer1_y_pos=originsY[room]>128?originsY[room]-128:0;
    for(int frame=0;frame<40;frame++){
      joypad1_lastkeys=joypad1_newkeys=frame==0?0x40:0;
      ProbeRunBounded(0x90b80d);projectile_index=0;
      if(projectile_bomb_instruction_ptr[0])ProbeRunBounded(0x90aece);
      fprintf(f,"%d,%d,%d,%d,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",room,beam,offset,frame,
        projectile_x_pos[0],projectile_bomb_x_subpos[0],projectile_y_pos[0],projectile_bomb_y_subpos[0],
        projectile_type[0],projectile_bomb_instruction_ptr[0],projectile_x_radius[0],projectile_y_radius[0],level_data[target]);
    }
  }
  fclose(f);return 0;
}

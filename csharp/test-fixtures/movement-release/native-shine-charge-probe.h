#include "native-bounded-cpu.h"

// Exercise the complete native palette dispatcher, including charged-shot carry.
// Seed post-shot glow at three distinct times; firing/projectile motion is outside
// this fixture, while the charge-glow and stored-shine interaction is not mocked.
int DiagnosticShineCharge(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"shots,suit,frame,shine,glow,kind,paletteframe,palette\n");
  for(int shots=0;shots<=3;shots++) for(int suit=0;suit<3;suit++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    speed_boost_counter=0x400; samus_suit_palette_index=suit*2;
    grapple_beam_function=0xc4f0;
    ProbeRunBounded(0x91f7b0);
    for(int frame=0;frame<190;frame++) {
      for(int shot=0;shot<shots;shot++) if(frame==20+50*shot) charged_shot_glow_timer=4;
      ProbeRunBounded(0x91d6f7);
      fprintf(f,"%d,%d,%d,%04X,%04X,%04X,%04X,",shots,suit,frame,
        samus_shine_timer,charged_shot_glow_timer,timer_for_shine_timer,special_samus_palette_frame);
      for(int i=0;i<16;i++) fprintf(f,"%04X",palette_buffer[192+i]);
      fprintf(f,"\n");
    }
  }
  fclose(f); return 0;
}

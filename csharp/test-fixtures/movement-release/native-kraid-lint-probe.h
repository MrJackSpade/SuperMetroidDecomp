#include "native-bounded-cpu.h"

// Original CPU, not translated C. Seed only the documented support boundary;
// execute the entire native firing routine, including its real A0 support call.
static void LintWrite(int address, uint16 value) { *(uint16 *)(g_ram+address)=value; }
static uint16 LintRead(int address) { return *(uint16 *)(g_ram+address); }
int DiagnosticKraidLint(const char *rom, const char *output) {
  int status=ProbeLoadRetailMovementRom(rom); if(status) return status;
  FILE *f=fopen(output,"wx"); if(!f) return 4;
  fprintf(f,"slot,startx,xcase,ycase,initial,carry,x,sub,properties,function,timer,next\n");
  int starts[]={128,59,34}, xs[]={-29,-28,0,28,29}, ys[]={-33,-32,-31,-3,-2};
  uint32 carries[]={0,0xfff0c000,0x00014000,0x7fff0000};
  for(int slot=2;slot<=4;slot++) for(int sx=0;sx<3;sx++)
  for(int xi=0;xi<5;xi++) for(int yi=0;yi<5;yi++) for(int ci=0;ci<4;ci++) {
    cpu_reset(g_snes->cpu); memset(g_ram,0,sizeof(g_ram));
    g_snes->cpu->e=false; g_snes->cpu->sp=0x1ff0; g_snes->cpu->dp=0;
    int offset=slot*64;
    LintWrite(0xf7a+offset,starts[sx]); // Enemy X position.
    LintWrite(0xf7e+offset,200);        // Enemy Y position.
    LintWrite(0xf82+offset,24); LintWrite(0xf84+offset,8);
    LintWrite(0xfa8+offset,0xb89b);
    samus_x_radius=5; samus_y_radius=21;
    samus_x_pos=starts[sx]-4+xs[xi]; samus_y_pos=200+ys[yi];
    extra_samus_x_displacement=carries[ci]>>16;
    extra_samus_x_subdisplacement=carries[ci];
    ProbeRunBoundedRegisters(0xa7b89b,0,offset,0);
    fprintf(f,"%d,%d,%d,%d,%08X,%04X%04X,%04X,%04X,%04X,%04X,%04X,%04X\n",
      slot,starts[sx],xi,yi,carries[ci],extra_samus_x_displacement,
      extra_samus_x_subdisplacement,LintRead(0xf7a+offset),LintRead(0xf7c+offset),
      LintRead(0xf86+offset),LintRead(0xfa8+offset),LintRead(0xfb2+offset),
      LintRead(0x7800+offset));
  }
  fclose(f); return 0;
}

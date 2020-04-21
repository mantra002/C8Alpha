using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace c8alpha
{
    class Program
    {
        static void Main(string[] args)
        {
            Chip8 c8 = new Chip8();
            byte[] romData = File.ReadAllBytes("H:\\source_code\\roms\\test_opcode.ch8");
            c8.LoadRAM(romData);
            while (true)
            {
                c8.ClockCycle();
                c8.DumpRegisters();
                DrawFrameBuffer(c8.FrameBuffer);
                //Console.ReadKey();
            }
        }

        static void DrawFrameBuffer(bool[,] frameBuffer)
        {
            int x = 64;
            int y = 32;
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < y; j++)
            {
                for (int i = 0; i < x; i++)
                {
                    if (frameBuffer[i, j] == true) sb.Append("*");
                    else sb.Append(" ");
                }
                sb.Append('\n');
            }
            Console.WriteLine(sb.ToString());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TempoWrite
{
    class TempoWrite
    {
        static void Main(string[] args)
        {
            //Not used in the editor, this is the source for tempowrite.exe in the bcstm generator zip
            int lstart = 0;
            int lend = 0;
            float tempoChange = 15;
            if (args.Length == 1) tempoChange = float.Parse(args[0]);
            else if (args.Length == 2) { lstart = int.Parse(args[0]); tempoChange = float.Parse(args[1]); }
            else if (args.Length == 3) { lstart = int.Parse(args[0]); int.Parse(args[1]); tempoChange = float.Parse(args[2]); }
            Double sampleAdd = ((lstart % 14336 > 0) ? 1 : 0) * 14336 - lstart % 14336;
            Double fixedlstart = lstart + sampleAdd;
            Double fixedlend = lend + sampleAdd;
            Double flstart = loopStartTempoAdjust(lstart, tempoChange);
            Double fsampleAdd = ((flstart % 14336 > 0) ? 1 : 0) * 14336 - flstart % 14336;
            flstart += fsampleAdd;
            Double flend = loopEndTempoAdjust(lend, tempoChange) + fsampleAdd;
            Console.Write("set brstmlstart=" + (fixedlstart).ToString() + "\r\nset brstmlend=" + (fixedlend).ToString() + "\r\nset flstart=" + (flstart).ToString() + "\r\nset flend=" + (flend).ToString() + "\r\nREM C# TempoWrite by exelix11\r\nREMThanks to the guy who made the original tempoWrite python source :D");
        }

        static int loopEndTempoAdjust(int loopEnd, float tempoCh)
        {
            Double a = 9.99972502058E-001;
            Double b = -9.99729521293E-003;
            Double c = 9.95326197331E-005;
            Double d = -9.62154038913E-007;
            Double e = 8.35056874072E-009;
            Double f = -5.65998662116E-011;
            Double g = 2.47431258370E-013;
            Double h = -4.99190378557E-016;
            Double y = a + b * tempoCh + c * Math.Pow(tempoCh, 2) + d * Math.Pow(tempoCh, 3) + e * Math.Pow(tempoCh, 4) + f * Math.Pow(tempoCh, 5) + g * Math.Pow(tempoCh, 6) + h * Math.Pow(tempoCh, 7);
            Double adjustedLoopEnd = loopEnd * y;
            return (int)(adjustedLoopEnd);
        }

        static int loopStartTempoAdjust(int loopStart, float tempoCh)
        {
            Double a = 9.99934013822E-001;
            Double b = -9.99688418675E-003;
            Double c = 9.95282911409E-005;
            Double d = -9.62195332321E-007;
            Double e = 8.35455483476E-009;
            Double f = -5.66841846351E-011;
            Double g = 2.48190851368E-013;
            Double h = -5.01714192476E-016;
            Double y = a + b * tempoCh + c * Math.Pow(tempoCh, 2) + d * Math.Pow(tempoCh, 3) + e * Math.Pow(tempoCh, 4) + f * Math.Pow(tempoCh, 5) + g * Math.Pow(tempoCh, 6) + h * Math.Pow(tempoCh, 7);
            Double adjustedLoopStart = loopStart * y;
            return (int)(adjustedLoopStart);
        }
    }
}

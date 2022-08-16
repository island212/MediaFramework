using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFramework
{
    public struct IntRational
    {
        public int Num, Denom;

        public IntRational(int num, int denom)
        {
            Num = num;
            Denom = denom;
        }
    }

    public struct UIntRational
    {
        public uint Num, Denom;

        public UIntRational(uint num, uint denom)
        {
            Num = num;
            Denom = denom;
        }
    }
}

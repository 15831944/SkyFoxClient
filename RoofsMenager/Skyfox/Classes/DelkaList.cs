using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyfox.Classes
{
    class DelkaList : IEqualityComparer<DelkaList>
    {
        public string full;
        public string color;
        public string x;
        public string y;
        public string z;
        public string value;

        public bool Equals(DelkaList b1, DelkaList b2)
        {
            if (b1.value == b2.value && b1.x==b2.x && b1.y == b2.y && b1.z == b2.z)
                return true;
            else
                return false;
        }

        public int GetHashCode(DelkaList bx)
        {
            return bx.full.GetHashCode();
        }
        public DelkaList(string fl,string cl,string X,string Y, string Z,string vl)
        {
            full = fl;
            color = cl;
            x = X;
            y = Y;
            z = Z;
            value = vl;
        }
        public DelkaList()
        {
            full = "";
            color = "";
            x = "";
            y = "";
            z = "";
            value = "";
        }
        public static bool operator !=(DelkaList c1, DelkaList c2)
        {
            if (c1.full != c2.full)
                return true;
            else
                return false;
        }
        public static bool operator ==(DelkaList c1, DelkaList c2)
        {
            if (c1.full == c2.full)
                return true;
            else
                return false;
        }
    }
}

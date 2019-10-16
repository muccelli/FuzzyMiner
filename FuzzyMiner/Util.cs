using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    class Util
    {
        public static int Similarity(String s, String t)
        {

            int n = s.Length; // length of s
            int m = t.Length; // length of t

            if (n == 0)
            {
                return m;
            }
            else if (m == 0)
            {
                return n;
            }
            int MAX_N = m + n;

            short[] swap; // placeholder to assist in swapping p and d

            // indexes into strings s and t
            short i; // iterates through s
            short j; // iterates through t

            Object t_j = null; // jth object of t

            short cost; // cost

            short[] d = new short[MAX_N + 1];
            short[] p = new short[MAX_N + 1];

            for (i = 0; i <= n; i++)
            {
                p[i] = i;
            }

            for (j = 1; j <= m; j++)
            {
                t_j = t[j - 1];
                d[0] = j;

                Object s_i = null; // ith object of s
                for (i = 1; i <= n; i++)
                {
                    s_i = s[i - 1];
                    cost = s_i.Equals(t_j) ? (short)0 : (short)1;
                    // minimum of cell to the left+1, to the top+1, diagonally left and up +cost
                    d[i] = Minimum(d[i - 1] + 1, p[i] + 1, p[i - 1] + cost);
                }

                // copy current distance counts to 'previous row' distance counts
                swap = p;
                p = d;
                d = swap;
            }

            // our last action in the above loop was to switch d and p, so p now
            // actually has the most recent cost counts
            return p[n];
        }

        private static short Minimum(int a, int b, int c)
        {
            return (short)Math.Min(a, Math.Min(b, c)); ;
        }
    }
}

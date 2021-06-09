using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormalSpecification
{
    class Util
    {
        public static Dictionary<string, string> AddPairs(Dictionary<string, string> original, Dictionary<string, string> clone)
        {
            foreach (var pair in clone)
            {
                original.Add(pair.Key, pair.Value);
            }

            return original;
        }

        public static void ParseCondition(ref string cond)
        {
            /* Explanation: inserting char will increase the length of the string by 2
             * to fix this add any 2 characters to the end of the string
             * this will fix the last condition not working as intended
             */
            cond += "\t\t";

            StringBuilder sb;
            Match m;

            for (int i = 0, j = cond.Length; i < j; ++i)
            {
                if (cond[i] == '=')
                {
                    m = Regex.Match(cond[i - 1].ToString() + cond[i].ToString(), @"<|>|<=|>=|!=|==");
                    //matches any < > <= >= != ==

                    if (!m.Success)
                    {
                        sb = new StringBuilder(cond);
                        sb.Insert(i, "=");
                        cond = sb.ToString(); //replaces = with ==
                    }
                }
            }

            cond = cond.Replace("\t\t", "");
        }
    }
}

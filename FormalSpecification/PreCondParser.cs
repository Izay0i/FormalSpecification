using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormalSpecification
{
    class PreCondParser
    {
        private List<string> varList;
        private string condition, expression;

        private bool ParseCondition(string cond)
        {
            cond = Util.ParseCondition(cond);

            this.condition = cond;

            return true;
        }

        private void BuildConditionBody(bool hasCond)
        {
            string body = String.Empty;

            foreach (string nameType in varList)
            {
                string[] elements = nameType.Split(new[] { " " }, StringSplitOptions.None);

                body += String.Format("\n\t\t\tConsole.WriteLine(\"Nhap {0}:\");", elements[1]);
                body += String.Format("\n\t\t\t{0} = ({1})Convert.ChangeType(Console.ReadLine(), typeof({1}));\n\t\t\t", elements[1], elements[0]);
            }

            if (hasCond)
            {
                expression = String.Format("do {{{0}}} while(!({1}));", body, condition);
            }
            else
            {
                expression = String.Format("{0}", body);
            }
        }

        public PreCondParser(string varList, string preCond)
        {
            this.varList = new List<string>(varList.Split(new[] { ";\n" }, StringSplitOptions.RemoveEmptyEntries));

            preCond = preCond.Replace("\t", "").Replace(" ", "");
            string[] pre = preCond.Split(new[] { "pre" }, StringSplitOptions.RemoveEmptyEntries);

            bool hasCond = false;
            if (pre.Length > 0)
            {
               hasCond = ParseCondition(pre[0]);
            }
            BuildConditionBody(hasCond);
        }

        public Dictionary<string, string> OutputResults()
        {
            Dictionary<string, string> results = new Dictionary<string, string>()
            {
                { "condExpression", expression }
            };

            return results;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FormalSpecification
{
    class PostBodyParser
    {
        private Stack<string> stack; //reverse polish notation baby
        private string expression;
        private string[] tokens;

        private void ParseBody(string body)
        {

        }

        public PostBodyParser(string post)
        {
            post = post.Replace("\t", "")
                        .Replace(" ", "")
                        .Replace("TRUE", "true")
                        .Replace("FALSE", "false");

            string[] p = post.Split(new[] { "post" }, StringSplitOptions.None);

            tokens = Regex.Split(p[0], @"\(|\)");
            stack = new Stack<string>(tokens);
        }

        public Dictionary<string, string> OutputResults()
        {
            Dictionary<string, string> results = new Dictionary<string, string>()
            {
                { "bodyExpression", expression }
            };

            return results;
        }
    }
}

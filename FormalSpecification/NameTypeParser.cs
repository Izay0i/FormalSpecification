using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FormalSpecification
{
    class NameTypeParser
    {
        private string funcName, input, output; //param -> used in PreCondParser

        private Dictionary<string, string> dataTypes = new Dictionary<string, string>()
        {
            { "R", "double" }, //REAL
            { "N", "int" }, //NUMERIC
            { "B", "bool" }, //BOOLEAN
            { "CHAR*", "string" } //CHAR POINTER
        };

        private void ParseInput(string input)
        {
            string[] delimComma = input.Split(',');

            foreach (string element in delimComma)
            {
                string[] p = element.Split(':');
                string type = dataTypes[p[1].ToUpper()];

                this.input += $"{type} {p[0]};\n";
            }
        }

        private void ParseOutput(string output)
        {
            string[] o = output.Split(':');
            string type = dataTypes[o[1].ToUpper()];

            this.output = $"{type} {o[0]};\n"; //e.g: output:type -> "type output;"
        }
        
        public NameTypeParser(string declaration) //function declaration Name(a:t, b:t) c:t
        {
            declaration = declaration.Replace("\t", "").Replace(" ", "");
            string[] declared = Regex.Split(declaration, @"\(|\)"); //split by ( and )
                                                                    //now [0]: Name [1]: a:t,b:t [2]: c:t

            this.funcName = declared[0];
            ParseInput(declared[1]);
            ParseOutput(declared[2]);
        }

        public Dictionary<string, string> OutputResults()
        {
            Dictionary<string, string> results = new Dictionary<string, string>()
            {
                { "funcName", funcName },
                { "input", input },
                { "output", output }
            };

            return results;
        }
    }
}

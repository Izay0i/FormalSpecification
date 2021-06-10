using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormalSpecification
{
    public partial class Form1 : Form
    {
        private NameTypeParser nameTypeParser;
        private PreCondParser preCondParser;
        private PostBodyParser postBodyParser;

        private Dictionary<string, string> props = new Dictionary<string, string>();

        private void ParseInput(string[] lines)
        {
            nameTypeParser = new NameTypeParser(lines[0]);
            /* funcName
             * input
             * output
             */
            props = Util.AddPairs(props, nameTypeParser.OutputResults());

            preCondParser = new PreCondParser(props["input"], lines[1]);
            /* condExpression
             */
            props = Util.AddPairs(props, preCondParser.OutputResults());

            postBodyParser = new PostBodyParser(props["output"], lines[2]);
            /* bodyExpression
             */
            props = Util.AddPairs(props, postBodyParser.OutputResults());

            rtbOutput.Text = BuildOutput(
                tbClassName.Text, 
                props["input"], 
                props["output"], 
                props["funcName"], 
                props["condExpression"], 
                ParsePropsOutput(props["output"]), 
                props["bodyExpression"]);
        }

        private string ParsePropsOutput(string output)
        {
            output = output.Replace(";\n", "");
            string[] nameType = output.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string boilerplate = "\n\t\t\tConsole.WriteLine(\"Ket qua la: {0}\", {1});\n";
            return String.Format(boilerplate, "{0}", nameType[1]);
        }

        private string BuildInputFunc(string inp, string output, string funcName, string cond)
        {
            string input = String.Empty;
            input += $"\t\t{inp}";
            input += $"\t\t{output}";
            input += $"\t\tpublic static void Nhap_{funcName}()\n\t\t{{\n";
            input += $"\t\t\t{cond}";
            input += "\n\t\t}\n";

            return input;
        }

        private string BuildOutputFunc(string outputCon, string funcName)
        {
            string output = String.Empty;
            output += $"\t\tpublic static void Xuat_{funcName}()\n\t\t{{\n";
            output += $"{outputCon}";
            output += "\n\t\t}\n";

            return output;
        }

        private string BuildExecFunc(string body, string funcName)
        {
            string exec = String.Empty;
            exec += $"\t\tpublic static void {funcName}()\n\t\t{{\n";
            exec += body;
            exec += "\n\t\t}\n";

            return exec;
        }

        private string BuildMainFunc(string funcName)
        {
            string main = String.Empty;
            main += "\t\tpublic static void Main(string[] args)\n\t\t{\n";
            main += $"\t\t\tNhap_{funcName}();\n";
            main += $"\t\t\t{funcName}();\n";
            main += $"\t\t\tXuat_{funcName}();";
            main += "\n\t\t}\n";

            return main;
        }

        private string BuildOutput(
            string className, 
            string input, 
            string output, 
            string funcName, 
            string cond, 
            string outputCon, 
            string body)
        {
            string result = String.Empty;
            result += "using System;\n";
            result += "namespace FormalSpecification\n{\n\t";
            result += $"public class {className}\n\t{{\n";
            result += BuildInputFunc(input, output, funcName, cond);
            result += BuildOutputFunc(outputCon, funcName);
            result += BuildExecFunc(body, funcName);
            result += BuildMainFunc(funcName);
            result += "\t}\n";
            result += "}";

            return result;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void bBuildSolution_Click(object sender, EventArgs e)
        {
            ParseInput(rtbInput.Lines);
            props.Clear();
        }
    }
}

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

            postBodyParser = new PostBodyParser(lines[2]);
            /* bodyExpression
             */
            props = Util.AddPairs(props, postBodyParser.OutputResults());

            rtbOutput.Text = String.Format("{0}\n{1}{2}{3}\n{4}{5}{6}", 
                tbClassName.Text,
                props["input"],
                props["output"],
                props["funcName"],
                props["condExpression"],
                ParseOutput(props["output"]),
                props["bodyExpression"]);
        }

        private string ParseOutput(string output)
        {
            output = output.Replace(";\n", "");
            string[] nameType = output.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string boilerplate = "\nConsole.WriteLine(\"Ket qua la: {0}\", {1});\n";
            return String.Format(boilerplate, "{0}", nameType[1]);
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

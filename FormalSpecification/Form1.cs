using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            if (rtbInput.Text.Split(new[] { "\n" }, StringSplitOptions.None).Length < 3)
            {
                MessageBox.Show("Invalid input");
                return;
            }

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
                tbClassName.Text.Length > 0 ? tbClassName.Text : "Program", 
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
            input += $"\t\tpublic void Nhap_{funcName}()\n\t\t{{\n";
            input += $"\t\t\t{cond}";
            input += "\n\t\t}\n";

            return input;
        }

        private string BuildOutputFunc(string outputCon, string funcName)
        {
            string output = String.Empty;
            output += $"\t\tpublic void Xuat_{funcName}()\n\t\t{{\n";
            output += $"{outputCon}";
            output += "\n\t\t}\n";

            return output;
        }

        private string BuildExecFunc(string body, string funcName)
        {
            string exec = String.Empty;
            exec += $"\t\tpublic void {funcName}()\n\t\t{{\n";
            exec += body;
            exec += "\n\t\t}\n";

            return exec;
        }

        private string BuildMainFunc(string className, string funcName)
        {
            char objName = className.ToLower()[0];

            string main = String.Empty;
            main += "\t\tpublic static void Main(string[] args)\n\t\t{\n";
            main += $"\t\t\t{className} {objName} = new {className}();\n";
            main += $"\t\t\t{objName}.Nhap_{funcName}();\n";
            main += $"\t\t\t{objName}.{funcName}();\n";
            main += $"\t\t\t{objName}.Xuat_{funcName}();\n";
            main += "\t\t\tConsole.ReadLine();\n";
            main += "\t\t}\n";

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
            result += BuildMainFunc(className, funcName);
            result += "\t}\n";
            result += "}";

            return result;
        }

        private void ResetTextBoxes()
        {
            rtbInput.Text = rtbOutput.Text = String.Empty;
        }

        private void OpenFile()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.OpenFile() != null)
                {
                    string fileName = openFileDialog1.FileName;
                    rtbInput.Text = File.ReadAllText(fileName);
                }
            }
        }

        private void SaveToFile()
        {
            if (rtbInput.Text.Length == 0)
            {
                MessageBox.Show("Cannot save empty input to file");
                return;
            }

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = saveFileDialog.FileName;
                string filePath = Path.GetFullPath(fileName);
                if (!File.Exists(filePath))
                {
                    using (StreamWriter sw = File.CreateText(filePath))
                    {
                        sw.Write(rtbInput.Text);
                    }
                }
                else
                {
                    File.WriteAllText(fileName, rtbInput.Text);
                }

                MessageBox.Show("File successfully created/overwritten");
            }
        }

        //https://stackoverflow.com/questions/58520440/how-do-i-programmatically-compile-a-c-sharp-windows-forms-app-from-a-richtextbox
        private void CompileOutput()
        {
            if (rtbOutput.Text.Length == 0)
            {
                MessageBox.Show("Invalid output");
                return;
            }

            //located in the bin folder
            string compiledOutput = tbExeFileName.Text;
            string[] refAssemblies = { "System.dll", "System.Drawing.dll", "System.Windows.Forms.dll" };

            CodeDomProvider _CodeCompiler = CodeDomProvider.CreateProvider("CSharp");
            CompilerParameters _CompilerParameters = new CompilerParameters(refAssemblies, "");

            _CompilerParameters.OutputAssembly = compiledOutput;
            _CompilerParameters.GenerateExecutable = true;
            _CompilerParameters.GenerateInMemory = false;
            _CompilerParameters.WarningLevel = 3;
            _CompilerParameters.TreatWarningsAsErrors = true;
            _CompilerParameters.CompilerOptions = "/optimize /target:exe";//!! HERE IS THE SOLUTION !!

            string _Errors = null;
            try
            {
                // Invoke compilation
                CompilerResults _CompilerResults = null;
                _CompilerResults = _CodeCompiler.CompileAssemblyFromSource(_CompilerParameters, rtbOutput.Text);

                if (_CompilerResults.Errors.Count > 0)
                {
                    // Return compilation errors
                    _Errors = "";
                    foreach (CompilerError CompErr in _CompilerResults.Errors)
                    {
                        _Errors += "Line number " + CompErr.Line +
                        ", Error Number: " + CompErr.ErrorNumber +
                        ", '" + CompErr.ErrorText + ";\r\n\r\n";
                    }
                }
            }
            catch (Exception _Exception)
            {
                // Error occurred when trying to compile the code
                _Errors = _Exception.Message;
            }
            
            //AFTER WORK
            if (_Errors == null)
            {
                // lets run the program
                MessageBox.Show(compiledOutput + " compiled!");
                System.Diagnostics.Process.Start(compiledOutput);
            }
            else
            {
                MessageBox.Show("Error occurred during compilation : \r\n" + _Errors);
            }
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

        private void bCompile_Click(object sender, EventArgs e)
        {
            CompileOutput();
        }

        private void rtbInput_TextChanged(object sender, EventArgs e)
        {
            //TODO: input syntax highlighting
        }

        private void rtbOutput_TextChanged(object sender, EventArgs e)
        {
            //TODO: output syntax highlighting
        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            ResetTextBoxes();
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (String.Compare(e.ClickedItem.Text, "About") == 0)
            {
                MessageBox.Show("Formal Specification Interpreter using C#", "About");
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetTextBoxes();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveToFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Quit?", "Exit Prompt", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}

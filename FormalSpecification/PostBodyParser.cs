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
        private struct CondBody
        {
            public string type; //for debugging
            public string content;
        }
        
        private string expression;

        private int OperatorOrder(string op)
        {
            switch (op)
            {
                case "&&":
                    return 3;
                case "|":
                    return 2;
                default:
                    return 1;
            }
        }

        private bool HasHigherPrecedence(string op1, string op2)
        {
            return OperatorOrder(op1) > OperatorOrder(op2);
        }

        private bool IsOperator(string s)
        {
            return Regex.Match(s, @"&|\|").Success;
        }

        private void ToPostfix(string outputVar, IEnumerable<string> body)
        {
            Stack<string> operatorStack = new Stack<string>();
            string output = String.Empty;

            foreach (string token in body) {
                if (!IsOperator(token))
                {
                    string t = token;

                    if (!token.Contains(outputVar))
                    {
                        t = Util.ParseCondition(token);
                    }

                    output += $"{t} ";
                }
                else
                {
                    if (operatorStack.Count != 0) {
                        if (HasHigherPrecedence(operatorStack.Peek(), token))
                        {
                            while (operatorStack.Count != 0) {
                                output += $"{operatorStack.Pop()} ";
                            }
                        }
                    }

                    operatorStack.Push(token);
                }
            }

            while (operatorStack.Count != 0)
            {
                output += $"{operatorStack.Pop()} ";
            }

            output = output.Trim();
            EvaluatePostfix(outputVar, output.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
        }

        private void EvaluatePostfix(string outputVar, IEnumerable<string> body)
        {
            Queue<CondBody> condBodyQueue = new Queue<CondBody>();
            Stack<string> operandStack = new Stack<string>();

            string branch = "if ({0}) {{{1}}}\n";
            //if there's no conditional branching, return the expression
            string output = body.Count<string>() == 1 ? $"\t\t\t{body.First<string>()};" : String.Empty;
            bool firstCondBranch = true;

            foreach (string token in body)
            {
                if (!IsOperator(token))
                {
                    Console.WriteLine(token);
                    operandStack.Push(token);
                }
                else
                {
                    if (String.Compare(token, "&&") == 0)
                    {
                        string operand2 = operandStack.Pop();
                        string operand1 = operandStack.Pop();

                        //is result of a condition
                        if (operand1.Contains(outputVar))
                        {
                            CondBody b = new CondBody();

                            b.type = "result";
                            b.content = $"\n\t\t\t\t{operand1};\n\t\t\t";
                            condBodyQueue.Enqueue(b);

                            b.type = "expression";
                            b.content = operand2;
                            condBodyQueue.Enqueue(b);
                        }
                        else
                        {
                            string expression = $"{operand2} {token} {operand1}";
                            operandStack.Push(expression);
                        }
                    }
                    else if (String.Compare(token, "||") == 0)
                    {
                        while (condBodyQueue.Count != 0)
                        {
                            string outputBranch = firstCondBranch ? branch.Insert(0, "\t\t\t") : branch.Insert(0, "\t\t\telse ");

                            string result = condBodyQueue.Dequeue().content;
                            string expression = condBodyQueue.Dequeue().content;

                            output += String.Format(outputBranch, expression, result);

                            firstCondBranch = false;
                        }
                    }
                }
            }

            this.expression = output;
        }

        public PostBodyParser(string output, string post)
        {
            post = post.Replace("\t", "")
                        .Replace(" ", "")
                        .Replace("TRUE", "true")
                        .Replace("FALSE", "false");

            string[] p = post.Split(new[] { "post" }, StringSplitOptions.RemoveEmptyEntries);

            //removes empty entries
            IEnumerable<string> body = Regex.Split(p[0], @"\(|\)").Where<string>(s => s != String.Empty);
            //e.g: "float c;\n" -> [0:"float c"] -> [0:"float", 1:"c"] -> outputVar = "c";
            string outputVar = output.Split(new[] { ";\n" }, StringSplitOptions.None)[0]
                                     .Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
            ToPostfix(outputVar, body);
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

using System.Collections.Generic;

namespace ZSpriteMaker
{
    public class LogicAction
    {
        int NestedLocation = -1;
        public List<LogicAction> actions = new List<LogicAction>();
        public string labelString { get; set; } = ">";
        public LogicActionType type { get; set; }
        public string asm { get; set; }
        public LogicAction parent { get; set; }
        public bool disposed { get; set; } = false;


        public LogicAction(LogicAction parent, LogicActionType type, string labelString, string asmString, string labelBranch = "")
        {

            this.parent = parent;
            this.labelString = labelString;
            this.type = type;
            NestedLocation = asmString.IndexOf("SPRed.AddBranch"); // that'll be used to generate sub asm
            if (NestedLocation != -1) // do nothing else add an empty nested action
            {

                actions.Add(new LogicAction(this, LogicActionType.Branch, labelBranch + ">", ""));

            }
            this.asm = asmString;

        }

        public string GetAsm()
        {
            string subAsm = "";
            foreach (LogicAction la in actions)
            {
                subAsm += la.asm;
                subAsm += "\r\n";
            }

            return asm.Replace("SPRed.AddBranch", subAsm);
        }

        public void Delete()
        {
            actions.Clear();
            disposed = true;
        }


    }

    public enum LogicActionType
    {
        Branch, End, Action
    }
}

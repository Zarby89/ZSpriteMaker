using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSpriteMaker
{
    internal class LogicState
    {
        public List<LogicAction> actions { get; set; } = new List<LogicAction>();
        public string name { get; set; }
        public LogicState(string name, List<LogicAction> actions)
        {
            this.actions = actions;
            this.name = name;


        }
    }
}

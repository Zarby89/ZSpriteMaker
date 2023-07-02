using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSpriteMaker
{
    internal class Action
    {
        public string actionName { get; set; } = "Nothing";
        public int frame { get; set; } = 0;
        public OamTile[] oamTiles { get; set; }

        public Action(string actionName, int frame, OamTile[] oamTiles)
        {
            this.actionName = actionName;
            this.frame = frame;
            this.oamTiles = oamTiles;
        }
    }
}

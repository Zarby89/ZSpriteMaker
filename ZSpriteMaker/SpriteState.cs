namespace ZSpriteMaker
{
    internal class SpriteState
    {
        public string name { get; set; }
        public string asm { get; set; }

        public SpriteState(string name, string asm)
        {
            this.name = name;
            this.asm = asm;
        }

    }
}

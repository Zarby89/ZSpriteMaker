﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZSpriteMaker
{
    public class UserRoutine
    {
        public string name { get; set; }
        public string code { get; set; }
        public UserRoutine(string name, string code)
        {
            this.name = name;
            this.code = code;
        }


    }
}

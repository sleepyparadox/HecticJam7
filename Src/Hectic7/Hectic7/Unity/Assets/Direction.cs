using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hectic7
{
    public enum Direction
    {
        Down = -1,
        Up = 1,
    }
    public static class DirectionHelper
    {
        public static Direction getOther (this Direction dir)
        {
            return (Direction)((int)dir * -1);
        }
    }
}

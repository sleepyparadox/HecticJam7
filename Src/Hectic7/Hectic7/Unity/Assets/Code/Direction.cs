using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hectic7
{
    public enum DirVertical
    {
        Down = -1,
        Up = 1,
    }

    public enum DirHorizontal
    {
        Left = -1,
        Right = 1,
    }

    public static class DirectionHelper
    {
        public static DirVertical GetOther (this DirVertical dir)
        {
            return (DirVertical)((int)dir * -1);
        }
    }
}

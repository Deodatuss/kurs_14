using System;
using System.Collections.Generic;
using System.Text;

namespace Kurs_14_Taksopark
{
    public abstract class Position
    {
        public Position(string Location_Name)
        {
            this.Location_Name = Location_Name;
        }
        public Position()
        {
            this.Location_Name = null;
        }
        private protected int X_Coordinate;
        private protected int Y_Coordinate;
        private protected string Location_Name;
    }
}

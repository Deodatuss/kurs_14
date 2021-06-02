using System;
using System.Collections.Generic;
using System.Text;

namespace Kurs_14_Taksopark
{
    class Calculate_Route
    {
        private readonly double DISTANCE;

        public double GetDistance
        {
            get { return DISTANCE; }
        }

        public Calculate_Route((int, int) User_Crnt_Position, (int, int) Destination)
        {
            DISTANCE = Math.Sqrt((Destination.Item1 - User_Crnt_Position.Item1)*(Destination.Item1 - User_Crnt_Position.Item1) + (Destination.Item2 - User_Crnt_Position.Item2)*(Destination.Item2 - User_Crnt_Position.Item2));
        }
        
    }
}

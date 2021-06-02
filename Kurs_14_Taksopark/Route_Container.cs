using System;
using System.Collections.Generic;
using System.Text;

namespace Kurs_14_Taksopark
{
    public class Route_Container
    {
        public string User_Login
        { get; set; } = null;
        public string Driver_Login
        { get; set; } = null;
        public string User_Email
        { get; set; } = null;
        public string Driver_Email
        { get; set; } = null;
        public string User_Name
        { get; set; } = null;
        public string Driver_Name
        { get; set; } = null;
        public (int?, int?) User_Location
        { get; set; } = (null, null);
        public (int?, int?) Destination
        { get; set; } = (null, null);
        public double? Distance
        { get; set; } = null;
        public string Car
        { get; set; } = null;
        public string Order_Creation_Date
        { get; set; } = null;
    }
}

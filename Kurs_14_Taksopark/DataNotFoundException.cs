using System;
using System.Collections.Generic;
using System.Text;

namespace Kurs_14_Taksopark
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException()
        {

        }
        public DataNotFoundException(string name)
            : base(name)
        {

        }
    }
}

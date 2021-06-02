using System;
using System.Collections.Generic;
using System.Text;

namespace Kurs_14_Taksopark
{
    public class InsufficientRightsException : Exception
    {
        public InsufficientRightsException()
        {

        }
        public InsufficientRightsException(string name)
            : base(name)
        {

        }
    }
}

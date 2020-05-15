using System;

namespace NintendoNetcode.Pia
{
    class PiaException : Exception
    {
        public PiaException(string message) : base(message)
        {
        }
    }
}

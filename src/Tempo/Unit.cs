using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tempo
{
    /// <summary>
    /// Represents the unit type. Use Unit.Value to get an instance of the unit type.
    /// A unit value always compares equal with all other unit values.
    /// </summary>
    public struct Unit : IEquatable<Unit>
    {
        private static readonly Unit value;

        static Unit()
        {
            value = new Unit();
        }

        public static Unit Value
        {
            get
            {
                return value;
            }
        }

        public override string ToString()
        {
            return "unit";
        }

        public bool Equals(Unit other)
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            return (obj is Unit);
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Unit first, Unit second)
        {
            return true;
        }

        public static bool operator !=(Unit first, Unit second)
        {
            return false;
        }
    }
}

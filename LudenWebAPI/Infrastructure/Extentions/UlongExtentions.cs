using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extentions
{
    public static class UlongExtensions
    {
        public static int ToInt(this ulong value)
        {
            if (value > int.MaxValue)
                throw new OverflowException($"Значение {value} превышает допустимый диапазон int.");

            return (int)value;
        }
    }

}

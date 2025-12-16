using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Extentions
{
    public static class IntExtensions
    {
        public static ulong ToUlong(this int value)
        {
            if (value < 0)
                throw new OverflowException($"Нельзя преобразовать отрицательное значение {value} в ulong.");

            return (ulong)value;
        }
    }

}

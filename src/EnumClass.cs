using System;

namespace EnumEx
{
    public class EnumClass<T> where T : struct
    {
        protected readonly T val;
        protected EnumClass(T _val)
        {
            val = _val;
        }
        protected EnumClass() { }
    }
}

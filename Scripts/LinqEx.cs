using System.Collections.Generic;
using System.Linq;

public class LinqEx
{
    public static T GetNext<T>(IEnumerable<T> list, T current)
    {
        try
        {
            return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
        }
        catch
        {
            return default(T);
        }
    }

    public static T GetPrevious<T>(IEnumerable<T> list, T current)
    {
        try
        {
            return list.TakeWhile(x => !x.Equals(current)).Last();
        }
        catch
        {
            return default(T);
        }
    }
}
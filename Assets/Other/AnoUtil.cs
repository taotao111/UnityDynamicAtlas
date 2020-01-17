
using System.Collections.Generic;


public static class AnoUtil {

    static public T Pop<T>(this List<T> list)
    {
        T result = default(T); 
        int index = list.Count - 1;
        if (index >=0)
        {
            result = list[index];
            list.RemoveAt(index);
            return result;
        }

        return result;
    }
}

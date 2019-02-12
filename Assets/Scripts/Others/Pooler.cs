using System;
using System.Collections.Generic;

public class Pooler<T> where T : IPoolable
{
    private Func<IPoolable> spawn;
    private List<T> Pool = new List<T>();

    public Pooler(Func<IPoolable> spawnMethod)
    {
        spawn = spawnMethod;
    }

    public T Get()
    {
        var result = Pool.Find(x => !x.Enabled);
        if (result == null)
        {
            result = (T)spawn?.Invoke();
            Pool.Add(result);
        }
        return result;
    }
}
using System;
using System.Collections.Generic;

public class Pooler<T> where T : IPoolable
{
    private Func<T> spawn;
    private List<T> Pool = new List<T>();

    public Pooler(Func<T> spawnMethod)
    {
        spawn = spawnMethod;
    }

    public T Get()
    {
        var result = Pool.Find(x => !x.Enabled);
        if (result == null)
        {
            result = spawn.Invoke();
            Pool.Add(result);
        }
        return result;
    }
}
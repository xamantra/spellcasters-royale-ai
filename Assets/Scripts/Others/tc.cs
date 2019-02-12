using System;

public static class tc
{
    public static void Run(Action action)
    {
        try
        {
            action?.Invoke();
        }
        catch
        {
            return;
        }
    }
}
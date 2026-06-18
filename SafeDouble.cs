using UnityEngine;

public struct SafeDouble
{
    private double offset;
    private double value;

    public SafeDouble(double val = 0)
    {
        offset = Random.Range(-100000f, 100000f);
        value = val + offset;
    }

    public double GetValue()
    {
        return value - offset;
    }

    public void Add(double addValue)
    {
        double current = GetValue();
        current += addValue;
        
        offset = Random.Range(-100000f, 100000f);
        value = current + offset;
    }
}
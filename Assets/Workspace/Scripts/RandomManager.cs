using UnityEngine;

public class RandomManager
{
    public class Type
    {
        public float weight = 0;
    }

    public class FloatType : Type
    {
        public float value = 0;
    }

    public class IntType : Type
    {
        public int value = 0;
    }


    public static T Get<T>(T[] elements) where T : Type
    {
        int length = elements.Length;
        float totalWeight = 0f;
        for (int a = 0; a < length; ++a)
        {
            totalWeight += elements[a].weight;
        }

        float randomNumber = Random.Range(0f, totalWeight);
        totalWeight = 0f;
        for (int a = 0; a < length; ++a)
        {
            totalWeight += elements[a].weight;
            if (randomNumber < totalWeight)
            {
                return elements[a];
            }
        }

        return elements[^1];
    }
}
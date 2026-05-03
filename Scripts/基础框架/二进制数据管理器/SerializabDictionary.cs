using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SerializableDictionary<T, K> : IEnumerable<KeyValuePair<T, K>>
{
    public List<T> Keys = new List<T>();
    public List<K> Values = new List<K>();

    //双列表拟造字典存储
    public void Add(T key, K value)
    {
        if (Keys.Contains(key))
        {
            int index = Keys.IndexOf(key);
            Keys[index] = key;
            
            Debug.Log("原有键存在，故值已被覆盖");
        }
        else
        {
            Keys.Add(key);
            Values.Add(value);
        }
    }

    public void Remove(T key)
    {
        if (Keys.Contains(key))
        {
            int index = Keys.IndexOf(key);
            Keys.Remove(key);
            Values.RemoveAt(index);
        }
        else
        {
            Debug.LogError("key not found");
        }
    }

    public bool ContainsKey(T key)
    {
        return Keys.Contains(key);
    }

    public K this[T key]
    {
        get
        {
            int index = Keys.IndexOf(key);

            if (index == -1)
            {
                return default(K);
            }
            
            return Values[index];
        }
        set
        {
            int index = Keys.IndexOf(key);

            if (index == -1)
            {
                Keys.Add(key);
                Values.Add(value);
            }
            else
            {
                Values[index] = value;
            }
        }
    }

    public K this[int index]
    {
        get
        {
            if (index < 0 || index >= Keys.Count)
            {
                Debug.LogError("index out of range");
            }
            
            return Values[index];
        }
        set
        {
            if (index < 0 || index >= Keys.Count)
            {
                Debug.LogError("index out of range");
            }
            
            Values[index] = value;
        }
    }
    
    public int Count{ get { return Keys.Count; } }

    public void Clear()
    {
        Keys.Clear();
        Values.Clear();
    }

    public IEnumerator<KeyValuePair<T, K>> GetEnumerator()
    {
        for (int i = 0; i < Keys.Count; i++)
        {
            yield return new KeyValuePair<T, K>(Keys[i], Values[i]);
        } 
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

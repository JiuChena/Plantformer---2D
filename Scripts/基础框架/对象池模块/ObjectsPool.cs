using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectsPool : MonoBehaviour
{
    private static ObjectsPool instance;

    public static ObjectsPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ObjectsPool>();
                if (instance == null)
                {
                    instance = new GameObject("ObjectsPool").AddComponent<ObjectsPool>();
                }
            }
            return instance;
        }
    }

    private GOInfo temp;
    private void Update()
    {
        int n = buffer.Count;
        for(int i = 0; i < n; i++)
        {
            temp = buffer.Dequeue();
        
            temp.time -= Time.deltaTime;

            if (temp.time < 0)
            {
                if (objectPool.ContainsKey(temp.obj.name))
                {
                    objectPool[temp.obj.name].Enqueue(temp.obj);
                }
                else
                {
                    objectPool.Add(temp.obj.name, new Queue<GameObject>());
                    objectPool[temp.obj.name].Enqueue(temp.obj);
                }
        
                temp.obj.transform.SetParent(this.transform);
                temp.obj.SetActive(false);
            }
            else
            {
                buffer.Enqueue(temp);
            }
        }
    }

    public Dictionary<string, Queue<GameObject>> objectPool = new Dictionary<string, Queue<GameObject>>();
    
    private Queue<GOInfo> buffer = new Queue<GOInfo>();

    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="objectName">所获取对象的名字</param>
    /// <param name="parentName">父对象名字</param>
    /// <param name="addToPool">是否在池中不存在对象时创建并添加</param>
    /// <returns></returns>
    public GameObject GetObjectFromPool(string path, string objectName, string parentName = null , UnityAction<GameObject> callback = null)
    {
        GameObject obj = null;
        if (objectPool.ContainsKey(objectName) && objectPool[objectName].Count > 0)
        {
            obj = objectPool[objectName].Dequeue();
            obj.transform.SetParent(GameObject.Find(parentName).transform);
            obj.SetActive(true);
            callback?.Invoke(obj);
        } 
        else
        {
            obj = ResourcesLoadManager.Instance.LoadSync<GameObject>("Prefabs/" + path + objectName, (objLoad) =>
            {
                objLoad.name = objectName;
                objLoad.transform.SetParent(GameObject.Find(parentName).transform);
                objLoad.SetActive(true);
                callback?.Invoke(objLoad);
            });
        }

        return obj;
    }
    
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="objectName">所获取对象的名字</param>
    /// <param name="parentName">父对象名字</param>
    /// <param name="addToPool">是否在池中不存在对象时创建并添加</param>
    /// <returns></returns>
    public GameObject GetObjectFromPool(GameObject prefab, Transform parent = null, UnityAction<GameObject> callback = null)
    {
        GameObject obj = null;
        if (objectPool.ContainsKey(prefab.name) && objectPool[prefab.name].Count > 0)
        {
            obj = objectPool[prefab.name].Dequeue();
            obj.transform.SetParent(parent);
            obj.SetActive(true);
            callback?.Invoke(obj);
        } 
        else
        {
            obj = Instantiate(prefab);
            
            obj.name = prefab.name;
            obj.transform.SetParent(parent);
            obj.SetActive(true);
            callback?.Invoke(obj);
        }

        return obj;
    }
    
    /// <summary>
    /// 从对象池获取对象
    /// </summary>
    /// <param name="objectName">所获取对象的名字</param>
    /// <param name="parentObj">父对象节点</param>
    /// <param name="addToPool">是否在池中不存在对象时创建并添加</param>
    /// <returns></returns>
    public GameObject GetObjectFromPool(string objectName, GameObject parentObj, UnityAction<GameObject> callback = null)
    {
        GameObject obj = null;
        if (objectPool.ContainsKey(objectName) && objectPool[objectName].Count > 0)
        {
            obj = objectPool[objectName].Dequeue();
            obj.transform.SetParent(parentObj.transform);
            obj.SetActive(true);
            callback?.Invoke(obj);
        }
        else
        {
            obj = ResourcesLoadManager.Instance.LoadSync<GameObject>("Prefabs/" + objectName, (objLoad) =>
            {
                objLoad.name = objectName;
                objLoad.transform.SetParent(parentObj.transform);
                objLoad.SetActive(true);
                callback?.Invoke(objLoad);
            });
        }
        
        return obj;
    }

    /// <summary>
    /// 返还对象到对象池
    /// </summary>
    /// <param name="obj">所返还的对象</param>
    public void ReturnObjectToPool(GameObject obj, float time = 0)
    {
        buffer.Enqueue(new GOInfo() { obj = obj, time = time });
    }
}

public class GOInfo
{
    public GameObject obj;
    public float time;
}
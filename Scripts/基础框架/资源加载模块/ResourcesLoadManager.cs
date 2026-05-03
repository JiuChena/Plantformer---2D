using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using IEnumerator = System.Collections.IEnumerator;

public class ResourcesLoadManager 
{
    private static ResourcesLoadManager instance = new ResourcesLoadManager();
    public static ResourcesLoadManager Instance => instance;
    
    private ResourcesLoadManager(){ }

    /// <summary>
    /// 同步加载资源
    /// </summary>
    /// <param name="resourceName">资源路径和名字</param>
    /// <typeparam name="T">资源类型</typeparam>
    /// <returns></returns>
    public T Load<T>(string resourceName) where T : Object
    {
        T resource = Resources.Load<T>(resourceName);
        if (resource is GameObject)
        {
            T obj = GameObject.Instantiate(resource);
            obj.name = resource.name;
            return obj;
        }
        return resource;
    }

    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <param name="resourceName">资源路径和名字</param>
    /// <param name="callback">回调函数</param>
    /// <typeparam name="T">资源类型</typeparam>
    public T LoadSync<T>(string resourceName, UnityAction<T> callback = null) where T : Object
    {
        IEnumerator ie = LoadSyncCoroutine<T>(resourceName, callback);
        T res= null;
        while (ie.MoveNext())
        {
            if (ie.Current is T)
            {
                res = ie.Current as T; 
            } 
        }
        return res;
    }

    private IEnumerator LoadSyncCoroutine<T>(string resourceName, UnityAction< T> callback = null) where T : Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(resourceName);
        yield return request;

        T ins = null; 
        
        if (request.asset is GameObject)
        {
            ins = GameObject.Instantiate(request.asset) as T;
            callback?.Invoke(ins);
            ins.name = request.asset.name;
        }
        else callback?.Invoke(request.asset as T);
        
        yield return ins;
    }
}

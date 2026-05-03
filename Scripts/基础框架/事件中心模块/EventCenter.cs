using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

interface IEventsContainer { }

public class EventsContainer<T> : IEventsContainer
{
    public UnityAction<T> eventsContainer;

    public EventsContainer(UnityAction<T> action)
    {
        eventsContainer += action;
    }
}

public class EventsContainer : IEventsContainer
{
    public UnityAction eventsContainer;

    public EventsContainer(UnityAction action)
    {
        eventsContainer += action;
    }
}

public class EventsContainer<T, K> : IEventsContainer
{
    public UnityAction<T, K> eventsContainer;

    public EventsContainer(UnityAction<T, K> action)
    {
        eventsContainer += action;
    }
}

public class EventCenter 
{
    private static EventCenter instance = new EventCenter();
    public static EventCenter Instance => instance;
    
    private EventCenter(){ }
    
    private Dictionary<string, IEventsContainer> events = new Dictionary<string, IEventsContainer>();


    #region 添加事件监听

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">传入事件</param>
    public void AddEventListener(string eventName, UnityAction action)
    {
        if (events.ContainsKey(eventName))
        {
            (events[eventName] as EventsContainer).eventsContainer += action;
        }
        else
        {
            events.Add(eventName, new EventsContainer( action ));
        }
    }
    //一参重载
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (events.ContainsKey(eventName))
        {
            (events[eventName] as EventsContainer<T>).eventsContainer += action;
        }
        else
        {
            events.Add(eventName, new EventsContainer<T>(action));
        }
    }
    //二参重载
    public void AddEventListener<T, K>(string eventName, UnityAction<T, K> action)
    {
        if (events.ContainsKey(eventName))
        {
            (events[eventName] as EventsContainer<T, K>).eventsContainer += action;
        }
        else
        {
            events.Add(eventName, new EventsContainer<T, K>(action));
        }
    }

    #endregion

    #region 移除事件监听

    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="action">事件</param>
    public void RemoveEventListener(string eventName, UnityAction action)
    {
        (events[eventName] as EventsContainer).eventsContainer -= action;
    }
    //一参重载
    public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
    {
        (events[eventName] as EventsContainer<T>).eventsContainer -= action;
    }
    //二参重载
    public void RemoveEventListener<T, K>(string eventName, UnityAction<T, K> action)
    {
        (events[eventName] as EventsContainer<T, K>).eventsContainer -= action;
    }

    #endregion

    #region 触发事件

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="info">传入参数</param>
    public void SetEventTrigger(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            (events[eventName] as EventsContainer).eventsContainer?.Invoke();
        }
    }
    //一参重载
    public void SetEventTrigger<T>(string eventName, T info)
    {
        if (events.ContainsKey(eventName))
        {
            (events[eventName] as EventsContainer<T>).eventsContainer?.Invoke(info);
        }
    }
    //二参重载
    public void SetEventTrigger<T, K>(string eventName, T Tinfo, K Kinfo)
    {
        if (events.ContainsKey(eventName))
        {
            (events[eventName] as EventsContainer<T, K>).eventsContainer?.Invoke(Tinfo, Kinfo);
        }
    }

    #endregion
    

    /// <summary>
    /// 清除事件中心所有事件
    /// </summary>
    public void EventDicClear()
    {
        events.Clear();
    }
}

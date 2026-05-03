using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadSceneManager
{
    private static LoadSceneManager instance = new LoadSceneManager();
    public static LoadSceneManager Instance => instance;

    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadScene(string sceneName, UnityAction callback = null)
    {
        SceneManager.LoadScene(sceneName);
        callback?.Invoke();
    }

    public void LoadSceneAsync(string sceneName, UnityAction callback = null)
    {
        PublicMono.Instance.StartCoroutine(LoadSceneAsyncCoroutine(sceneName, callback));
    }

    /// <summary>
    /// 异步加载场景，返回 AsyncOperation 供外部控制 allowSceneActivation
    /// </summary>
    public AsyncOperation LoadSceneAsync(string sceneName, bool allowSceneActivation = true)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        if (asyncOperation != null)
        {
            asyncOperation.allowSceneActivation = allowSceneActivation;
        }
        return asyncOperation;
    }

    private IEnumerator LoadSceneAsyncCoroutine(string sceneName, UnityAction callback = null)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            EventCenter.Instance.SetEventTrigger("LoadSceneProgress", asyncOperation.progress);
            yield return asyncOperation.progress;
        }
        callback?.Invoke();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SoftwareManager : MonoBehaviour
{
    [SerializeField] private int realDesiredMaximumNumberOfThreads = Environment.ProcessorCount;
    [SerializeField] private int desiredMaximumNumberOfThreads = Environment.ProcessorCount/2;
    private int defaultMaximumNumberOfThreads;
    private int lowestAcceptableValueForMaximumNumberOfThreads = Environment.ProcessorCount;
    private int defaultMinimumNumberOfThreads;

    public static int mainThreadId { get; private set; }

    public static SoftwareManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("Multiple SoftwareManager exist. Destroying the last one registered.", gameObject);
            Destroy(this);
        }
        else
        {
            Instance = this;
            
            mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            
            ThreadPool.GetMaxThreads(out defaultMaximumNumberOfThreads, out int _);
            ThreadPool.GetMinThreads(out defaultMinimumNumberOfThreads, out int _);

            if (!ThreadPool.SetMaxThreads(realDesiredMaximumNumberOfThreads, realDesiredMaximumNumberOfThreads))
                Debug.LogWarning("Error setting maximum number of threads. The value can not be below " + lowestAcceptableValueForMaximumNumberOfThreads);
        }
    }
    
    /*private void Start()
    {
        ThreadPool.GetMinThreads(out int min, out int _);
        ThreadPool.GetMaxThreads(out int max, out int _);
        Debug.Log("MIN THREADS: " + min + ", MAX THREADS: " + max);
    }*/

    /*private IEnumerator ReportThreadCount()
    {
        while (true)
        {
            Debug.Log("The current number of threads is " + GetCurrentNumberOfActiveThreads());
            yield return new WaitForSeconds(1f);
        }
    }*/

    private int GetCurrentNumberOfActiveThreads()
    {
        ThreadPool.GetMaxThreads(out int max, out int _);
        ThreadPool.GetAvailableThreads(out int available, out int _);
        return max - available;
    }

    public bool ShouldNewThreadsBeEnqueued()
    {
        return GetCurrentNumberOfActiveThreads() < desiredMaximumNumberOfThreads;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bejeweled
{
    [Serializable]
    public class Pool : MonoBehaviour
    {
        private PoolController pool;

        [SerializeField]
        private string id;
        public string Id
        {
            get => id;
        }

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int initialAmount;

        [Header("Runtime")]        
        [SerializeField]
        private Stack<GameObject> instanceStack;

        GameObject _instance;

        private void CreateInstance()
        {
            _instance = Instantiate(prefab, transform, false);
            _instance.SetActive(false);

            instanceStack.Push(_instance);
        }

        public GameObject RetrieveInstance()
        {
            if (instanceStack.Count == 0)
                CreateInstance();

            _instance = instanceStack.Pop();
            _instance.SetActive(true);
            _instance.transform.SetParent(null, false);

            return _instance;
        }

        public void StoreInstance(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            instanceStack.Push(instance);
        }

        public void Initialize()
        {
            instanceStack = new Stack<GameObject>();

            for (int i = 0; i < initialAmount; i++)
            {
                CreateInstance();
            }
        }
    }
}
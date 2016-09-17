using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public class GameObjectRegistry
    {
        Dictionary<string, GameObject> registry;
        public GameObjectRegistry()
        {
            registry = new Dictionary<string, GameObject>();
        }

        public bool Add(string name, GameObject gameObject)
        {
            if (!registry.ContainsKey(name))
            {
                registry.Add(name, gameObject);
                return true;
            }
            Debug.Log(string.Format("registry already contains key[{0}] value[{1}]", name, registry[name]));
            return false;
        }

        public void Remove(string name)
        {
            if (registry.ContainsKey(name))
            {
                registry.Remove(name);
            }        
        }

        public bool TryGet(string name, out GameObject gameObject)
        {
            gameObject = null;
            if (registry.ContainsKey(name))
            {
                gameObject = registry[name];
                return true;
            }
            return false;
        }

        public void Destroy(string name)
        {
            GameObject toBeTerminated = null;
            if (registry.ContainsKey(name))
            {
                toBeTerminated = registry[name];
                Remove(name);                
            }
            GameObject.Destroy(toBeTerminated);
        }
    }
}

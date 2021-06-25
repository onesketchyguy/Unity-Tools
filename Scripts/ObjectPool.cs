// Forrest Lowe 2021

using System.Collections.Generic;
using UnityEngine;

namespace Convienience
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool instance;

        private Dictionary<string, GameObject[]> pools = new Dictionary<string, GameObject[]>();

        [SerializeField]
        private Transform parentObject;

        private void Awake() => instance = this;

        /// <summary>
        /// Request an object from the object pool.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static GameObject Get(GameObject obj, Vector3 position, Quaternion rotation)
        {
            string key = obj.name;

            // Store an index to return later
            int index = -1;

            // Check if the object has been added to the pool already
            if (instance.pools.ContainsKey(key))
            {
                // If so we need to store the length of that pool
                int length = instance.pools[key].Length;

                // Iterate through the pool for any inactive objects
                for (int i = 0; i < length; i++)
                {
                    var item = instance.pools[key][i];

                    // If an object is inactive we can return it
                    if (item.gameObject.activeSelf == false)
                    {
                        index = i;
                        break;
                    }
                }

                // If the index is not -1 then we are attempting to return an object
                if (index == -1)
                {
                    // But if it is still -1 then we need to create a new object for the pool to return
                    var go = Instantiate(obj, position, rotation, instance.parentObject);

                    instance.pools[key] = Utilities.AddToArray(instance.pools[key], go);
                    index = length;
                }
            }
            else
            {
                // If not go ahead and create a new pool for it, and return it.
                var go = Instantiate(obj, position, rotation, instance.parentObject);

                instance.pools.Add(key, Utilities.AddToArray(null, go));
                index = 0;
            }

            // Finally return the object at that index
            var toReturn = instance.pools[key][index];
            toReturn.transform.position = position;
            toReturn.transform.rotation = rotation;

            instance.pools[key][index].gameObject.SetActive(true);

            return toReturn;
        }
    }
}
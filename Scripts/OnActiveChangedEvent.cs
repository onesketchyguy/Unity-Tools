// Forrest Lowe 2021

using UnityEngine;

namespace Convienience
{
    public class OnActiveChangedEvent : MonoBehaviour
    {
        public enum AwakeType
        {
            [Tooltip("Called on object awake")]
            Awake,

            [Tooltip("Called on object start, after awake")]
            Start,

            [Tooltip("Called on object enabled, before start, after awake")]
            OnEnable,

            [Tooltip("Called on object disabled")]
            OnDisable,

            [Tooltip("Called on object destroyed")]
            OnDestroy
        }

        [System.Serializable]
        public struct OnAwakeEvent
        {
            public AwakeType eventType;
            public UnityEngine.Events.UnityEvent @event;
        }

        [SerializeField] private OnAwakeEvent[] events;

        private void OnEnable()
        {
            InvokeType(AwakeType.OnEnable);
        }

        private void OnDisable()
        {
            InvokeType(AwakeType.OnDisable);
        }

        private void OnDestroy()
        {
            InvokeType(AwakeType.OnDestroy);
        }

        private void Awake()
        {
            InvokeType(AwakeType.Awake);
        }

        private void Start()
        {
            InvokeType(AwakeType.Start);
        }

        private void InvokeType(AwakeType type)
        {
            foreach (var item in events)
            {
                if (item.eventType == type)
                {
                    item.@event.Invoke();
                }
            }
        }
    }
}
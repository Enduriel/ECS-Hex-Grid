using Unity.Mathematics;
using UnityEngine;

namespace View
{
    // inspired by https://gist.github.com/JohnnyTurbo/7ca0e3135aa775630052e999e5cc653e#file-camerasingleton-cs
    public class CameraSingleton : MonoBehaviour
    {
        public static CameraSingleton Instance { get; private set; }
        
        public void SetPosition(float3 position)
        {
            transform.position = position;
        }
        
        public void SetRotation(quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public float3 GetPosition()
        {
            return transform.position;
        }
        
        public quaternion GetRotation()
        {
            return transform.rotation;
        }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}
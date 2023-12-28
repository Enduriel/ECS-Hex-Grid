using Unity.Entities;
using UnityEngine;

namespace MyNamespace
{
    struct MeshDataComponent : IComponentData
    {
        public Mesh.MeshDataArray Value;
        
        public MeshDataComponent(Mesh.MeshDataArray value)
        {
            Value = value;
        }
    }
}
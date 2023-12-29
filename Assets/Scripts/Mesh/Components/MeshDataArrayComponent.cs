using Unity.Entities;
using UnityEngine;

namespace MyNamespace
{
    public struct MeshDataArrayComponent : IComponentData
    {
        public MeshDataArrayID ID;
        public int Index;
    }
}
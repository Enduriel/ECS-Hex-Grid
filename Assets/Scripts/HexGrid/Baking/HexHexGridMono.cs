using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace MyNamespace
{
    public class HexHexGridMono : MonoBehaviour
    {
        public ushort Radius;
    }
    
    public class HexHexGridBaker : Baker<HexHexGridMono>
    {
        public override void Bake(HexHexGridMono authoring)
        {
            // this can probably be Renderable but would require reworking other stuff, todo later
            var hexGridEntity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(hexGridEntity, new HexHexGridData
            {
                Radius = authoring.Radius,
            });
        }
    }
}
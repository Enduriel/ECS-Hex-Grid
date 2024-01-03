using Unity.Entities;

namespace Trideria.Mesh
{
	public struct MeshDataArrayComponent : IComponentData
	{
		public MeshDataArrayID ID;
		public int Index;
	}
}
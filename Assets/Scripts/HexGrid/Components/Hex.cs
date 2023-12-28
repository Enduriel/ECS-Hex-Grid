using Unity.Entities;

namespace MyNamespace
{
    public struct Hex
    {
        public HexCoordinates Coords;
    }
    
    public enum HexTerrainEnum
    {
        White,
        Green,
        Blue,
        Yellow,
    }

    public struct HexTerrain
    {
        public HexTerrainEnum Value;
    }

    [InternalBufferCapacity(100)]
    public struct HexBuffer : IBufferElementData
    {
        public Hex Value;
        public HexTerrain Terrain;
        public HexBuffer(HexCoordinates coords)
        {
            Value = new Hex { Coords = coords };
            Terrain = new HexTerrain { Value = HexTerrainEnum.White };
        }
        
        public HexBuffer(HexCoordinates coords, HexTerrainEnum terrain)
        {
            Value = new Hex { Coords = coords };
            Terrain = new HexTerrain { Value = terrain };
        }
    }
}
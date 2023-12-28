using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace MyNamespace
{
    public static class HexHelpers
    {
        public const float OuterRadius = 1f;
        public const float InnerRadius = OuterRadius * 0.866025404f;
        
	    public static readonly float3[] Vertices = {
		    new float3(-OuterRadius * 0.5f, 0f, InnerRadius),
		    new float3(OuterRadius * 0.5f, 0f, InnerRadius),
		    new float3(OuterRadius, 0f, 0f),
		    new float3(OuterRadius * 0.5f, 0f, -InnerRadius),
		    new float3(-OuterRadius * 0.5f, 0f, -InnerRadius),
		    new float3(-OuterRadius, 0f, 0f),
		    new float3(-OuterRadius * 0.5f, 0f, InnerRadius),
		};

	    public static readonly (ushort, ushort, ushort)[] Triangles =
	    {
		    (0, 1, 2),
		    (0, 2, 3),
		    (0, 3, 4),
		    (0, 4, 5),
		    (0, 5, 6),
		    (0, 6, 1)
	    };

	    public static int GetDistance(HexCoordinates a, HexCoordinates b)
	    {
		    return (math.abs(a.q - b.q)
		            + math.abs(a.q + a.r - b.q - b.r)
		            + math.abs(a.r - b.r)) / 2;
	    }
	    
	    public static readonly NativeHashMap<HexCoordinates, HexCoordinates> DirectionMap = new(6, Allocator.Persistent)
        {
            {HexCoordinates.N, HexCoordinates.NE},
            {HexCoordinates.NE, HexCoordinates.SE},
            {HexCoordinates.SE, HexCoordinates.S},
            {HexCoordinates.S, HexCoordinates.SW},
            {HexCoordinates.SW, HexCoordinates.NW},
            {HexCoordinates.NW, HexCoordinates.N}
        };
	    
	    public static int GetNumHexes(int radius)
        {
            return 1 + 3 * radius * (radius - 1);
        }

	    public static Vector3 GetRelativePositionVec(HexCoordinates origin, HexCoordinates target)
	    {
		    // todo optimize
		    return new Vector3(
			    (target.q - origin.q) * OuterRadius * 1.5f,
			    0f,
			    (target.q - origin.q) * OuterRadius * Mathf.Sqrt(3f)/2f + (target.r - origin.r) * OuterRadius * Mathf.Sqrt(3f)
		    );
		    return new Vector3(
			    (target.q - origin.q) * OuterRadius * 1.5f,
			    0f,
			    (target.r - origin.r) * InnerRadius * 2f + ((target.q - origin.q) & 1) * InnerRadius
		    );
	    }
	    
	    public static float3 GetRelativePositionFloat3(HexCoordinates origin, HexCoordinates target)
	    {
		    return new float3(
			    (target.q - origin.q) * OuterRadius * 1.5f,
			    0f,
			    (target.q - origin.q) * OuterRadius * Mathf.Sqrt(3f)/2f + (target.r - origin.r) * OuterRadius * Mathf.Sqrt(3f)
		    );
	    }

	    // todo improve this function, it's almost certainly overestimating
	    public static float3 GetMaxDistanceFromCenter(HexHexGridData hexGridData)
	    {
		    var x = OuterRadius * 1.5f * (hexGridData.Radius - 1) + OuterRadius * 0.5f;
		    var z = InnerRadius * (hexGridData.Radius - 1) + InnerRadius;
		    var diag = math.sqrt(x * x + z * z);
		    return new float3(diag, 0f, diag);
	    }

	    public static LocalTransform GetRelativePosition(HexCoordinates origin, HexCoordinates target)
	    {
		    // todo optimize
		    return new LocalTransform()
		    {
				Position = GetRelativePositionVec(origin, target)
		    };
	    }

    }
}
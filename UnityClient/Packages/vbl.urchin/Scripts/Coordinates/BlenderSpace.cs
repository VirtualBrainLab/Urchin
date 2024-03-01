using UnityEngine;
using BrainAtlas.CoordinateSystems;

public class BlenderSpace : CoordinateSpace
{
    public override string Name => throw new System.NotImplementedException();

    public override Vector3 Dimensions => throw new System.NotImplementedException();

    public override Vector3 Space2World(Vector3 coordSpace, bool useReference = true)
    {
        return Space2World(coordSpace);
    }

    public override Vector3 Space2World_Vector(Vector3 vecSpace)
    {
        return new Vector3(vecSpace.y, vecSpace.z, vecSpace.x);
    }

    public override Vector3 World2Space(Vector3 coordWorld, bool useReference = true)
    {
        return World2Space_Vector(coordWorld);
    }

    public override Vector3 World2Space_Vector(Vector3 vecWorld)
    {
        return new Vector3(vecWorld.z, vecWorld.x, vecWorld.y);
    }
}

using UnityEngine;

namespace Urchin.Managers
{
    public abstract class Manager : MonoBehaviour
    {
        public abstract ManagerType Type { get; }

        public abstract string ToSerializedData();
        public abstract void FromSerializedData(string serializedData);


        public static string Type2String(ManagerType type)
        {
            switch (type)
            {
                case ManagerType.PrimitiveMeshManager:
                    return "meshes";
            }
            return "";
        }
    }

    public enum ManagerType
    {
        PrimitiveMeshManager = 0,

    }
}
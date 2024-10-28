using UnityEngine;

namespace RPG.Saving
{
    [System.Serializable] // bu sınıftaki verileri al ve kaydetme dosyasında 
    // güvenli olduğundan emin ol
    public class SerializableVector3
    {
        float x, y, z;

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector()
        {
            return new Vector3(x, y, z);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace XNACamera
{
    public class KeyFrame
    {
        public KeyFrame(Vector3 pos, Quaternion orient, float time)
        {
            Position = pos;
            Orientation = orient;
            Time = time;
        }

        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }
        public float Time { get; set; }

        public static Matrix Interpolate(KeyFrame key1, KeyFrame key2, float time)
        {
            float timeDiff = key2.Time - key1.Time;

            float t = (time - key1.Time) / timeDiff;

            Vector3 translation = Vector3.Lerp(key1.Position, key2.Position, t);
            Quaternion rotation = Quaternion.Lerp(key1.Orientation, key2.Orientation, t);

            //Vector3 pos = key1.Position + (translation * t);
            Matrix result = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(translation);

            return result;
        }
    }
}

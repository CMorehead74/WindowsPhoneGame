using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace GameFramework
{
    public abstract class MatrixParticleObjectBase : MatrixObjectBase
    {

        //-------------------------------------------------------------------------------------
        // Class constructors

        public MatrixParticleObjectBase(GameHost game)
            : base(game)
        {
            // Default to active
            IsActive = true;
        }

        public MatrixParticleObjectBase(GameHost game, Texture2D texture, Vector3 position, Vector3 scale)
            : this(game)
        {
            ObjectTexture = texture;
            Position = position;
            Scale = scale;
        }

        //-------------------------------------------------------------------------------------
        // Properties

        /// <summary>
        /// Is this object active or dormant?
        /// </summary>
        public bool IsActive { get; set; }


    }
}

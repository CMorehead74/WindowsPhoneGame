//-----------------------------------------------------------------------------
// Copyright (c) 2007-2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Dhpoware
{
    /// <summary>
    /// Custom vertex structure used for normal mapping.
    /// </summary>
    public struct NormalMappedVertex : IVertexType
    {
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.Tangent, 0)
        );

        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector4 Tangent;

        public NormalMappedVertex(Vector3 position, Vector2 texCoord, Vector3 normal, Vector4 tangent)
        {
            Position = position;
            TexCoord = texCoord;
            Normal = normal;
            Tangent = tangent;
        }

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        /// <summary>
        /// Given the 3 vertices (position and texture coordinate) and the
        /// face normal of a triangle calculate and return the triangle's
        /// tangent vector. This method is designed to work with XNA's default
        /// right handed coordinate system and clockwise triangle winding order.
        /// Undefined behavior will result if any other coordinate system
        /// and/or winding order is used. The handedness of the local tangent
        /// space coordinate system is stored in the tangent's w component.
        /// </summary>
        /// <param name="pos1">Triangle vertex 1 position</param>
        /// <param name="pos2">Triangle vertex 2 position</param>
        /// <param name="pos3">Triangle vertex 3 position</param>
        /// <param name="texCoord1">Triangle vertex 1 texture coordinate</param>
        /// <param name="texCoord2">Triangle vertex 2 texture coordinate</param>
        /// <param name="texCoord3">Triangle vertex 3 texture coordinate</param>
        /// <param name="normal">Triangle face normal</param>
        /// <param name="tangent">Calculated tangent vector</param>
        public static void CalcTangent(ref Vector3 pos1,
                                       ref Vector3 pos2,
                                       ref Vector3 pos3,
                                       ref Vector2 texCoord1,
                                       ref Vector2 texCoord2,
                                       ref Vector2 texCoord3,
                                       ref Vector3 normal,
                                       out Vector4 tangent)
        {
            // Create 2 vectors in object space.
            // edge1 is the vector from vertex positions pos1 to pos3.
            // edge2 is the vector from vertex positions pos1 to pos2.
            Vector3 edge1 = pos3 - pos1;
            Vector3 edge2 = pos2 - pos1;

            edge1.Normalize();
            edge2.Normalize();

            // Create 2 vectors in tangent (texture) space that point in the
            // same direction as edge1 and edge2 (in object space).
            // texEdge1 is the vector from texture coordinates texCoord1 to texCoord3.
            // texEdge2 is the vector from texture coordinates texCoord1 to texCoord2.
            Vector2 texEdge1 = texCoord3 - texCoord1;
            Vector2 texEdge2 = texCoord2 - texCoord1;

            texEdge1.Normalize();
            texEdge2.Normalize();

            // These 2 sets of vectors form the following system of equations:
            //
            //  edge1 = (texEdge1.x * tangent) + (texEdge1.y * bitangent)
            //  edge2 = (texEdge2.x * tangent) + (texEdge2.y * bitangent)
            //
            // Using matrix notation this system looks like:
            //
            //  [ edge1 ]     [ texEdge1.x  texEdge1.y ]  [ tangent   ]
            //  [       ]  =  [                        ]  [           ]
            //  [ edge2 ]     [ texEdge2.x  texEdge2.y ]  [ bitangent ]
            //
            // The solution is:
            //
            //  [ tangent   ]        1     [ texEdge2.y  -texEdge1.y ]  [ edge1 ]
            //  [           ]  =  -------  [                         ]  [       ]
            //  [ bitangent ]      det A   [-texEdge2.x   texEdge1.x ]  [ edge2 ]
            //
            //  where:
            //        [ texEdge1.x  texEdge1.y ]
            //    A = [                        ]
            //        [ texEdge2.x  texEdge2.y ]
            //
            //    det A = (texEdge1.x * texEdge2.y) - (texEdge1.y * texEdge2.x)
            //
            // From this solution the tangent space basis vectors are:
            //
            //    tangent = (1 / det A) * ( texEdge2.y * edge1 - texEdge1.y * edge2)
            //  bitangent = (1 / det A) * (-texEdge2.x * edge1 + texEdge1.x * edge2)
            //     normal = cross(tangent, bitangent)

            Vector3 t;
            Vector3 b;
            float det = (texEdge1.X * texEdge2.Y) - (texEdge1.Y * texEdge2.X);

            if ((float)Math.Abs(det) < 1e-6f)    // almost equal to zero
            {
                t = Vector3.UnitX;
                b = Vector3.UnitY;
            }
            else
            {
                det = 1.0f / det;

                t.X = (texEdge2.Y * edge1.X - texEdge1.Y * edge2.X) * det;
                t.Y = (texEdge2.Y * edge1.Y - texEdge1.Y * edge2.Y) * det;
                t.Z = (texEdge2.Y * edge1.Z - texEdge1.Y * edge2.Z) * det;

                b.X = (-texEdge2.X * edge1.X + texEdge1.X * edge2.X) * det;
                b.Y = (-texEdge2.X * edge1.Y + texEdge1.X * edge2.Y) * det;
                b.Z = (-texEdge2.X * edge1.Z + texEdge1.X * edge2.Z) * det;

                t.Normalize();
                b.Normalize();
            }

            // Calculate the handedness of the local tangent space.
            // The bitangent vector is the cross product between the triangle face
            // normal vector and the calculated tangent vector. The resulting bitangent
            // vector should be the same as the bitangent vector calculated from the
            // set of linear equations above. If they point in different directions
            // then we need to invert the cross product calculated bitangent vector. We
            // store this scalar multiplier in the tangent vector's 'w' component so
            // that the correct bitangent vector can be generated in the normal mapping
            // shader's vertex shader.

            Vector3 bitangent = Vector3.Cross(normal, t);
            float handedness = (Vector3.Dot(bitangent, b) < 0.0f) ? -1.0f : 1.0f;

            tangent.X = t.X;
            tangent.Y = t.Y;
            tangent.Z = t.Z;
            tangent.W = handedness;
        }
    }


    /// <summary>
    /// The NormalMappedQuad class is used to procedurally generate a quad
    /// made up of two triangles. This class generates geometry using the
    /// NormalMappedVertex structure.
    /// </summary>
    public class NormalMappedQuad
    {
        private NormalMappedVertex[] vertices;

        public NormalMappedQuad()
        {
        }

        public NormalMappedQuad(Vector3 origin, Vector3 normal, Vector3 up,
                                float width, float height,
                                float uTile, float vTile)
        {
            Generate(origin, normal, up, width, height, uTile, vTile);
        }

        /// <summary>
        /// Procedurally generate a quad. The quad is made using 2 triangles.
        /// </summary>
        /// <param name="origin">The center position of the quad.</param>
        /// <param name="normal">The quad's surface normal.</param>
        /// <param name="up">A vector pointing at the top of the quad.</param>
        /// <param name="width">Width of the quad.</param>
        /// <param name="height">Height of the quad.</param>
        /// <param name="uTile">Horizontal texture tiling factor.</param>
        /// <param name="vTile">Vertical texture tiling factor.</param>
        public void Generate(Vector3 origin, Vector3 normal, Vector3 up,
                             float width, float height,
                             float uTile, float vTile)
        {
            Vector3 left = Vector3.Cross(normal, up);
            Vector3 posUpperCenter = (up * height / 2.0f) + origin;
            Vector3 posUpperLeft = posUpperCenter + (left * width / 2.0f);
            Vector3 posUpperRight = posUpperCenter - (left * width / 2.0f);
            Vector3 posLowerLeft = posUpperLeft - (up * height);
            Vector3 posLowerRight = posUpperRight - (up * height);

            Vector2 textureUpperLeft = new Vector2(0.0f, 0.0f);
            Vector2 textureUpperRight = new Vector2(1.0f * uTile, 0.0f);
            Vector2 textureLowerLeft = new Vector2(0.0f, 1.0f * vTile);
            Vector2 textureLowerRight = new Vector2(1.0f * uTile, 1.0f * vTile);

            Vector4 tangent;

            vertices = new NormalMappedVertex[6];

            NormalMappedVertex.CalcTangent(
                ref posUpperLeft, ref posUpperRight, ref posLowerLeft,
                ref textureUpperLeft, ref textureUpperRight, ref textureLowerLeft,
                ref normal, out tangent);

            vertices[0] = new NormalMappedVertex(posUpperLeft, textureUpperLeft, normal, tangent);
            vertices[1] = new NormalMappedVertex(posUpperRight, textureUpperRight, normal, tangent);
            vertices[2] = new NormalMappedVertex(posLowerLeft, textureLowerLeft, normal, tangent);

            NormalMappedVertex.CalcTangent(
                ref posLowerLeft, ref posUpperRight, ref posLowerRight,
                ref textureLowerLeft, ref textureUpperRight, ref textureLowerRight,
                ref normal, out tangent);

            vertices[3] = new NormalMappedVertex(posLowerLeft, textureLowerLeft, normal, tangent);
            vertices[4] = new NormalMappedVertex(posUpperRight, textureUpperRight, normal, tangent);
            vertices[5] = new NormalMappedVertex(posLowerRight, textureLowerRight, normal, tangent);
        }

        /// <summary>
        /// Returns the quad's vertex list.
        /// </summary>
        public NormalMappedVertex[] Vertices
        {
            get { return vertices; }
        }
    }
}
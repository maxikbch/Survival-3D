#region File Description

//-----------------------------------------------------------------------------
// CubePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion File Description

#region Using Statements

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#endregion Using Statements

namespace SurvivalGame.Geometries
{
    /// <summary>
    ///     Geometric primitive class for drawing cubes.
    /// </summary>
    public class TrianglePrismPrimitive : GeometricPrimitive
    {
        public TrianglePrismPrimitive(GraphicsDevice graphicsDevice, ContentManager content) : this(graphicsDevice, content, 1, Color.White, Color.White,
            Color.White, Color.White, Color.White)
        {
        }

        public TrianglePrismPrimitive(GraphicsDevice graphicsDevice, ContentManager content, float size, Color color) : this(graphicsDevice, content, size, color,
            color, color, color, color)
        {
        }

        /// <summary>
        ///     Constructs a new cube primitive, with the specified size.
        /// </summary>
        public TrianglePrismPrimitive(GraphicsDevice graphicsDevice, ContentManager content, float size, Color color1, Color color2, Color color3,
            Color color4, Color color5)
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                // front normal
                Vector3.UnitZ,
                // back normal
                -Vector3.UnitZ,
                // right normal
                -Vector3.UnitX,
                // left normal
                new Vector3(0f, 1f, 0f),
                // bottom normal
                -Vector3.UnitY
            };

            Color[] colors =
            {
                color1, color2, color3, color4, color5
            };

            var i = 0;
            // Create each face in turn.

            foreach (var normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
                var side1 = new Vector3(normal.Y, normal.Z, normal.X);
                var side2 = Vector3.Cross(normal, side1);

                if (i == 0)
                {
                    // Three indices (one triangles) per face.
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    // Three vertices per face.
                    AddVertex((normal - side1 - side2) * size / 2, colors[0], normal);
                    AddVertex((normal - side1 + side2) * size / 2, colors[0], normal);
                    AddVertex((normal + side1 + side2) * size / 2, colors[0], normal);
                } else if (i == 1)
                {
                    // Three indices (one triangles) per face.
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    // Three vertices per face.
                    AddVertex((normal + side1 - side2) * size / 2, colors[1], normal);
                    AddVertex((normal - side1 + side2) * size / 2, colors[1], normal);
                    AddVertex((normal + side1 + side2) * size / 2, colors[1], normal);
                }
                else if (i == 2)
                {
                    // Six indices (two triangles) per face.
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 2);
                    AddIndex(CurrentVertex + 3);

                    // Four vertices per face.
                    AddVertex((normal - side1 - side2) * size / 2, colors[2], normal);
                    AddVertex((normal - side1 + side2) * size / 2, colors[2], normal);
                    AddVertex((normal + side1 + side2) * size / 2, colors[2], normal);
                    AddVertex((normal + side1 - side2) * size / 2, colors[2], normal);
                }
                else if (i == 3)
                {
                    // Six indices (two triangles) per face.
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 2);
                    AddIndex(CurrentVertex + 3);

                    // Four vertices per face.
                    AddVertex((-normal + new Vector3(side1.X + side2.X, side1.Y + side2.Y, side1.Z + side2.Z)) * size / 2, colors[3], normal);
                    AddVertex((-normal + new Vector3(side1.X - side2.X, side1.Y - side2.Y, side1.Z - side2.Z)) * size / 2, colors[3], normal);
                    AddVertex((normal - new Vector3(side1.X + side2.X, side1.Y + side2.Y, side1.Z + side2.Z)) * size / 2, colors[3], normal);
                    AddVertex((normal - new Vector3(side1.X - side2.X, side1.Y - side2.Y, side1.Z - side2.Z)) * size / 2, colors[3], normal);
                }
                else if (i == 4)
                {
                    // Six indices (two triangles) per face.
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 2);
                    AddIndex(CurrentVertex + 3);

                    // Four vertices per face.
                    AddVertex((normal - side1 - side2) * size / 2, colors[4], normal);
                    AddVertex((normal - side1 + side2) * size / 2, colors[4], normal);
                    AddVertex((normal + side1 + side2) * size / 2, colors[4], normal);
                    AddVertex((normal + side1 - side2) * size / 2, colors[4], normal);
                }

                i++;
            }

            InitializePrimitive(graphicsDevice,content);
        }
    }
}
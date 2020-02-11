using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;
using Assimp;

namespace KartongDX
{
    class MeshLoader
    {
        public static Assimp.Scene Load(string fileName)
        {
            Assimp.AssimpContext importer = new AssimpContext();
            importer.SetConfig(new Assimp.Configs.NormalSmoothingAngleConfig(0.66f));
            Scene model = importer.ImportFile(fileName, PostProcessPreset.TargetRealTimeMaximumQuality);
            return model;
        }

        public static Rendering.Mesh LoadMesh(string fileName)
        {
            Scene scene = Load(fileName);

            List<Rendering.Vertex> vertices = new List<Rendering.Vertex>();
            List<Rendering.Triangle> indexes = new List<Rendering.Triangle>();

            foreach (Assimp.Mesh aMesh in scene.Meshes)
            {
                for (int i = 0; i < aMesh.VertexCount; ++i)
                {

                    Vector3 position = ToVector3(aMesh.Vertices[i]);
                    Vector3 normal = ToVector3(aMesh.Normals[i]);

                    var uvChannel = aMesh.TextureCoordinateChannels[0];
                    Vector2 uv = ToVector2(uvChannel[i]);

                    Rendering.Vertex vertex = new Rendering.Vertex(position, normal, uv, Color4.White);
                    vertex.Bitangent = ToVector3(aMesh.BiTangents[i]);
                    vertex.Tangent = ToVector3(aMesh.Tangents[i]);

                    if (Vector3.Dot(Vector3.Cross(vertex.Normal, vertex.Tangent), vertex.Bitangent) < 0.0f)
                    {
                        vertex.Tangent = vertex.Tangent * -1.0f;
                    }

                    vertices.Add(vertex);
                }

                uint[] ind = aMesh.GetUnsignedIndices();

                for (int i = 0; i < ind.Length; i += 3)
                {
                    indexes.Add(new Rendering.Triangle(ind[i], ind[i + 1], ind[i + 2]));
                }
            }


            Rendering.Mesh mesh = new Rendering.Mesh(vertices.ToArray(), indexes.ToArray());

            return mesh;
        }

        private static Vector3 ToVector3(Vector3D v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        private static Vector2 ToVector2(Vector3D v)
        {
            return new Vector2(v.X, v.Y);
        }
    }


}

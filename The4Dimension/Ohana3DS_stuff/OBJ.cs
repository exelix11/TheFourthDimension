using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace The4Dimension.Ohana
{
    public class OBJ
    {
        MeshUtils msh = new MeshUtils();
        /// <summary>
        ///     Exports a model to the Wavefront OBJ format.
        /// </summary>
        /// <param name="model">The model to be exported</param>
        /// <param name="fileName">The output file name</param>
        /// <param name="modelIndex">The index of the model that should be exported</param>
        public void export(RenderBase.OModelGroup model, string fileName, int modelIndex)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder outputMTL = new StringBuilder();

            RenderBase.OModel mdl = model.model[modelIndex];
            output.AppendLine("mtllib " + Path.GetFileNameWithoutExtension(fileName) + ".mtl");
            for (int i = 0; i < model.texture.Count; i++)
            {                
                model.texture[i].texture.Save(@"models\Tex\" + model.texture[i].name + ".png");
            }
            int faceIndexBase = 1;
            for (int objIndex = 0; objIndex < mdl.mesh.Count; objIndex++)
            {
                output.AppendLine("g " + mdl.mesh[objIndex].name);
                output.AppendLine(null);
                output.AppendLine("usemtl " + mdl.material[mdl.mesh[objIndex].materialId].name0);
                output.AppendLine(null);

                MeshUtils.optimizedMesh obj = msh.optimizeMesh(mdl.mesh[objIndex]);
                foreach (RenderBase.OVertex vertex in obj.vertices)
                {
                    output.AppendLine("v " + getString(vertex.position.x) + " " + getString(vertex.position.y) + " " + getString(vertex.position.z));
                    output.AppendLine("vn " + getString(vertex.normal.x) + " " + getString(vertex.normal.y) + " " + getString(vertex.normal.z));
                    output.AppendLine("vt " + getString(vertex.texture0.x) + " " + getString(vertex.texture0.y));
                }
                output.AppendLine(null);

                for (int i = 0; i < obj.indices.Count; i += 3)
                {
                    output.AppendLine(
                        string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                        faceIndexBase + obj.indices[i],
                        faceIndexBase + obj.indices[i + 1],
                        faceIndexBase + obj.indices[i + 2]));
                }
                faceIndexBase += obj.vertices.Count;
                output.AppendLine(null);
            }
            //MakeMTL
            List<string> MatNames = new List<string>();           

            foreach (RenderBase.OMaterial mat in mdl.material)
            {
                if (!MatNames.Contains(mat.name0))
                {
                    MatNames.Add(mat.name0);
                    outputMTL.AppendLine(null);
                    outputMTL.AppendLine("newmtl " + mat.name0);
                    outputMTL.AppendLine("Ka " + string.Format("{0} {1} {2}",
                            mat.materialColor.ambient.R.ToString(),
                            mat.materialColor.ambient.G.ToString(),
                            mat.materialColor.ambient.B.ToString()));
                    outputMTL.AppendLine("Kd " + string.Format("{0} {1} {2}",
                            mat.materialColor.diffuse.R.ToString(),
                            mat.materialColor.diffuse.G.ToString(),
                            mat.materialColor.diffuse.B.ToString()));
                    outputMTL.AppendLine("Ks 0 0 0");
                    outputMTL.AppendLine("d 1");
                    if (mat.name0 == null) outputMTL.AppendLine("map_Kd " + @"Tex\dummy.png"); else outputMTL.AppendLine("map_Kd " + @"Tex\" + mat.name0 + ".png");
                }
            }
            File.WriteAllText(fileName, output.ToString());
            File.WriteAllText(Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + ".mtl", outputMTL.ToString());
        }

        /// <summary>
        ///     Transforms a Float into a String that will always have "." into decimal places,
        ///     even if the region uses ",".
        /// </summary>
        /// <param name="value">The Float value</param>
        /// <returns></returns>
        private  string getString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Imports a Wavefront OBJ model from file.
        /// </summary>
        /// <param name="fileName">The complete file name</param>
        /// <returns></returns>
        public  RenderBase.OModelGroup import(string fileName)
        {
            RenderBase.OModelGroup output = new RenderBase.OModelGroup();
            RenderBase.OModel model = new RenderBase.OModel();

            string obj = File.ReadAllText(fileName);

            List<RenderBase.OVector3> vertices = new List<RenderBase.OVector3>();
            List<RenderBase.OVector3> normals = new List<RenderBase.OVector3>();
            List<RenderBase.OVector2> uvs = new List<RenderBase.OVector2>();
            List<RenderBase.OVertex> currVertices = new List<RenderBase.OVertex>();

            string name = string.Empty, oldName;
            string[] lines = obj.Split((char)0xa);
            foreach (string l in lines)
            {
                string line = l.Trim();
                string[] lineParams = Regex.Split(line, "\\s+");

                switch (lineParams[0])
                {
                    case "v":
                    case "vn":
                        RenderBase.OVector3 pvec = new RenderBase.OVector3();
                        pvec.x = float.Parse(lineParams[1], CultureInfo.InvariantCulture);
                        pvec.y = float.Parse(lineParams[2], CultureInfo.InvariantCulture);
                        pvec.z = float.Parse(lineParams[3], CultureInfo.InvariantCulture);
                        if (lineParams[0] == "v") vertices.Add(pvec); else normals.Add(pvec);
                        break;
                    case "vt":
                        RenderBase.OVector2 tvec = new RenderBase.OVector2();
                        tvec.x = float.Parse(lineParams[1], CultureInfo.InvariantCulture);
                        tvec.y = float.Parse(lineParams[2], CultureInfo.InvariantCulture);
                        uvs.Add(tvec);
                        break;
                    case "f":
                        string[][] vtx = new string[lineParams.Length - 1][];
                        for (int i = 0; i < lineParams.Length - 1; i++)
                        {
                            vtx[i] = lineParams[i + 1].Split('/');
                        }

                        for (int i = 0; i < lineParams.Length - 1; i++)
                        {
                            RenderBase.OVertex vertex = new RenderBase.OVertex();

                            vertex.position = vertices[int.Parse(vtx[i][0]) - 1];
                            if (vtx[i].Length > 1 && vtx[i][1] != string.Empty) vertex.texture0 = uvs[int.Parse(vtx[i][1]) - 1];
                            if (vtx[i].Length > 2) vertex.normal = normals[int.Parse(vtx[i][2]) - 1];
                            vertex.diffuseColor = 0xffffffff;

                            if (i > 2)
                            {
                                currVertices.Add(currVertices[currVertices.Count - 3]);
                                currVertices.Add(currVertices[currVertices.Count - 2]);
                                currVertices.Add(vertex);
                            }
                            else
                                currVertices.Add(vertex);
                        }
                        break;
                    case "g":
                        oldName = name;
                        if (lineParams.Length > 1)
                            name = lineParams[1];
                        else
                            name = "mesh";

                        if (currVertices.Count > 0)
                        {
                            RenderBase.OMesh mesh = new RenderBase.OMesh();
                            mesh.vertices = currVertices;
                            mesh.name = oldName;
                            mesh.hasNormal = true;
                            mesh.texUVCount = 1;
                            model.mesh.Add(mesh);

                            currVertices = new List<RenderBase.OVertex>();
                        }
                        break;
                }
            }

            if (currVertices.Count > 0)
            {
                RenderBase.OMesh mesh = new RenderBase.OMesh();
                mesh.vertices = currVertices;
                mesh.name = name;
                mesh.hasNormal = true;
                mesh.texUVCount = 1;
                model.mesh.Add(mesh);
            }

            model.material.Add(new RenderBase.OMaterial());
            output.model.Add(model);
            return output;
        }
    }
}

// using System.Collections.Generic;
// using UnityEngine;
// using System.Globalization;

// public static class ObjLoaderFromString
// {
//     public static void Load(string objData)
//     {
//         Mesh mesh = new Mesh();
//         List<Vector3> vertices = new List<Vector3>();
//         List<Color> colors = new List<Color>();
//         List<int> triangles = new List<int>();

//         string[] lines = objData.Split('\n');
//         foreach (string line in lines)
//         {
//             if (line.StartsWith("v "))
//             {
//                 string[] parts = line.Trim().Split(' ');
//                 float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
//                 float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
//                 float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
//                 float r = parts.Length > 4 ? float.Parse(parts[4], CultureInfo.InvariantCulture) : 1f;
//                 float g = parts.Length > 5 ? float.Parse(parts[5], CultureInfo.InvariantCulture) : 1f;
//                 float b = parts.Length > 6 ? float.Parse(parts[6], CultureInfo.InvariantCulture) : 1f;
//                 vertices.Add(new Vector3(x, y, z));
//                 colors.Add(new Color(r, g, b));
//             }
//             else if (line.StartsWith("f "))
//             {
//                 string[] parts = line.Trim().Split(' ');
//                 for (int i = 1; i <= 3; i++)
//                 {
//                     string[] sub = parts[i].Split('/');
//                     int index = int.Parse(sub[0]) - 1;
//                     triangles.Add(index);
//                 }
//             }
//         }

//         mesh.SetVertices(vertices);
//         mesh.SetTriangles(triangles, 0);
//         mesh.SetColors(colors);
//         mesh.RecalculateNormals();

//         GameObject obj = new GameObject("ImportedObject", typeof(MeshFilter), typeof(MeshRenderer));
//         obj.GetComponent<MeshFilter>().mesh = mesh;

//         Material mat = new Material(Shader.Find("Standard"));
//         obj.GetComponent<MeshRenderer>().material = mat;

//         obj.transform.position = Vector3.zero;
//     }
// }



// using System.Collections.Generic;
// using UnityEngine;
// using System.Globalization;

// public static class ObjLoaderFromString
// {
//   public static void Load(string objData)
//   {
//     Mesh mesh = new Mesh();
//     List<Vector3> vertices = new List<Vector3>();
//     List<Color> colors = new List<Color>();
//     List<int> triangles = new List<int>();

//     string[] lines = objData.Split('\n');
//     foreach (string line in lines)
//     {
//       if (line.StartsWith("v "))
//       {
//         string[] parts = line.Trim().Split(' ');
//         float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
//         float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
//         float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
//         float r = parts.Length > 4 ? float.Parse(parts[4], CultureInfo.InvariantCulture) : 1f;
//         float g = parts.Length > 5 ? float.Parse(parts[5], CultureInfo.InvariantCulture) : 1f;
//         float b = parts.Length > 6 ? float.Parse(parts[6], CultureInfo.InvariantCulture) : 1f;
//         vertices.Add(new Vector3(x, y, z));
//         colors.Add(new Color(r, g, b));
//       }
//       else if (line.StartsWith("f "))
//       {
//         string[] parts = line.Trim().Split(' ');
//         for (int i = 1; i <= 3; i++)
//         {
//           string[] sub = parts[i].Split('/');
//           int index = int.Parse(sub[0]) - 1;
//           triangles.Add(index);
//         }
//       }
//     }

//     Debug.Log($"Parsed {vertices.Count} vertices and {triangles.Count / 3} triangles.");

//     if (vertices.Count == 0 || triangles.Count == 0)
//     {
//       Debug.LogWarning("No mesh data found. Skipping object creation.");
//       return;
//     }

//     mesh.SetVertices(vertices);
//     mesh.SetTriangles(triangles, 0);
//     mesh.SetColors(colors);
//     mesh.RecalculateNormals();

//     GameObject obj = new GameObject("ImportedObject", typeof(MeshFilter), typeof(MeshRenderer));
//     obj.GetComponent<MeshFilter>().mesh = mesh;

//     Material mat = new Material(Shader.Find("Standard"));
//     obj.GetComponent<MeshRenderer>().material = mat;

//     // ✅ Place object 2 meters in front of the camera
//     if (Camera.main != null)
//     {
//       obj.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
//     }
//     else
//     {
//       obj.transform.position = Vector3.zero;
//     }

//     Debug.Log("Imported object created and placed in front of camera.");
//   }
// }



using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public static class ObjLoaderFromString
{
    public static void Load(string objData)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        string[] lines = objData.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("v "))
            {
                string[] parts = line.Trim().Split(' ');
                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                float r = parts.Length > 4 ? float.Parse(parts[4], CultureInfo.InvariantCulture) : 1f;
                float g = parts.Length > 5 ? float.Parse(parts[5], CultureInfo.InvariantCulture) : 1f;
                float b = parts.Length > 6 ? float.Parse(parts[6], CultureInfo.InvariantCulture) : 1f;
                vertices.Add(new Vector3(x, y, z));
                colors.Add(new Color(r, g, b));
            }
            else if (line.StartsWith("f "))
            {
                string[] parts = line.Trim().Split(' ');
                for (int i = 1; i <= 3; i++)
                {
                    string[] sub = parts[i].Split('/');
                    int index = int.Parse(sub[0]) - 1;
                    triangles.Add(index);
                }
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();

        GameObject obj = new GameObject("ImportedObject", typeof(MeshFilter), typeof(MeshRenderer));
        obj.GetComponent<MeshFilter>().mesh = mesh;

        // Load the custom material (must be in a Resources folder)
        Material mat = Resources.Load<Material>("VertexColorMaterial");
        if (mat == null)
        {
            Debug.LogError("VertexColorMaterial not found in Resources!");
        }
        obj.GetComponent<MeshRenderer>().material = mat;

        // Position 2m in front of the camera
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            obj.transform.position = mainCam.transform.position + mainCam.transform.forward * 2f;
        }
        else
        {
            obj.transform.position = Vector3.zero;
        }
    }
}
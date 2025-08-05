// using System.IO;
// using UnityEngine;
// using UnityEngine.UI;
// using SimpleFileBrowser;

// public class ObjectImporterUI : MonoBehaviour
// {
//     public Button importButton;

//     void Start()
//     {
//         FileBrowser.HideDialog(); // Make sure it's hidden before we start

//         importButton.onClick.AddListener(() =>
//         {
//             FileBrowser.SetFilters(true, new FileBrowser.Filter("Model Files", ".obj", ".txt"));
//             FileBrowser.SetDefaultFilter(".obj");

//             FileBrowser.ShowLoadDialog(
//                 onSuccess: (paths) =>
//                 {
//                     string filePath = paths[0];
//                     Debug.Log("Selected file: " + filePath);

//                     string objData = File.ReadAllText(filePath);
//                     ObjLoaderFromString.Load(objData);
//                 },
//                 onCancel: () =>
//                 {
//                     Debug.Log("User canceled file import.");
//                 },
//                 FileBrowser.PickMode.Files
//             );
//         });
//     }
// }


using UnityEngine;
using UnityEngine.UI;

public class ObjectImporterUI : MonoBehaviour
{
    public Button importButton;

    void Start()
    {
        importButton.onClick.AddListener(() =>
        {
            Debug.Log("Import button clicked!");

            // Try to load from Resources/model.txt or model.obj (no extension needed)
            TextAsset objFile = Resources.Load<TextAsset>("model"); // Put your file in Assets/Resources/model.txt
            if (objFile == null)
            {
                Debug.LogError("Could not find 'model.txt' in Resources folder.");
                return;
            }

            string objData = objFile.text;
            Debug.Log("Loaded text from model.txt, parsing...");

            ObjLoaderFromString.Load(objData);
        });
    }
}
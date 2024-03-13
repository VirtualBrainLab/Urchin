using UnityEngine;
using UnityEditor;
using System.IO;

public class SchemaManager
{
    [MenuItem("Tools/Update Schemas")]
    public static void UpdateSchemas()
    {

        GetSchemas("C:\\proj\\VBL\\vbl-aquarium\\src\\vbl_aquarium\\csharp\\urchin",
            "Packages/vbl.urchin/Scripts/JSON");

        GetSchemas("C:\\proj\\VBL\\vbl-aquarium\\src\\vbl_aquarium\\csharp\\generic",
            "Packages/vbl.urchin/Scripts/JSON/Generic");

        Debug.Log("Schemas updated successfully!");
    }

    private static void GetSchemas(string inFolder, string outFolder)
    {

        if (!Directory.Exists(outFolder))
        {
            Directory.CreateDirectory(outFolder);
        }

        string[] files = Directory.GetFiles(inFolder, "*.cs");

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destFilePath = Path.Combine(outFolder, fileName);
            File.Copy(file, destFilePath, true);
        }

        AssetDatabase.Refresh();
    }
}

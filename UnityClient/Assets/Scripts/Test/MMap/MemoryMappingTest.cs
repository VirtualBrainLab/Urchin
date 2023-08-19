using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class MemoryMappingTest : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            using (MemoryMappedFile mmf = MemoryMappedFile.OpenExisting("C:\\proj\\VBL\\Urchin\\API\\testing\\mmap_example.bin"))
            {
                using (MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor())
                {
                    // Assuming the Python example wrote "Hello from Python!"
                    byte[] buffer = new byte[19];
                    accessor.ReadArray(0, buffer, 0, buffer.Length);
                    string message = Encoding.ASCII.GetString(buffer);
                    Debug.Log($"Message from memory-mapped file: {message}");
                    text.text = "Success";
                    text.color = Color.green;
                }
            }
        }
        catch (FileNotFoundException)
        {
            Debug.LogError("Memory-mapped file not found. Make sure the Python script created it.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred: {ex.Message}");
            Debug.Log(ex.StackTrace);
        }
    }

}

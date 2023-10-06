using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using NJsonSchema.CodeGeneration.CSharp;
using System.IO;
using NJsonSchema;
using System.Text.RegularExpressions;

public class SchemaManager
{
    private static List<string> _schemaURLs = new List<string>()
    {
        "https://github.com/VirtualBrainLab/vbl-json-schema/raw/main/urchin/camera-schema.json",
        "https://github.com/VirtualBrainLab/vbl-json-schema/raw/main/urchin/neuron-schema.json"
    };
    private static string _schemaOutputFolder = "Packages/vbl.urchin/Scripts/JSON";

    [MenuItem("Tools/Build Schemas")]
    public static void BuildSchemaStructs() {

        foreach (string schemaURL in _schemaURLs)
        {
            BuildSchemaFromURL(schemaURL);
        }
    }

    private static async void BuildSchemaFromURL(string schemaURL)
    {
        var fetchSchemaTask = FetchSchema(schemaURL);
        await fetchSchemaTask;

        Debug.Log(fetchSchemaTask.Result);

        // Load the JSON schema
        var schema = JsonSchema.FromJsonAsync(fetchSchemaTask.Result).Result;
        schema.AllowAdditionalProperties = false;

        var settings = new CSharpGeneratorSettings
        {
            Namespace = "Urchin.JSON", // Specify the desired namespace
            GenerateJsonMethods = false, // Disable generation of JSON methods
            GenerateDataAnnotations = false, // Disable data annotations
        };

        //Build the file
        var generator = new CSharpGenerator(schema, settings);
        var file = generator.GenerateFile();

        // Remove additional properties
        string apPattern = @"private\s\S*\s\S*\s_additionalProperties;";
        Match match = Regex.Match(file, apPattern, RegexOptions.Multiline);
        while (match.Success)
        {
            // Get the start index of the matched line
            int startIndex = file.IndexOf(match.Value);

            // Find the end index of the line
            int endIndex = file.IndexOf('\n', startIndex);

            // Remove the next six lines following the matched line
            for (int i = 0; i < 7; i++)
            {
                endIndex = file.IndexOf('\n', endIndex + 1);
            }

            // Remove the lines
            file = file.Remove(startIndex, endIndex - startIndex + 1);

            // Try again
            match = Regex.Match(file, apPattern, RegexOptions.Multiline);
        }

        // Make the file a struct instead of a class
        file = file.Replace("partial class", "struct");
        file = file.Replace("double", "float");

        // Remove all instances of [...]
        string pattern = @"\[[^\]]*\]";
        file = Regex.Replace(file, pattern, "");

        // Remove any public struct X {} patterns, where X then gets turned into a Vector3
        string vec3pattern = @"public struct (\w+)\s*{[^}]*public float X { get; set; }[^}]*public float Y { get; set; }[^}]*public float Z { get; set; }[^}]*}";
        var matches = Regex.Matches(file, vec3pattern);

        foreach (Match cMatch in matches)
        {
            if (cMatch.Success)
            {
                string vec3Name = cMatch.Groups[1].Value;
                file = Regex.Replace(file, $@"{vec3Name}\s{vec3Name}", $"Vector3 {vec3Name}");
            }
        }

        // Remove the classes
        file = Regex.Replace(file, vec3pattern, "");

        // Remove any public struct X {} patterns, where X then gets turned into a Vector2
        string vec2pattern = @"public struct (\w+)\s*{[^}]*public float X { get; set; }[^}]*public float Y { get; set; }[^}]*}";
        var matches2 = Regex.Matches(file, vec2pattern);

        foreach (Match cMatch in matches2)
        {
            if (cMatch.Success)
            {
                string vec2Name = cMatch.Groups[1].Value;
                file = Regex.Replace(file, $@"{vec2Name}\s{vec2Name}", $"Vector2 {vec2Name}");
            }
        }

        // Remove the classes
        file = Regex.Replace(file, vec2pattern, "");

        file = "using UnityEngine;\n" + file;

        // Strip extra new lines characters
        file = Regex.Replace(file, @"(\n{2,})", "\n");

        string outputFilePath = Path.Join(_schemaOutputFolder, $"{schema.Title}.cs");
        File.WriteAllText(outputFilePath, file);
    }

    private static async Task<string> FetchSchema(string schemaURL)
    {
        using (var httpClient = new HttpClient())
        {
            try
            {
                // Fetch the JSON schema content
                HttpResponseMessage response = await httpClient.GetAsync(schemaURL);
                response.EnsureSuccessStatusCode();
                string jsonSchemaString = await response.Content.ReadAsStringAsync();
                return jsonSchemaString;
            }
            catch (HttpRequestException)
            {
                return null; // Failed to fetch
            }
        }
    }
}

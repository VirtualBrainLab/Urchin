using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Urchin.Utils
{
    public class Colormaps : MonoBehaviour
    {
        #region Events
        public UnityEvent<Colormap> MainColormapChangedEvent;
        #endregion

        #region Colormaps

        // Colormaps
        public static Dictionary<string, Colormap> ColormapDict { get; private set; }
        public static Colormap MainColormap { get; private set; }

        // Starting colors
        private Color teal = new Color(0f, 1f, 1f, 1f);
        private Color magenta = new Color(1f, 0f, 1f, 1f);
        private Color lightgreen = new Color(14f / 255f, 1f, 0f, 1f);
        private Color darkgreen = new Color(6f / 255f, 59 / 255f, 0f, 1f);
        private Color lightpurple = new Color(202f / 255f, 105f / 255f, 227f / 255f, 1f);
        private Color darkpurple = new Color(141f / 255f, 10f / 255f, 157f / 255f, 1f);
        private Color lightred = new Color(1f, 165f / 255f, 0f, 1f);
        private Color darkred = new Color(1f, 0f, 0f, 1f);
        #endregion

        #region Unity
        private void Awake()
        {
            ColormapDict = new();
            ColormapDict.Add("main", MainColormap);
            ColormapDict.Add("cool", new Colormap("cool", teal, magenta));
            ColormapDict.Add("greyscale", new Colormap("greyscale", Color.black, Color.white));
            ColormapDict.Add("grey-green", new Colormap("grey-green", lightgreen, darkgreen, true));
            ColormapDict.Add("grey-purple", new Colormap("grey-purple", lightpurple, darkpurple, true));
            ColormapDict.Add("grey-red", new Colormap("grey-red", lightred, darkred, true));
            ColormapDict.Add("grey-rainbow", new Colormap("grey-rainbow", Color.black, Color.black, true, true));
            MainColormap = ColormapDict["cool"];
        }
        #endregion

        #region Colormap functions

        public void ChangeMainColormap(string newColormapName)
        {
            if (ColormapDict.ContainsKey(newColormapName))
                MainColormap = ColormapDict[newColormapName];
            else
                Debug.Log("Colormap " + newColormapName + " not an available option");

            MainColormapChangedEvent.Invoke(MainColormap);
        }
        #endregion
    }
    public struct Colormap
    {
        public string Name;
        public Color Stop0;
        public Color Stop1;
        bool Grey0;
        bool Rainbow;

        public Colormap(string name, Color stop0, Color stop1, bool grey0 = false, bool rainbow = false)
        {
            Name = name;
            Stop0 = stop0;
            Stop1 = stop1;
            Grey0 = grey0;
            Rainbow = rainbow;
        }

        public Color Value(float perc)
        {
            if (Grey0 && perc == 0)
                return Color.grey;

            if (Rainbow)
            {
                float red = Mathf.Abs(2f * perc - 0.5f);
                float green = Mathf.Sin(perc * Mathf.PI);
                float blue = Mathf.Cos(perc * Mathf.PI / 2);
                return new Color(red, green, blue);
            }

            return Color.Lerp(Stop0, Stop1, perc);
        }
    }

}

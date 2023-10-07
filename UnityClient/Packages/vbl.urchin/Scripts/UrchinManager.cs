using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Urchin
{
    public class UrchinManager : MonoBehaviour
    {
        #region Events
        public UnityEvent<Converter<float, Color>> ColormapChangeEvent;
        #endregion

        #region Colormaps

        // Colormaps
        private List<Colormap> colormaps;
        private Colormap activeColormap;

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
            colormaps = new();
            colormaps.Add(new Colormap("cool", teal, magenta));
            colormaps.Add(new Colormap("grayscale", Color.black, Color.white));
            colormaps.Add(new Colormap("gray-green", lightgreen, darkgreen, true));
            colormaps.Add(new Colormap("gray-purple", lightpurple, darkpurple, true));
            colormaps.Add(new Colormap("gray-red", lightred, darkred, true));
            colormaps.Add(new Colormap("gray-rainbow", Color.black, Color.black, true, true));
            activeColormap = colormaps[0];
        }
        #endregion

        #region Colormap functions

        public Color GetColormapColor(float perc)
        {
            return activeColormap.Value(perc);
        }

        public void ChangeColormap(string newColormapName)
        {
            try
            {
                activeColormap = colormaps.Find(x => x.Name.Equals(newColormapName));
            }
            catch
            {
                Debug.Log("Colormap " + newColormapName + " not an available option");
            }

            ColormapChangeEvent.Invoke(activeColormap.Value);
        }

        public float CheckColormapRange(float perc)
        {
            return Mathf.Clamp(perc, 0, 1);
        }

        public Color GreyGradient(float perc, Vector3 lightcolor, Vector3 darkcolor)
        {
            if (perc == 0)
                return Color.grey;
            else
            {
                Vector3 colorVector = Vector3.Lerp(lightcolor, darkcolor, perc);
                return new Color(colorVector.x, colorVector.y, colorVector.z, 1f);
            }
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

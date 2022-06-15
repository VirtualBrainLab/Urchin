# Unity Renderer for Neuroscience

<p float="left">
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/flatmap_layout.png" width="25%"> 
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/data_onesided.png" width="25%"> 
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/RS_fig1.png " width="35%">
</p>

<p float="center">
 <img src="https://github.com/dbirman/UnityNeuroscience/raw/main/Examples/gallery/brain_rotate_cropped.gif" width="45%"> 
</p>

This project allows you to connect your Python scripts to a standalone "mouse brain renderer" program, to create graphics like the ones above.

# Quickstart

To install the package run `pip install unityneuro`. To get started open a Python terminal and run:

```
import unityneuro.render as urn
urn.setup()
urn.set_area_visibility({"grey":True})
```

# Documentation

For detailed instructions please see the [documentation](https://virtualbrainlab.org/build/html/03_unity_neuro/01_urn_intro.html).

# Citing

If you use this to make figures for a publication you should cite this repo, email me (dbirman@uw.edu) and I can generate a DOI for the version you are using.
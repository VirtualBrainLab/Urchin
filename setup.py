from setuptools import setup

# Copyright (C) 2022 - Daniel Birman
LONG_DESCRIPTION = """\
Unity Renderer for Neuroscience is a python package for making interactive 3D visualizations of neuroscience data.
"""

setup(
    name='unityneuro',
    version='0.4.0',    
    description='Urchin - Unity Renderer for Neuroscience Python API',
    long_description=LONG_DESCRIPTION,
    url='https://virtualbrainlab.org/build/html/03_unity_neuro/01_urn_intro.html',
    download_url='https://github.com/VirtualBrainLab/UnityNeuroscience',
    author='Daniel Birman',
    author_email='danbirman@gmail.com',
    license='GNU GPLv3',
    packages=['unityneuro'],
    install_requires=['python-socketio[client]','numpy'],

    classifiers=[
        'Intended Audience :: Science/Research',
        'Topic :: Scientific/Engineering :: Visualization',
        'Topic :: Multimedia :: Graphics',
        'License :: OSI Approved :: GNU General Public License v3 (GPLv3)',  
        'Operating System :: OS Independent',    
        'Programming Language :: Python :: 3.9',
    ],
)

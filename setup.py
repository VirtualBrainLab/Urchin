from setuptools import setup

setup(
    name='unityneuro',
    version='0.1.0',    
    description='Unity Renderer for Neuroscience Python API',
    url='https://github.com/dbirman/UnityNeuroscience',
    author='Daniel Birman',
    author_email='danbirman@gmail.com',
    license='GNU GPLv3',
    packages=['unityneuro'],
    install_requires=['python-socketio[client]','numpy'],

    classifiers=[
        'License :: GNU GPLv3',  
        'Operating System :: OS Independent',    
        'Programming Language :: Python :: 3.9',
    ],
)

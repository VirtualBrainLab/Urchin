from setuptools import setup

setup(
    name='unitymouse',
    version='0.1.0',    
    description='Unity mouse brain renderer python link',
    url='https://github.com/dbirman/UMRenderer',
    author='Daniel Birman',
    author_email='danbirman@gmail.com',
    license='GNU GPLv3',
    packages=['unitymouse'],
    install_requires=['python-socketio[client]'],

    classifiers=[
        'License :: GNU GPLv3',  
        'Operating System :: OS Independent',    
        'Programming Language :: Python :: 3.9',
    ],
)

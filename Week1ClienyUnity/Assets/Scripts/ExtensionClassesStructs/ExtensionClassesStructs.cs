using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//THIS SCRIPT HANDLES ALL EXTENSIONS TO THE CODE

namespace Extensions.Vector
{
    //this class creates a Vector3 that is serializable, since its mandatory to be able to send serializable data through the network, and the normal vector 3 isnt serializable
    [Serializable]
    public class Vector3Serializable
    {
        public float x, y, z;

        public Vector3Serializable(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3Serializable() { }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
public static class ExtensionClassesStructs
{
   
}


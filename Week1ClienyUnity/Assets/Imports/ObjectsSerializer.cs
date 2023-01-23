using UnityEngine;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

//THIS CODE WAS TAKEN FROM:https://bitbucket.org/stupro_hskl_betreuung_kessler/learnit_merged_ss16/raw/e5244ebb38c8fe70759e632ea4224e48f5ca5833/Unity/LearnIT_Merged/Assets/Scripts/Util/ObjectSerializationExtension.cs
//I USE THIS CODE TO BE ABLE TO SERIALIZE AN OBJECT INTO A BYTE ARRAY

namespace Serializer
{
    //Extension class to provide serialize / deserialize methods to object.
    //src: http://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
    //NOTE: You need add [Serializable] attribute in your class to enable serialization
    public static class ObjectsSerializer
    {
        
        public static byte[] SerializeToByteArray(this object obj)
        {
            if (obj == null)
            {
                return null;
            }
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(this byte[] byteArray) where T : class
        {
            if (byteArray == null)
            {
                return null;
            }
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(byteArray, 0, byteArray.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = (T)binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}

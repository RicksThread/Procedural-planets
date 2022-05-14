using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
public class BinarySerializer
{
    public static BinaryFormatter GetFormatter()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        //declaring and initializing hte surrogates 
        Vector3Surrogate vec3Surrogate = new Vector3Surrogate();
        Vector4Surrogate vec4Surrogate = new Vector4Surrogate();
        QuaternionSurrogate quatSurrogate = new QuaternionSurrogate();

        //assing such surrogates
        SurrogateSelector surrogateSelector = new SurrogateSelector();
        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), vec3Surrogate);
        surrogateSelector.AddSurrogate(typeof(Vector4), new StreamingContext(StreamingContextStates.All), vec4Surrogate);
        surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), quatSurrogate);

        binaryFormatter.SurrogateSelector = surrogateSelector;

        return binaryFormatter;
    }

}

public class Vector3Surrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector3 vector = (Vector3)obj;
        info.AddValue("x", vector.x);
        info.AddValue("y", vector.y);
        info.AddValue("z", vector.z);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector3 vector = (Vector3)obj;

        vector.x = (float)info.GetValue("x", typeof(float));
        vector.y = (float)info.GetValue("y", typeof(float));
        vector.z = (float)info.GetValue("z", typeof(float));

        return vector;
    }
}
public class Vector4Surrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Vector4 vector = (Vector4)obj;
        info.AddValue("x", vector.x);
        info.AddValue("y", vector.y);
        info.AddValue("z", vector.z);
        info.AddValue("W", vector.w);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Vector4 vector = (Vector4)obj;

        vector.x = (float)info.GetValue("x", typeof(float));
        vector.y = (float)info.GetValue("y", typeof(float));
        vector.z = (float)info.GetValue("z", typeof(float));
        vector.w = (float)info.GetValue("W", typeof(float));

        return vector;
    }
}
public class QuaternionSurrogate : ISerializationSurrogate
{
    public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        Quaternion quaternion = (Quaternion)obj;
        info.AddValue("x", quaternion.x);
        info.AddValue("y", quaternion.y);
        info.AddValue("z", quaternion.z);
        info.AddValue("W", quaternion.w);
    }

    public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        Quaternion quaternion = (Quaternion)obj;

        quaternion.z = (float)info.GetValue("x", typeof(float));
        quaternion.y = (float)info.GetValue("y", typeof(float));
        quaternion.z = (float)info.GetValue("z", typeof(float));
        quaternion.w = (float)info.GetValue("W", typeof(float));

        return quaternion;
    }
}
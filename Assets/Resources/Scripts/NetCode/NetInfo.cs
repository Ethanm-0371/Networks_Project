using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface NetInfo
{
    //This function serializes the class from which
    //it has been called and returns it as a byte[].
    public byte[] Serialize();

    //This function is called from a new instance of the target class
    //and fills it with the information in the byte[].
    public void Deserialize(byte[] data);
}

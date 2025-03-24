using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Exporter : MonoBehaviour
{
    [SerializeField] string path;
    public void Export(Texture2D texture)
    {
        File.WriteAllBytes(path, texture.EncodeToPNG());
    }
}

using MelonLoader;
using UnityEngine;

namespace Personality;

[RegisterTypeInIl2Cpp]
public class Chirality : MonoBehaviour
{
    public Chirality(IntPtr ptr) : base(ptr) { }
    public bool isLeftHanded;
    void LateUpdate()
    {
        if (gameObject != null && Settings.options.leftHanded)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
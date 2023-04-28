using MelonLoader;
using UnityEngine;

[RegisterTypeInIl2Cpp]
public class Chirality : MonoBehaviour
{
    public Chirality(IntPtr ptr) : base(ptr) { }
    public bool isLeftHanded;
    void LateUpdate()
    {
        if (gameObject != null && isLeftHanded)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }
}
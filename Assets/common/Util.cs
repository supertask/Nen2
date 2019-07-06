using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static GameObject FindRecursively(GameObject target, string name)
    {
        foreach (Transform child in target.GetComponentsInChildren<Transform>()) {
            if (child.gameObject.name == name) { return child.gameObject; }
        }
        return null;
    }
}

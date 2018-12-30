﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New String List")]
public class StringList : ScriptableObject {

    [SerializeField] List<string> stringList = new List<string>();

    public string this[int i]
    {
       get
        {
            if (i < stringList.Count)
            {
                return stringList[i];
            }
            return null;
        }
    }

    public int count
    {
        get
        {
            return stringList.Count;
        }
    }

}

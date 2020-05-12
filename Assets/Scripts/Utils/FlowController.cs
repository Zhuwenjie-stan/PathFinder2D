﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowController : MonoBehaviour
{
    void Awake()
    {
        GameMgr.Instance.Init();
    }

    private void Update()
    {
        GameMgr.Instance.Update();
    }
}

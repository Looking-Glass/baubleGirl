//Copyright 2017 Looking Glass Factory Inc.
//All rights reserved.
//Unauthorized copying or distribution of this file, and the source code contained herein, is strictly prohibited.

using System.Collections;
using System.Collections.Generic;

using HoloPlaySDK;

using UnityEngine;

namespace HoloPlaySDK_UI
{
    public class CalibratedPointer : MonoBehaviour
    {
        private bool autoScale = true;
        public float pointerScale = 1;

        void Start()
        {
            if (autoScale)
                transform.localScale = HoloPlay.Main.size * Vector3.one * .1f * pointerScale;
        }

        void Update()
        {
            transform.position = RealsenseCalibrator.Instance.GetWorldPos(0);
        }
    }
}
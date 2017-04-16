﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a container for slice 'modifiers'.
/// Slice modifiers are intended to be used to apply a GUI or other overlay to particular slices.
/// Be aware that when displaying on different devices its possible that multiple overlays could collide on a slice.
/// In such cases only one of the overlays will work, so overlays should not be placed too close together unless the application is device specific (for example: the application is only intended to work on 10 slice Volumes).
/// </summary>

namespace hypercube
{
    [ExecuteInEditMode]
    [System.Serializable]
    public class sliceModifier
    {
        [Tooltip("The slice that this slice modifier should apply to.\nDifferent models of Volume have different amounts of slices. This is used to select the appropriate slice between front (0) and back (1).\n\nNOTE: to change this value from code in realtime, use setDepth(d) on the sliceModifier.")]
        [Range(0f, 1f)]
        public float depth; //0-1 front to back
        private float _lastDepth = -1; //used to detect change in value, while still allowing depth editability in inspector (don't use get/set)
        [Tooltip("How should this modification be blended with the rendered slice?\n\nNONE:\nNo blending, the normal slice is used and the given texture is ignored.\n\nOVER:\nThe given texture is alpha blended on top of the chosen slice.\n\nUNDER:\nThe given texture is alpha blended below the chosen slice.\n\nADD:\nThe pixel value of the given texture and the slice are added together (made brighter).\n\nMULTIPLY:\nThe pixel value of the given texture and the slice are multiplied (made darker).\n\nREPLACE:\nThe rendered slice is ignored, and the given texture is used instead.")]
        public slicePostProcess.blending blend;
        [Tooltip("The modification. Put the texture that you want to blend with the desired slice. It can be a renderTexture.")]
        public Texture tex;

        /// <summary>
        /// Use this call when dynamically setting depth from code.
        /// This way, it will get updated properly.
        /// </summary>
        /// <param name="d">What depth value to set this modifier to. 0 = front, 1 = back</param>
        public void setDepth(float d)
        {
            depth = Mathf.Clamp(d, 0f, 1f);      
            updateSliceModifiers();     
        }

        //keep track of all modifiers.
        static sliceModifier[] allModifiers = null;
        static public bool areModifiersNull() { if (allModifiers == null) return true; if (allModifiers[0] == null) return true; return false; }

        public static sliceModifier getSliceModifier(int sliceNum)
        {
            if (allModifiers == null)
                return null;

            if (sliceNum >= allModifiers.Length)
                return null;

            return allModifiers[sliceNum];
        }

        public static void updateSliceModifiers()
        {
            if (hypercubeCamera.mainCam)
                updateSliceModifiers(hypercubeCamera.mainCam.sliceModifiers);
            else
                allModifiers = null;
        }
        public static void updateSliceModifiers(List<sliceModifier> mods)
        {
            if (castMesh.canvas)
                updateSliceModifiers(castMesh.canvas.getSliceCount(), mods);
            else
                allModifiers = null;
        }
        public static void updateSliceModifiers(int sliceCount, List<sliceModifier> mods)
        {
            if (sliceCount < 1 || mods == null) //probably still starting up, this is bogus.
            {
                allModifiers = null;
                return;
            }

            allModifiers = new sliceModifier[sliceCount];

            foreach (sliceModifier m in mods)
            {
                int s = m.getSlice(sliceCount);
                allModifiers[s] = m;
            }
        }

        int slice = -1;
        public int updateSlice()
        {
            if (!hypercube.castMesh.canvas)
            {
                slice = -1;
                return slice;
            }
            return updateSlice(hypercube.castMesh.canvas.getSliceCount());
        }
        public int updateSlice(int totalSlices)
        {
            if (depth == 0f)
                slice = 0;
            else if (depth == 1f)
                slice = totalSlices - 1;
            else
                slice = Mathf.RoundToInt(Mathf.Lerp(0, totalSlices - 1, depth));

            _lastDepth = depth;

            return slice;
        }

        public int getSlice(int totalSlices)
        {
            if (depth == _lastDepth)
                return slice;

            return updateSlice(totalSlices);
        }
    }
}

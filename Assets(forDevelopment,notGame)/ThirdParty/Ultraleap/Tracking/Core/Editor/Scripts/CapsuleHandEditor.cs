/******************************************************************************
 * Copyright (C) Ultraleap, Inc. 2011-2024.                                   *
 *                                                                            *
 * Use subject to the terms of the Apache License 2.0 available at            *
 * http://www.apache.org/licenses/LICENSE-2.0, or another agreement           *
 * between Ultraleap and you, your company or other organization.             *
 ******************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Leap
{
    [CustomEditor(typeof(CapsuleHand), editorForChildClasses: true), CanEditMultipleObjects]
    public class CapsuleHandEditor : CustomEditorBase<CapsuleHand>
    {
        public override void OnInspectorGUI()
        {
            CapsuleHand targ = target;
            CapsuleHand.CapsuleHandPreset newValue = (CapsuleHand.CapsuleHandPreset)EditorGUILayout.EnumPopup("Preset", targ.Preset);

            if (newValue != targ.Preset)
            {
                targ.ChangePreset(newValue);
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }
    }
}
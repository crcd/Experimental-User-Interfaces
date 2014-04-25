/*****************************************************************************

Content    :   Inspector behaviour for RUISCharacterController script
Authors    :   Mikael Matveinen, Tuukka Takala
Copyright  :   Copyright 2013 Mikael Matveinen, Tuukka Takala. All Rights reserved.
Licensing  :   RUIS is distributed under the LGPL Version 3 license.

******************************************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(RUISCharacterController))]
[CanEditMultipleObjects]
public class RUISCharacterControllerEditor : Editor
{
	int maxPSMoveControllers = 4;
    SerializedProperty characterPivotType;
	SerializedProperty kinectPlayerId;
    SerializedProperty moveControllerId;
    SerializedProperty ignorePitchAndRoll;
    SerializedProperty groundLayers;
    SerializedProperty groundedErrorTweaker;
	SerializedProperty dynamicFriction;
	SerializedProperty dynamicMaterial;
	SerializedProperty psmoveOffset;
	SerializedProperty feetAlsoAffectGrounding;

    public void OnEnable()
    {
        characterPivotType = serializedObject.FindProperty("characterPivotType");
		kinectPlayerId = serializedObject.FindProperty("kinectPlayerId");
        moveControllerId = serializedObject.FindProperty("moveControllerId");
        ignorePitchAndRoll = serializedObject.FindProperty("ignorePitchAndRoll");
        groundLayers = serializedObject.FindProperty("groundLayers");
        groundedErrorTweaker = serializedObject.FindProperty("groundedErrorTweaker");
        dynamicFriction = serializedObject.FindProperty("dynamicFriction");
        dynamicMaterial = serializedObject.FindProperty("dynamicMaterial");
		psmoveOffset = serializedObject.FindProperty("psmoveOffset");
		feetAlsoAffectGrounding = serializedObject.FindProperty("feetAlsoAffectGrounding");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(characterPivotType, new GUIContent(  "Character Pivot Type", "Rotation pivot for the character, in other "
		                                                                 + "words, what is the rotation center for the character when turning with "
		                                                                 + "the " + typeof(RUISCharacterLocomotion).Name + " script. Torso is "
		                                                                 + "recommended for Kinect."));
		
		
        EditorGUI.indentLevel += 2;
        switch (characterPivotType.enumValueIndex)
        {
			
			case (int)RUISCharacterController.CharacterPivotType.KinectHead:
				EditorGUILayout.LabelField(new GUIContent(	  "Kinect Player ID " + kinectPlayerId.intValue, "You can change this value from " 
				                                          + typeof(RUISSkeletonController).ToString() + " script that is in one of the child objects."));
				break;
			case (int)RUISCharacterController.CharacterPivotType.KinectTorso:
           		EditorGUILayout.LabelField(new GUIContent(	  "Kinect Player ID " + kinectPlayerId.intValue, "You can change this value from " 
                          									+ typeof(RUISSkeletonController).ToString() + " script that is in one of the child objects."));
				break;
            case (int)RUISCharacterController.CharacterPivotType.MoveController:
			{
				moveControllerId.intValue = Mathf.Clamp(moveControllerId.intValue, 0, maxPSMoveControllers - 1);
                EditorGUILayout.PropertyField(moveControllerId, new GUIContent("PS Move ID", "Between 0 and 3"));
                EditorGUILayout.PropertyField(psmoveOffset, new GUIContent(   "Position Offset (meters)", "PS Move controller's position in "
                															+ "the tracked pivot's local coordinate system. Set these values "
																			+ "according to the controller's offset from the tracked pivot's "
																			+ "origin (torso center point etc.)."));
                break;
			}

        }
        EditorGUI.indentLevel -= 2;

        EditorGUILayout.PropertyField(ignorePitchAndRoll, new GUIContent(  "Ignore Pitch and Roll", "Should the pitch and roll values of the pivot "
																		 + "rotation be taken into account when transforming directions into character "
		                                                                 + "coordinates? In most cases this should be enabled."));

        EditorGUILayout.PropertyField(groundLayers, new GUIContent(  "Ground Layers", "The layers to take into account when checking whether the character is grounded "
																   + "(and able to jump)."));

        EditorGUILayout.PropertyField(groundedErrorTweaker, new GUIContent("Ground Distance Tweaker", "This value (in meters) can be adjusted to allow for some "
		                                                                   + "leniency in the checks whether the character is grounded. Should be above zero."));
		
        EditorGUILayout.PropertyField(dynamicFriction, new GUIContent(  "Dynamic Friction", "Enable this if you want the character collider to switch "
																	  + "to a different Physics Material whenever the character is not grounded. We "
																	  + "recommend that you enable this."));
		
		if(dynamicFriction.boolValue)
		{
			EditorGUI.indentLevel += 2;
	        EditorGUILayout.PropertyField(dynamicMaterial, new GUIContent(  "Dynamic Material", "We recommend that you leave this to None. Then a "
																		  + "frictionless material will be used and the character won't be able to "
																		  + "climb walls with friction, and he will slide down steep hills." ));
			EditorGUI.indentLevel -= 2;
		}
		
        EditorGUILayout.PropertyField(feetAlsoAffectGrounding, new GUIContent(  "Feet Affect Grounding", "When this option is disabled, the "
																		  + "avatar is grounded (and able to jump) only if its Stabilizing Collider is "
																		  + "standing on a collider from Ground Layers. By enabling this option, "
																		  + "the avatar will also be grounded when at least one of its feet is "
																		  + "standing on a non-kinematic Rigidbody from Ground Layers. We recommend "
																	  	  + "that you enable this."  ));
        serializedObject.ApplyModifiedProperties();
    }
}

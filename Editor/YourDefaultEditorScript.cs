using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PabloSorribes.FmodHelper
{
	[CustomEditor(typeof(FmodHelperExamples))]
	public class YourDefaultEditorScript : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			var script = (FmodHelperExamples)target;

			if (GUILayout.Button("Get default paths info"))
			{
				Object jsonFile = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(script.assemblyDefinitionDefaultPaths));
				var assemblyDef = JsonUtility.FromJson<DefaultValues>(jsonFile.ToString());

				Debug.Log($"Path Info: '{assemblyDef.DefaultFmodPluginPath}'");
				Debug.Log($"Path Info: '{assemblyDef.PaaloFmod_Core_AssemblyHijacker_Path}'");
				Debug.Log($"Path Info: '{assemblyDef.PaaloFmod_Editor_AssemblyHijacker_Path}'");
				Debug.Log($"Path Info: '{assemblyDef.PaaloFmod_Timeline_AssemblyHijacker_Path}'");
				Debug.Log($"Path Info: '{assemblyDef.PaaloFmod_TimelineEditor_AssemblyHijacker_Path}'");
			}


			if (GUILayout.Button("Get assembly definition template info"))
			{
				//Object jsonFile = AssetDatabase.LoadAssetAtPath<Object>("Assets/com.pablosorribes.fmodhelper/Assembly Definition Templates/PaaloFmod_TimelineEditor_AssemblyHijacker.json");

				Object jsonFile = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GetAssetPath(script.assemblyDefinitionJsonTemplate));
				var assemblyDef = JsonUtility.FromJson<AssemblyInJsonFormat>(jsonFile.ToString());

				Debug.Log($"Assembly Name: '{assemblyDef.name}'");
				for (int i = 0; i < assemblyDef.references.Length; i++)
				{
					Debug.Log($"Assembly Ref {i}: '{assemblyDef.references[i]}'");
				}
			}
		}
	}

	[System.Serializable]
	public class AssemblyInJsonFormat
	{
		public string name = "";
		public string[] references = null;
	}

	[System.Serializable]
	public class DefaultValues
	{
		public string DefaultFmodPluginPath = "";
		public string PaaloFmod_Core_AssemblyHijacker_Path = "";
		public string PaaloFmod_Editor_AssemblyHijacker_Path = "";
		public string PaaloFmod_Timeline_AssemblyHijacker_Path = "";
		public string PaaloFmod_TimelineEditor_AssemblyHijacker_Path = "";
	}
}
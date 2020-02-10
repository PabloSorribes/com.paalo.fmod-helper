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
		}
	}
}
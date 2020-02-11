using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PabloSorribes.FmodHelper
{
	public class FmodHelperExamples : MonoBehaviour
	{
		public Object assemblyDefinitionDefaultPaths;
		public Object assemblyDefinitionJsonTemplate;

		private void Start()
		{
			PabloSorribes.FmodHelper.AudioManager.Instance.PlayOneShot("event:/TestEvent");

		}

	}
}
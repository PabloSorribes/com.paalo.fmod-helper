using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paalo.FmodHelper
{
	public class FmodHelperExamples : MonoBehaviour
	{
		public Object assemblyDefinitionDefaultPaths;
		public Object assemblyDefinitionJsonTemplate;

		private void Start()
		{
			Paalo.FmodHelper.AudioManager.Instance.PlayOneShot("event:/TestEvent");

		}

	}
}
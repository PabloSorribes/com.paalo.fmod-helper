using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PabloSorribes.FmodHelper
{
	public class FmodHelperExamples : MonoBehaviour
	{
		private void Start()
		{
			PabloSorribes.FmodHelper.AudioManager.Instance.PlayOneShot("event:/TestEvent");

		}

	}
}
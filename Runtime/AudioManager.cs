using UnityEngine;

namespace Paalo.FmodHelper
{
	/// <summary>
	/// Singleton wrapper-class for accessing the most common functions needed from Fmod in a more convenient way. 
	/// <para></para>
	/// This way you don't have to know the different classes in Fmod and just get this class's "Instance" to use easy one-line-functions such as:
	/// <para></para>
	/// - Playing a OneShot-sound with: <see cref="PlayOneShot(string)"/> for 2D-sounds, and <see cref="PlayOneShot3D(string, UnityEngine.Vector3)"/> for 3D-sounds.
	/// <para></para>
	/// - Initializing StudioEventEmitters with: <see cref="InitializeAudioOnObject(UnityEngine.GameObject, string)"/>
	/// <para></para>
	/// - Playing an emitter only if isn't already playing with: <see cref="PlayEmitterOnce(FMODUnity.StudioEventEmitter)"/>
	/// </summary>
	public class AudioManager : GenericSingleton<AudioManager>
	{
		#region BUSSES

		public enum AudioBusses { mainBus, musicBus, sfxBus }

		public FMOD.Studio.Bus[] AllBusses => new FMOD.Studio.Bus[] { masterBus, musicBus, sfxBus, sfxInGameBus };

		private FMOD.Studio.Bus masterBus;
		public FMOD.Studio.Bus musicBus;
		public FMOD.Studio.Bus sfxBus;
		public FMOD.Studio.Bus sfxInGameBus;
		private float busVolumeToSet = 1;

		#endregion BUSSES

		private void Awake()
		{
			SetAudioPlaysInBackground();
			InitializeBuses();
		}

		/// <summary>
		/// Makes Audio run in background (when Alt-Tabbing for live-mixing and so on).
		/// </summary>
		private void SetAudioPlaysInBackground(bool playInBackground = true)
		{
			Application.runInBackground = playInBackground;
		}

		private void InitializeBuses()
		{
			masterBus = FMODUnity.RuntimeManager.GetBus("bus:/MAIN_OUT");
			musicBus = FMODUnity.RuntimeManager.GetBus("bus:/MAIN_OUT/MU_OUT");
			sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/MAIN_OUT/SFX_OUT");
			sfxInGameBus = FMODUnity.RuntimeManager.GetBus("bus:/MAIN_OUT/SFX_OUT/SFX_InGame");
		}

		/// <summary>
		/// <paramref name="mute"/> → TRUE for turning off audio. FALSE for turning it back on.
		/// </summary>
		public void MuteAudio(AudioBusses busToMute, bool mute)
		{
			switch (busToMute)
			{
				case AudioBusses.mainBus:
					masterBus.setMute(mute);
					break;

				case AudioBusses.musicBus:
					musicBus.setMute(mute);
					break;

				case AudioBusses.sfxBus:
					sfxBus.setMute(mute);
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// "<paramref name="volumeToChangeWith"/>" should preferably be something small, such as 0.15f (volume increase) or -0.15f (volume decrease)
		/// <para></para> "<paramref name="volumeToChangeWith"/>" = 0 → No volume, -80 dB.
		/// <para></para> "<paramref name="volumeToChangeWith"/>" = 1 → Max volume, the level of the Bus's fader set in the Fmod-project.
		/// </summary>
		public void SetAudioVolume(AudioBusses busToChangeVolumeOn, float volumeToChangeWith)
		{
			busVolumeToSet += volumeToChangeWith;
			busVolumeToSet = Mathf.Clamp01(busVolumeToSet);

			switch (busToChangeVolumeOn)
			{
				case AudioBusses.mainBus:
					masterBus.setVolume(busVolumeToSet);
					break;

				case AudioBusses.musicBus:
					musicBus.setVolume(busVolumeToSet);
					break;

				case AudioBusses.sfxBus:
					sfxBus.setVolume(busVolumeToSet);
					break;

				default:
					break;
			}
			//print(busVolumeToSet);
		}

		public float GetAudioVolume(AudioBusses bus)
		{
			float volume = 0;
			float final;
			switch (bus)
			{
				case AudioBusses.mainBus:
					masterBus.getVolume(out volume, out final);
					break;

				case AudioBusses.musicBus:
					musicBus.getVolume(out volume, out final);
					break;

				case AudioBusses.sfxBus:
					sfxBus.getVolume(out volume, out final);
					break;

				default:
					break;
			}

			return volume;
		}

		/// <summary>
		/// Returns an instance to use in OneShot-functions.
		/// </summary>
		public FMOD.Studio.EventInstance CreateFmodEventInstance(string eventName)
		{
			return FMODUnity.RuntimeManager.CreateInstance(eventName);
		}

		/// <summary>
		/// Use for 2D-events. <paramref name="eventPath"/> is a string to the fmod event, eg. "event:/Arena/countDown".
		/// </summary>
		public void PlayOneShot(string eventPath)
		{
			FMODUnity.RuntimeManager.PlayOneShot(eventPath);
		}

		/// <summary>
		/// For setting a parameter before playing.
		/// </summary>
		public void PlayOneShot(string eventPath, string parameterName, float parameterValue)
		{
			var eventInstance = CreateFmodEventInstance(eventPath);

			eventInstance.setParameterValue(parameterName, parameterValue);
			eventInstance.start();
			eventInstance.release();
		}

		public void PlayOneShotAttached(string eventPath, GameObject gameObject, string parameterName = "", float parameterValue = 0f)
		{
			var transform = gameObject.GetComponent<Transform>();
			var rb = gameObject.GetComponent<Rigidbody>();

			var eventInstance = CreateFmodEventInstance(eventPath);

			eventInstance.setParameterValue(parameterName, parameterValue);

			eventInstance.start();
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInstance, transform, rb);
			eventInstance.release();
		}

		/// <summary>
		/// For playing 3D-events at a set position, without setting a parameter. "<paramref name="position"/>" could be your transform.position.
		/// </summary>
		public void PlayOneShot3D(string eventPath, Vector3 position)
		{
			FMODUnity.RuntimeManager.PlayOneShot(eventPath, position);
		}

		/// <summary>
		/// Use this for overriding the event's 3D-attenuation settings (the min/max distance of the sound, usually set in the 3D-panner of the event).
		/// </summary>
		public void PlayOneShot3D(string eventPath, Vector3 position, float minDistanceOverride, float maxDistanceOverride)
		{
			FMOD.Studio.EventInstance eventInstance = AudioManager.Instance.CreateFmodEventInstance(eventPath);

			AudioManager.Instance.Set3DAttenuationSettings(eventInstance, minDistanceOverride, maxDistanceOverride);

			eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
			eventInstance.start();
			eventInstance.release();
		}

		/// <summary>
		/// For playing 3D-events at a set position and setting a parameter before playing a 3D-event.
		/// </summary>
		public void PlayOneShot3D(string eventPath, Vector3 position, string parameterName, float parameterValue)
		{
			var eventInstance = CreateFmodEventInstance(eventPath);

			eventInstance.setParameterValue(parameterName, parameterValue);
			eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
			eventInstance.start();
			eventInstance.release();
		}

		/// <summary>
		/// For playing 3D-events at a set position and setting a parameter before playing a 3D-event.
		/// </summary>
		public void PlayOneShot3D(string eventPath, Vector3 position, string parameterName, float parameterValue, float minDistanceOverride, float maxDistanceOverride)
		{
			var eventInstance = CreateFmodEventInstance(eventPath);

			AudioManager.Instance.Set3DAttenuationSettings(eventInstance, minDistanceOverride, maxDistanceOverride);

			eventInstance.setParameterValue(parameterName, parameterValue);
			eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
			eventInstance.start();
			eventInstance.release();
		}

		/// <summary>
		/// Use to add a StudioEventEmitter on an object. Set your StudioEventEmitter-variable to this function.
		/// </summary>
		public FMODUnity.StudioEventEmitter InitializeAudioOnObject(GameObject gameObject, string eventPath)
		{
			var fmodEventEmitter = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
			fmodEventEmitter.Event = eventPath;
			return fmodEventEmitter;
			//TODO: Add the created event to an array or something.
		}

		/// <summary>
		/// Use to add a StudioEventEmitter on an object and overriding its Distance Attenuation Settings. 
		/// Set your StudioEventEmitter-variable to this function.
		/// </summary>
		public FMODUnity.StudioEventEmitter InitializeAudioOnObject(GameObject gameObject, string eventPath, float minDistanceOverride, float maxDistanceOverride)
		{
			var fmodEventEmitter = gameObject.AddComponent<FMODUnity.StudioEventEmitter>();
			fmodEventEmitter.Event = eventPath;
			AudioManager.Instance.Set3DAttenuationSettings(fmodEventEmitter, minDistanceOverride, maxDistanceOverride);
			return fmodEventEmitter;
		}

		/// <summary>
		/// Plays the emitter if it isn't playing already.
		/// </summary>
		public void PlayEmitterOnce(FMODUnity.StudioEventEmitter fmodComponent)
		{
			if (!fmodComponent.IsPlaying())
			{
				fmodComponent.Play();
			}
		}

		/// <summary>
		/// Either play or stop the inserted StudioEventEmitter-component.<para></para>
		/// TRUE = Play sound.
		/// <para></para> FALSE = Stop the sound.
		/// </summary>
		public void PlayStopSound(FMODUnity.StudioEventEmitter fmodComponent, bool playStop)
		{
			if (playStop)
			{
				fmodComponent.Play();
			}
			else
			{
				fmodComponent.Stop();
			}
		}

		/// <summary>
		/// Changes parameter values for the FMODUnity.StudioEventEmitter-object that is passed into the function.
		/// <para></para>
		/// This will only work for events that are currently playing, ie. you cannot set this before playing a OneShot. Use FMOD.Studio.CreateInstance for that instead.
		/// </summary>
		public void SetParameter(FMODUnity.StudioEventEmitter eventEmitter, string parameterName, float parameterValue)
		{
			eventEmitter.SetParameter(parameterName, parameterValue);
		}

		public void SetParameter(FMOD.Studio.EventInstance eventInstance, string parameterName, float parameterValue)
		{
			eventInstance.setParameterValue(parameterName, parameterValue);
		}

		public float GetParameterValue(FMODUnity.StudioEventEmitter eventEmitter, string parameterName)
		{
			return AudioManager.Instance.GetParameterValue(eventEmitter.EventInstance, parameterName);
		}

		public float GetParameterValue(FMOD.Studio.EventInstance eventInstance, string parameterName)
		{
			FMOD.Studio.ParameterInstance parameterInstance;
			float paramValue;

			eventInstance.getParameter(parameterName, out parameterInstance);
			parameterInstance.getValue(out paramValue);

			return paramValue;
		}

		/// <summary>
		/// <see cref="Vector2.x"/> = Minimum, <see cref="Vector2.y"/> = Maximum. 
		/// </summary>
		public Vector2 GetParameterRange(FMODUnity.StudioEventEmitter eventEmitter, string parameterName)
		{
			FMOD.Studio.ParameterInstance paramInstance;
			eventEmitter.EventInstance.getParameter(parameterName, out paramInstance);

			FMOD.Studio.PARAMETER_DESCRIPTION paramDescription;
			paramInstance.getDescription(out paramDescription);

			//Debug.Log($"ParamMax Before: {paramDescription.maximum}");
			//paramDescription.maximum = 40f;
			//Debug.Log($"ParamMax After: {paramDescription.maximum}");

			//FMOD.Studio.PARAMETER_DESCRIPTION paramDescription;
			//eventEmitter.EventDescription.getParameter(_Fmod.Params.height, out paramDescription);

			Vector2 parameterRange = new Vector2();
			parameterRange.x = paramDescription.minimum;
			parameterRange.y = paramDescription.maximum;
			return parameterRange;
		}

		public bool IsEventInstancePlaying(FMOD.Studio.EventInstance eventInstance)
		{
			FMOD.Studio.PLAYBACK_STATE playbackState;
			eventInstance.getPlaybackState(out playbackState);
			bool isPlaying = playbackState != FMOD.Studio.PLAYBACK_STATE.STOPPED;
			return isPlaying;
		}

		/// <summary>
		/// Doesn't seem to work until the 3rd or 4th frame for some reason.
		/// </summary>
		/// <param name="eventEmitter"></param>
		public void SetRandomTimelinePosition(FMODUnity.StudioEventEmitter eventEmitter)
		{
			SetRandomTimelinePosition(eventEmitter.EventInstance);
		}

		/// <summary>
		/// Doesn't seem to work until the 3rd or 4th frame for some reason.
		/// </summary>
		/// <param name="eventEmitter"></param>
		public void SetRandomTimelinePosition(FMOD.Studio.EventInstance eventInstance)
		{
			FMOD.Studio.EventDescription eventDescription;
			eventInstance.getDescription(out eventDescription);

			int eventLength;
			eventDescription.getLength(out eventLength);
			int randomTimelinePosition = Random.Range(0, eventLength);
			eventInstance.setTimelinePosition(randomTimelinePosition);
		}

		/// <summary>
		/// Overrides the StudioEventEmitter's min/max values to whatever the sound designer wants. 
		/// <paramref name="minDistance"/> cannot be greater than <paramref name="maxDistance"/>.
		/// </summary>
		/// <param name="eventEmitter"></param>
		/// <param name="minDistance"></param>
		/// <param name="maxDistance"></param>
		public void Set3DAttenuationSettings(FMODUnity.StudioEventEmitter eventEmitter, float minDistance, float maxDistance)
		{
			//Stop minDistance from being greater than maxDistance and vice versa.
			minDistance = Mathf.Clamp(minDistance, 0, maxDistance);
			maxDistance = Mathf.Max(minDistance, maxDistance);

			eventEmitter.OverrideAttenuation = true;
			eventEmitter.OverrideMinDistance = minDistance;
			eventEmitter.OverrideMaxDistance = maxDistance;
		}

		/// <summary>
		/// Doesn't seem to work.
		/// </summary>
		/// <param name="eventInstance"></param>
		/// <param name="minDistance"></param>
		/// <param name="maxDistance"></param>
		public void Set3DAttenuationSettings(FMOD.Studio.EventInstance eventInstance, float minDistance, float maxDistance)
		{
			//Stop minDistance from being greater than maxDistance and vice versa.
			minDistance = Mathf.Clamp(minDistance, 0, maxDistance);
			maxDistance = Mathf.Max(minDistance, maxDistance);

			//Set values.
			eventInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, minDistance);
			eventInstance.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
		}

		/// <summary>
		/// Doesn't seem to work.
		/// </summary>
		/// <param name="eventEmitter"></param>
		/// <param name="minDistance"></param>
		/// <param name="maxDistance"></param>
		public void Get3DAttenuationSettings(FMODUnity.StudioEventEmitter eventEmitter, out float minDistance, out float maxDistance)
		{
			Get3DAttenuationSettings(eventEmitter.EventInstance, out minDistance, out maxDistance);
		}

		/// <summary>
		/// Doesn't seem to work.
		/// </summary>
		/// <param name="eventInstance"></param>
		/// <param name="minDistance"></param>
		/// <param name="maxDistance"></param>
		public void Get3DAttenuationSettings(FMOD.Studio.EventInstance eventInstance, out float minDistance, out float maxDistance)
		{
			eventInstance.getProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, out minDistance);
			eventInstance.getProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, out maxDistance);
		}
	}
}
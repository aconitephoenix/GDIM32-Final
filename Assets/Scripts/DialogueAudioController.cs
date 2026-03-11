using System.Collections.Generic;
using UnityEngine;

public class DialogueAudioController : MonoBehaviour
{
    public List<AudioClip> _clips = new List<AudioClip>();
    [SerializeField] private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Plays the clip
    public void PlayClip()
    {
        _audioSource.Play();
    }

    // Stops the clip from playing
    public void StopClip()
    {
        _audioSource.Stop();
    }

    // Sets the clip list
    public void AddAudioClips(List<AudioClip> clips)
    {
        foreach (AudioClip clip in clips)
        {
            _clips.Add(clip);
        }
    }

    // Removes all clips from the list + resets the audio source's clip
    public void RemoveAudioClips()
    {
        _clips.Clear();
        _audioSource.clip = null;
    }

    // Sets the audio source's clip to a random clip in the list
    public void SetClip()
    {
        if (_clips.Count > 0)
        {
            _audioSource.clip = _clips[Random.Range(0, _clips.Count)];
        }
    }
}

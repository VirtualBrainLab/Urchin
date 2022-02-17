using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource mainAudio;

    private Utils util;

    private int audioSampleFreq = 44000;

    AudioClip IBL_GO_TONE;
    private float go_vol = 0.25f;
    private int go_freq = 5000;
    private float go_dur = 0.1f;
    AudioClip IBL_WN;
    private float wn_vol = 0.05f;
    private float wn_dur = 0.5f;

    private float ramp_dur = 0.01f;

    // Start is called before the first frame update
    void Start()
    {
        util = GameObject.Find("main").GetComponent<Utils>();

        float[] samples = CreatePureTone(go_freq, audioSampleFreq, go_dur, ramp_dur);
        IBL_GO_TONE = AudioClip.Create("GO_TONE", samples.Length, 2, audioSampleFreq, false);
        IBL_GO_TONE.SetData(samples, 0);
        float[] samples2 = CreateWhiteNoise(audioSampleFreq, wn_dur, ramp_dur);
        IBL_WN = AudioClip.Create("WHITENOISE", samples2.Length, 2, audioSampleFreq, false);
        IBL_WN.SetData(samples2, 0);
    }

    public void PlayGoTone()
    {
        mainAudio.PlayOneShot(IBL_GO_TONE, go_vol);
    }

    public void PlayWhiteNoise()
    {
        mainAudio.PlayOneShot(IBL_WN, wn_vol);
    }

    private float[] CreateWhiteNoise(int sampleFreq, float length, float ramp)
    {
        float[] samples = new float[(int)(sampleFreq * length)];
        int rampSamples = (int)ramp * sampleFreq;

        for (int i = 0; i < samples.Length; i++)
        {
            float value = util.GaussianNoise();
            if (i < rampSamples)
            {
                value *= Ramp(rampSamples, i, true);
            }
            else if (i > (samples.Length - rampSamples))
            {
                value *= Ramp(rampSamples, i, false);
            }
            samples[i] = value;
        }

        return samples;
    }

    private float[] CreatePureTone(int frequency, int sampleFreq, float length, float ramp)
    {
        float[] samples = new float[(int) (sampleFreq * length)];
        int rampSamples = (int) ramp * sampleFreq;

        for (int i =0; i < samples.Length; i++)
        {
            float value = Mathf.Sin(2 * Mathf.PI * i * frequency / sampleFreq);
            if (i < rampSamples)
            {
                value *= Ramp(rampSamples, i, true);
            }
            else if (i > (samples.Length - rampSamples))
            {
                value *= Ramp(rampSamples, i, false);
            }
            samples[i] = value;
        }

        return samples;
    }

    private float Ramp(int rampDur, int idx, bool up)
    {
        // Use cosine to go from 1->0
        float rampValue = Mathf.Cos(idx / rampDur * Mathf.PI) / 2 + 1;
        return up ? 1 - rampValue : rampValue;
    }
}

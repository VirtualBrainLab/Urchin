using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LickBehavior : MonoBehaviour
{
    public Animator lickAnimator;
    public Animator waterAnimator;

    public void Lick()
    {
        lickAnimator.Play("Lick");
    }

    public void Drop()
    {
        waterAnimator.Play("Drop_Grow");
    }

    private void DelayedLickEvents()
    {
        lickAnimator.Play("Lick");
    }
}

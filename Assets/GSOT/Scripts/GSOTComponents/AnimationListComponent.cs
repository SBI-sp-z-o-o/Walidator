using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AnimationListComponent : MonoBehaviour
{
    public List<AnimationClip> animations = new List<AnimationClip>();
    public new Animation animation;
    void Start()
    {
        animation = gameObject.GetComponentInChildren<Animation>();

        if (animation == null)
        {
            animation = gameObject.AddComponent<Animation>();
        }
    }

    public void LoadAnimations(string name)
    {
        animations = Assets.GSOT.Scripts.Utils.FilesUtils.GetAnimations(name);
    }

    public void PlayAnimation(string name)
    {
        try
        {
            var animationClip = animations.Where(x => x.name == name).FirstOrDefault();
            if (animationClip == null) return;

            if (animation == null)
            {
                animation = gameObject.AddComponent<Animation>();
                animation = gameObject.GetComponentInChildren<Animation>();
            }
            if ((animation.clip == null || animation.clip.name != animationClip.name) && animationClip != null)
            {
                animationClip.legacy = true;
                animation = gameObject.GetComponentInChildren<Animation>();
                animation.AddClip(animationClip, name);
                animation.clip = animationClip;
                //animation.Play();
                animation.wrapMode = WrapMode.Loop;
                animation.Play(name);
            }
            if (animation.clip != null && !animation.isPlaying)
            {
                animation.Play(name);
            }
            var animator = gameObject.GetComponent<Animator>();
            if (animator)
            {
                animator.speed = 0.1f;
            }
        }
        catch(Exception ex)
        {
            Debug.LogWarning($"Ex: {ex.Message}");
        }
    }
}

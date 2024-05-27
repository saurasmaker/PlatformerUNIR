using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    [Serializable]
    public enum AnimationType
    {
        Idle,
        Move,
        Jump,
        Fall,
        Roll,
        Crouch,
        Attack1,
        Attack2
    }


    [SerializeField]
    private Animator m_Animator;
    [SerializeField]
    private AnimationType[] _animationTypes;



    public Animator Animator { get { return m_Animator; } }



    private void Awake()
    {
        if(m_Animator == null)
            m_Animator = GetComponent<Animator>();
    }


    public bool PlayAnimation(AnimationType animationType) 
    {

        m_Animator.Play(animationType.ToString());

        return false;
    }
}

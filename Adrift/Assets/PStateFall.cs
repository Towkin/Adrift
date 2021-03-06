﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrift.Game
{
    public class PStateFall : APlayerState
    {
        public APlayerState gronudedState;
        public override void StateUpddateFixed(PlayerController p)
        {
            if (p.mGrounded)
            {
                if (p.Audio.Landing.EventInstance.hasHandle())
                {
                    p.Audio.Landing.Play();
                    p.Audio.Landing.SetParameter("Speed", Mathf.Abs(p.mVelocity.y));
                }
                p.Transit(gronudedState);
            }
            else
            {
                if (p.mTimeSinceGround < 0.16f)
                {
                    if (p.mBufferedJumpTime + 0.15f > Time.time)
                    {
                        p.mBufferedJumpTime = -1;
                        float d = Vector3.Dot(p.mVelocity, p.mLastGroundNormal);
                        if (d < 0)
                        {  //TODO: Fix so we an use the normal from th actual real ground, and not the slope as we walked over...
                            p.mVelocity -= p.mLastGroundNormal * d;
                        }
                        p.mVelocity.y += p.JumpPower;
                        p.GroundingBlockTimer = 0.1f;
                        p.mGrounded = false;
                    }
                }
                Vector3 flatVel = p.mVelocity;
                flatVel.y = 0;
                float speed = flatVel.magnitude;
                if (speed > 0)
                {
                    flatVel /= speed;
                }
                if (Vector3.Dot(p.mInput, flatVel) < -0.3f)
                {
                    p.mVelocity.x -= p.mVelocity.x * p.FallBreakFriction * Time.fixedDeltaTime;
                    p.mVelocity.z -= p.mVelocity.z * p.FallBreakFriction * Time.fixedDeltaTime;
                }
                if (speed > p.MaxAirControlAcceleration)
                {
                    p.mInput -= flatVel * Mathf.Clamp(Vector3.Dot(flatVel, p.mInput) * 1.1f, 0, 1);
                }

                p.mVelocity += p.mInput * (p.MoveForce * Time.fixedDeltaTime) * p.AirControlFraction;
                p.mVelocity.y -= p.Gravity * Time.fixedDeltaTime;
                p.mVelocity -= p.mVelocity * p.FallFriction * Time.fixedDeltaTime;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adrift.Game
{
    public class PStateMove : APlayerState
    {
        float mTimer = 0;
        public APlayerState fallState;

        public override void Enter(PlayerController p, APlayerState fromState)
        {
            mTimer = 0;
        }

        public override void StateUpddateFixed(PlayerController p)
        {
            if (p.mGrounded)
            {
                
                Vector3 input = PlayerController.GetDirectionReorientedOnSlope(p.mInput, p.mLastGroundNormal);
                if (p.mLastGroundNormal.y > 0.66f)
                {
                    //Calculate initial state variables used by logics below
                    Vector3 flatVelDir = p.mVelocity;
                    flatVelDir.y = 0;
                    float speed = flatVelDir.magnitude;
                    if (speed > 0)
                    {
                        flatVelDir /= speed;
                    }
                    mTimer += Time.fixedDeltaTime* speed;

                    //update grounding movement. Do it as an offset, as we don't want to affect jumps or such things with walking off small steps
                    //TODO:[Gafgar: Sat/01-02-2020] consider moving the grounding stuff into a function, as the logic there is a bit general but also very special and a bit complex. 
                    //we might need to tweak it, so it would then probably do best in only having it written once but used multiple times. But wait with that till we have at least one more use case
                    {
                        if (p.mGroundDistance < p.mCtrl.skinWidth + 0.1f)
                        {
                            p.mVelocity -= p.mLastGroundNormal * Vector3.Dot(p.mVelocity, p.mLastGroundNormal) * (1 - (p.mGroundDistance / (p.mCtrl.skinWidth + 0.4f))) * 0.8f;
                        }
                        if (p.mGroundDistance > p.mCtrl.skinWidth)
                        {
                            p.mMoveOffset.y -= (p.mGroundDistance - p.mCtrl.skinWidth * 0.4f) * Time.fixedDeltaTime * 176.0f; //slowly get full ground contact
                        }
                    }
                    
                    //update input and acceleration
                    if (Vector3.Dot(p.mVelocity, input) < p.mRunSpeedThresh)
                    {
                        p.mVelocity += input * (p.mMoveForce * Time.fixedDeltaTime * 4.5f);
                    }
                    else
                    {
                        p.mVelocity += input * (p.mMoveForce * Time.fixedDeltaTime);
                    }
                    
                    //update friction/breaking
                    float friction = p.mMoveFriction;
                    if (input.sqrMagnitude < 0.2f || Vector3.Dot(input, flatVelDir) < 0.77f)
                    {

                        friction *= 1.0f + p.mFrictionBreakFactor * Mathf.Clamp(1 + (p.mVelocity.y * Mathf.Abs(p.mVelocity.y)), 0.1f, 1.3f); //don't break stronger when moving down slopes

                    }
                    friction *= Time.fixedDeltaTime; //apply delta time here so we can clamp it and never remove more than 100% of the speed no matter the time step or friction value.
                    if (friction > 1) //TODO:[Gafgar: Sat/01-02-2020] maybe make this 0.9 to make sure that no matter waht settings or calcualtions above we don't remove over 90% in one frame
                    {
                        friction = 1;
                    }

                    p.mVelocity.x -= p.mVelocity.x * (friction);
                    p.mVelocity.z -= p.mVelocity.z * (friction);
                    if (p.mVelocity.y > 0)
                    {
                        p.mVelocity.y -= p.mVelocity.y * (friction);
                    }

                    p.mHeadBobOffset.x = Mathf.Sin(mTimer)*0.12f;
                    p.mHeadBobOffset.y = Mathf.Abs(Mathf.Sin(mTimer)) * 0.23f;

                    //Handle jumping
                    if (p.mLastGronuded && p.mBufferedJumpTime + 0.15f > Time.fixedTime)
                    {
                        p.mMoveOffset.y = 0;
                        p.mBufferedJumpTime = -1;
                        p.mVelocity -= p.mLastGroundNormal * Vector3.Dot(p.mVelocity, p.mLastGroundNormal);
                        p.mVelocity.y += p.mJumpPower;
                        p.mGroundingBlockTimer = 0.1f;
                        p.mGrounded = false;
                    }
                }
                else
                {
                    //Sliding on steep floor!
                    p.mVelocity += input * (p.mMoveForce * Time.fixedDeltaTime) * 0.1f;
                    p.mVelocity -= p.mVelocity * (p.mMoveFriction * Time.fixedDeltaTime * 0.5f);
                    p.mVelocity.y -= p.mGravity * Time.fixedDeltaTime;

                    //jumping
                    if (p.mLastGronuded && p.mBufferedJumpTime + 0.15f > Time.fixedTime)
                    {
                        p.mBufferedJumpTime = -1;
                        p.mVelocity -= p.mLastGroundNormal * Vector3.Dot(p.mVelocity, p.mLastGroundNormal);
                        p.mVelocity.y += p.mJumpPower * 0.7f;
                        p.mVelocity += p.mLastGroundNormal * p.mJumpPower * 0.3f; //jump a bit out from the surface
                        p.mGroundingBlockTimer = 0.1f;
                        p.mGrounded = false;
                    }
                    //convert downward motion into the slope to be along it
                    float d = Vector3.Dot(p.mVelocity, p.mLastGroundNormal);
                    if (d < 0)
                    {
                        p.mVelocity -= p.mLastGroundNormal * d;
                    }
                }
            }
            else
            {
                //Not on the ground? Enter fall!
                p.Transit(fallState);
                //TODO:[Gafgar: Sat/01-02-2020] feels a bit bad that we kind of take on frame to react here... as the fall don't get to update this frame we do the transit...
                //feels like we would like to check this right after the move and have us be able to react to it. But atm we've made the move intro a general logic... 
                //maybe we should make it into a function call we call from within the states instead?
                //But that is not fool proof either, as if we have moving objects or something else that can affect if we are on the ground or not between our updates we want to do the ground test in the beginning after all...
                //Calling update on the fall state from here would be strange too...
                //TODO:[Gafgar: Sat/01-02-2020] rethink this
            }
        }

    }
}
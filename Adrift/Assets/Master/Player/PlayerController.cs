using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Adrift.Game
{
    public abstract class APlayerState : MonoBehaviour
    {
        //Called in awake of the owner, before enter of any state.
        public virtual void Setup(PlayerController p)
        { }
        //return if we can enter this state or not (check for prerequisites if there are any)
        public virtual bool CanEnter(PlayerController p, APlayerState fromState)
        {
            return true;
        }
        //Called after previous state have exited. NOTICE: Never call transit from within the enter or exit functions to avoid recursive. 
        //Enter on the default state must be able to accept null as the from state.
        public virtual void Enter(PlayerController p, APlayerState fromState)
        { }
        //Called when chaining to a new state. Before the new state can call enter. NOTICE: Never call transit from within the enter or exit functions to avoid recursive. 
        public virtual void Exit(PlayerController p, APlayerState toState)
        { }

        //execute fixed state logic
        public virtual void StateUpddateFixed(PlayerController p)
        { }
    }

    public class PlayerController : MonoBehaviour
    {
        [System.Serializable]
        public struct AudioData
        {
            public FMODUnity.StudioEventEmitter 
                Music,
                Ambience,
                Footstep,
                Landing;

        }

        RaycastHit[] _hitBuffer = new RaycastHit[16];

        //TODO:[Gafgar: Sat/01-02-2020] move over a bunch of variables to the states that use them as long as they are exclusive or meant as "settings"
        public CharacterController mCtrl;
        public Camera mCam;
        public Vector3 mStartPos;
        public Vector3 mCameraRotation;
        public float mMoveForce = 20.0f;
        public float mMoveFriction = 4.0f;
        public float mFrictionBreakFactor = 8.0f;
        public float mFallFriction = 4.0f;
        public float mFallBreakFriction = 1.0f;
        public float mGravity = 8.0f;
        public float mJumpPower = 38.0f;
        public float mMaxAirControlAcceleration = 7.0f;
        public float mAirControlFraction = 0.4f;
        public float mRunSpeedThresh = 5.0f;
        public float mGroundingBlockTimer = 0.0f;
        public Vector3 mMoveOffset; //movement applied one frame and zeroed after every move. Used when we want to move without adding to velocity
        public Vector3 mVelocity;
        public Vector3 mVelocitySoft;
        public Vector3 mLeanPos;
        public Vector3 mHeadBobOffset; //local space of camera
        public Vector3 mHeadBobOffsetSoft; //interpolated head bob
        public Vector3 mInput;
        public bool mLastGronuded;
        public bool mGrounded;
        public float mGroundDistance;
        public float mBufferedJumpTime = -1;
        public float mTimeSinceGround = 99;
        public float mPickupRange = 5.0f;
        public Vector3 mLastGroundNormal;
        public GameObject mLastHeighlightObj = null;

        public ComponentBase mCarryingComponent;

        public Vector3 mCameraOffset;
        public Vector3 mCameraStartOffset { get; private set; }

        [SerializeField]
        APlayerState mState;

        public AudioData Audio;
        
        public Rigidbody PlayerBody { get; private set; }  //[Gafgar: Sat/01-02-2020]: not sure if we want this? We can just go with a character controller right?
        private void Awake()
        {
            mCtrl = GetComponent<CharacterController>();
            foreach (Transform child in this.transform)
            {
                mCam = child.GetComponent<Camera>();
                if (mCam != null)
                {

                    break;
                }
            }
            mLeanPos.y = 3;

            mCameraStartOffset = mCameraOffset = mCtrl.transform.InverseTransformPoint(mCam.transform.position);

            PlayerBody = GetComponent<Rigidbody>(); 
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            APlayerState[] states = GetComponents<APlayerState>();
            foreach (var sate in states)
            {
                sate.Setup(this);
            }
            if(mState)
            {
                mState.Enter(this, null);
            }
        }

        
        public bool Transit(APlayerState toState)
        {
            //TODO:[Gafgar: Sat/01-02-2020] implement recursive guard!
            if(toState.CanEnter(this, mState))
            {
                mState.Exit(this, toState);
                var oldState = mState;
                mState = toState;
                mState.Enter(this, oldState);
                return true;
            }
            return false;
        }


        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Jump"))
            {
                mBufferedJumpTime = Time.time;
            }

            mCameraRotation += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);
            if (mCameraRotation.x < -89.0f)
            {
                mCameraRotation.x = -89.0f;
            }
            else if (mCameraRotation.x > 89.0f)
            {
                mCameraRotation.x = 89.0f;
            }
            Vector3 upOffset = new Vector3(mLeanPos.x, 3.9f, mLeanPos.z);
            upOffset.Normalize();
            mCam.transform.rotation = Quaternion.FromToRotation(Vector3.up, upOffset) * Quaternion.Euler(mCameraRotation);

            mInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            Quaternion q = Quaternion.Euler(0, mCameraRotation.y, 0);
            mInput = q * mInput;
            //update camera offset in acceleration leaning
            {
                ////TODO:[Gafgar: Sat/01-02-2020] add collision tracing here
                mCam.transform.position = mCtrl.transform.position + mCameraStartOffset + mLeanPos + q * mHeadBobOffsetSoft;
            }
            {
                GameObject rayHit = null;
                
                Debug.DrawLine(mCam.transform.position, mCam.transform.position + mCam.transform.forward * mPickupRange);
                int nbHits = Physics.RaycastNonAlloc(mCam.transform.position, mCam.transform.forward, _hitBuffer, mPickupRange, -1, QueryTriggerInteraction.Ignore);
                if (nbHits> 0)
                {
                    for (int index = nbHits; index < _hitBuffer.Length; index++)
                    {
                        _hitBuffer[index].distance = mPickupRange * 50;
                    }
                    System.Array.Sort(_hitBuffer, (a, b) => (a.distance > b.distance) ? 1 : ((a.distance < b.distance) ? -1 : 0));
                    var obj = _hitBuffer[0].collider;
                    if(obj.gameObject == gameObject)
                    {
                        if (nbHits > 1)
                        {
                            obj = _hitBuffer[1].collider;
                        }
                        else
                        {
                            obj = null;
                        }
                    }
                    if (obj)
                    {
                        rayHit = obj.gameObject;
                        Debug.Log("Hit: " + rayHit.name);
                    }
                }
                if (rayHit != mLastHeighlightObj)
                {
                    if(mLastHeighlightObj)
                    {
                        
                        ComponentBase comp = mLastHeighlightObj.GetComponent<ComponentBase>();
                        if (comp)
                        {

                        }
                        AbstractConnection con = mLastHeighlightObj.GetComponent<AbstractConnection>();
                        if (con)
                        {
                            con.SetHighlighted(false);
                        }
                    }
                    mLastHeighlightObj = rayHit;
                    if(mLastHeighlightObj)
                    {
                        Debug.DrawLine(mCam.transform.position + mCam.transform.forward, mLastHeighlightObj.transform.position);
                        if (!mCarryingComponent)
                        {
                            ComponentBase comp = mLastHeighlightObj.GetComponent<ComponentBase>();
                            if (comp)
                            {
                            }
                        }
                        AbstractConnection con = mLastHeighlightObj.GetComponent<AbstractConnection>();
                        if (con)
                        {
                            con.SetHighlighted(true);
                        }
                    }
                }
                if (mLastHeighlightObj)
                {
                    Debug.DrawLine(mCam.transform.position + mCam.transform.forward, mLastHeighlightObj.transform.position);
                }
            }
            if (mLastHeighlightObj && Input.GetButtonDown("Fire1"))
            {
                if (!mCarryingComponent)
                {
                    ComponentBase comp = mLastHeighlightObj.GetComponent<ComponentBase>();
                    if (comp)
                    {
                        if (comp.IsConnected())
                        {
                            if (comp.CanDisconnect())
                            {
                                comp.Disconnect();
                            }
                        }

                        if (!comp.IsConnected())
                        {
                            Debug.Log("Picked up: " + comp.name);
                            mCarryingComponent = comp;
                            Collider[] colliders = mCarryingComponent.GetComponentsInChildren<Collider>();
                            foreach (Collider c in colliders)
                            {
                                c.enabled = false;
                            }
                            mCarryingComponent._Body.isKinematic = true;
                        }
                    }
                }
                else
                {
                    Collider[] colliders = mCarryingComponent.GetComponentsInChildren<Collider>();
                    foreach(Collider c in colliders)
                    {
                        c.enabled = true;
                    }

                    AbstractConnection con = mLastHeighlightObj.GetComponent<AbstractConnection>();
                    if (con)
                    {
                        if (con.CanConnect(mCarryingComponent))
                        {
                            mCarryingComponent.ConnectTo(con);
                        }
                    }

                    if (!mCarryingComponent.IsConnected())
                    {
                        Debug.Log("Dropped: " + mCarryingComponent.name);
                        mCarryingComponent._Body.isKinematic = false;
                        mCarryingComponent._Body.AddForce(mCam.transform.forward * 2000);
                    }
                    else
                    {
                        Debug.Log("Connected: " + mCarryingComponent.name + " to " + mCarryingComponent._Connection.name);
                    }
                    mCarryingComponent = null;

                }
            }
        }

        private void OnTriggerEnter(Collider other)
            => other.GetComponent<IAutoInteractable>()?.Enter(gameObject);

        private void OnTriggerExit(Collider other)
            => other.GetComponent<IAutoInteractable>()?.Exit(gameObject);

        private void FixedUpdate()
        {

            //Update ground state
            UpdateGround();
            if (!mGrounded)
            {
                mTimeSinceGround += Time.fixedDeltaTime;
            }

            //[Gafgar: Sat/01-02-2020]: feels like we should be able to have a less special case for this.. or handle this in the grounding function?
            if (mGroundingBlockTimer > 0)
            {
                mGroundingBlockTimer -= Time.fixedDeltaTime;
                if (mGroundingBlockTimer < 0)
                {
                    mGroundingBlockTimer = 0;
                }
            }

            mState.StateUpddateFixed(this);

            //get positions for collision test before the move
            Vector3 bottomSphere = mCtrl.transform.position + new Vector3(0, mCtrl.height * -0.5f + mCtrl.radius + 0.1f, 0);
            if (mGrounded)
            {
                bottomSphere.y += mCtrl.stepOffset + mCtrl.radius*0.5f; //move up from the ground so we don't count small steps as blocking
            }
            Vector3 topSphere = mCtrl.transform.position + new Vector3(0, mCtrl.height * 0.5f + mCtrl.radius - 0.1f, 0);

            //perform the move
            mCtrl.Move((mVelocity + mMoveOffset) * Time.fixedDeltaTime);
            mMoveOffset = Vector3.zero;

            //look for walls and react to them (don't react to if we move in to stuff, as this has more control and it's enough to react to one hit per frame
            RaycastHit hit;
            if (Physics.CapsuleCast(bottomSphere, topSphere, mCtrl.radius, mVelocity.normalized, out hit, mVelocity.magnitude * Time.fixedDeltaTime * 3, -1, QueryTriggerInteraction.Ignore))
            {
               // Debug.Log("hit :" + hit.collider.gameObject.name + ": normal: " + hit.normal);
                if(hit.rigidbody && !hit.rigidbody.isKinematic)
                {
                    hit.rigidbody.AddForceAtPosition(mVelocity*200.7f, hit.point, ForceMode.Force);
                }
                if (hit.normal.y < 0.75f)//not ground?
                {
                    Vector3 n = hit.normal;
                    if (hit.normal.y > -0.1f)
                    {
                        n.y *= 0.7f; //don't block the downward or upward velocity completely by reducing the normal size
                    }
                    float d = Vector3.Dot(mVelocity, n);
                    if (d < 0)
                    {
                        mVelocity -= n * d; //[Gafgar: Sat/01-02-2020]: consider only removing a % here, to make it easier to move around corners
                    }
                }
            }

            if(mCarryingComponent)
            {
                float distance = 12;
                float heightOffset = 0.3f;
                if(mCarryingComponent._collider)
                {
                    distance = mCarryingComponent._collider.bounds.extents.magnitude * 2 + 2.3f;
                    heightOffset = mCarryingComponent._collider.bounds.extents.y;
                }
                mCarryingComponent.transform.position = mCam.transform.position + mCam.transform.forward * distance + new Vector3(0,-heightOffset,0);
            }

            mVelocitySoft += (mVelocity - mVelocitySoft) * 3.0f * Time.fixedDeltaTime;
            mHeadBobOffsetSoft += (mHeadBobOffset - mHeadBobOffsetSoft) * 9.0f * Time.fixedDeltaTime;
            mHeadBobOffset -= mHeadBobOffset * 6.0f * Time.fixedDeltaTime;
            //handle leaning in acceleration feedback
            {
                var pos = Vector3.zero;
                var diff = (mVelocity - mVelocitySoft) * 0.05f;
                diff.y *= 0.5f;
                diff = Vector3.ClampMagnitude(diff, 2.4f);

                if (mGrounded)
                {
                    pos.x = diff.x * 0.4f;
                    pos.z = diff.z * 0.4f;
                }
                else
                {
                    pos.x = Mathf.Clamp(mVelocitySoft.x * 0.04f, -2.0f, 2.0f) * Mathf.Clamp(mVelocity.y * 0.1f, -1.0f, 1.0f);
                    pos.z = Mathf.Clamp(mVelocitySoft.z * 0.04f, -2.0f, 2.0f) * Mathf.Clamp(mVelocity.y * 0.1f, -1.0f, 1.0f);
                }
                pos.y = (diff.y * (mGrounded ? -5.0f : -0.5f)) - (diff.x * diff.x) * 0.6f - (diff.z * diff.z) * 0.6f;
                mLeanPos += (pos - mLeanPos) * 8.0f * Time.fixedDeltaTime;
            }

            mLastGronuded = mGrounded;

        }


        //custom ground check to find more flat ground normals and handle stepping down over small edges.
        void UpdateGround()
        {
            float thresh = 0.1f + mCtrl.skinWidth;
            if (mGrounded)
            {
                thresh += 0.3f + mCtrl.stepOffset * 0.5f;
            }
            if (mGroundingBlockTimer <= 0)
            {
                RaycastHit hit;
                if (Physics.SphereCast(new Ray(transform.position + new Vector3(0, -mCtrl.height * 0.5f + mCtrl.radius, 0), -Vector3.up), mCtrl.radius, out hit, 5.0f + thresh))
                {
                    mGroundDistance = hit.distance;
                    float d = Vector3.Dot(mVelocity, hit.normal);
                    if (mGroundDistance < thresh && hit.normal.y > (mGrounded ? 0.4f : 0.55f))
                    {
                        mLastGroundNormal = hit.normal;
                        mGrounded = true;
                        mTimeSinceGround = 0;
                        if (Physics.Raycast(new Ray(transform.position + new Vector3(0, -mCtrl.height * 0.5f + 0.1f, 0), -Vector3.up), out hit, mCtrl.stepOffset + 0.2f))
                        {
                            if (mLastGroundNormal.y < hit.normal.y)
                            {
                                mLastGroundNormal = hit.normal;
                            }
                        }
                        if (Physics.SphereCast(new Ray(transform.position + new Vector3(0, -mCtrl.height * 0.5f + mCtrl.radius * 1.9f, 0), -Vector3.up), mCtrl.radius * 1.4f, out hit, 6.0f))
                        {
                            if (hit.distance > 0.001f && mLastGroundNormal.y < hit.normal.y)
                            {
                                mLastGroundNormal = hit.normal;
                            }
                        }
                    }
                    else
                    {
                        mGrounded = false;
                    }
                }
                else
                {

                    mGrounded = false;
                }
            }
            else
            {

                mGrounded = false;
            }
        }

        public static Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
        {
            float length = direction.magnitude;
            direction -= slopeNormal * Vector3.Dot(slopeNormal, direction);
            //if(direction.y > 0)
            //{
            //    direction.y = 0;
            //}
            direction.Normalize();
            direction *= length;
            return direction;
        }
    }
}
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
        public struct StartData
        {
            public CharacterController Ctrl;
            public Camera Cam;
            public Vector3 StartPos;
            public Vector3 CameraRotation;
        }
        private StartData m_LoadData;
        public CharacterController Ctrl => m_LoadData.Ctrl;
        public Camera Cam => m_LoadData.Cam;
        public Vector3 StartPos => m_LoadData.StartPos;

        public Vector3 CameraRotation { get; set; }

        [System.Serializable]
        public struct Settings
        {
            public Settings(bool _)
            {
                MoveForce = 35.0f;
                MoveFriction = 2.2f;
                FrictionBreakFactor = 4.0f;
                FallFriction = 0.05f;
                FallBreakFriction = 0.4f;
                Gravity = 33.0f;
                JumpPower = 11.0f;
                MaxAirControlAcceleration = 7.0f;
                AirControlFraction = 0.3f;
                RunSpeedThresh = 3.0f;
                PickupRange = 3.0f;
            }
                
            public float MoveForce;
            public float MoveFriction;
            public float FrictionBreakFactor;
            public float FallFriction;
            public float FallBreakFriction;
            public float Gravity;
            public float JumpPower;
            public float MaxAirControlAcceleration;
            public float AirControlFraction;
            public float RunSpeedThresh;
            public float PickupRange;
        }

        [SerializeField]
        private Settings mSettings = new Settings(true);

        public float MoveForce => mSettings.MoveForce;
        public float MoveFriction => mSettings.MoveFriction;
        public float FrictionBreakFactor => mSettings.FrictionBreakFactor;
        public float FallFriction => mSettings.FallFriction;
        public float FallBreakFriction => mSettings.FallBreakFriction;
        public float Gravity => mSettings.Gravity;
        public float JumpPower => mSettings.JumpPower;
        public float MaxAirControlAcceleration => mSettings.MaxAirControlAcceleration;
        public float AirControlFraction => mSettings.AirControlFraction;
        public float RunSpeedThresh => mSettings.RunSpeedThresh;
        public float PickupRange => mSettings.PickupRange;

        public float GroundingBlockTimer { get; set; }

        [HideInInspector] public Vector3 mMoveOffset; //movement applied one frame and zeroed after every move. Used when we want to move without adding to velocity
        [HideInInspector] public Vector3 mVelocity;
        [HideInInspector] public Vector3 mVelocitySoft;
        [HideInInspector] public Vector3 mLeanPos;
        [HideInInspector] public Vector3 mHeadBobOffset; //local space of camera
        [HideInInspector] public Vector3 mHeadBobOffsetSoft; //interpolated head bob
        [HideInInspector] public Vector3 mInput;
        [HideInInspector] public bool mLastGronuded;
        [HideInInspector] public bool mGrounded;
        [HideInInspector] public float mGroundDistance;
        [HideInInspector] public float mBufferedJumpTime = -1;
        [HideInInspector] public float mTimeSinceGround = 99;
        [HideInInspector] public Vector3 mLastGroundNormal;
        [HideInInspector] public GameObject mLastHeighlightObj = null;
        [HideInInspector] public AbstractConnection mHeighlightingConnectionj = null;
        [HideInInspector] float mHeighlightingConnectionRemoveTimer = 99.0f;

        [HideInInspector] public ComponentBase mCarryingComponent;

        [HideInInspector] public Vector3 mCameraOffset;
        [HideInInspector] public Vector3 mCameraStartOffset { get; private set; }

        Vector3 mCarryPosition;
        Quaternion mCarryRotation;
        float mCarryTime;

        [SerializeField]
        APlayerState mState;

        [HideInInspector]
        public bool RecieveInput = true;

        public AudioData Audio;

        StatusGUI _statusGUI;

        public Rigidbody PlayerBody { get; private set; }  //[Gafgar: Sat/01-02-2020]: not sure if we want this? We can just go with a character controller right?
        private void Awake()
        {
            m_LoadData.Ctrl = GetComponent<CharacterController>();
            m_LoadData.Cam = GetComponentInChildren<Camera>();
            mLeanPos.y = 3;

            mCameraStartOffset = mCameraOffset = Ctrl.transform.InverseTransformPoint(Cam.transform.position);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;


            APlayerState[] states = GetComponents<APlayerState>();
            foreach (var sate in states)
            {
                sate.Setup(this);
            }
            if (mState)
            {
                mState.Enter(this, null);
            }

            _statusGUI = GetComponentInChildren<StatusGUI>();
        }


        public bool Transit(APlayerState toState)
        {
            //TODO:[Gafgar: Sat/01-02-2020] implement recursive guard!
            if (toState.CanEnter(this, mState))
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
            if (RecieveInput)
            {
            if (Input.GetButtonDown("Jump"))
            {
                mBufferedJumpTime = Time.time;
            }

            CameraRotation += new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0f);

            CameraRotation.Set(Mathf.Clamp(CameraRotation.x, -89f, 89f), CameraRotation.y, CameraRotation.z);
            Vector3 upOffset = new Vector3(mLeanPos.x, 3.9f, mLeanPos.z);
            upOffset.Normalize();
            Cam.transform.rotation = Quaternion.FromToRotation(Vector3.up, upOffset) * Quaternion.Euler(CameraRotation);

            mInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            Quaternion q = Quaternion.Euler(0, CameraRotation.y, 0);
            mInput = q * mInput;

            //update camera offset in acceleration leaning
            {
                ////TODO:[Gafgar: Sat/01-02-2020] add collision tracing here
                Cam.transform.position = Ctrl.transform.position + mCameraStartOffset + mLeanPos + q * mHeadBobOffsetSoft;
            }
            }


            {
                GameObject rayHit = null;

                int nbHits = Physics.RaycastNonAlloc(Cam.transform.position, Cam.transform.forward, _hitBuffer, PickupRange, -1, QueryTriggerInteraction.Ignore);
                if (nbHits > 0)
                {
                    for (int index = nbHits; index < _hitBuffer.Length; index++)
                    {
                        _hitBuffer[index].distance = PickupRange * 50;
                    }
                    System.Array.Sort(_hitBuffer, (a, b) => (a.distance > b.distance) ? 1 : ((a.distance < b.distance) ? -1 : 0));
                    var obj = _hitBuffer[0].collider;
                    if (obj.gameObject == gameObject)
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
                    }
                }
                if (mHeighlightingConnectionRemoveTimer > 0)
                {
                    if (mHeighlightingConnectionRemoveTimer < 0.18f)
                    {
                        mHeighlightingConnectionRemoveTimer += Time.deltaTime;
                        if (mHeighlightingConnectionRemoveTimer >= 0.18f)
                        {
                            mHeighlightingConnectionj = null;
                        }
                    }
                }
                if (rayHit != mLastHeighlightObj)
                {
                    if (mLastHeighlightObj)
                    {

                        ComponentBase comp = mLastHeighlightObj.GetComponent<ComponentBase>();
                        if (comp)
                        {
                            comp.SetHighlighted(false);

                        }
                        AbstractConnection con = mLastHeighlightObj.GetComponent<AbstractConnection>();
                        if (con)
                        {
                            con.SetHighlighted(false);
                        }
                    }
                    mLastHeighlightObj = rayHit;
                    if (mHeighlightingConnectionRemoveTimer < 0)
                    {
                        mHeighlightingConnectionRemoveTimer = 0.01f;
                    }
                    bool hasSetStatusGUI = false;
                    if (mLastHeighlightObj)
                    {
                        Debug.DrawLine(Cam.transform.position + Cam.transform.forward, mLastHeighlightObj.transform.position);
                        if (!mCarryingComponent)
                        {
                            ComponentBase comp = mLastHeighlightObj.GetComponent<ComponentBase>();
                            if (comp)
                            {
                                comp.SetHighlighted(true);

                                if (_statusGUI)
                                {
                                    hasSetStatusGUI = true;
                                    _statusGUI.ShowStatusFor(comp);
                                }
                            }
                        }
                        AbstractConnection con = mLastHeighlightObj.GetComponent<AbstractConnection>();

                        if (con && !con.IsOccupied())
                        {
                            if (mCarryingComponent)
                            {
                                if ( con.CanConnect(mCarryingComponent))
                                {
                                    con.SetHighlighted(true);



                                    mHeighlightingConnectionj = con;
                                    mHeighlightingConnectionRemoveTimer = -1;
                                }
                            }
                            else
                            {

                                con.SetHighlighted(true);
                            }
                        }
                    
                    }

                    if (_statusGUI && !hasSetStatusGUI)
                    {
                        _statusGUI.ShowStatusFor(null);
                    }
                }
            }
            if (Input.GetButtonDown("Fire1"))
            {
                if (!mCarryingComponent)
                {
                    if (mLastHeighlightObj)
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

                                mCarryPosition = mCarryingComponent.transform.position;
                                mCarryRotation = mCarryingComponent.transform.rotation;
                                mCarryTime = 0;

                                Collider[] colliders = mCarryingComponent.GetComponentsInChildren<Collider>();
                                foreach (Collider c in colliders)
                                {
                                    c.enabled = false;
                                }
                                mCarryingComponent._Body.isKinematic = true;
                            }
                        }
                    }
                }
                else
                {
                    Collider[] colliders = mCarryingComponent.GetComponentsInChildren<Collider>();
                    foreach (Collider c in colliders)
                    {
                        c.enabled = true;
                    }

                    if (mLastHeighlightObj)
                    {
                        AbstractConnection con = mLastHeighlightObj.GetComponent<AbstractConnection>();
                        if (con)
                        {
                            if (con.CanConnect(mCarryingComponent))
                            {
                                mCarryingComponent.ConnectTo(con);
                            }
                        }
                    }

                    if (!mCarryingComponent.IsConnected())
                    {
                        Debug.Log("Dropped: " + mCarryingComponent.name);
                        mCarryingComponent._Body.isKinematic = false;
                        mCarryingComponent._Body.AddForce(Cam.transform.forward * 2000);

                        mVelocitySoft += (new Vector3(-10.0f* Cam.transform.forward.x, 0.0f, -10.0f* Cam.transform.forward.z));
                    }
                    else
                    {
                        mVelocitySoft += (new Vector3(3.0f * Cam.transform.forward.x, -2.0f, 3.0f * Cam.transform.forward.z));
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
            if (GroundingBlockTimer > 0)
            {
                GroundingBlockTimer -= Time.fixedDeltaTime;
                if (GroundingBlockTimer < 0)
                {
                    GroundingBlockTimer = 0;
                }
            }

            mState.StateUpddateFixed(this);

            //get positions for collision test before the move
            Vector3 bottomSphere = Ctrl.transform.position + new Vector3(0, Ctrl.height * -0.5f + Ctrl.radius + 0.1f, 0);
            if (mGrounded)
            {
                bottomSphere.y += Ctrl.stepOffset + Ctrl.radius * 0.5f; //move up from the ground so we don't count small steps as blocking
            }
            Vector3 topSphere = Ctrl.transform.position + new Vector3(0, Ctrl.height * 0.5f + Ctrl.radius - 0.1f, 0);

            //perform the move
            Ctrl.Move((mVelocity + mMoveOffset) * Time.fixedDeltaTime);
            mMoveOffset = Vector3.zero;

            //look for walls and react to them (don't react to if we move in to stuff, as this has more control and it's enough to react to one hit per frame
            RaycastHit hit;
            if (Physics.CapsuleCast(bottomSphere, topSphere, Ctrl.radius, mVelocity.normalized, out hit, mVelocity.magnitude * Time.fixedDeltaTime * 3, ~LayerMask.NameToLayer("Items"), QueryTriggerInteraction.Ignore))
            {
                // Debug.Log("hit :" + hit.collider.gameObject.name + ": normal: " + hit.normal);
                if (hit.rigidbody && !hit.rigidbody.isKinematic)
                {
                    hit.rigidbody.AddForceAtPosition(mVelocity * 200.7f, hit.point, ForceMode.Force);
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

            if (mCarryingComponent)
            {
                if (!mHeighlightingConnectionj)
                {
                    float distance = 12;
                    float heightOffset = 0.3f;
                    distance = mCarryingComponent._colliderExt.magnitude + 0.3f;
                    Vector3 up = Cam.transform.up;

                    float speedMod = Mathf.Min(1.0f, 0.2f + mCarryTime * 0.8f);

                    heightOffset = 1.0f - up.y * 0.5f;

                    mCarryPosition += ((Cam.transform.position + Cam.transform.forward * distance - up * heightOffset) - mCarryPosition) * 50.0f * speedMod * Time.fixedDeltaTime;
                    mCarryRotation = Quaternion.Slerp(mCarryRotation, Cam.transform.rotation, 12.0f * speedMod * Time.fixedDeltaTime);
                    mCarryingComponent.transform.position = mCarryPosition;
                    mCarryingComponent.transform.rotation = mCarryRotation;

                    if (mCarryTime < 1.5f)
                    {
                        mCarryTime += Time.fixedDeltaTime;
                    }
                }
                else
                {
                    if(mCarryTime > 0.01f)
                    {
                        if (mCarryTime > 0.9f)
                        {
                            mCarryTime = 0.9f;
                        }
                        mCarryTime -= Time.fixedDeltaTime*1.5f;
                        if(mCarryTime < 0.01f)
                        {
                            mCarryTime = 0.01f;
                        }
                    }

                    mCarryPosition += ((mHeighlightingConnectionj._TranformProxy.transform.position + mHeighlightingConnectionj._TranformProxy.transform.up*0.5f) - mCarryPosition) * 4.0f * Time.fixedDeltaTime;
                    mCarryRotation = Quaternion.Slerp(mCarryRotation, mHeighlightingConnectionj._TranformProxy.transform.rotation, 6.0f *  Time.fixedDeltaTime);
                    mCarryingComponent.transform.position = mCarryPosition;
                    mCarryingComponent.transform.rotation = mCarryRotation;

                }
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
            float thresh = 0.1f + Ctrl.skinWidth;
            if (mGrounded)
            {
                thresh += 0.3f + Ctrl.stepOffset * 0.5f;
            }
            if (GroundingBlockTimer <= 0)
            {
                RaycastHit hit;
                if (Physics.SphereCast(new Ray(transform.position + new Vector3(0, -Ctrl.height * 0.5f + Ctrl.radius, 0), -Vector3.up), Ctrl.radius, out hit, 5.0f + thresh, ~LayerMask.GetMask("Items"), QueryTriggerInteraction.Ignore))
                {
                    mGroundDistance = hit.distance;
                    float d = Vector3.Dot(mVelocity, hit.normal);
                    if (mGroundDistance < thresh && hit.normal.y > (mGrounded ? 0.4f : 0.55f))
                    {
                        mLastGroundNormal = hit.normal;
                        mGrounded = true;
                        mTimeSinceGround = 0;
                        if (Physics.Raycast(new Ray(transform.position + new Vector3(0, -Ctrl.height * 0.5f + 0.1f, 0), -Vector3.up), out hit, Ctrl.stepOffset + 0.2f, ~LayerMask.GetMask("Items"), QueryTriggerInteraction.Ignore))
                        {
                            if (mLastGroundNormal.y < hit.normal.y)
                            {
                                mLastGroundNormal = hit.normal;
                            }
                        }
                        if (Physics.SphereCast(new Ray(transform.position + new Vector3(0, -Ctrl.height * 0.5f + Ctrl.radius * 1.9f, 0), -Vector3.up), Ctrl.radius * 1.4f, out hit, 6.0f, ~LayerMask.GetMask("Items"), QueryTriggerInteraction.Ignore))
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
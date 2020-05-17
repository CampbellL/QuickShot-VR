using Game;
using OculusSampleFramework;
using UnityEngine;

namespace Player
{
    public class AssaultRifleGrabber : MonoBehaviour
    {
        public static AssaultRifleGrabber Instance;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }
        
        [SerializeField] private OVRInput.Button rifleTransformButton;
        public OVRGrabber rightHand;
        public OVRGrabber leftHand;
        [SerializeField] private Transform tempRightObjectParent;
        [SerializeField] private Transform tempLeftObjectParent;
        [SerializeField] private DistanceGrabbable rifle;

        [HideInInspector] public bool isUsingRifle;

        private DistanceGrabbable _rightObject;
        private DistanceGrabbable _leftObject;

        // Start is called before the first frame update
        void Start()
        {
            rifle.SetObjectActivation(false);
            SetRifleParent(GameSettings.Instance.isRightHand);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isUsingRifle && (OVRInput.Get(OVRInput.RawButton.RHandTrigger, rightHand.GetController()) && OVRInput.Get(OVRInput.RawButton.LHandTrigger, leftHand.GetController())))
            {
                //Get the Assault Rifle
                if(rightHand.grabbedObject) _rightObject = (DistanceGrabbable)rightHand.grabbedObject;
                if(leftHand.grabbedObject) _leftObject = (DistanceGrabbable)leftHand.grabbedObject;
            
                if(_rightObject) _rightObject.SetObjectActivation(false);
                if(_leftObject) _leftObject.SetObjectActivation(false);

                if (GameSettings.Instance.isRightHand)
                {
                    if(_rightObject) _rightObject.transform.SetParent(tempRightObjectParent);
                    rightHand.GrabBegin(rifle.GetComponent<DistanceGrabbable>());
                }
                else
                {
                    if(_leftObject) _leftObject.transform.SetParent(tempLeftObjectParent);
                    leftHand.GrabBegin(rifle.GetComponent<DistanceGrabbable>());
                }
            
                rifle.SetObjectActivation(true);
                isUsingRifle = true;
                rightHand.isUsingRifle = true;
                leftHand.isUsingRifle = true;
            } 
            else if (isUsingRifle && (OVRInput.GetUp(rifleTransformButton, rightHand.GetController()) ||
                                      OVRInput.GetUp(rifleTransformButton, leftHand.GetController())))
            {
                rightHand.isUsingRifle = false;
                leftHand.isUsingRifle = false;
            
                if (GameSettings.Instance.isRightHand)
                {
                    rightHand.GrabEnd(true);
                    //if(_rightObject) _rightObject.transform.SetParent(null);
                    rightHand.GrabBegin(_rightObject);
                }
                else
                {
                    leftHand.GrabEnd(true);
                    //if(_leftObject) _leftObject.transform.SetParent(null);
                    leftHand.GrabBegin(_leftObject);
                }

                rifle.SetObjectActivation(false);
                isUsingRifle = false;

                if(_rightObject) _rightObject.SetObjectActivation(true);
                if(_leftObject) _leftObject.SetObjectActivation(true);

                _rightObject = null;
                _leftObject = null;
            }
        }

        public void SetRifleParent(bool isRightHand)
        {
            //Parent the assault rifle to the right hand anchor, depending on the users primary hand choice
            rifle.transform.SetParent(isRightHand ? tempRightObjectParent : tempLeftObjectParent);
        }
    }
}

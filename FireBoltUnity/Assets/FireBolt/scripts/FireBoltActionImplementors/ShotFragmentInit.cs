using LN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Oshmirto;

namespace Assets.scripts
{
    public class ShotFragmentInit : FireBoltAction
    {
        //passed in params
        private bool initialized = false;
        private string anchor=string.Empty;
        private float? height;
        private float? pan;
        private string cameraName; //this is actually going to manipulate the rig most likely, but what we call it doesn't matter much from in here
        private CameraBody cameraBody; //need a reference to this guy for setting fstop and lens
        private string lensName;
        private string fStopName;
        private List<Framing> framings;
        private Oshmirto.Direction direction;
        private Oshmirto.Angle cameraAngle;
        private string focusTarget;

        //intermediate calculated values
        Vector3 targetLookAtPoint = new Vector3();

        //parameter grounding
        Vector3Nullable tempCameraPosition;
        Vector3Nullable tempCameraOrientation;
        ushort? tempLensIndex;
        ushort? tempFStopIndex;
        float? tempFocusDistance;

        //saved camera values
        GameObject camera;
        Quaternion previousCameraOrientation = Quaternion.identity;
        Vector3 previousCameraPosition = Vector3.zero;
        ushort previousLensIndex;
        ushort previousFStopIndex;
        float previousFocusDistance;

        //final camera values
        Quaternion newCameraOrientation;
        Vector3 newCameraPosition;
        ushort newLensIndex;
        ushort newFStopIndex;
        float newfocusDistance;

        public ShotFragmentInit(float startTick, string cameraName, string anchor, float? height, float? pan,
                                string lensName, string fStopName, List<Framing> framings, Oshmirto.Direction direction,
                                Oshmirto.Angle cameraAngle, string focusTarget) :
            base(startTick, startTick)
        {
            this.startTick = startTick;            
            this.cameraName = cameraName;
            this.anchor = anchor;
            this.height = height;
            this.pan = pan;
            this.lensName = lensName;
            this.fStopName = fStopName;
            this.framings = framings;
            this.direction = direction;
            this.cameraAngle = cameraAngle;
            this.focusTarget = focusTarget;
        }

        public override bool Init()
        {
            if(initialized) return true;

            Extensions.RenderColliders();

            //don't throw null refs in the debug statement if framing isn't there.  it's not required
            string framingDescriptor = string.Empty;
            if (existsFraming())
                framingDescriptor = framings[0].ToString();

            Debug.Log(string.Format("init shot fragment start[{0}] end[{1}] anchor[{2}] height[{3}] lens[{4}] fStop[{5}] framing[{6}] direction[{7}] angle[{8}] focus[{9}] d:s[{10}:{11}]",
                                    startTick,endTick,anchor,height,lensName,fStopName,framingDescriptor,direction,cameraAngle,focusTarget,
                                    ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
            if(!findCamera()) return false;           
            savePreviousCameraState();

            //ground parameters
            tempCameraPosition = new Vector3Nullable(null, null, null); //if y not specified in our new params, we will propagate last height forward
            tempCameraOrientation = new Vector3Nullable(null, null, null); 

            //find our anchor if specified
            Vector2 anchorPosition;
            if (calculateAnchor(anchor, out anchorPosition))
            {
                Debug.Log(string.Format("setting camera anchor[{0}]", anchorPosition));
                tempCameraPosition.X = anchorPosition.x;
                tempCameraPosition.Z = anchorPosition.y;
            }

            //set y directly from oshmirto
            //not in new version:tempCameraPosition.Y = height;
            GameObject framingTarget = null;
            if (existsFraming())
            {
                if (getActorByName(framings[0].FramingTarget, out framingTarget))
                {
                    targetLookAtPoint = findTargetLookAtPoint(framingTarget);
                }
                else
                {
                    Debug.LogError(string.Format("could not find actor [{0}]",
                    framings[0].FramingTarget).AppendTimestamps());
                }
            }

            //height
            if (existsFraming()) //default to even height with point of interest on framed target
            {
                tempCameraPosition.Y = targetLookAtPoint.y;
            }
            else if (height.HasValue)
            {
                tempCameraPosition.Y = height;
            }
            else
            {
                tempCameraPosition.Y = 1; //in the absence of all information just put the camera not at 0 height
            }


            //set lens 
            ushort tempLens;
            if(!string.IsNullOrEmpty(lensName) && 
               CameraActionFactory.lenses.TryGetValue(lensName, out tempLens))
            {
                tempLensIndex = tempLens;
            }

            //set F Stop
            ushort tempFStop;
            if(!string.IsNullOrEmpty(fStopName) &&
               CameraActionFactory.fStops.TryGetValue(fStopName, out tempFStop))
            {
                tempFStopIndex = tempFStop;
            }

            //framing 
            if (existsFraming() && framingTarget)
            {
                Bounds targetBounds = framingTarget.GetComponent<BoxCollider>().bounds;
                targetBounds.BuildDebugBox(5, Color.cyan);

                Debug.Log(String.Format("framing target[{0}] bounds[{1},{2}]", framingTarget.name,
                                        targetBounds.min.y, targetBounds.max.y));

                FramingParameters framingParameters = FramingParameters.FramingTable[framings[0].FramingType];

                //default our aperture to one appropriate to the framing if it's not set
                if (!tempFStopIndex.HasValue &&
                    CameraActionFactory.fStops.TryGetValue(framingParameters.DefaultFStop, out tempFStop))
                    tempFStopIndex = tempFStop;

                if (tempLensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue)
                {
                    //case is here for completeness.  rotation needs to be done for all combinations of lens and anchor specification, so it goes after all the conditionals
                }
                else if (!tempLensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue)//direction still doesn't matter since we can't move in the x,z plane
                {
                    //naively guessing and checking
                    Quaternion savedCameraRotation = cameraBody.NodalCamera.transform.rotation;
                    //point the camera at the thing
                    cameraBody.NodalCamera.transform.rotation = Quaternion.LookRotation(targetBounds.center - cameraBody.NodalCamera.transform.position);
                    float targetFov = 0;
                    //need to keep from stepping up and down over some boundary
                    bool incremented = false;
                    bool decremented = false;
                    while (!(incremented && decremented))  //if we haven't set a value and we haven't stepped both up and down.  
                    {
                        //find where on the screen the extents are.  using viewport space so this will be in %. z is world units away from camera
                        Vector3 bMax = cameraBody.NodalCamera.WorldToViewportPoint(targetBounds.max);
                        Vector3 bMin = cameraBody.NodalCamera.WorldToViewportPoint(targetBounds.min);

                        float FovStepSize = 2.5f;//consider making step size a function of current size to increase granularity at low fov.  2.5 is big enough to jump 100-180 oddly

                        if (bMax.y - bMin.y > framingParameters.MaxPercent && bMax.y - bMin.y < framingParameters.MinPercent)
                        {
                            break;//we found our answer in cameraBody.NodalCamera.fov
                        }
                        else if (bMax.y - bMin.y < framingParameters.MinPercent)
                        {
                            cameraBody.NodalCamera.fieldOfView -= FovStepSize;
                            decremented = true;
                        }
                        else //(bMax.y - bMin.y >= fp.MaxPercent)
                        {
                            cameraBody.NodalCamera.fieldOfView += FovStepSize;
                            incremented = true;
                        }

                        //force matrix recalculations on the camera after adjusting fov
                        cameraBody.NodalCamera.ResetProjectionMatrix();
                        cameraBody.NodalCamera.ResetWorldToCameraMatrix();
                    }
                    //reset camera position...we should only be moving the rig
                    targetFov = cameraBody.NodalCamera.fieldOfView;
                    cameraBody.NodalCamera.transform.rotation = savedCameraRotation;
                    tempLensIndex = (ushort)ElPresidente.Instance.GetLensIndex(targetFov);
                }
                else if (tempLensIndex.HasValue && //direction matters here.  
                    (!tempCameraPosition.X.HasValue || !tempCameraPosition.Z.HasValue))//also assuming we get x,z in a pair.  if only one is provided, it is invalid and will be ignored
                {
                    //allow full exploration of circle about target since we can't move in or out and keep the same framing                        
                    if (!findCameraPositionByRadius(framingTarget, targetBounds, framingParameters, 1.0f))
                    {
                        Debug.Log(String.Format("failed to find satisfactory position for camera to frame [{0}] [{1}] with lens [{2}]. view will be obstructed",
                                                framings[0].FramingTarget, framings[0].FramingType.ToString(), ElPresidente.Instance.lensFovData[tempLensIndex.Value]._focalLength));
                    }
                }
                else //we are calculating everything by framing and direction.  
                {
                    //x,z does not have value
                    //pick a typical lens for this type of shot
                    tempLensIndex = CameraActionFactory.lenses[framingParameters.DefaultFocalLength];
                    //see if we can find a camera location for this lens
                    //allow less than some % of a circle variance from ideal viewing.  if we don't find an answer, change the lens

                    bool sign = true;
                    short iterations = 0;
                    ushort maxLensChangeIterations = 6;
                    while (!findCameraPositionByRadius(framingTarget, targetBounds, framingParameters, 0.35f))
                    {
                        iterations++;
                        if (iterations > maxLensChangeIterations)
                        {
                            Debug.Log(String.Format("exceeded max lens change iterations[{0}] solving framing[{1}] on target[{2}]",
                                                    maxLensChangeIterations, framingParameters.Type, framingTarget));
                            break; //framing is just not working out.  we will return a shot that's not so good and get on with things
                        }
                        int offset = sign ? iterations : -iterations;
                        if (tempLensIndex + offset < 0)
                        {
                            //should never get here since the smallest we specify is 27mm and we will cap at +-3 lenses
                        }
                        else if (tempLensIndex + offset > CameraActionFactory.lenses.Values.Max<ushort>())
                        {
                            //explore on the other side of our start lens until we hit our max iterations
                            iterations++;
                            offset = sign ? -iterations : iterations;
                        }
                        tempLensIndex = (ushort)(tempLensIndex + offset);
                    }
                }

                tempCameraOrientation.Y = Quaternion.LookRotation(framingTarget.transform.position - tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.y;

            }
            else if (pan.HasValue) //no framing, we can pay attention to a direct rotate command
            {
                tempCameraOrientation.Y = pan.Value.BindToSemiCircle();
            }

            //this destroys the ability to angle with respect to anything but the framing target if specified
            //this does not seem terribly harmful. subject is attached to angle mostly because we wanted to not have
            //to specify a framing (if we used an absolute anchor for camera positioning)
            tiltCameraAtSubject(cameraAngle, framingTarget);

            //focus has to go after all possible x,y,z settings to get the correct distance to subject
            Vector3 focusPosition;
            if (calculateFocusPosition(focusTarget, out focusPosition))
            {
                tempFocusDistance = Vector3.Distance(tempCameraPosition.Merge(previousCameraPosition), focusPosition);
            }
            else if (framingTarget != null)//we didn't specify what to focus on, but we framed something.  let's focus on that by default
            {
                tempFocusDistance = Vector3.Distance(tempCameraPosition.Merge(previousCameraPosition), targetLookAtPoint);
            }

            //sort out what wins where and assign to final camera properties
            //start with previous camera properties in case nothing fills them in
            newCameraPosition = tempCameraPosition.Merge(previousCameraPosition);
            newCameraOrientation = Quaternion.Euler(tempCameraOrientation.Merge(previousCameraOrientation.eulerAngles));
            newLensIndex = tempLensIndex.HasValue ? tempLensIndex.Value : previousLensIndex;
            newFStopIndex = tempFStopIndex.HasValue ? tempFStopIndex.Value : previousFStopIndex;
            newfocusDistance = tempFocusDistance.HasValue ? tempFocusDistance.Value : previousFocusDistance;

            Skip();

            initialized = true;
            return initialized;
        }

        private void tiltCameraAtSubject(Angle cameraAngle, GameObject framingTarget)
        {
            GameObject subject = framingTarget;
            if (!subject &&
                !getActorByName(cameraAngle.Target, out subject))
            {
                Debug.Log("Cannot find subject to as framing target or angle target to tilt toward");
                return;
            }
            tempCameraOrientation.X = Quaternion.LookRotation(framingTarget.transform.position - tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.x;
        }


        private bool existsFraming()
        {
            return framings.Count > 0 && framings[0] != null;
        }

        private Vector3 findTargetLookAtPoint(GameObject target)
        {
            var cmMetaData = target.GetComponent<CinematicModelMetaDataComponent>();
            Framing framing = framings.Find(x => x.FramingTarget == target.name); //see if there is a target being framed with that name

            float pointOfInterestScalar = 0;
            if (framing != null && framing.FramingType <= FramingType.Waist)
            {
                pointOfInterestScalar = cmMetaData.PointOfInterestScalar;
            }
            Vector3 targetLookAtPoint = new Vector3();
            var collider = target.GetComponent<BoxCollider>();
            if (collider != null)
            {
                targetLookAtPoint = new Vector3(collider.bounds.center.x,
                                            collider.bounds.center.y + pointOfInterestScalar * collider.bounds.extents.y,
                                            collider.bounds.center.z);
            }
            else
            {
                targetLookAtPoint = target.transform.position;
                Debug.Log(String.Format("actor [{0}] has no collider to calculate look-at point.  using actor root",
                                        target.name));
            }
            return targetLookAtPoint;
        }

        private bool findCameraPositionByRadius(GameObject framingTarget, Bounds targetBounds, FramingParameters framingParameters,
                                                float maxHorizontalSearchPercent)
        {
            //converting to radians when we lookup so we don't have to worry about it later
            float vFov = ElPresidente.Instance.lensFovData[tempLensIndex.Value]._unityVFOV * Mathf.Deg2Rad;

            float frustumHeight = (1 / framingParameters.TargetPercent) * (targetBounds.max.y - targetBounds.min.y);

            float distanceToCamera = frustumHeight / Mathf.Tan(vFov / 2);

            float horizontalSearchSign = 1;
            float horizontalSearchStepSize = 5f * Mathf.Deg2Rad;
            ushort horizontalSearchIterations = 0;
            float horizontalSearchAngleCurrent = 0;
            //bool verticalSearchSign = true; only searching up atm
            float verticalSearchStepSize = 1.5f;
            ushort verticalSearchIterations = 0;
            ushort verticalSearchIterationsMax = 10;
            float verticalSearchAngleInitial = cameraAngle == null ? 0f : CameraActionFactory.angles[cameraAngle.AngleSetting];
            bool subjectVisible = false;
            while (!subjectVisible)//search over the range about ideal position
            {
                horizontalSearchIterations++;


                float verticalSearchAngleCurrent = verticalSearchAngleInitial;
                verticalSearchIterations = 0;
                while (!subjectVisible && verticalSearchIterations < verticalSearchIterationsMax)
                {


                    //find direction vector given "direction" and angle measure
                    Vector3 subjectToCamera = get3DDirection(framingTarget, new Vector3(verticalSearchAngleCurrent, horizontalSearchAngleCurrent));
                    //put camera at position on the r=distance sphere
                    tempCameraPosition.X = targetBounds.center.x + subjectToCamera.x * distanceToCamera;
                    tempCameraPosition.Y = targetBounds.center.y + subjectToCamera.y * distanceToCamera;
                    tempCameraPosition.Z = targetBounds.center.z + subjectToCamera.z * distanceToCamera;

                    //raycast to check for LoS
                    RaycastHit hit;
                    Vector3 from = tempCameraPosition.Merge(previousCameraPosition);
                    Vector3 direction = targetLookAtPoint - tempCameraPosition.Merge(previousCameraPosition);
                    if (Physics.Raycast(from, direction, out hit) &&
                        hit.transform == framingTarget.transform)
                    {
                        //we can see our target
                        subjectVisible = true;
                        break;
                    }
                    else //we can't see the subject.  change camera height and try again
                    {
                        verticalSearchAngleCurrent += verticalSearchStepSize;
                        verticalSearchIterations++;
                    }
                }
                if (!subjectVisible)//search around the circle
                {
                    horizontalSearchSign = -horizontalSearchSign;
                    horizontalSearchAngleCurrent = horizontalSearchSign * horizontalSearchIterations * horizontalSearchStepSize;

                    if (Mathf.Abs(horizontalSearchAngleCurrent) > 1.8 * maxHorizontalSearchPercent) //have we gone more than the allotted amount around the circle?
                    {
                        break;
                    }
                }
            }
            return subjectVisible;
        }

        Vector3 get3DDirection(GameObject framingTarget, Vector3 angleOffsets)
        {
            //convert incoming angles to radians
            angleOffsets.Scale(new Vector3(Mathf.Deg2Rad, Mathf.Deg2Rad, Mathf.Deg2Rad));
            //default to framing target in case direction wasn't specified
            Vector3 subjectToCamera = framingTarget.transform.forward;
            float directionBasedYRotation = 0;
            if (direction != null)
            {
                GameObject directionTarget;
                if (getActorByName(direction.Target, out directionTarget) &&
                    directionTarget != null)
                {
                    //Quaternion savedRotation = directionTarget.transform.rotation;

                    switch (direction.Heading)
                    {
                        case Heading.Right:
                            ;//exists for completeness.  
                            break;
                        case Heading.Left:
                            directionBasedYRotation = Mathf.PI;
                            break;
                        case Heading.Away:
                            directionBasedYRotation = -Mathf.PI / 2;
                            break;
                        case Heading.Toward:
                            directionBasedYRotation = Mathf.PI / 2;
                            break;
                        default:
                            break;
                    }
                    //add on the rotation from the direction target's orientation about y
                    directionBasedYRotation += directionTarget.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
                }
            }
            else //get the root y rotation from the framing target
            {
                directionBasedYRotation += framingTarget.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;
            }

            //convert from spherical to rectangular coordinates
            float theta = angleOffsets.y + directionBasedYRotation;
            float phi = Mathf.PI / 2 - angleOffsets.x;
            subjectToCamera = new Vector3(Mathf.Cos(theta) * Mathf.Sin(phi),
                                          Mathf.Cos(phi), //not using traditional mathematical coordinates, swapping z and y calculations
                                          Mathf.Sin(theta) * Mathf.Sin(phi));
            return subjectToCamera.normalized;
        }

        ///// <summary>
        ///// Given a shot angle, finds the distance to travel from the target's baseline y position.
        ///// Finds the distance by solving the equation: tan(base/hyp angle) * base = height.
        ///// Returns the height found by solving the equation.
        ///// </summary>
        //private float findCameraYPosition(float alpha, Vector3 sourcePosition, Vector3 targetPosition, AngleSetting angleSetting)
        //{
        //    // If the shot is a medium angle it is on the same y-plane as the target.
        //    if (angleSetting == Oshmirto.AngleSetting.Medium) return targetPosition.y;

        //    // Otherwise, find the length of the triangle's base by finding the (x,z) distance between the camera and target.
        //    float baseLength = Mathf.Abs(targetPosition.x - sourcePosition.x) + Mathf.Abs(targetPosition.z - sourcePosition.z);

        //    // Next, find the tangent of the shot angle converted to radians.
        //    float tanAlpha = Mathf.Tan(Mathf.Deg2Rad * alpha);

        //    // If this is a high shot move in the positive y direction.
        //    if (angleSetting == Oshmirto.AngleSetting.High) return baseLength * tanAlpha;

        //    // Otherwise, move in the negative y direction.
        //    return baseLength * tanAlpha * -1 + targetPosition.y;
        //}

        /// <summary>
        /// capturing state for Undo()'ing
        /// </summary>
        private void savePreviousCameraState()
        {
            previousCameraOrientation = camera.transform.rotation;
            previousCameraPosition = camera.transform.position;
            previousLensIndex = (ushort)cameraBody.IndexOfLens;
            previousFStopIndex = (ushort)cameraBody.IndexOfFStop;
            previousFocusDistance = cameraBody.FocusDistance;
        }

        private bool findCamera()
        {
            
            if (camera == null &&
                !getActorByName(cameraName, out camera))
            {
                Debug.LogError(string.Format("could not find camera[{0}] at time d:s[{1}:{2}].  This is really bad.  What did you do to the camera?",
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
                return false;
            }

            cameraBody = camera.GetComponentInChildren<CameraBody>();
            if (cameraBody == null)
            {
                Debug.LogError(string.Format("could not find cameraBody component as child of camera[{0}] at time d:s[{1}:{2}].  Why isn't your camera a cinema suites camera?",
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
                return false;
            }
            return true;
        }

        private bool calculateFocusPosition(string focusTarget, out Vector3 focusPosition)
        {
            focusPosition = new Vector3();
            if (string.IsNullOrEmpty(focusTarget))
                return false;

            //try to parse target as a coordinate                
            if (focusTarget.TryParseVector3(out focusPosition))
            {
                Debug.Log("focus @" + focusPosition);
                return true;
            }

            //try to find the target as an actor
            GameObject target;
            if (!getActorByName(focusTarget, out target))
            {
                Debug.Log("actor name [" + focusTarget + "] not found. cannot change focus");
                return false;
            }
            focusPosition = target.transform.position;
            //Debug.Log(string.Format("focus target[{0}] @{1} tracking[{2}]", focusTarget, target.transform.position));

            return true;
        }

        private bool calculateAnchor(string anchor, out Vector2 anchorPosition)
        {
            anchorPosition = new Vector2();
            //if there's nothing there, then nothing to ground to
            if (string.IsNullOrEmpty(anchor)) return false;
            Vector2 planarCoords;
            if (anchor.TryParsePlanarCoords(out planarCoords))
            {
                //we can read the anchor string as planar coords
                anchorPosition = planarCoords;
                return true;
            }
            else
            {
                //we can't read anchor string as planar coords.  hopefully this is the name of an actor
                GameObject actorToAnchorOn;

                if (!getActorByName(anchor, out actorToAnchorOn))                    
                {
                    //sadly there is no such thing.  we should complain and then try to get on with business
                    Debug.LogError(string.Format("anchor actor [{0}] not found at time d:s[{1}:{2}].  calculating anchor freely.", 
                        anchor, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentStoryTime));
                    return false;
                }
                Vector3 actorPosition = actorToAnchorOn.transform.position;
                anchorPosition = new Vector2(actorPosition.x, actorPosition.z);
                return true;
            }
        }

        public override void Execute(float currentTime)
        {
            //nothing to see here.  this is all instant
        }

        public override void Stop()
        {
            //nothing to do and nothing to stop
        }

        public override void Undo()
        {
            camera.transform.position = previousCameraPosition;
            camera.transform.rotation = previousCameraOrientation;
            cameraBody.IndexOfLens = previousLensIndex;
            cameraBody.IndexOfFStop = previousFStopIndex;
            cameraBody.FocusDistance = previousFocusDistance;
            
        }

        public override void Skip()
        {
            //since this action always happens instantaneously we can assume that the 
            //skip will get run anytime it's selected for addition in the 
            //executing queue in el Presidente
            camera.transform.position = newCameraPosition;
            camera.transform.rotation = newCameraOrientation;
            cameraBody.IndexOfLens = newLensIndex;
            cameraBody.IndexOfFStop = newFStopIndex;
            cameraBody.FocusDistance = newfocusDistance;                  
        }

        public override string ToString()
        {
            string framingString = "no frame";
            if (framings[0] != null)
            {
                framingString = string.Format("frame {0} {1};", framings[0].FramingTarget, framings[0].FramingType);
            }
            string directionString = "no dir;";
            if (direction != null)
            {
                directionString = string.Format("dir {0} {1};", direction.Target, direction.Heading);
            }
            string angleString = "no angle;";
            if (cameraAngle != null)
            {
                angleString = string.Format("angle {0} {1};", cameraAngle.Target, cameraAngle.AngleSetting);
            }
            return string.Format("SFInit {0} {1} {2} {3} {4}",
                                 framingString, directionString, angleString,
                                 string.IsNullOrEmpty(lensName) ? "no lens;" : lensName,
                                 string.IsNullOrEmpty(fStopName) ? "no fstop;" : fStopName);
        }

        public override string GetMainActorName()
        {
            var subject = string.Empty;
            if (framings != null && framings[0] != null)
            {
                subject = framings[0].FramingTarget;
            }
            return subject;
        }

    }
}

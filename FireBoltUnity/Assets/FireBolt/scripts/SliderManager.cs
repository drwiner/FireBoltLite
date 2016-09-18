using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SliderManager : MonoBehaviour 
{
    //slider we are putting thumbnail over
    private Slider slider;

    // The slider's transform rectangle.
    private RectTransform sliderRect;


    // Stores whether initialization has occurred.
    private bool hasInitialized;

    //create a function type that we can attach to event listeners on the slider
    public delegate void SliderEventDelegate(UnityEngine.EventSystems.BaseEventData baseEventData);

	// Use this for initialization
	void Start () 
    {
        //if we are here, then el presidente attached this component to a slider
        slider = ElPresidente.Instance.whereWeAt;


        if (!registerSliderMouseEvents())
        {
            return;
        }

        // Get the slider's transform rectangle.
        sliderRect = slider.GetComponent<RectTransform>();

        // Remember that initialization has not occurred.
        hasInitialized = false;
	}

    //dynamically assign slider mouse events so we can avoid requiring a bunch more stuff in the hierarchy
    //when someone imports the firebolt package and doesn't want to use sliders or keyframes
    private bool registerSliderMouseEvents()
    {
        //enter
        EventTrigger eventTrigger = slider.GetComponent<EventTrigger>();
        if(!eventTrigger)
        {
            Debug.Log("no event trigger attached to slider object.  can't add via code b/c the api is fail.  you won't be getting any keyframes out of this thing");
            return false;
        }
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback = new EventTrigger.TriggerEvent();
        //pointerEnterEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOn));
        eventTrigger.triggers.Add(pointerEnterEntry);

        //exit
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback = new EventTrigger.TriggerEvent();
        //pointerExitEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOff));
        eventTrigger.triggers.Add(pointerExitEntry);

        //drag
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback = new EventTrigger.TriggerEvent();
        //dragEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOn));
        eventTrigger.triggers.Add(dragEntry);

        //end drag
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        endDragEntry.callback = new EventTrigger.TriggerEvent();
        //endDragEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOff));
        eventTrigger.triggers.Add(endDragEntry);

        return true;
    }

	void Update ()
    {
        // If keyframes have not been loaded and El Presidente has initialized the keyframes...
        if (!hasInitialized)// && ElPresidente.Instance.KeyframesGenerated)
        {
            // Remember that we've initialized.
            hasInitialized = true;
        }

    }


}

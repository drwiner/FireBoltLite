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

    // The thumbnail UI object.
    private UnityEngine.UI.RawImage thumb;

    // Stores whether the thumbnail is enabled.
    private bool thumbnailEnabled;

    // An array containing keyframe images.
    private Texture2D[] images;

    // Stores whether initialization has occurred.
    private bool hasInitialized;

    //create a function type that we can attach to event listeners on the slider
    public delegate void SliderEventDelegate(UnityEngine.EventSystems.BaseEventData baseEventData);

	// Use this for initialization
	void Start () 
    {
        //if we are here, then el presidente attached this component to a slider
        slider = ElPresidente.Instance.whereWeAt;

        // Set the thumbnail to be hidden on initialization.
        thumbnailEnabled = false;

        if (!registerSliderMouseEvents())
        {
            return;
        }

        // Get the slider's transform rectangle.
        sliderRect = slider.GetComponent<RectTransform>();

        //create a thumbnail to use for displaying keyframes and staple it onto the canvas
        thumb = (GameObject.Instantiate(Resources.Load("Thumbnail")) as GameObject).GetComponent<UnityEngine.UI.RawImage>();
        var canvas = GameObject.Find("Canvas");
        thumb.transform.SetParent(canvas.transform);

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
        pointerEnterEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOn));
        eventTrigger.triggers.Add(pointerEnterEntry);

        //exit
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback = new EventTrigger.TriggerEvent();
        pointerExitEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOff));
        eventTrigger.triggers.Add(pointerExitEntry);

        //drag
        EventTrigger.Entry dragEntry = new EventTrigger.Entry();
        dragEntry.eventID = EventTriggerType.Drag;
        dragEntry.callback = new EventTrigger.TriggerEvent();
        dragEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOn));
        eventTrigger.triggers.Add(dragEntry);

        //end drag
        EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
        endDragEntry.eventID = EventTriggerType.EndDrag;
        endDragEntry.callback = new EventTrigger.TriggerEvent();
        endDragEntry.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(ThumbnailOff));
        eventTrigger.triggers.Add(endDragEntry);

        return true;
    }

	void Update ()
    {
        // If keyframes have not been loaded and El Presidente has initialized the keyframes...
        if (!hasInitialized && ElPresidente.Instance.KeyframesGenerated)
        {
            // Load the keyframe images from file.
            loadImages();

            // Remember that we've initialized.
            hasInitialized = true;
        }

        // If the thumbnail is enabled and keyframes have been loaded, call the draw method.
        if (thumbnailEnabled && hasInitialized) drawThumbnail();
    }

    /// <summary>
    /// Initializes the keyframe array and loads the images into it from file.
    /// </summary>
    private void loadImages ()
    {
        // Create a new array to hold the keyframe images.
        images = new Texture2D[20];

        // Loop through the image array
        for (int i = 0; i < 20; i++)
            // And load each keyframe from file into the array.
            images[i] = LoadPNG(@"Assets/.screens/" + (i * 5) + ".png");

        // Scale the keyframe thumbnail to the correct size ratio.
        if (images[0].height < images[0].width)
        {
            float imageOff = (float)images[0].height / (float)images[0].width;
            float thumbOff = thumb.rectTransform.rect.height / thumb.rectTransform.rect.width;
            float offset = imageOff / thumbOff; 
            thumb.uvRect = new Rect((1 - offset) / 2, 0, offset, 1);
        }
        else if (images[0].height > images[0].width)
        {
            float imageOff = (float)images[0].width / (float)images[0].height;
            float thumbOff = thumb.rectTransform.rect.width / thumb.rectTransform.rect.height;
            float offset = imageOff * thumbOff; 
            thumb.uvRect = new Rect(0, (1 - offset) / 2, 1, offset);
        }
    }

    /// <summary>
    /// Toggles the keyframe thumbnail on.
    /// </summary>
    public void ThumbnailOn(BaseEventData baseEventData)
    {
        // Remember that the thumbnail is toggled on.
        thumbnailEnabled = true;

        // Set the thumbnail object to be visible.
        thumb.color = new Color(255, 255, 255, 255);
    }

    /// <summary>
    /// Toggles the keyframe thumbnail off.
    /// </summary>
    public void ThumbnailOff(BaseEventData baseEventData)
    {
        // Remember that the thumbnail is toggled off.
        thumbnailEnabled = false;

        // Set the thumbnail object to be invisible.
        thumb.color = new Color(255, 255, 255, 0);
    }

    /// <summary>
    /// Positions and draws the correct keyframe in the thumbnail.
    /// </summary>
    private void drawThumbnail ()
    {
        // Calculate half the width of the thumb used on the scrollbar.
        float thumbOffset = slider.targetGraphic.rectTransform.rect.width / 2;

        // Calculate the x-position of the left side of the slider in canvas space.
        float sliderLeft = sliderRect.position.x - (sliderRect.rect.width / 2) + thumbOffset;

        // Calculate the x-position of the right side of the slider in canvas space.
        float sliderRight = sliderLeft + sliderRect.rect.width - (2 * thumbOffset);

        // Create a variable to track the position of the mouse relative to the slider.
        float mouseRel = 0;

        // If the mouse is in the range of the slider, calculate the position of the mouse relative to the slider.
        if (Input.mousePosition.x > sliderLeft && Input.mousePosition.x < sliderRight) mouseRel = (Input.mousePosition.x - sliderLeft) / sliderRect.rect.width;

        // Otherwise, if the mouse is past the right boundary, set the relative position to one.
        else if (Input.mousePosition.x >= sliderRight) mouseRel = 1;

        // Set the thumbnail's x-position to that of the mouse.
        float imageX = Input.mousePosition.x;

        // Make the thumbnail stop at the left edge of the slider.
        if (mouseRel == 0) imageX = sliderLeft;

        // Also make the thumbnail stop at the right edge of the slider.
        else if (mouseRel == 1) imageX = sliderRight;

        // Set the thumbnail UI object's position.
        thumb.rectTransform.position = new Vector3(imageX, sliderRect.position.y + (thumb.rectTransform.rect.height / 1.5f), thumb.rectTransform.position.z);

        // Calculate the array position of the current keyframe image to display.
        int position = System.Convert.ToInt32(Mathf.Round((mouseRel * 100.0f) / 5.0f));

        // Scale position back if it is over length.
        if (position >= images.Length) position = images.Length - 1;

        // Set the thumbnail UI object's texture to the calculate keyframe.
        thumb.texture = images[position];
    }

    /// <summary>
    /// Loads and returns a .png from file as a Texture2D.
    /// </summary>
    public static Texture2D LoadPNG(string filePath)
    {
        // Create a new Texture2D.
        Texture2D tex = null;

        // Create a byte array to store the file data.
        byte[] fileData;

        // Ensure a file exists at the specified path.
        if (File.Exists(filePath))
        {
            // Read the file data into the byte array.
            fileData = File.ReadAllBytes(filePath);

            // Create a new Texture2D.
            tex = new Texture2D(2, 2);

            // Load the file data into the Texture2D.
            tex.LoadImage(fileData);
        }

        // Return the loaded Texture2D.
        return tex;
    }
}

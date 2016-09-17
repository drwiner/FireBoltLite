using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class AboutCinemaSuiteWindow : EditorWindow
{
    const string TITLE = "About Cinema Suite";
    const string MENU_ITEM = "Window/Cinema Suite/About";

    #region Language
    const string CINEMA_MO_CAP = "Cinema Mo Cap:";
    const string CINEMA_PRO_CAMS = "Cinema Pro Cams:";
    const string CINEMA_DIRECTOR = "Cinema Director:";

    const string VISIT_WEBSITE = "Visit our website:";
    const string WEBSITE = "www.cinema-suite.com";
    
    const string SUPPORT = "Support:";
    const string SUPPORT_WEBSITE = "www.cinema-suite.com/forum";

    const string EMAIL_SUPPORT = "Email Support:";
    const string EMAIL = "support@cinema-suite.com";

    const string LIKE_US = "Like us on Facebook:";
    const string CINEMA_SUITE_INC = "/CinemaSuiteInc";

    const string FOLLOW_US = "Follow us on Twitter:";

    const string WATCH_US = "Watch us on YouTube:";

    const string DISCLAIMER1 = "Cinema Suite has been developed by Cinema Suite Inc. Any attempt to decompile and/or reverse engineer Cinema Suite source code is strictly prohibited under copyright laws applicable in the country of use. \n\n";
    const string DISCLAIMER2 = "While Cinema Suite Inc. make every effort to deliver high quality products, we do not guarantee that our products are free from defects. Our software is provided 'as is' and you use the software at your own risk. \n\n";
    const string DISCLAIMER3 = "We make no warranties as to performance, merchantability, fitness for a particular purpose, or any other warranties whether expressed or implied. \n\n";
    const string DISCLAIMER4 = "No oral or written communication from or information provided by Cinema Suite Inc. shall create a warranty. \n\n";
    const string DISCLAIMER5 = "Under no circumstances shall Cinema Suite Inc. be liable for direct, indirect, special, incidental, or consequential damages resulting from the use, misuse, or inability to use this software, even if Cinema Suite Inc. has been advised of the possibility of such damages. \n\n";
    const string DISCLAIMER6 = "Microsoft® and the Microsoft Kinect® are registered trademarks of Microsoft Corporation. Cooke® is a registered trademark of Cook Optics Limited. Cinema Mo Cap, Cinema Director, and Cinema Suite Copyright© 2013. All rights reserved.";

    const string CMF_THANKS = "Cinema Suite Inc. extends a special thank you to the Canada Media Fund (CMF) for assistance in making Cinema Suite possible.";
    #endregion

    private Texture2D cinemaSuiteLogo;
    private Texture2D cmfLogo;
    private Texture2D facebookLogo;
    private Texture2D youtubeLogo;
    private Texture2D twitterLogo;

    private Vector2 scrollPosition;

    bool isMocapInstalled = false;
    string mocapProductVersion = string.Empty;

    public void Awake()
    {
        base.title = TITLE;
        this.minSize = new Vector2(450, 600f);
        
        // Load resources
        cinemaSuiteLogo = Resources.Load("Cinema Suite") as Texture2D;
        if (cinemaSuiteLogo == null)
        {
            UnityEngine.Debug.LogWarning("Cinema Suite.png missing from the Cinema Suite/About/Resources folder.");
        }

        cmfLogo = Resources.Load("CMF") as Texture2D;
        if (cmfLogo == null)
        {
            UnityEngine.Debug.LogWarning("CMF.jpg missing from the Cinema Suite/About/Resources folder.");
        }

        facebookLogo = Resources.Load("Facebook") as Texture2D;
        if (facebookLogo == null)
        {
            UnityEngine.Debug.LogWarning("Facebook.png missing from the Cinema Suite/About/Resources folder.");
        }

        twitterLogo = Resources.Load("Twitter") as Texture2D;
        if (twitterLogo == null)
        {
            UnityEngine.Debug.LogWarning("Twitter.png missing from the Cinema Suite/About/Resources folder.");
        }

        youtubeLogo = Resources.Load("YouTube") as Texture2D;
        if (youtubeLogo == null)
        {
            UnityEngine.Debug.LogWarning("YouTube.png missing from the Cinema Suite/About/Resources folder.");
        }

        // Find the installed Cinema Suite products.
        isMocapInstalled = System.IO.File.Exists("Assets/Cinema Suite/Cinema Mocap/Editor/CinemaMocap.dll");
        if (isMocapInstalled)
        {
            mocapProductVersion = FileVersionInfo.GetVersionInfo("Assets/Cinema Suite/Cinema Mocap/Editor/CinemaMocap.dll").ProductVersion;
        }

    }

    protected void OnGUI()
    {
        GUILayout.BeginVertical();
        
        // Display Logo
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(cinemaSuiteLogo);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // Display installed products
        GUILayout.Space(5);
        GUILayout.FlexibleSpace();

        if (EditorPrefs.HasKey("CinemaDirectorVersion"))
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(CINEMA_DIRECTOR);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("version {0}", EditorPrefs.GetString("CinemaDirectorVersion")));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (isMocapInstalled)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(CINEMA_MO_CAP);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("version {0}", mocapProductVersion));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (EditorPrefs.HasKey("CinemaProCamsVersion"))
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(CINEMA_PRO_CAMS);
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format("version {0}", EditorPrefs.GetString("CinemaProCamsVersion")));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        // Display links and emails
        GUILayout.Space(5);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.Space(50f);
        GUILayout.Label(VISIT_WEBSITE);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(WEBSITE))
        {
            Application.OpenURL(WEBSITE);
        }
        GUILayout.Space(50f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(50f);
        GUILayout.Label(SUPPORT);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(SUPPORT_WEBSITE))
        {
            Application.OpenURL(SUPPORT_WEBSITE);
        }
        GUILayout.Space(50f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(50f);
        GUILayout.Label(EMAIL_SUPPORT);
        GUILayout.FlexibleSpace();
        GUILayout.Label(EMAIL);
        GUILayout.Space(50f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(50f);
        GUILayout.Label(LIKE_US);
        GUILayout.FlexibleSpace();
        if(GUILayout.Button(facebookLogo))
        {
            Application.OpenURL("www.facebook.com/CinemaSuiteInc");
        }
        GUILayout.Label(CINEMA_SUITE_INC);
        GUILayout.Space(50f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(50f);
        GUILayout.Label(FOLLOW_US);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(twitterLogo))
        {
            Application.OpenURL("www.twitter.com/CinemaSuiteInc");
        }
        GUILayout.Label(CINEMA_SUITE_INC);
        GUILayout.Space(50f);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(50f);
        GUILayout.Label(WATCH_US);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(youtubeLogo))
        {
            Application.OpenURL("www.youtube.com/CinemaSuiteInc");
        }
        GUILayout.Label(CINEMA_SUITE_INC);
        GUILayout.Space(50f);
        GUILayout.EndHorizontal();

        // Display disclaimer
        GUILayout.Space(5);
        GUILayout.FlexibleSpace();

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
        GUILayout.TextArea(DISCLAIMER1 + DISCLAIMER2 + DISCLAIMER3 + DISCLAIMER4 + DISCLAIMER5 + DISCLAIMER6);

        GUILayout.EndScrollView();

        // Display supporters
        GUILayout.Space(10f);

        GUILayout.TextArea(CMF_THANKS);

        GUILayout.Space(5f);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(cmfLogo);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(5f);
        GUILayout.EndVertical();
    }

    [MenuItem(MENU_ITEM)]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AboutCinemaSuiteWindow));
    }
}


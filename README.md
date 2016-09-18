FireBoltLite
===========
A lite version of FireBolt, a story action and camera action temporal sequencer for the Unity Game Engine. Major attributes of liteness: FireBoltLite does not use AssetBundles to load assets, and does not generate keyframes. FireBoltLite features support for in-game-scripts as optional components of story actions. 


Instructions to get test started:

1) Load scene

2) Add "el presidente" script to FireBolt gameobject script1

3) Add FireBolt_Initialization script to FireBolt gameobject script2

4) Go to Resources folder and change rig to humanoid (from generic) for any animations. Required on animations "humanoid_idle" and "state". Change animation clip name in "state" to "state".

5) Go to Window - CinemaSuite - Create Pro Cam. Create it and replace the pro cam in scene under Firebolt/rig

6) Test the prefabs in Resources/Constants folder - to make sure the test assets are ready. If not, create an actor and name "cowboy" to use with the test script. Optionally add a terrain and place at (0,0,0) with orientation (0,0,0)


Instructions to create your own:

1) Look in "Input Set" for which of 3 scripts are being used to drive the story, drive camera, and cinematic model to associate story with specific assets.

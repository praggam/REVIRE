# REVIRE (REhabilitation in VIrtual REality): A Virtual Reality Platform for BCI-based Motor Rehabilitation

Designed for the **Meta (Oculus) Quest 2** HMD

# Device setup
- Download the Oculus App (https://www.meta.com/de-de/help/quest/articles/headsets-and-accessories/oculus-rift-s/install-app-for-link/) for PC and connect the headset via Link Cable or compatible USB-C cable.
- In Oculus app on PC
  - To test connection open **Oculus App > Devices > Oculus Quest and Touch > Device Setup** and go though the setup steps.
  - In **Settings > General** enable unknown sources
  - In **Graphics Preferences** set refresh Rate to 90 Hz
- In Oculus mobile app
  -  In **Settings > More Settings > Developer Mode** enable developer mode
- In the headset
  - In **Settings > Device > Hands and Controllers** enable hand-tracking and automatic switching to hands

# Importing this project to Unity
- Clone the repository
- Download Unity version 2019.4.14f1
- In **Unity Hub** click **Add** and select the **REVIRE** folder. All dependencies should download automatically (this may take a while).
- To run the game in the headset through Unity Editor, connect the headset via Link-compatible cable before opening Unity and allow Data Access in the headset.
- Open Unity and press Play to start the scene in the headset (this can be also done directly in the headset through Virtual Desktop), the scene should load automatically.

## Troubleshooting
- Unity scene doesn't load in the headset
  - Go to **File > Build Settings > Scenes In Build** and check if current current scene is on the list, if it is not, click **Add Open Scenes** 
  - Check if correct build platform was set up. Go to **File > Build Settings > Android**. Check if Oculus Quest 2 is shown in **Run Device** list and click **Switch Platform**
  - Check if XR Plugin is installed. Go to **Edit > Project Settings > XR Plugin Management and Install Plugin Management**. Set **Plugin Providers** to **Oculus** both for Android and Standalone
  - Repeat the **Device Setup** steps above

# Running built APK file on Oculus Quest
- Download the latest APK file
- Connect Oculus Quest device to your Windows PC via Oculus Link Cable
- Make sure that Unknown Sources and Hand-tracking are enabled in Oculus App, see **Device Setup** above.
- APK can be imported using Sidequest or Oculus Developer Hub:
  - Sidequest
    - Open [Sidequest](https://sidequestvr.com/setup-howto) and make sure Oculus is recognized (device connection status window in the upper-left corner should show Oculus Quest 2)
    - Go to **Install APK file from folder on computer** in the upper-right menu and select the .apk file
    - After successful installation, you can run the app in 2 ways:
      - Through SideQuest while connected via Link Cable
          - Go to **Currently installed apps** in the upper-right menu
          - Go to **Com.AnetaBarloga.HandsOculusIntegration** and click on **Settings** icon on the right-hand side
          - **Manage App > Launch App**
      - Or directly in the Oculus headset (the headset doesn't have to be connected via Link Cable)
          - Open app from **Apps > Unknown Sources**
  - Oculus Developer Hub
    - Go to **File Manager > On Device > App**, drag and drop the .apk file to install it 

# Imported assets
Third-party asset used in the project:
- [Oculus Integration](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022 )
- Environment
  - [HDRP Furniture Pack](https://assetstore.unity.com/packages/3d/props/furniture/hdrp-furniture-pack-153946)
  - [15 Original Wood Texture | 2D Wood](https://assetstore.unity.com/packages/2d/textures-materials/wood/15-original-wood-texture-71286)
  - [Imola from Bo Concept | Armchair](https://www.turbosquid.com/3d-models/imola-bo-concept-3ds-free/572435)
  - [Fuwl cage table model](https://www.turbosquid.com/3d-models/3d-fuwl-cage-table-model-1388495)
  - [Free 3D dining-tables Molteni](https://www.turbosquid.com/3d-models/3d-dining-tables-molteni--model-1188806)

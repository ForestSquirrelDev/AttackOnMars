PBR Rockets

Created & imported into unity by FuntechGames

Prefabs: Prefabs are located: Assets\PBRTurrets\Prefabs. Simply drag and drop a prefab into your scene. Test scripts have been left on the prefabs, use at your own will, commercial or private. 

Please note the scripts are test or sample scripts, they are not intended for final development. 

The prefab has a LOD system setup, you will need to adjust the camera distances to your needs. 
It will fire at any gameobject with the tag “Enemy”, with a rigidbody and collider.

Gatling Gun:
go_baseRotation: Rotating base
go_GunBody:    Gun body    
Go_barrel: Gun barrel
barrelRotationSpeed: Speed at which the barrel rotates
firingRange: Range in unity units in with the turret will aim and fire at the “Enemy”
muzzelFlash: Muzzle flash particle system, speed of the flashing is controlled by setting: Emission/Rate over time.


Snap Settings:
simply drag and drop them into your scene and use the following snap settings.
Move X: 1.3
Move Y: 0.5
Move Z:	1.125
Rotation: 60



Lighting:

It is important to set your lighting correctly to achieve good results as all textures a PBR textures. Follow the steps below to have the same setup as in my renders or demos. Please note, all objects must be marked as static to be baked correctly. Animated objects like the hangar doors can’t be marked as static and the use of  Light Probes is necessary to achieve good results.

The best Color Space for realistic rendering is Linear. This can be selected using the ‘Color Space’ property from (Edit>Project Settings>Player).
Global Illumination must be turned on. You can do this by going to (Window > Lighting > Settings), Open Scene tab inside Lighting window and enable Realtime Global Illumination.
Use Post-Processing image effects. I’ve used the FREE Post Processing Stack from Unity Asset Store. Bloom is needed to create the glows around the lights.

Tip: Turn off Auto “Generate Light maps(Window > Lighting > Settings)”  while building your scene. This will save on processing.

Materials:

Materials contain 2048 x 2048 maps, including Albedo, Metallic, Normal and Emission. Please contact me if you require larger maps. The normals are baked from high poly meshes.

You do not have the right to sell or distribute the models, contents or package separate of your final Unity game or Unity application.

Contact me if @ funtechgames@gmail.com you have questions.

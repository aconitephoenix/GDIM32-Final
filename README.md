# GDIM32-Final
## Check-In
### Group Devlog (Prompt B)
Our project currently utilizes raycasting to detect if the player is grounded or not and to move the camera towards the currently talking NPC once dialogue is initiated; both instances of raycasting are located in the PlayerController class.

There is a bool variable called "isGrounded" that is true when the player is touching the ground and false when they are currently in the air, and its value is changed in the HandleJump() class via raycasting. If the ray hits the floor, then isGrounded is set to true. Otherwise, it's set to false. This then determines whether or not the player is able to jump, as we don't want the player to be able to infinitely jump mid-air (that would be too crazy of a game...). We decided to use a raycast for the isGrounded value because it would make it easier to detect if the player was currently standing on a surface or not, since rays will behave based on whether or not they hit something, making our physics run smoothly. 

As for when the player is currently in dialogue, we used raycasting to detect where exactly the camera was hitting the NPC once the player was close enough in LerpToNPC() (which triggers when the player is in the InDialogue state). The position of the NPC that the ray last hit is then stored in a new Vector3 "npcPosition" that is used to calculate another Vector3 between the NPC and the player camera's positions. Using those values, the camera can then rotate at specific angles according to the NPC's position. We chose to use raycasting for this method specifically since the ray would be able to double-check if the player is actually close enough to the NPC and is currently looking in its direction before it enables the InDialogue state, preventing the player from properly interacting with an NPC if they're looking elsewhere or aren't close enough.


### Jess Tran
My contributions were mainly focused on the UI/Dialogue elements; I wrote most of the UI NPC/Interactable/Controller classes, heavily racking my brain when it came to figuring out the dialogue methods (especially the IEnmuerator TypeLine which I still have to fully fix...).

Personally, I believe that our proposal was detailed enough to build my parts of the project, and actually assisted in thinking about how to architect our solutions. For example, the plan that we made in week 7 helped me navigate the inheritance required for Interactables (as in how the NPC class should inherit from the Interactable class, since I was kind of having trouble with differentiating interactable detection until I remembered inheritance was a thing). I referred to the Trello board that we made every now and then to keep track of my tasks and mark which one I'd completed just to ensure I'd focus on the most important parts of the project first (aka the DIALOGUE!!!)
### Kaleb Reyes
My contributions were mostly on the environment of the project. I worked on the layout of the map, finding implementing a skybox asset that fit our game's aesthetic, implementing fog into our game, and getting the player's flashlight lighting working. For me, I didn't really see myself referring back to the proposal or break-down we did for this project. This could be because I focused more on the scene rather than coding, so far, which made looking back at either not really helpful since I knew the kinds of things I needed to add. We do have a trello board that helps us keep track of what's been done and what still needs working on and I occasionally look back at that to see what I could work on. It has also been helpful for looking at the scope of our game and seeing if we were on track to finishing the project on time.
### Sebastian Magana
My main contributions to the project were the player movement, camera movement, and the art. I implemented the player movement with a jump using rigidbody and raycasting for IsGrounded as well as a sprint with an FOV change in the camera as well as a sprint bar that decreases. I also made a specific method that ties into the NPC interactions, LerpToNpc which switches the players state from Normal to InDialogue and when in the InDialogue state the player can no longer move, and the camera lerps to the NPC's position which is found from a raycast. I also made the 3D Model for the 7/11 and imported the tree model and the freddy model from online. The freddy model is still a bit bugged, so I just put the placeholder animations in the scene for now which will properly be implemented into the NPC behavior this week. Overall the proposal break-down helped a ton. Specifically when I made the finite state machine for the player, it ended up working just how I wanted it from the breakdown, and I often went back to the document for the expected behavior. The trello board was also helpful for keeping focus of my tasks that needed to be done for Check In and what was needed for the others to continue working.

## Final Submission
### Group Devlog
Put your group Devlog here.


### Jess Tran
Put your individual final Devlog here.
### Kaleb Reyes
Put your individual final Devlog here.
### Sebastian Magana
Put your individual final Devlog here.

## Open-Source Assets
- [PSX-Style Shader](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/psx-style-shader-351978) - PSX Shader
- [Real Stars Skybox Lite](https://assetstore.unity.com/packages/3d/environments/sci-fi/real-stars-skybox-lite-116333) - Skybox Texture
- [Freddy Model](https://sketchfab.com/3d-models/forsaken-ar-freddy-fazbear-f6e019333d694cbfbb2f3fbc9e791763) - Model and Animations

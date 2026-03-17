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
Since the check-in, I've continued work on the dialogue/NPC logic while also implementing the quest system in our game (albeit it is REALLY messy...)

I specifically worked around our NPCs inheriting from the NPC class and figuring out QuestStates in the Player to control what options would appear for the NPCs' dialogue. For example, Slenderman has around 4 options hooked up to him in his dialogue node that checks the player's quest progress, but the UIController will change the option available depending on the current quest state. This is certainly not a well-scaling solution, but it works..!! I also added different dialogue fonts and an audio controller that could randomize the dialogue sounds!
### Kaleb Reyes
Put your individual final Devlog here.
### Sebastian Magana
This is a bit of a restatement of my self review but I mainly contributed to the aesthetics to the game. I did the music, the footsteps of the characters, and the page collection sound effect. I recorded them in my room. I reworked the UI for the dialogue, changing the text style and font, I imported the PSX style shader, I did the 3D Model for the building, I imported the trees, made the terrain elevation changes to the environment. I also added the sprite for Slenderman and the model and animation states for Freddy, and did the AI code for Freddy, as well as the player controller finite state machine, and rigidbody first-person controller/camera controller script. I also made a fog system with particle systems that renders in chunks around the player which gives the game its eerie vibe. Architecturally, my main contribution was the player controller and the freddy ai finite state machines and methods for each. They served a vital purpose in their behaviours and how they effect the mechanics outside of page collection and dialogue.







































































## Open-Source Assets
- [PSX-Style Shader](https://assetstore.unity.com/packages/vfx/shaders/fullscreen-camera-effects/psx-style-shader-351978) - PSX Shader
- [Real Stars Skybox Lite](https://assetstore.unity.com/packages/3d/environments/sci-fi/real-stars-skybox-lite-116333) - Skybox Texture
- [Freddy Model](https://sketchfab.com/3d-models/forsaken-ar-freddy-fazbear-f6e019333d694cbfbb2f3fbc9e791763) - Model and Animations

# Games and Artificial Intelligence Techniques - Assignment 2
This is the README file for Assignment 2 in Games and Artificial Intelligence Techniques (COSC2527/2528).

# Game Overview

Our game centres around a final boss battle, but since your magic is useless on the boss, you have to use it to brainwash enemies into fighting the boss for you. If you can dodge the boss’s attacks long enough for them to kill it, you win!

## How to play:
Move your character with WASD or the arrow keys. <br>
Click to cast a spell in the direction your mouse is pointing. <br>
Above your head is your health bar. When it runs out, it’s game over. <br>
Health can be restored when standing near a campfire.

These are the four types of “minion” enemies. Casting enough spells towards them turns the skull icon above their heads blue, meaning you have successfully brainwashed them into fighting the boss, instead of you. Your magic is only capable of brainwashing 5 minions at once. The blue skulls at the top tell you how many you’ve got left. If you notice the bar refilling, that’s because someone you recruited died, so you might want to find a replacement for your team.

There are also puddles around the map which upon being entered, will slow the entering unit down. Enemies that are seeking their target will try to avoid these puddles if possible. A tip to evasion is to sit in puddles and wait for the enemies to enter before you leave so you can create distance between them and yourself.

# Starting the game 
The scene used to start the game is the [Main Menu Scene](https://github.com/rmit-computing-technologies/cosc2527-2528-2023-assignment-2-team-01-cosc2527-23/blob/main/Assets/Scenes/MainMenu.unity)

# Video
Video about the assignment is found here: https://www.youtube.com/watch?v=AxRd6nhgqn4

# Mandatory: Student contributions
Please summarise each team member's contributions here, including both:
* An approximate, percentage-based breakdown (e.g., "Anna: 25%, Tom: 25%, Claire: 25%, Halil: 25%").
* A high-level summary of what each member worked on, in dot-point form.
Bear in mind that all members of your group must contribute to either the PCG or the generated art assets.

Ideally, any disputes reagarding the contributions should be resolved prior to submission, but if there is an unresolved dispute then please indicate that this is the case. Do not override each other's contribution statements immediately prior to the deadline -- this will be viewed dimly by the markers!

## Daniel Schellekens (s3900792) 30%
- Adapted Wave Function Collapse algorithm to run in Unity
- Created WFC sample images for puddles and walls
- Wrote algorithm to select and place tiles from the tilemap to match WFC outputs
- Wrote algorithm to post-process WFC output and remove blobs of wall/puddle below a certain threshold area.
- Implemented world generation loading screen with WFC animation and fade camera effect.

## Dien Xuan Dang Vo (s3899439) 25%
- Create new assests with generative AI using Stable Diffusion
   - Workflow document
   - Player - Sprite & sprite rendering based on movement
   - Campfire - Sprite & feature to heal the player in game
   - Phoenix - Sprite & feature to shoot fireballs and orbit the boss
- Wrote algorithm to place decorations (trees, wells, etc.) and minion spawning within the play area 
- Presented in the video for the respective sections worked on

## Laskaris Dionyssopoulos (s3845221) 25%
- Wrote code to open up walls in level output and make all areas on main level accessible
- Talked in video about wall openings
- Edited Video

## Alan Linto (s3806891) 20%
- Wrote code to generate the boss rooms around the outer walls of the scene.
- Implemented the random spawning of the player depending on the amount of free space in the scene.
- Implemented the spawning of boss-phoenix depending on the spawned boss room.
- Demonstrated the Boss Room part in the video.

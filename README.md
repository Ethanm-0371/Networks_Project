# GitHub Repository
https://github.com/Ethanm-0371/Networks_Project

# GitHub Release
https://github.com/Ethanm-0371/Networks_Project/releases/tag/v.0.2

# List of Contributions

- Jonathan Cacay - [xGauss05](https://github.com/xGauss05)
- Ethan Martín - [Ethanm-0371](https://github.com/Ethanm-0371)
  
All of the work was done using pair programming, where both of us collaborated at a single workstation. This ensured that we could work together seamlessly on the same .cs scripts without facing problems related to merging code. Merging changes in Unity can sometimes create more problems than it solves, so working this way helped us avoid such problems.

- Shooting behavior from the player where we used Raycast in order to implement it. Also implemented a way to avoid the camera clipping through the objects.
- Zombie behavior using a behavior state machine (Idle & Chase). These zombies are killable and are managed through an EntityManager and spawned in ZombieSpawnPoint/ZombieSpawnRoom tagged empty GameObjects.
- These behaviors also have a Wrapper of their own to send the data to the server.
- An extraction zone (green area) that if all of the current players in the lobby are in, the level will be completed and everyone will return to the lobby.

# Instructions
- Make sure you create a server.
- Then, other clients should put the local host IP (127.0.0.1).
- WASD for movement Input. LMB to shoot.
- B to Start the game (only the host).

# Main Scene to run in Unity
Scenes/Main_Menu

# Some difficulties

- In the shooting mechanic we wanted to implement for a third-person view, we encountered an issue. Our goal was to have the bullet originate from the gun’s muzzle and travel toward the crosshair in the center of the screen. However, if the player stood next to a column and the crosshair aimed at another column behind the character, the bullet would end up shooting backward. This created a logical inconsistency and resulted in unrealistic behavior.
- We encountered issues when trying to Destroy the GameObjects of the zombies. This led to a problem where, upon finishing a level, the zombies were not properly removed. Consequently, the client and server became unsynchronized, disrupting the correct functioning of the game.

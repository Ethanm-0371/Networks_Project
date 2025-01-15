# GitHub Repository
https://github.com/Ethanm-0371/Networks_Project

# GitHub Release
https://github.com/Ethanm-0371/Networks_Project/releases/tag/v.0.3

# List of Contributions

- Jonathan Cacay - [xGauss05](https://github.com/xGauss05)
- Ethan Martín - [Ethanm-0371](https://github.com/Ethanm-0371)
  
Most of the work was done using pair programming, where both of us collaborated at a single workstation. This ensured that we could work together seamlessly on the same .cs scripts without facing problems related to merging code. Merging changes in Unity can sometimes create more problems than it solves, so working this way helped us avoid such problems.

- Implemented Pause Menu. In this screen you can Continue, Exit Server and Exit to Desktop.
- A brand new Level_2 and MainMenu screen.
- Fixed jitter/latency for client side.
- Implemented ping to server. When a client does not receive a pong from the server, client gets timeout.
- A bandwidth problem was solved. Packets that were more than 1024 bytes are sent in the next update.
- When a Client exits the game, its GameObject destroys and broadcasts it to all the players. If the Server exits the game, all clients will be timeout.

# Instructions
- Make sure you create a server.
- Then, other clients should put the local host IP (127.0.0.1).
- WASD for movement Input. LMB to shoot.
- E to Open the door.
- ESC open Pause Screen. (only in Gameplay)

# Main Scene to run in Unity
Scenes/Main_Menu

# Some difficulties
- There's an error when trying to move before starting the game. The game receives an action packet in which there is no owner for it and a null exception occurs.
- There was a bug in which the players were spawning outside the safezone. It is currently fixed, but this took a lot of time to fix.




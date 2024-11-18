# GitHub Repository
https://github.com/Ethanm-0371/Networks_Project

# GitHub Release
https://github.com/Ethanm-0371/Networks_Project/releases/tag/v.0.1

# List of Contributions

- Jonathan Cacay - [xGauss05](https://github.com/xGauss05)
- Ethan Martín - [Ethanm-0371](https://github.com/Ethanm-0371)
  
All of the work was done using pair programming, where both of us collaborated at a single workstation. This ensured that we could work together seamlessly on the same .cs scripts without facing problems related to merging code. Merging changes in Unity can sometimes create more problems than it solves, so working this way helped us avoid such problems.

What we can say is who originally came up with the ideas.

For the things we done for the first delivery are the following (you can check the commits as well):

- PacketHandler class, in charge of handling incoming and outgoing packets. Idea by Jonathan Cacay.

- SceneHandler class, so that when the client has finished loading the static objects, would request the server for the dynamic objects. Idea by Ethan Martín.

- NetObjectsHandler class, a script that handles the creation, update of the NetObjects. Idea by Jonathan Cacay.

- Action-Based inputs, player behavior accepts keyboard inputs, but translates it into Actions. Idea by Ethan Martín.

- Passive World State replication, users send their "Actions" and the server acknowledges it and broadcasts it back towards the other clients. Idea by Jonathan Cacay.

- Wrappers namespace, a .cs script where all the Wrappers are located. Useful for sending stuff through the networks. Idea by Ethan Martín.

- Game has a "GameClient" and a "GameServer". Normal clients have only GameClient while the host has GameClient and a GameServer. Idea by both.

Overall, our goal was to establish the foundation of the game. At the moment, we are transmitting all the information, but in the future, we aim to optimize this by sending only specific variable changes.

# Instructions
- Make sure you create a server.
- Then, other clients should put the local host IP (127.0.0.1)

# Main Scene to run in Unity
Scenes/Main_Menu

# Some difficulties
We faced difficulties when trying to serialize a Dictionary, as both the key and value are needed to be included. The solution we found was to create a Serializable Wrapper class, "DictionaryEntryWrapper" which holds both the key and the value. Then, we created a list of DictionaryEntryWrapper objects, added all the dictionary entries to this list, and serialized the list. Then, deserialize back into a list with which we populate a dictionary on the receiving end.

Another issue we encountered was the difference in input frequency. A client with higher FPS would send more inputs to the network which sometimes caused data loss. To address this temporarily, we capped everyone’s FPS at 60 and turned off VSync.

Additionally, we faced a stuttering problem when updating positions across all clients. This was caused by a Coroutine that sent updates for all network objects every 0.2 seconds. As a temporary solution, we removed this delay to ensure smoother updates.

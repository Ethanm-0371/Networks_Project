# GitHub Repository
https://github.com/Ethanm-0371/Networks_Project

# GitHub Release
to do

# List of Contributions
All of the work was done in Pair-Programming, where both of us were working together in
one workstation for the reason that if we are coding both of us in the same .cs script,
we will then have problems on merging stuff, and merging things in Unity could cause
more problems rather than solutions.

What we can say, is the one who gave the idea for the project.

For the things we done for the first delivery are the following (you can check the commits
as well):

- PacketHandler class, in charge of handling incoming and outgoing packets. Idea by 
Jonathan Cacay.

- SceneHandler class, so that when the client has finished loading the static objects, would
request the server for the dynamic objects. Idea by Ethan Martín.

- NetObjectsHandler class, a script that handles the creation, update of the NetObjects.
Idea by Jonathan Cacay.

- Action-Based inputs, player behavior accepts keyboard inputs, but translates it into
Actions. Idea by Ethan Martín.

- Passive World State replication, users send their "Actions" and the server acknowledges it
and broadcasts it back towards the other clients. Idea by Jonathan Cacay.

- Wrappers namespace, a .cs script where all the Wrappers are located. Useful for sending
stuff through the networks. Idea by Ethan Martín.

- Game has a "GameClient" and a "GameServer". Normal clients have only GameClient while the
host has GameClient and a GameServer. Idea by both.

Overall, we wanted to create the base for the game. Currently we are sending the whole
information, but in the future we want to send variable specific changes.

# Instructions
- Make sure you create a server.
- Then, other clients should put the local host IP (127.0.0.1)

# Main Scene to run in Unity
Scenes/Main_Menu

# Some difficulties
We encountered difficulties on how to serialize a Dictionary, where a key and a value were
needed.
The solution we found, is to create a Serializable Wrapper class, DictionaryEntryWrapper 
where the key and a value is needed.
Then, we create a List of DictionaryEntryWrapper and add all the dictionary entries
towards that list and serialize it.
When receiving this packets, deserialize it as a list.

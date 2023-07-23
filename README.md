# RememberMe

A [NeosModLoader](https://github.com/neos-modding-group/NeosModLoader) mod for [Neos VR](https://neos.com/) that can automatically sign in or auto-fill even after the 7 days by remembering the username and optionally the password..

Passwords are by default encrypted with extra entropy using your secret machine ID, but this can be configured using a custom password. 

**WARNING!**: All saved passwords are stored using c# built in 'ProtectedData' using either the secretMachineID or a custom password for added entropy, this does mean that the password is stored on the device though. 

As this mod is dealing with your username and password, I suggest inspecting the code and compile it yourself, though a compiled form is shared. 


## Installation
1. Install [NeosModLoader](https://github.com/neos-modding-group/NeosModLoader).
2. Place [RememberMe.dll](https://github.com/AlexW-578/RememberMe/releases/latest/download/RememberMe.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Neos logs.


Inspired by the old VRChat mod [RememberMe](https://github.com/loukylor/VRC-Mods/tree/RM-1.0.6#rememberme) by [loukylor](https://github.com/loukylor)

I am not liable for any damages that may occur when using this mod as specified in the licence.


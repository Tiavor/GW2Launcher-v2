# GW2Launcher-v2

GuildWars2 accountmanager version 2

Setup



1. Browse for GW2.exe
2. enter command lines if needed e.g. -maploadinfo
3. ForEach ( Account in UserAccounts)

{

    3.1. Start GuildWars2
    3.2. activate "remember accountname" and "remember password"
    3.3. login to GW2 (replace the login data with thedata of the current account)
    3.4. [setup the options in game (they might be wrong)]
    3.5. close GW2
    3.6. Press the "Set" button
    3.7. [enter a name for that account, e.g. "Main"]
    3.8. [check the shader button if you want to use shaders*]
}



Info:

After an update or when you change the graphical or audio options ingame, you have to press Set again if you want to save the changes.

Check Autosave for doing this automated (saves after gw2 is closed)



* When "shaders?" is checkt, it will rename a file from "D3D9.dll2" to "D3D9.dll" in the bin folder.

Additionally when there is a file named ReShadeUnlocker.exe present in the GW2 folder, it will start it.

When unchecked, it will rename it the other way round.



Usage:

The button right to the Start button sets the last login state to the current time without starting GW2

Indication states of the RadialButton left to the last login state:

[green]    -> last used account today

[blue]     -> equals "today"

[yellow]   -> last started yesturday, daily login available today

[red]      -> missed a day or more of login rewards

[empty]    -> no account file available

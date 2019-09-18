TextChat
======
made by Joker 119
## Description
Adds text-based chat into SCP:SL via the player console.

### Features
 - Staff can block certain people from sending messages either permanently or for a number of rounds.
 - Players can locally mute players to not see their messages.
 - Configuration to determine what teams see messages from what other teams.
 - Area-based text chat options.
 - Persistant blocks and mutes through server restarts.

### Config Settings
Config option | Config Type | Default Value | Description
:---: | :---: | :---: | :------
tc_blacklisted_words | String List(array) | Empty | A list of words which will prevent a user from sending a message. This matches based on if the message contains the word in any way, IE: "hello" contains "hell".
tc_teamA_cansee_teamB | Bool | true | Whether or not (teamA) can see messages from (teamB). Valid teams are listed below.
tc_blocked_path | String | {The directory the SCPSL executable is in}/TextChat/blocked.txt | Where to hold the persistant list of sbloced users.
tc_local_mute_path | String | {The directory the SCPSL executable is in}/TextChat/muted.txt | The location to save persistant local player mutes.
tc_area_chat | Bool | false | Whether or not messages should be checked by distance to determine who sees them. (THIS DOES NOT OVERWRITE THE TEAM SETTINGS ABOVE)
tc_area_size | Float | 60f | The range of area-based messages, if enabled.
tc_admin_bypass | Bool | true | Whether or not staff should bypass all team and range checks for messages. Blocks and local mutes still apply.
tc_admin_badges | String List(array) | Empty | If badge names are supplied here, only those badges will recieve admin_bypass, instead of all those with RA access.
tc_cooldown_time | Float | 1.5f | The cooldown time incured on a player between being able to send messages. (to prevent spam)
tc_intercom_send_all | Bool | true | Allows a user to use the intercom for a single server-wide message. Does not bypass intercom cooldown, requires they be in normal activation range of the Intercom to use.

### Teams
  Name | | | Who is in that team
:---: | :---: | :---: | :------
ci | | | Chaos Insurgency / Class-D
mtf | | | Cadets, Lieutenants, Commanders, Guards and Nerds
scp | | | Anomalies
tut | | | Tutorials
spec | | | Spectators


### RA Commands
  Command |  |  | Description
:---: | :---: | :---: | :------
**Aliases** | **chat** | 
chat block | PlayerID | Round Count (use -1 for permanent) | Blocks the indicated user for the specified number of rounds.
chat unblock | PlayerID | ~~ | Unblocs the user.

### Console Commands
  Command | | | Description
:---: | :---: | :---: | :------
.chat | message | | Sends a chat message to other players' consoles.
.mute | PlayerName | | Locally mutes the players, preventing you from seeing that persons messages.
.unmute | PlayerName | | Unmutes the player locally (blocks still apply)

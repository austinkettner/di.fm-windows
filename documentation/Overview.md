# DI.FM
## A Digital Imported Radio App for Windows 8

<img src="http://quixby.github.com/DI.FM/img/loading_5.png" height="350px">

DI.FM is an application that streams radio's available on [Digitally Imported Radio](http://www.di.fm/).

## Windows 8 Features 
[Reference](http://msdn.microsoft.com/en-us/windows/apps/hh852650.aspx)

1. All users will have a cloud-enabled object save. Favorites, Play time (on each channel, total), and user settings should be saved to the cloud via the [Live SDK](http://msdn.microsoft.com/en-us/library/live/hh826551.aspx).
	1. Any channel marked as a 'favorite' should be saved to the users cloud profile
	2. As users play channels, we should store this play information on their cloud profile. This includes total listen time, total listen time per channel, last channel listened to.
	3. Everytime a user starts a channel, we need to mark the start-time of the stream and when the user ends/closes that stream, this includes if they switch to another channel (and activate it, not jsut browse to another channel page). This should be saved to their cloud profile as well. Unix Timestamps are likely a great choice here, or if windows has another format.
2. Implementation of Search, Share, and Play To App Contracts
3. Semantic Zoom support for the Main View, enabling quick picking over to various 'sections'. As shown in Main_4

## App Workflow and Details

### Main View
Users start in DI.FM on the Main View. Which is a WrapPanel type layout, spanning out to the right, past the user's initial visibility. This main view is distributed into a few sections 'Now Playing', 'Favorite Channels', and 'All Radio Channels'.

<img src="http://quixby.github.com/DI.FM/img/main_4.png" height="175px">

1. Now Playing showcases whatever is the currently playing channel in DI.FM. When no channel is currently active, then this should show the most recently played channel. This lists the Channel Name, and Currently playing song title under the Album Art.
2. Favorite Channels lists out all of the channels the user has designated as a 'favorite' (stored in their cloud profile). The display of these channels are in the form of a rectangle with the currently playing song on that channel listed as well. main_6 to main_11 desmontrate the UI implementation further.
3. All Radio Channels provides a Grid list of all of the Radio Channels currently available, each channel's Album Art acts the Entire Square image, with the Channel Title Listed on the bottom of the Tile with a semi-transparent black bar behind the text.

### Channel View
Upon clicking any tile, such as favorite or one of the tile's under 'All Radio Channels', the user switches to the Channel View. In this view, they are greeted with a highly emersive view of the that specific channel only.

<img src="http://quixby.github.com/DI.FM/img/channel_5.png" height="350px">

1. Now Playing Area showcases the currently playing song, and a Play/Pause button
2. Under Now Playing are two buttons providing the user the ability to view other channels near Vocal Trance, this is alphabetically determined currently, but may evolve into a more dynamic/smarter system.
3. Track History is a section that simply lists the previous tracks that have played on this Channel, with the Arrow in the top Right of this section being clickable to load 5 even older tracks (6, 7, 8, 9 and 10).
4. The Title and Arrow in the Top Left are simple Title + SubTitle headings with a navigational button back to 'Main View' (where they last left off scroll wise, not a reset position).

Each Channel View will have a custom background image, and some possible variation on coloring for elements such as the Now Playing, and Track History. Each channel should be built with this in mind.

### Snap View
At any given time the user should be able to load a Snap View of DI.FM. This is simple a constrained view of the Channel and Main View, but should not limit functionality.

<img src="http://quixby.github.com/DI.FM/img/snap_10.png" height="350px">

No example is available for the Main View Snap View yet, however all content in any snap view should Scroll vertically only, never horizontally. The Channel Snap View however has no srolling whatsoever. The heavy scrolling situation present in the Main View (due to the list of the radio channels) is acceptable due to Favorites, and previous example cases on Windows Phone show a non-negative user experience.


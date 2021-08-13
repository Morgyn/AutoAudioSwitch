# AutoAudioSwitch
Set Application Default audio device on application start.

## Description
Useful if you use a different device for specific applications who do not observe or override application audio settings.

An example use of this is the game Escape from tarkov, which ignores App volume device preferences and will always use the default device.

This application watches for applications to start based on their filename, once they have started and a 30 second delay has passed (to be tunable) it will then set the target application's sound device to the default, then to the chosen device.

## Configuration
The format of the ini is:

```
[Application.exe]
Device=SoundVolumeView\Command-Line\Friendly-ID\Render
Delay=30
```
Delay is optional, if omitted a default of 30 will be used.

The ini will need to be configured for your use case.

To find your Command-Line Friendly-ID run SoundVolumeView.exe in the SoundVolumeView directory. Locate your Render device (Sound output) in the list, right click and go to properties. It will be listed 5th from bottom, labeled "Command-Line Friendly ID:". Copy and Paste that after "Device=".

The inifile and application supports multiple exes and will watch them all.

If you forget to run AutoAudioSwitch before the application started, when AutoAudioSwitch will check if the target exes are already running and try and set them immediately.

## SoundVolume View
This application is reliant on another excellent peice of software, bundled with this release. It is called SoundVolumeView from NirSoft.
This can be found at: https://www.nirsoft.net/utils/sound_volume_view.html

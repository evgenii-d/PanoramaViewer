# PanoramaViewer

360 Photo Video Panorama Viewer

Features:

* Automatic playback
* Transition between panoramas with a Fade In/Fade Out effect
* Zoom In with Right Mouse Button (Windows/Linux) or two-finger tap (Android)
* Switch between panoramas using the Arrow Left/Page Down and Arrow Right/Page Up keys (**autoPlay** must be set to `false`)

Made with Unity 2022.3.16.f

Tested on Windows 11 (23H2), Ubuntu 22.04, Android 13

## Compatible files

### 360 content

|Type|Images|Videos|
|----------|----------|----------|
|spherical (2:1)|+|+|
|stereo spherical equirectangular (1:1)|+|+|
|cubemap (6:1)|+||

### File formats

|Images|Videos|
|------|------|
|.jpg|.mp4 (H.264, H.265; AAC)|
|.png|.webm (VP8; Vorbis)|

#### Recommended encoding settings for ffmpeg

`ffmpeg -i input.mov -c:v libx264 -preset medium -crf 23 -vf "scale=XXXX:XXXX" -c:a aac "output.mp4"`

`ffmpeg -i input.mov -color_primaries bt709 -color_trc bt709 -colorspace bt709 -color_range pc -c:v libx265 -vf scale=XXXX:XXXX -pix_fmt yuv420p -profile:v main -level:v 3.0 -c:a aac output.mp4`

References:

* [Unity Manual. Video file compatibility][1]
* [Android for Developers. Supported media formats][2]
* [FFmpeg. H.264 Video Encoding Guide][3]
* [FFmpeg. H.265/HEVC Video Encoding Guide][4]

## Application Settings

There are two .json files that will be generated upon the first launch.

Path to the setting files:

* Android: `/storage/emulated/<userid>/Android/data/<packagename>/files`
* Windows/Linux: Application folder

### PanoramaViewerSettings.json

```json
{
    "autoPlay": true,
    "imageDelay": 17.0,
    "fadeDuration": 2.0,
    "imageFormats": [
        ".jpg",
        ".png"
    ],
    "videoFormats": [
        ".mp4",
        ".webm"
    ]
}
```

### FreeCameraSettings.json

```json
{
    "fieldOfView": 60.0,
    "mouseSensitivity": 1.0,
    "zoom": true,
    "zoomMagnification": 1.5
}
```

## Nota bene

### Android Settings

#### Build settings

Edit > Project Settings > Player > Android Settings > Other Settings

Under Configuration

* Set **Scripting Backend** to **IL2CPP**
* Disable ARMv7
* Enable ARM64

#### Custom Main Manifest

Edit > Project Settings > Player > Android Settings > Publishing Settings > Enable **Custom Main Manifest**

Example of custom manifest with additional permissions
```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest
    xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.unity3d.player"
    xmlns:tools="http://schemas.android.com/tools">
    <application>
        <activity android:name="com.unity3d.player.UnityPlayerActivity"
                  android:theme="@style/UnityThemeSelector">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
            <meta-data android:name="unityplayer.UnityActivity" android:value="true" />
        </activity>
    </application>
    <uses-permission android:name="android.permission.READ_MEDIA_IMAGES" />
    <uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
</manifest>
```

### Include shaders

Edit > Project Settings > Graphics

Under **Built-in Shader Settings** change **Size** and select required shader

[1]:https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
[2]:https://developer.android.com/media/platform/supported-formats#recommendations
[3]:https://trac.ffmpeg.org/wiki/Encode/H.264
[4]:https://trac.ffmpeg.org/wiki/Encode/H.265

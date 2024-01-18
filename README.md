# PanoramaViewer

Made with Unity 2022.3.16.f

Tested on Windows 11 (23H2), Ubuntu 22.04, Android 13

## Supported 360 content and file formats

### 360 images

* spherical (2:1)
* stereo spherical equirectangular (1:1)
* cubemaps (6:1)

### 360 videos

* spherical (2:1)
* stereo spherical equirectangular (1:1)

### File formats

* images - .jpg, .png
* videos - .mp4, .webm

### Recommended encoding settings for ffmpeg

 `ffmpeg -i input.mov -color_primaries bt709 -color_trc bt709 -colorspace bt709 -color_range pc -c:v libx265 -vf scale=XXXX:XXXX -pix_fmt yuv420p -profile:v main -level:v 3.0 -c:a aac output.mp4`

## Nota bene

### Android build settings

Edit > Project Settings > Player > Android Settings > Other Settings

Under Configturation

* Set "Scripting Backend" to "IL2CPP"
* Disble ARMv7
* Enable ARM64

---

Edit > Project Settings > Player > Android Settings > Publishing Settings

Enable "Custom Main Manifest"

### Include necessary shaders

Edit > Project Settings > Graphics

Under "Built-in Shader Settings" change "Size" and select required shader

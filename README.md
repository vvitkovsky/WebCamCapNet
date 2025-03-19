# WebCamCapNet
This project was created to capture video and audio from web cameras and screen using FFMpeg.

To define devices and capture parameters - need to modify CaptureParameters list in appsettings.json file.

Possible parameters: 
* VideoDeviceName - video device name, can be omitted if only audio is needed
* AudioDeviceName - audio device name, can be omitted if only video is needed
* VideoResolution - video resolution
* RTBufferSizeMb - real time buffer size in mb
* EncodingSpeed - encoding speed, possible values are 0 - 8 (ultrafast, superfast, veryfast, fast, medium, slow, slower, veryslow)
* IntraFrameInterval - intra frame interval (frame count)
* FrameRate - result video frame rate
* ConstantRateFactor - compression quality
* VideoCodec - video compression codec
* AudioCodec - audio compression codec 
* SegmentTimeSec - number of seconds which can be used to split recording to several files, 0 - no split
* SegmentMaxFileCount - maximum number of recorded files (in case of split enabled)
* OutFilePrefix - output file prefix, result file name is <prefix>_%Y-%m-%d_%H-%M-%S.avi
* OutputDir - output directory (can be relative)

Application supports several captures in parallel from different devices.
Devices should exist, list of current devices and codecs can be checked using swagger web page: 
http://localhost:8900/swagger

App can be registered as Windows service (cmd files attached).
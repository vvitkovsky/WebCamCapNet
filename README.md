# WebCamCapNet
This project was created to capture video and audio from WebCamera's and screen using FFMpeg.

To define devices and capture parameters - need to modify CaptureParameters list in appsettings.json file.

Application supports several captures in parallel from different devices.
Devices should exist, list of current devices can be checked using swagger web page: 
http://localhost:8900/swagger

App can be registeed as Windows service (cmd files attached).

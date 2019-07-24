# All Caps

[![Build Status](https://scaryrawr.visualstudio.com/AllCaps/_apis/build/status/scaryrawr.allcaps?branchName=master)](https://scaryrawr.visualstudio.com/AllCaps/_build/latest?definitionId=1&branchName=master)

All Caps is a Windows only tool to generate subtitles for audio that is being played on your computer.

## Why All Caps

Hmmm... so this project is called All Caps as a play on captions and typing in ALL CAPS (angry online typing).

Did you know that twitch supports [captions](https://help.twitch.tv/s/article/guide-to-closed-captions?language=en_US#HowtoUseLiveClosedCaptionsforBroadcasters
) being embedded in your stream?

[OBS](https://obsproject.com/) also supports streaming captions if you turn it on [in Windows](https://projectobs.com/en/news/obs-studio-17-0-0/).

There's also [web captioner](https://webcaptioner.com/).

All these solutions require action on the streamer's side and aren't enabled by default (try to find some twitch streams with captions).

**The goal** with all caps is that using the audio playing on your computer you can feed it back into a speech recognizer to get a similar effect.

## Status

- Using .NET's [SpeechRecognitionEngine](https://docs.microsoft.com/en-us/dotnet/api/system.speech.recognition.speechrecognitionengine?view=netframework-4.8) which is built into Windows
- Can use [Azure Cognitive Speech Services](https://azure.microsoft.com/en-us/services/cognitive-services/directory/speech/) if configured with key,region pair
- Dependency on [NAudio](https://github.com/naudio/NAudio)
- Using [FontAwesome](https://fontawesome.com/) for icons
- Works even if speakers are muted (audio is captured before being sent to hardware)

# Finally a shot timer for PC guys! #
Detects gunshots and tracks the time between them.

## Features ##
  * Customizable random delay before BEEPing
  * You can use your own BEEP sound
  * Playing random "Are you ready? Standby..." sounds file before BEEPing
  * Choosing an audio input device (Webcam with mic, default microphone, etc.)
  * Configuring the shot detector (so you can practice dry firing, detect airsoft, detect hand claps, finger snaps, etc.)
  * Big interface, big buttons so you could use this app on a touchscreen
  * Free and opensource baby!
  * PAR times will be implemented in the future

That's pretty much it for now. Based on Open Shot Timer, this project is a WPF C# conversion of the old Java code.


---

## Here are a few screens for ya ##

<a href='https://picasaweb.google.com/lh/photo/OxoomXUzdE9cDAE4u8nWwdMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh6.googleusercontent.com/-4Hir1-sWw3I/VEE2aEdY7iI/AAAAAAAAAKo/KLDSZfRO4D0/s640/oKfOrsB.png' /></a>

<a href='https://picasaweb.google.com/lh/photo/GM-egICYAPDrMGwy5_KMAdMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh4.googleusercontent.com/-lFOhAQFhTcU/VEE2YJeWfzI/AAAAAAAAAKw/fiquZA3BbZ4/s800/8mWLWFI.png' height='100px' /></a> <a href='https://picasaweb.google.com/lh/photo/GtoKjP9nqG2W7Rc4A8iGZdMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh6.googleusercontent.com/-9IOVPz5l1RI/VEE2YxixLkI/AAAAAAAAAKI/R7eeBvDht-0/s800/MrpR4hS.png' height='100px' /></a>

<a href='https://picasaweb.google.com/lh/photo/v8xlkBo-Wrm0Y3E8R_54hNMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh4.googleusercontent.com/-zIDpI3l8evQ/VEE2YrbgYfI/AAAAAAAAAKs/BLERAnLRkxw/s400/BeDr4wW.png' /></a> <a href='https://picasaweb.google.com/lh/photo/1vF-scIuadMjTT3y7JRZOdMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh3.googleusercontent.com/-7TkZupjHJQQ/VEE2ZRZMvFI/AAAAAAAAAKQ/XrYfA-xeLd8/s400/YZntUPE.png' /></a>

<a href='https://picasaweb.google.com/lh/photo/24b2jQgYTZhWUOtPAbScUtMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh4.googleusercontent.com/-LUcdgdPYRY8/VEE2aPXu_oI/AAAAAAAAALk/DS-FFoC5GHk/s400/wu0YcJl.png' /></a> <a href='https://picasaweb.google.com/lh/photo/3GoLZZ9hl4MRE0yxI6pbSdMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh3.googleusercontent.com/-zjSb1DPkjb8/VEE2ZKxFYXI/AAAAAAAAAKk/BcruDyw-AMk/s400/Ta9myIw.png' /></a>

<a href='https://picasaweb.google.com/lh/photo/hGXw1PKjjo6g-JukOeVudtMTjNZETYmyPJy0liipFm0?feat=embedwebsite'><img src='https://lh4.googleusercontent.com/-R2Z-3ONXd7k/VEE2YJa-nRI/AAAAAAAAAK0/6nkKImB4WTA/s800/6N3E8Oj.png' /></a>
You can also check them [there](http://tacticalfreak.blogspot.com/p/pc-shot-timer.html).


---

## The idea ##
I first had a need: measuring time between shots. Since I know it's not that complicated to code, I looked on the Interweb to find a software doing just that. Didn't find one except [there](http://www.brianenos.com/forums/index.php?showtopic=86330). However the Java app that has been posted (the very old .jar) worked but... well it wasn't enough for me.

Lots of shot timers exist on smartphones. Many are paying software. As I don't have no smartphone, I ended up creating my own shot timer; this page here being the whole result of the project. I coded the basic functions of that stuff in 2 days pretty much. Gotta ask yourself why other people are charging for such a thing...

The guys who created Open Shot Timer:
http://www.brianenos.com/forums/index.php?showtopic=86330

And their sources:
https://code.google.com/p/openshottimer/


---

## Prerequisites ##
  * Windows Vista or more recent
  * .NET Framework 4.0 Client Profile
  * A microphone or something like that


---

## TODOs/Bugs ##
  * Different sensibility config
  * Implementing PAR times :) oh yeah
  * Implementing audio file import and reading?


---

## Downloads ##
[All](https://drive.google.com/folderview?id=0B4j_jC5UOtTPOFdNZTkwdnVOZ2s&usp=drive_web#list)

[Version 1.0.0.7](https://drive.google.com/open?id=0B4j_jC5UOtTPY3JmSzZpN25OTDA&authuser=0)
  * This version broke my ballz
  * Added sounds selection for everything in the option window
  * Default config created when none is found
  * Starting from a command line will output stuff in it
  * UI tweaks

[Version 1.0.0.6](https://drive.google.com/open?id=0B4j_jC5UOtTPWmFGMll1RXR4RUE&authuser=0)
  * Perfs improvements (okay just a little)
  * Starting from a command line will output stuff in it but only in debug right now
  * New "HUD buttons" directly on the timer to change some options on the fly
  * The "loudness" parameter is now exposed in the options
  * The config is saved in the same folder, portable style
  * UI tweaks and reskin
  * Few bug fixes

[Version 1.0.0.5](https://drive.google.com/open?id=0B4j_jC5UOtTPakFwSnJEQ1NpVXc&authuser=0)
  * Fixed crash on startup when no input device is available
  * Better error handling
  * UI fixes

[Version 1.0.0.4](https://drive.google.com/open?id=0B4j_jC5UOtTPU1pLTjdwc1IxYjg&authuser=0)
  * Styled the option window tabs
  * Console output
  * Input device added in the options

[Version 1.0.0.3](https://drive.google.com/open?id=0B4j_jC5UOtTPamlGMlI1ZlJCT1U&authuser=0)
  * kinda stable I think.


---

## License ##
|![http://i.imgur.com/oGGeSQP.png](http://i.imgur.com/oGGeSQP.png)|The license for "PC Shot Timer" is the [WTFPL](http://www.wtfpl.net/): _Do What the Fuck You Want to Public License_.|
|:----------------------------------------------------------------|:--------------------------------------------------------------------------------------------------------------------|

I don't think the app can actually comply with that license.
Custom fonts are used and are under a different license for instance. Same goes for the Open Shot Timer ported code that I still consider not altered enough to be considered as 'brand new' pieces of code that I could 'own' and release under WTFPL.
Oh well.


---

## How do I talk to you? ##
Go [there](http://tacticalfreak.blogspot.com/p/pc-shot-timer.html) and post a comment.


---

![http://i.imgur.com/oAyq62K.png](http://i.imgur.com/oAyq62K.png)


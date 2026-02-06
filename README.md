# DSBar

DSBar is a simple Windows application designed to display your DualSense controllers' (***note the use of plural!*** *multiple controllers can be displayed at the same time*) battery level.

![A demonstration of what the app looks like in action. Two controllers are connected; one is charging and its battery is almost full, one is almost empty.](./artwork/tray_screenshot.png)

## Supported devices

Currently, only DualSense (its base version, ***not*** the Edge) is supported at the moment. Support for other controllers should be fairly trivial to add, but I do not own an Edge to test it with. Feel free to contribute support for Edge, if you can test it on one.

## Why?

Steam does not show battery information, and while programs like this one already exist, I could not find one that supported multiple controllers at once. For this reason, I decided to create my own tool that would combine the features of existing tools into one, definitive package.

Most of the inspiration for this project came from [go-dualsense-battery](https://github.com/neolit123/go-dualsense-battery), [DS4Windows](https://github.com/schmaldeo/DS4Windows) and [TraySense](https://github.com/filipmachalowski/TraySense).
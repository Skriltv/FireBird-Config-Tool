# FireBird Config Tool

A simple Windows desktop application for opening and using the FireBird configuration tool in its own dedicated window.

## Features

- Dedicated Windows desktop interface
- Opens the FireBird configuration tool directly inside the app
- Uses Microsoft Edge WebView2 for the embedded browser
- Supports the FireBird configuration page's browser-based controller/device features
- Clean startup screen while the configuration tool loads
- Automatic error messages when a required browser component is unavailable

## Requirements

- Windows 10 or Windows 11
- Microsoft Edge WebView2 Runtime
- Internet access when loading the FireBird configuration page

## Installation

1. Download the latest **FireBird Config Tool** release.
2. Extract the downloaded ZIP if necessary.
3. Run **FireBird Config Tool.exe**.
4. If Windows displays a security warning, choose the option to allow the application to run if you trust the downloaded release.

## Using the App

When the application starts, it loads the FireBird configuration tool automatically.

Follow the instructions shown inside the application to connect and configure your compatible FireBird device.

The application itself is only the desktop wrapper around the configuration tool; configuration options and device support are provided by the FireBird web tool.

## WebView2

FireBird Config Tool uses Microsoft Edge WebView2 to display the configuration interface.

If WebView2 is missing or cannot be initialized, the application will display an error explaining what is required.

Windows 10/11 systems commonly already have the WebView2 Runtime installed. If yours does not, install the Microsoft Edge WebView2 Runtime and restart the application.

## Troubleshooting

### The app does not open

Try the following:

1. Make sure you are running the latest release.
2. Restart Windows and try again.
3. Install or repair the Microsoft Edge WebView2 Runtime.
4. Extract the application to a normal folder such as `Downloads` or `Documents` instead of running it directly from inside a ZIP file.
5. If Windows Defender or another security program blocks the application, verify that you downloaded the release from the expected source.

### The app opens but the configuration page does not load

Check that:

- Your PC has an active internet connection.
- The FireBird configuration website is available.
- Windows Firewall or security software is not blocking the application.

### My device is not detected

Make sure:

- The device is connected correctly.
- Any required device drivers are installed.
- No other application is currently using the device.
- Your browser/device supports the required USB functionality.

## Privacy

FireBird Config Tool does not require an account to use the desktop application itself.

The application loads the FireBird configuration web tool in an embedded browser. Any information or network requests made by that web tool are subject to the behavior and policies of the service it connects to.

## Disclaimer

FireBird Config Tool is an unofficial desktop wrapper/launcher for the FireBird configuration tool.

It is not affiliated with, endorsed by, or sponsored by the original FireBird tool or its developers unless explicitly stated in the release.

## Support

When reporting a problem, include:

- Windows version
- FireBird Config Tool version
- What happened when you launched the application
- Any error message shown by the application
- Whether WebView2 is installed
- Whether the device is detected by other compatible software

<<<<<<< HEAD
This information makes troubleshooting much easier.
=======
This information makes troubleshooting much easier.
>>>>>>> 39b3ea18aa0efdeb52e1aeec05ffd8ec3fec13e6

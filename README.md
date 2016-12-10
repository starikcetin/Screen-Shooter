# ScreenShooter for Unity3d

ScreenShooter allows you to take multiple screenshots at different resolutions with just one click, right from the Unity editor.

![Screenshoter logo](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/screenshooter_logo.png)

Features:
* Take multiple screenshots at multiple resolutions with a single click
* Quickly create good high quality screenshots or wallpapers
* Easily create all required screenshots for App Store or Google Play (presets included)
* Add or remove resolutions with ease and save them for later use.
* Take screenshots using any available camera

### ScreenShooter window

Before being able to take screenshots, you will first need to open ScreenShooter window. Select **Window → ScreenShoooter** from the main menu, as follows:

![Screenshooter menu](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/menu.png)

### Camera

You need to specify from which camera you want to take screenshots. The first enabled camera tagged "MainCamera" is selected by default.

![Camera select](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/camera.png)

### Screenshots Configuration

With **ScreehShooter** multiple screenshots can be taken with a single click. For each screenshot, you can specify desired name, resolution and file format (JPG or PNG).

![Screenshot Configurations](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/screenshots.png)

You can do that manually or use predefined values from the dropdown menu:

![Presets](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/presets.png)

### Tag

The tag is just a convenient way to specify common file name prefix for all screenshots that will be taken with one click. While this field is not required, it can save time in case you need to make few different sets of screenshots with the same configuration.

![Screenshots Tag](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/tag.png)

### Save To

Screenshots will be saved to `%YOUR_PROJECT%/Screenshots` folder by default. If you want to change the save path, click on **Browse** button and choose another folder, or enter the new path manually.

![Save To](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/saveto.png)

If target folder already exists then **Show** button will be enabled. You can use this button to open screenshots folder directly in system file manager.

If target folder doesn’t exist yet, it will be created when taking screenshots.

### Take Screenshots

Finally, you’re now able to take as many screenshots as you want, simply by pressing the **Take Screenshots** button!

**Please note:** There is currently a known bug within Unity itself preventing *"Screen space - Overlay"* UI items from being captured. Once Unity's Developers fix this bug UI elements should be captured correctly. As a workaround you can switch canvas render mode to *"Screen Space - Camera"* and set canvas plane distance close to camera near clipping plane.

![Screen Space](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/canvas.png)

### Folder Location

The “ScreenShoter” folder doesn’t require to be in the root of your project, you can freely  move it wherever you want.  Then just go to **Edit -> Preferences -> ScreenShooter** and update the folder location:

![Screen Space](https://raw.githubusercontent.com/PhannGor/phanngor.github.io/master/stuff/screenshooter/images/v1.2/prefs.png)

## Asset Store
ScreenShooter is donationware. If you want to support future development or just say "thanks" to autor, please buy it on the [Asset Store](http://u3d.as/q0j). Reviews are also highly appreciated.

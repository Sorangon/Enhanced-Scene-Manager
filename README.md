# Enhanced-Scene-Manager
This addon allow you to create levels using multiple scenes to optimize yout workflow and your application.

&nbsp;

# How to setup ?

### <ins><b>1 - Add the plugin to your project</b></ins>
This addon should be cloned into your Unity project to access new updates. It's highly recommended to use git submodules : https://git-scm.com/book/en/v2/Git-Tools-Submodules

- <b>Method 1 (Recommended)</b> : Create a folder "Enhanced Scene Manager" into your Asset folder and clone the project into it from new repository or git submodule.
- <b>Method 2</b> : Download the project .zip file and unpack it directly in your Asset folder.

### <ins><b>2 - Setup Enhanced Scene Manager in your project</ins></b>
Once the addon is imported in your project, two new types of assets appeared in your create panel.

- `Scene Bundle` : This asset is a group of scenes you want to load simultaneaously.
- `Scene Bundle List` :  The equivalent of build setting scenes, group every `Scene Bundle` you want to use and build in your application. Contains also a <b>Persistant Scene Bundle</b> that will be loaded once and persist while application is open. 

To setup this, go to "Window/Enhanced Scene Manager" to call the Enhanced Scene Manager window. 

By default the window suggest you to create a new `Scene Bundle List`. Create a new one and create new scene bundles.
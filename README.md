<p>
  <h3 align="center">Files Clean Up</h3>
  <p align="center">Simple files clean up tool</p>
</p>



## The Project
This project / app aims to simplify your workflow when you need to remove unused files that got deployed on your previous deployment. 
Also to  clean up any files you configured.



## Getting Started
As for now there's no binary provided, you need to manually build the project yourself. 
It should be easy enough.



### Prerequisites
The project developed under [.Net Framework 4.5](https://dotnet.microsoft.com/download/dotnet-framework), which come pre-installed on Visual Studio 2012 and also should be existed inside latest Windows 10 version.



### Installation
There's no installation needed, as this app is a simple console app. 
You just need to make sure the `config.xml` is there.



## Usage
Configure first then you're ready to go.  
The configuration is straight forward, you just need to create configuration under profile.  
Basically you need `RootDir` as a root directory, can be multiple. You need `Include` and `Exclude` as a Regex pattern.
If you need to confirm that included and excluded files are correct you can configure `GenerateFile` or if you don't need it, you could remove it.



```xml
<?xml version="1.0" encoding="UTF-8"?>
<Configuration>
  <Profile Name="front-end">
    <RootDir>E:\Sitecore-Project\vendor\zk_1</RootDir>

    <Include>.+\src\\.+</Include>

    <Exclude>.+\dist\\</Exclude>

    <GenerateFile IncludeList="true" ExcludeList="true" />
  </Profile>
</Configuration>
```



When you run the app, it'll ask you to `List` using `L` or `l`, or to `Delete` using `D` or `d`  
Before delete you need to generate the file list first by using `List` option. Afterthe list is generated you can use `Delete` option. 
Or if you have your own list you can just paste it into file named `FinalList-{something_like_date}.log`. It'll delete according to the list.



## License
Distributed under the UNLICENSE License. See `LICENSE` for more information.

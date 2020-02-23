**Elias**

I started this project about two weeks ago to convert early Flash furniture (2010-2012, for example) to cast files that work on the Shockwave client. It took me five entire days to write this and perfect it, including all animations, shadows, icons to convert perfectly to a .cct file. The whole point of this is to do fully automated furniture conversion without needing to touch a single thing.

It's called Elias because it's from the word 'alias' and the file memberalias.index which tells the client which images and frames are flipped depending on which rotation comes up a lot on every Shockwave file, so why not call it after something that it's similar to... 

This project consists of three other executables to complete the conversion process. The first is JPEXS to export all assets, the second is swfmill to export the SymbolClass (so we know which images are duplicates) and the last is a project file I've written entirely in Lingo and is a Macromedia projector (don't panic it's just just a simple .exe) which reads all images in a directory and spits out a .cct file since there's no third party library to create .cct files.

**Features**

- Converts furniture icon.
- Converts all furniture shadows.
- Converts all furniture z-heights (may not 100% accurate but very rarely this occurs).
- Converts all furniture frames/animations including correct speeds.
- Converts all furniture states.
- Converts all furniture blending/ink defined in XML per frame.
- Supports both wall/floor items (may need manual editing to get wall item placement to work again in the Shockwave client, but this is uncommon/rare)
- If a modern furniture (usually anything 2015+) doesn't have zoomed out images defined, the program will generate its own and downscale the images to create the "small" CCT versions if ticked yes in the configuration.
.
**Conversion Steps (steps the project does)**

- Run EliasApp.exe with either the directory of files to read / single .cct to read.
- EliasApp verifies each file has a furnidata.txt/.xml entry (program supports either formats) so it knows whether it's a wall or floor item.
- EliasApp calls JPEXS and swfmill to export assets into the temporary directory.
- EliasApp parses all assets, renames the files and puts it into the temporary "cast_data" directory ready for **EliasCompiler.exe** to read.
- EliasCompiler.exe reads all files and creates the necessary CCT.

**Screenshots**

Here's an example with doing the entire furniture of Celestial, with small generation of modern furnis turned off.


![](https://i.imgur.com/VRjDUPd.png)



![](https://i.imgur.com/6ujTmdM.gif)


**Command Arguments**

-directory "<folder path>"

The directory to parse every possible .swf inside.

-cct "<file path>"

The file path to a single file to parse.

**Configuration**

Here's an example configuration. JPEXS (listed as FFDec) is required. The converter path is a link to the Elias projector file written in Adobe Shockwave Lingo language. The furnidata can either be furnidata.xml or traditional furnidata.txt, it will detect either by the file extension.

**A furnidata entry for each .swf is mandatory, otherwise it won't convert.**

If 'small_furni' is set to true, it will generate small furni CCT files, if false, it overrides 'generate_modern' and will not generate any small CCTs.

If 'generate_modern' is set to true, it will generate small furni CCT files for modern Habbo files missing the zoomed out/32 images by downscaling the large images and recalculating the regPoints.

If 'save_as_cst' is set to true, it will save as .cst instead of .cct.


```
<configuration>
   <ffdec>C:\Program Files (x86)\FFDec\ffdec.exe</ffdec>
   <converter_app>C:\Users\Alex\Documents\GitHub\Elias\EliasDirector\elias_app.exe</converter_app>
   <output_path>C:\Users\Alex\Documents\GitHub\Elias\CCTs</output_path>
   <furnidata_path>furnidata.xml</furnidata_path>
   <small_furni>
      <generate>true</generate>
      <generate_modern>false</generate_modern>
   </small_furni>
   <options>
      <save_as_cst>false</save_as_cst>
      <close_when_finished>false</close_when_finished>
   </options>
</configuration>
```


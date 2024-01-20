# Palworld XGP Save Extractor

C# console app to package a PalWorld save from Xbox Game Pass to another platform (e.g. steam)

Based off of https://github.com/Z1ni/XGP-save-extractor/ but re-wrote for Palworld only as I didn't have python installed on my gaming PC to modify the script.

This program was rushed to work and not in a good state, but I have released it to help others. I apologise if it doesn't work.

## Prerequisites
- Install [.NET Runtime 8.0.1](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.1-windows-x64-installer) or [.NET Desktop Runtime 8.0.1](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.1-windows-x64-installer) if using the famework dependant release

## Instructions
1. [Download](https://github.com/StryderUK/PalWorldXGPSaveExtractor/releases) and extract
2. Run PalWorldXGPSaveExtractor.exe 
3. Follow promps
4. Once finished check "extracted" folder for the save archive
5. Go to %LocalAppData%\Pal\Saved\SaveGames
6. Extract and overwrite UserOption.sav
7. Extract save data folder into steam user save folder

The savegames folder should look like this:
<pre>
SaveGames
 └  &lt;steam_user_id_code> e.g 12345678901234567
   └ &lt;savefile_guid> e.g.71787C374B88F0E4F81E9DBAD77B81E6
     ├ Players
     | └ 00000000000000000000000000000001.sav
     ├ Level.sav
     ├ LevelMeta.sav
     ├ LocalData.sav
     └ WorldOption.sav
</pre>
 

## Thanks
Thanks to [@Z1ni](https://github.com/Z1ni/) for the python script this is based on & [@snoozbuster](https://github.com/snoozbuster) for figuring out the container format at https://github.com/goatfungus/NMSSaveEditor/issues/306.

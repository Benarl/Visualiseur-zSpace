using UnityEngine;
using System.Collections;

public class NewBehaviourScript1 : MonoBehaviour
{
    //skins and textures
    public GUISkin[] skins;
    public Texture2D file, folder, back, drive;
    public bool start = true;
    string[] layoutTypes = { "Type 0", "Type 1" };
    //initialize file browser
    FileBrowser fb = new FileBrowser();
    public string output = "no file";
    // Use this for initialization
    void Start()
    {
        //setup file browser style
        //fb.guiSkin = skins[0]; //set the starting skin
                               //set the various textures
        fb.fileTexture = file;
        fb.directoryTexture = folder;
        fb.backTexture = back;
        fb.driveTexture = drive;
        //show the search bar
        fb.showSearch = true;
        //search recursively (setting recursive search may cause a long delay)
        fb.searchRecursively = true;
    }

    void OnGUI()
    {
        if (!start)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            //draw and display output
            if (fb.draw())
            { //true is returned when a file has been selected
              //the output file is a member if the FileInfo class, if cancel was selected the value is null
                output = (fb.outputFile == null) ? "cancel hit" : fb.outputFile.ToString();
                start = true;
            }
        }
    }
}

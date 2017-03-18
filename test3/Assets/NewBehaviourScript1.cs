using UnityEngine;
using System.Collections;

public class NewBehaviourScript1 : MonoBehaviour
{
    public bool start = true;
    //initialize file browser
    FileBrowser fb = new FileBrowser("C:/");
    public string output = "no file";

    void Start()
    {
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

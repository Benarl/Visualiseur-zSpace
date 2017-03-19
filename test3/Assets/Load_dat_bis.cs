#define thread //comment out this line if you would like to disable multi-threaded search
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
#if thread
using System.Threading;
#endif


using System.ComponentModel;


public class MRMesh
{
    public struct meshStruct//structure permettant de stocker toutes les informations sur le fichier dat et qui est chargé une seul fois
    {
        public Vector3[] vertices;
        public List<Vector3[]> current_vertices;
        public int level;
        public int depth;
        public int number_triangles;
        public int primal_triangles;
        public int[][,] triangles;
        public int[][] current_triangles;
        public string fileName;
        public int[] multi_res;
        public int[,] face_res;
        public List<int> sort_vertices;
        public bool sens;
        public void inverse()
        {
            if (sens)
                sens = false;
            else
                sens = true;
        }
    }

    public meshStruct mesh_struct;

    public void delete()
    {
        mesh_struct.triangles = null;
        mesh_struct.vertices = null;
        mesh_struct.current_vertices = null;
        mesh_struct.current_triangles = null;
        mesh_struct.multi_res = null;
        mesh_struct.face_res = null;
        mesh_struct.sort_vertices = null;
    }
    
    /*
    Normalization : informations sur le nuage de vertex en renvoyant un premier vecteur contenant
    les dimensions de la bounding box (largeur, longueur, hauteur), un second contenant les coordonnées
    du centre du nuage et un troisième contenant le facteur d'échelle à appliquer à l'objet 
    */
    public List<Vector3> Normalization()
    {
        Vector3 CoordMax = new Vector3();
        Vector3 CoordMin = new Vector3();
        Vector3 mean = new Vector3();
        float alpha;
        for (int i = 0; i < mesh_struct.multi_res[0]; i++)
        {
            CoordMax = Vector3.Max(mesh_struct.vertices[i], CoordMax);
            CoordMin = Vector3.Min(mesh_struct.vertices[i], CoordMin);
            mean += mesh_struct.vertices[i];
        }

        mean /= mesh_struct.multi_res[0];
        alpha = Vector3.SqrMagnitude(CoordMax - CoordMin);
        alpha = (CoordMax - CoordMin).x + (CoordMax - CoordMin).x + (CoordMax - CoordMin).x;
        List<Vector3> Transformation = new List<Vector3>();
        Transformation.Add(new Vector3((CoordMax - CoordMin).x, (CoordMax - CoordMin).y, (CoordMax - CoordMin).z));
        Transformation.Add(new Vector3(mean.x, mean.y, mean.z));
        Transformation.Add(new Vector3(alpha, alpha, alpha));
        return Transformation;
    }

    /*
    changeConnectivity : Mise à jour du niveau de résolution. En argument l'entier du nombre de niveaux de résolution à incrémenter 
    tel que : level = level + c. Recharge current_vertices et current_triangle pour réactualiser l'affichage du maillage et charger les bons
    niveaux de résolutions
    */
    public bool changeConnectivity(int c)
    {
        if (this.mesh_struct.level + c < 0 || this.mesh_struct.level + c > this.mesh_struct.depth - 1)
            return false;
        
        mesh_struct.sort_vertices = new List<int>();
        mesh_struct.current_vertices = new List<Vector3[]>();
        List<Vector3> n = new List<Vector3>();
        n = Normalization();

        this.mesh_struct.level += c;
        for (int nb = 0; nb < mesh_struct.primal_triangles; nb++)
        {

            this.mesh_struct.current_triangles[nb] = new int[this.mesh_struct.face_res[nb, this.mesh_struct.level] * 3];

            int somme = 0;
            for (int i = 0; i < this.mesh_struct.level; i++)
                somme += this.mesh_struct.face_res[nb, i];

            for (int j = 0; j < this.mesh_struct.face_res[nb, this.mesh_struct.level]; j++)
            {
                this.mesh_struct.current_triangles[nb][3 * j] = this.mesh_struct.triangles[nb][0, somme + j];
                this.mesh_struct.current_triangles[nb][3 * j + 1] = this.mesh_struct.triangles[nb][1, somme + j];
                this.mesh_struct.current_triangles[nb][3 * j + 2] = this.mesh_struct.triangles[nb][2, somme + j];
            }


            foreach (int i in mesh_struct.current_triangles[nb])
            {
                if (!mesh_struct.sort_vertices.Contains(i))
                    mesh_struct.sort_vertices.Add(i);
            }
            mesh_struct.sort_vertices.Sort();
            mesh_struct.current_vertices.Add(new Vector3[mesh_struct.sort_vertices.Count]);

            for (int i = 0; i < mesh_struct.sort_vertices.Count; i++)
            {
                this.mesh_struct.current_vertices[nb][i] =(this.mesh_struct.vertices[mesh_struct.sort_vertices[i]] - n[1])/n[2][0];
            }

            for (int i = 0; i < mesh_struct.current_triangles[nb].Length; i++)
            {
                mesh_struct.current_triangles[nb][i] = mesh_struct.sort_vertices.IndexOf(mesh_struct.current_triangles[nb][i]);
            }
            
            mesh_struct.sort_vertices.Clear();
        }
        return true;
    }

    /*
    createMeshStruct : Première lecture du fichier dat pour fixer la longueur de tous les tableaux
    de la structure meshStruct et aussi pour définir le nombre de niveaux de résolution, le nombre de triangles
    primaire, le nombre de vertex et de triangles par niveaux de résolution. 
    */
    public void createMeshStruct(string filename)
    {

        int depth = 0;
        int depth_current = 0;
        int depth_before = 0;//track which level of depth in the hierarchy
        int before = 0;
        int now = 0;
        int parent_number = 0;
        int test = 0;
        int i = 0;
        int m = 0;
        int j = 0;
        int number_triangles = 0;
        int iterator_mesh = 0;
        int stop = 1;

        mesh_struct.fileName = filename;
        StreamReader stream = File.OpenText(filename);
        string entireText = stream.ReadToEnd();
        stream.Close();
         using (StringReader reader = new StringReader(entireText))
         {
             string currentText = reader.ReadLine();
             char[] splitIdentifier = { ' ' };
             string[] brokenString;
             while (currentText != null)
             {
                 currentText = currentText.Trim();                           //Trim the current line
                 brokenString = currentText.Split(splitIdentifier, 50);
                 if (brokenString[0] == "name:" )
                {
                    Int32.TryParse(brokenString[2], out test);
                    if(test == 1)
                    {
                        number_triangles++;
                    }

                }
                currentText = reader.ReadLine();
                if (currentText != null)
                {
                    if (currentText.Split(splitIdentifier, 50).Length != 4 && currentText.Split(splitIdentifier, 50)[0] != "name:")
                    {
                        currentText = currentText.Replace("   ", " ");//Some .obj files insert triple spaces, this removes them
                        currentText = currentText.Replace("  ", " ");
                    }
                    else if (currentText.Replace("  ", " ").Split(splitIdentifier, 50).Length != 6 && currentText.Split(splitIdentifier, 50)[0] == "name:")
                    {
                        currentText = currentText.Replace("   ", " ");//Some .obj files insert double spaces, this removes them.
                        currentText = currentText.Replace("  ", " ");
                    }
                }
            }
        }

         stream = File.OpenText(filename);
         entireText = stream.ReadToEnd();
         stream.Close();
        using (StringReader reader = new StringReader(entireText))
        {

            string currentText = reader.ReadLine();
            char[] splitIdentifier = { ' ' };
            string[] brokenString;
            while (stop != 0)
            {
                currentText = reader.ReadLine();
                currentText = currentText.Trim();                           //Trim the current line
                brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
                if (brokenString[0] == "depth")
                {
                    Int32.TryParse(brokenString[1], out depth);//store the number of level in depth
                    before = depth + 1;
                    stop = 0;
                }
            }
            int[] depth_parent_list = new int[depth + 1];
            int[] multi_res = new int[depth + 1];
            int[] count = new int[depth + 1];
            for (int k = 0; k < depth + 1; k++)
                depth_parent_list[k] = 0;
            mesh_struct.face_res = new int[number_triangles, depth + 1];
            mesh_struct.triangles = new int[number_triangles][,];
            mesh_struct.current_triangles = new int[number_triangles+1][];
            mesh_struct.primal_triangles = number_triangles;
            test = 0;

           currentText = reader.ReadLine();
            while (currentText != null)
            {
                currentText = currentText.Trim();     //Trim the current line
                brokenString = currentText.Split(splitIdentifier, 50);      //Split the line into an array, separating the original line by blank spaces
                if (Int32.TryParse(brokenString[0], out now))//condition if the first string of the split is a number
                {

                    if (now != before)//Compared the first number of each row and enter in the if when this number change
                    {
                        multi_res[j] = i;//Store the number of vertices per level of resolution
                        j++;
                        i = 0;
                    }
                    i++;
                    before = now;
                }
                else
                {
                    if (brokenString[0] == "name:")
                    {
                        Int32.TryParse(brokenString[2], out test);//test is equal to 1 for the first triangle and zero for the others
                        if (test == 1)
                        {
                            depth_before = 0;
                            depth_current = 0;
                            Int32.TryParse(brokenString[1], out parent_number);
                            depth_parent_list[0] = parent_number;//depth_parent_list store the value of the children and parent triangle
                            if (iterator_mesh != 0)
                            {
                                for (int k = 0; k < count.Length; k++)
                                    mesh_struct.face_res[iterator_mesh - 1, k] = count[k];

                                m = 0;
                                for (int v = 0; v < count.Length; v++)
                                {
                                    m = m + count[v];
                                    count[v] = 0;
                                }
                                mesh_struct.triangles[iterator_mesh - 1] = new int[3, m];

                            }
                            count[depth_current]++;
                            iterator_mesh++;
                        }
                        else
                        {
                            depth_before = depth_current;
                            Int32.TryParse(brokenString[1], out parent_number);
                            if (depth_parent_list[depth_before] == 3)
                            {
                                while (depth_parent_list[depth_current] == 3 && parent_number != 0)//determine how many level we go up if we fullfill one or many parent triangle
                                                                                                   //and store it in depth_jump
                                {
                                    depth_current--;
                                }
                            }
                            if (parent_number == 0)
                            {
                                depth_current++;
                            }
                            depth_parent_list[depth_current] = parent_number;
                            count[depth_current]++;
                        }
                    }
                    else if (brokenString[0] == "depth")
                    {
                        Int32.TryParse(brokenString[1], out depth);//store the number of level in depth
                        before = depth + 1;
                    }
                }
                currentText = reader.ReadLine();
                if (currentText != null)
                {      //Some .obj files insert double spaces, this removes them.
                    currentText = currentText.Replace("   ", " ");
                }
            }
            multi_res[depth] = i;

            mesh_struct.multi_res = multi_res;
            m = 0;
            for (int v = 0; v < multi_res.Length; v++)
                m = m + multi_res[v];
            mesh_struct.vertices = new Vector3[m];

            for (int k = 0; k < count.Length; k++)
                mesh_struct.face_res[iterator_mesh - 1, k] = count[k];

            m = 0;
            for (int v = 0; v < count.Length; v++)
            {
                m = m + count[v];
                count[v] = 0;
            }
            mesh_struct.triangles[iterator_mesh - 1] = new int[3, m];
        }

        mesh_struct.depth = depth + 1;
        mesh_struct.level = 0;
    }

    /*
    populateMeshStruct : Seconde lecture pour charger les données dans les tableaux vertices et triangles

    */
    public void populateMeshStruct()
    {
        StreamReader stream = File.OpenText(mesh_struct.fileName);
        string entireText = stream.ReadToEnd();
        stream.Close();
        using (StringReader reader = new StringReader(entireText))
        {
            string currentText = reader.ReadLine();

            char[] splitIdentifier = { ' ' };
            string[] brokenString;
            int v = 0;
            int g = 0;
            int f = 0;
            int now = 0;
            int[] count = new int[mesh_struct.depth + 1];
            int depth_current = 0;
            int iterator_mesh = 0;
            int depth_before = 0;//track which level of depth in the hierarchy
            int parent_number = 0;
            int test = 0;
            iterator_mesh = 0;
            int[] depth_parent_list = new int[mesh_struct.depth + 1];
            for (int k = 0; k < mesh_struct.depth + 1; k++)
                depth_parent_list[k] = 0;
            while (currentText != null)// && iterator_mesh != 3)
            {
                currentText = currentText.Trim();
                brokenString = currentText.Split(splitIdentifier, 50);
                if (Int32.TryParse(brokenString[0], out now))
                {
                    mesh_struct.vertices[v].x = Convert.ToSingle(brokenString[1]);
                    mesh_struct.vertices[v].y = Convert.ToSingle(brokenString[2]);
                    mesh_struct.vertices[v].z = Convert.ToSingle(brokenString[3]);
                    v++;
                }
                else if (brokenString[0] == "name:")
                {
                    Int32.TryParse(brokenString[2], out test);//test is equal to 1 for the first triangle and zero for the others
                    if (test == 1)
                    {
                        depth_before = 0;
                        depth_current = 0;
                        Int32.TryParse(brokenString[1], out parent_number);
                        depth_parent_list[0] = parent_number;//depth_parent_list store the value of the children and parent triangle
                        for (int i = 0; i < count.Length; i++)
                            count[i] = 0;
                        iterator_mesh++;
                    }
                    else
                    {
                        depth_before = depth_current;
                        Int32.TryParse(brokenString[1], out parent_number);
                        if (depth_parent_list[depth_before] == 3 && parent_number != 0)
                        {
                            while (depth_parent_list[depth_current] == 3)//determine how many level we go up if we fullfill one or many parent triangle
                                                                         //and store it in depth_jump
                            {
                                depth_current--;
                            }
                        }
                        if (parent_number == 0)
                        {
                            depth_current++;
                        }
                    }
                    count[depth_current]++;
                    depth_parent_list[depth_current] = parent_number;

                    g = 0;//g gives the index regarding the depth from which one must store the triangles
                    for (int i = 0; i < depth_current; i++)
                        g += mesh_struct.face_res[mesh_struct.number_triangles, i];

                    if (mesh_struct.sens)
                        f = 1;//f ensure to store vertex in the clockwise direction if it is not the case
                    else
                        f = -1;
                    for (int i = 0; i <= depth_current; i++)
                        if (depth_parent_list[i] != 0)
                            f *= -1;

                    if (f == 1)//permutation in clockwise direction
                    {
                        mesh_struct.triangles[iterator_mesh-1][0, count[depth_current] - 1 + g] = Convert.ToInt32(brokenString[3]);
                        mesh_struct.triangles[iterator_mesh-1][1, count[depth_current] - 1 + g] = Convert.ToInt32(brokenString[5]);
                        mesh_struct.triangles[iterator_mesh-1][2, count[depth_current] - 1 + g] = Convert.ToInt32(brokenString[4]);
                    }
                    else//already in the right order
                    {
                        mesh_struct.triangles[iterator_mesh-1][0, count[depth_current] - 1 + g] = Convert.ToInt32(brokenString[3]);
                        mesh_struct.triangles[iterator_mesh-1][1, count[depth_current] - 1 + g] = Convert.ToInt32(brokenString[4]);
                        mesh_struct.triangles[iterator_mesh-1][2, count[depth_current] - 1 + g] = Convert.ToInt32(brokenString[5]);
                    }

                }
                currentText = reader.ReadLine();
                if (currentText != null)
                {
                    if (currentText.Split(splitIdentifier, 50).Length != 4 && currentText.Split(splitIdentifier, 50)[0] != "name:")
                    {
                        currentText = currentText.Replace("   ", " ");//Some .obj files insert triple spaces, this removes them
                        currentText = currentText.Replace("  ", " ");
                    }
                    else if (currentText.Replace("  ", " ").Split(splitIdentifier, 50).Length != 6 && currentText.Split(splitIdentifier, 50)[0] == "name:")
                    {
                        currentText = currentText.Replace("   ", " ");//Some .obj files insert double spaces, this removes them.
                        currentText = currentText.Replace("  ", " ");
                    }
                }
            }
        }
    }
}

public class Load_dat_bis : MonoBehaviour
{
    public MRMesh MeshStr = new MRMesh();

    public MeshFilter[] filter;
    public Renderer rend;
    public MeshCollider col;
    int first = 0;
    List<GameObject> obj = new List<GameObject>();
    public Shader shader1;
    public Shader shader2;

    int faceNumber = 0;
    int level = 0;
    

    void Start()
    {
        if (GetComponent<NewBehaviourScript1>().start)
        {
            MeshStr.delete();
            foreach (GameObject GO in obj)
                Destroy(GO);
            MeshStr.mesh_struct.fileName = GetComponent<NewBehaviourScript1>().output;

            obj = new List<GameObject>();
            UnityEngine.Debug.Log("start");
            MeshStr.createMeshStruct(MeshStr.mesh_struct.fileName);

            MeshStr.populateMeshStruct();
            MeshStr.changeConnectivity(0);
            filter = new MeshFilter[MeshStr.mesh_struct.primal_triangles];
            UnityEngine.Debug.Log(MeshStr.mesh_struct.primal_triangles);
            filter[0] = gameObject.AddComponent<MeshFilter>();
            faceNumber = 0;
            for (int i = 0; i < MeshStr.mesh_struct.primal_triangles; i++)
                faceNumber += MeshStr.mesh_struct.face_res[i, level];
            for (int i = 0; i < filter.Length; i++)
            {

                obj.Add(new GameObject());
                obj[obj.Count - 1].name = "SRmesh";
                obj[obj.Count - 1].AddComponent<MeshCollider>();
                obj[obj.Count - 1].AddComponent<MeshRenderer>();
                obj[obj.Count - 1].GetComponent<Renderer>();
                obj[obj.Count - 1].AddComponent<Rigidbody>();
                obj[obj.Count - 1].GetComponent<Rigidbody>().useGravity = false;
                obj[obj.Count - 1].GetComponent<Rigidbody>().isKinematic = true;

                filter[i] = new MeshFilter();
                filter[i] = obj[obj.Count - 1].AddComponent<MeshFilter>();
                filter[i].mesh.vertices = MeshStr.mesh_struct.current_vertices[i];
                filter[i].mesh.triangles = MeshStr.mesh_struct.current_triangles[i];
                filter[i].mesh.RecalculateNormals();

                filter[i].mesh.RecalculateBounds();
                filter[i].mesh.MarkDynamic();
                filter[i].mesh.Optimize();

                obj[obj.Count - 1].transform.localScale /= 2;
                col = filter[i].GetComponent<MeshCollider>();
                rend = filter[i].GetComponent<Renderer>();
                rend.receiveShadows = false;
                Material material = new Material(Shader.Find("Standard"));
                rend.material = material;
                //material.color = Color.blue;
                col.sharedMesh = filter[i].mesh;

            }
            col = GetComponent<MeshCollider>();
        }
        first++;
    }

    void Update()
    {
        if (first == 1 && GetComponent<NewBehaviourScript1>().start)
        {
            Start();

        }

        if (Input.GetMouseButton(0) == true)
        {
            var tab = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "SRmesh");
            float rotSpeed = 5f;
            Vector3 sum = Vector3.zero;
            int n = 0;
            foreach (var t in tab)
            {
                sum += t.transform.position;
                n++;
            }
            sum /= n;
                foreach (var t in tab)
            {
                float rotX = Input.GetAxis("Mouse X") * rotSpeed;// * Mathf.Deg2Rad;
                float rotY = Input.GetAxis("Mouse Y") * rotSpeed;// * Mathf.Deg2Rad;
                t.transform.Rotate(Vector3.up , -rotX, Space.World);
                t.transform.Rotate(Vector3.right, rotY, Space.World);

            }
        }
        if (Input.GetMouseButton(1) == true)
        {

            var tab = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "SRmesh");
            float rotSpeed = 0.01f;
            foreach (var t in tab)
            {
                float rotX = Input.GetAxis("Mouse X") * rotSpeed;// * Mathf.Deg2Rad;
                float rotY = Input.GetAxis("Mouse Y") * rotSpeed;// * Mathf.Deg2Rad;
                t.transform.Translate(Vector3.up * rotY, Space.World);
                t.transform.Translate(Vector3.right * rotX, Space.World);
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {

            float rotSpeed = 2f;
            var tab = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "SRmesh");
            float scale = Input.GetAxis("Mouse ScrollWheel") * rotSpeed;
            foreach (var t in tab)
            {
                t.transform.localScale *= Mathf.Pow(2, scale);
            }

        }
        if (Input.GetKeyDown(KeyCode.P) && MeshStr.changeConnectivity(1))
        {
            level++;
            faceNumber = 0;
            for (int i = 0; i < MeshStr.mesh_struct.primal_triangles; i++)
                faceNumber += MeshStr.mesh_struct.face_res[i, level];
            for (int i = 0; i < filter.Length; i++)
            {
                filter[i] = obj[i].GetComponent<MeshFilter>();
                filter[i].mesh.Clear();
                filter[i].mesh.vertices = MeshStr.mesh_struct.current_vertices[i];
                filter[i].mesh.triangles = MeshStr.mesh_struct.current_triangles[i];
                filter[i].mesh.RecalculateNormals();

                filter[i].mesh.RecalculateBounds();
                filter[i].mesh.MarkDynamic();
                filter[i].mesh.Optimize();
            }
        }
        if (Input.GetKeyDown(KeyCode.M) && MeshStr.changeConnectivity(-1))
        {
            level--;
            faceNumber = 0;
            for (int i = 0; i < MeshStr.mesh_struct.primal_triangles; i++)
                faceNumber += MeshStr.mesh_struct.face_res[i, level];
            for (int i = 0; i < filter.Length; i++)
            {
                filter[i] = obj[i].GetComponent<MeshFilter>();
                filter[i].mesh.Clear();
                filter[i].mesh.vertices = MeshStr.mesh_struct.current_vertices[i];
                filter[i].mesh.triangles = MeshStr.mesh_struct.current_triangles[i];
                filter[i].mesh.RecalculateNormals();

                filter[i].mesh.RecalculateBounds();
                filter[i].mesh.MarkDynamic();
                filter[i].mesh.Optimize();
            }
        }
    }
    
    void OnGUI()
    {

            GUI.Box(new UnityEngine.Rect(10, 10, 200, 320), "Menu");

            // Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
            if (GUI.Button(new UnityEngine.Rect(20, 40, 110, 20), "Wireframe ON"))
            {
                GL.wireframe = true;
            }

            // Make the second button.
            if (GUI.Button(new UnityEngine.Rect(20, 70, 110, 20), "Wireframe OFF"))
            {
                GL.wireframe = false;
            }

            if (GUI.Button(new UnityEngine.Rect(20, 100, 110, 20), "Niveau + 1"))
            {
                if (MeshStr.changeConnectivity(1))
                {
                    level++;
                    faceNumber = 0;
                    for (int i = 0; i < MeshStr.mesh_struct.primal_triangles; i++)
                        faceNumber += MeshStr.mesh_struct.face_res[i, level];
                    for (int i = 0; i < filter.Length; i++)
                    {
                        filter[i] = obj[i].GetComponent<MeshFilter>();
                        filter[i].mesh.Clear();
                        filter[i].mesh.vertices = MeshStr.mesh_struct.current_vertices[i];
                        filter[i].mesh.triangles = MeshStr.mesh_struct.current_triangles[i];
                        filter[i].mesh.RecalculateNormals();

                        filter[i].mesh.RecalculateBounds();
                        filter[i].mesh.MarkDynamic();
                        filter[i].mesh.Optimize();
                    }
                }
            }

            if (GUI.Button(new UnityEngine.Rect(20, 130, 110, 20), "Niveau - 1"))
            {
                if (MeshStr.changeConnectivity(-1))
                {
                    level--;
                    faceNumber = 0;
                    for (int i = 0; i < MeshStr.mesh_struct.primal_triangles; i++)
                        faceNumber += MeshStr.mesh_struct.face_res[i, level];
                    for (int i = 0; i < filter.Length; i++)
                    {
                        filter[i] = obj[i].GetComponent<MeshFilter>();
                        filter[i].mesh.Clear();
                        filter[i].mesh.vertices = MeshStr.mesh_struct.current_vertices[i];
                        filter[i].mesh.triangles = MeshStr.mesh_struct.current_triangles[i];
                        filter[i].mesh.RecalculateNormals();

                        filter[i].mesh.RecalculateBounds();
                        filter[i].mesh.MarkDynamic();
                        filter[i].mesh.Optimize();
                    }
                }
            }

        if (GUI.Button(new UnityEngine.Rect(20, 160, 110, 20), "Charger fichier"))
        {
            GetComponent<NewBehaviourScript1>().start = false;
            first = 1;
        }

        if (GUI.Button(new UnityEngine.Rect(20, 190, 110, 20), "Reset"))
        {
            MeshStr.mesh_struct.level = 0;
            foreach (GameObject GO in obj)
                Destroy(GO);
            
            Start();
        }

        if (GUI.Button(new UnityEngine.Rect(20, 220, 110, 20), "Inverser normal"))
            {
                Debug.Log("sens : " + MeshStr.mesh_struct.sens);
                MeshStr.mesh_struct.inverse();
            MeshStr.mesh_struct.level = 0;
                foreach (GameObject GO in obj)
                    Destroy(GO);
                Debug.Log("sens : " + MeshStr.mesh_struct.sens);
                Start();
            }
        if (GetComponent<NewBehaviourScript1>().start)
        {
            GUI.TextField(new UnityEngine.Rect(20, 250, 100, 20), "Profondeur : " + MeshStr.mesh_struct.level + "/" + (MeshStr.mesh_struct.depth - 1));
            GUI.TextField(new UnityEngine.Rect(20, 280, 180, 20), "Nombre de Vertex : " + MeshStr.mesh_struct.multi_res[level]);
            GUI.TextField(new UnityEngine.Rect(20, 310, 180, 20), "Nombre de Face : " + faceNumber);
        }
        
    }
    
}

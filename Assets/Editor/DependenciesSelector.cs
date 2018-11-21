using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace Dependency{
    public class DependenciesSelector : EditorWindow {
        // 上一个选中的prefab对象
        static Object preObj;
        // 当前选中的prefab对象
        static Object obj;
        // obj的所有依赖对象
        static List<Object> selectedObjs = new List<Object>();
        // 保存存放资源的文件夹路径
        static string folder;
        static Vector2 scrollPos;
        // 等待复制的文件个数
        static int remainCount;

        // 保存项目的绝对路径        
        static string projectDir = System.Environment.CurrentDirectory.Replace('\\', '/');
        // 保存所有依赖资源的路径
        static List<string> dependentPaths = new List<string>();
        

        [MenuItem("Assets/Find Prefab Dependencies")]
        static void FindPrefabDependencies(){
            preObj = null;
            obj = null;
            folder = "";

            if(Selection.activeObject == null){
                return;
            }

            if(PrefabUtility.GetPrefabType(Selection.activeObject) != PrefabType.None) {
                obj = (Selection.activeObject) as GameObject;
                Debug.Log(obj.name);
            }

            Init();
        }

        private static void Init() {
            DependenciesSelector window = (DependenciesSelector)EditorWindow.GetWindow(typeof(DependenciesSelector), false, "Dependencies");
            window.Show();
        }

        // 获取所有引用资源的路径
        private static void GetAllDependencies(Object prefab){
            // 清空当前的保存的依赖资源路径
            dependentPaths.Clear();

            selectedObjs.Clear();

            string currentPath = AssetDatabase.GetAssetPath(prefab);
            Object[] roots = new Object[] { prefab };

            // 添加prefab所在路径
            dependentPaths.Add(currentPath);
            selectedObjs.Add(prefab);

            Object[] dependentObjs = EditorUtility.CollectDependencies(roots);
            string path;
            foreach(Object dependency in dependentObjs){
                path = AssetDatabase.GetAssetPath(dependency);
                Debug.Log(path);
                if(path == currentPath){
                    // 如果是自身，则不添加
                    continue;
                }

                // 因为EditorUtility.CollectDependencies有可能返回默认的Unity内置资源
                // 因此需要根据返回形式来判断当前引用是否为默认资源
                if(File.Exists(path)){
                    dependentPaths.Add(path);
                    selectedObjs.Add(dependency);
                }
            }
        }

        // 复制资源到指定文件夹
        private static void CopyAllDependencies(string copyFolder){
            // 重置当前未复制的文件数
            remainCount = dependentPaths.Count;
            // 开始复制资源操作
            string sourPath, destPath;
            string destDirectory;
            try {
                // 如果选择的文件夹不存在，那么就创建文件夹
                if(!Directory.Exists(copyFolder)){
                    Directory.CreateDirectory(copyFolder);
                }

                foreach(string dependentPath in dependentPaths){
                    sourPath = projectDir + "/" + dependentPath;
                    destPath = copyFolder + "/" + dependentPath;
                    destDirectory = Path.GetDirectoryName(destPath);
                    
                    // 创建目标资源所在的文件夹
                    if(!Directory.Exists(destDirectory)){
                        Directory.CreateDirectory(destDirectory);
                    }

                    // 采用强制覆盖的形式拷贝资源
                    File.Copy(sourPath, destPath, true);
                    remainCount --;

                    // 显示进度条
                    EditorUtility.DisplayProgressBar("Copy progress", "Remians " + remainCount + " files haven't been copyed.", remainCount / dependentPaths.Count);
                }
            } catch (System.Exception e) {
                EditorUtility.DisplayDialog("Copy faild", e.Message, "OK", "");
                // 创建目录失败
                return;
            }

            // 关闭进度条
            EditorUtility.ClearProgressBar();
        }

        // 修改文件路径里面的分隔符
        private static string ModifyPath(string path){
            return path.Replace('\\', '/');
        }

        void OnGUI() {
            EditorGUILayout.BeginVertical();
            
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Prefab：", GUILayout.Width(75));
                obj = EditorGUILayout.ObjectField(obj, typeof(Object), false);
                if(preObj != obj){
                    dependentPaths.Clear();                        
                }
                preObj = obj;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Folder：", GUILayout.Width(75));
#if UNITY_5_3_OR_NEWER            
            folder = EditorGUILayout.DelayedTextField(folder,  GUILayout.ExpandWidth(true));
#else
            folder = EditorGUILayout.TextField(folder,  GUILayout.ExpandWidth(true));
#endif   
            if(GUILayout.Button("Browser", GUILayout.Height(14), GUILayout.Width(80))) {
                // 默认定位到Temp目录下
                folder = EditorUtility.OpenFolderPanel("Folder to save dependencies", projectDir + "/Temp", "" );
            }
            EditorGUILayout.EndHorizontal();
            
            // 显示提示
            EditorGUILayout.BeginHorizontal();
            if (obj){
                if(string.IsNullOrEmpty(folder)){
#if UNITY_5_3_OR_NEWER            
                    EditorGUILayout.LabelField("Select the folder to save dependencies first!", EditorStyles.centeredGreyMiniLabel);
#else
                    EditorGUILayout.LabelField("Select the folder to save dependencies first!");
#endif   
                } else {
                    if (GUILayout.Button("Check Dependencies")){
                        GetAllDependencies(obj);
                    }
                }
            }
            else{
#if UNITY_5_3_OR_NEWER            
                EditorGUILayout.LabelField("Select an prefab first!", EditorStyles.centeredGreyMiniLabel);
#else
                EditorGUILayout.LabelField("Select an prefab first!");
#endif   
            }
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            // 使用ScollView显示所有资源路径
            if(dependentPaths.Count > 0) {
                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Dependent resource path:");
                if(GUILayout.Button("Copy Resources", GUILayout.Height(18), GUILayout.Width(120))) {
                    string dialog = "";
                    if(folder.Equals(projectDir)) {
                        dialog = "Choose root folder of project will overwrite the original resources! Are you sure to continue?";
                    } else {
                        dialog = "Are you sure you want to copy all dependent resources to the selected?";
                    }

                    // 弹出对话框进行确认
                    if(EditorUtility.DisplayDialog("Copy Resource to selected folder?", dialog, "Yes", "No")){
                        CopyAllDependencies(folder);
                    }
                }

                if(GUILayout.Button("Select in project", GUILayout.Height(18), GUILayout.Width(140))){
                    Selection.objects = selectedObjs.ToArray();
                }
                EditorGUILayout.EndHorizontal();

                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);
                foreach(string path in dependentPaths){
                    EditorGUILayout.SelectableLabel(path, GUILayout.Height(18));
                }
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.EndVertical();
        }

        void OnInspectorUpdate() {
            Repaint();
        }
    }
}